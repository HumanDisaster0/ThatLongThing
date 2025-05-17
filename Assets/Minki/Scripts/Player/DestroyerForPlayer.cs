using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestroyerForPlayer : MonoBehaviour
{
    [Header("감지 반경")]
    public Vector2 detectionHalfExtend = new Vector2(0.5f,1.0f) * 6f * 0.5f + new Vector2(0.5f,0.5f);

    [Header("파편 물리 힘")]
    public float upwardForce = 12f;      // 위로 튕겨나가는 힘
    public float sideJitter = 1.5f;      // 좌우로 흔들리듯 튕기는 범위
    public float torqueMin = -180f;      // 회전 최소값
    public float torqueMax = 180f;       // 회전 최대값

    [Header("파괴 Y축 하한선")]
    public int tilemapYMin = 11; // 이 Y값 이하에 위치한 타일은 보호(파괴하지 않음)

    [Header("참조")]
    public Tilemap[] destructibleTilemaps;    // 파괴 대상이 되는 타일맵들
    public GameObject flyingTilePrefab;       // 파괴 연출로 날아가는 프리팹 (SpriteRenderer + Rigidbody2D 필수)

    // 이동 방향 계산용
    private Vector3 lastPosition;             // 이전 프레임 위치
    private Vector2 moveDir = Vector2.right;  // 현재 이동 방향 (초기값 오른쪽)

    // 나중에 복구할 타일 정보 저장용
    private Dictionary<Tilemap, Dictionary<Vector3Int, TileBase>> destroyedTileMapData = new();

    void Update()
    {
        // 매 프레임, 지정된 모든 타일맵에 대해 파괴 대상이 있는지 검사
        foreach (var tilemap in destructibleTilemaps)
        {
            TryDestroyNearbyTiles(tilemap);
        }
    }

    void LateUpdate()
    {
        // 현재 이동 방향 계산 (position 변화량 기반)
        Vector3 delta = transform.position - lastPosition;
        if (delta.sqrMagnitude > 0.001f)
            moveDir = delta.normalized;

        lastPosition = transform.position;
    }

    void TryDestroyNearbyTiles(Tilemap tilemap)
    {
        // 1) 박스 중심 → 셀 중심
        Vector3 centerWorld = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(centerWorld);

        // 2) halfExtents(월드) → 셀 좌표계 폭
        int halfX = Mathf.CeilToInt(detectionHalfExtend.x / tilemap.cellSize.x);
        int halfY = Mathf.CeilToInt(detectionHalfExtend.y / tilemap.cellSize.y);

        // 3) 셀 범위 루프 (박스 안 후보 셀만)
        for (int dx = -halfX; dx <= halfX; ++dx)
        {
            for (int dy = -halfY; dy <= halfY; ++dy)
            {
                Vector3Int cellPos = centerCell + new Vector3Int(dx, dy, 0);

                if (cellPos.y <= tilemapYMin) continue;               // Y 하한 제한

                TileBase tile = tilemap.GetTile(cellPos);
                if (tile == null) continue;                           // 빈 칸 skip

                // 4) 월드 좌표 기준 박스 내부인지 세밀 확인 (셀 앵커 보정 포함)
                Vector3 tileCenter = tilemap.GetCellCenterWorld(cellPos);
                Vector3 local = tileCenter - centerWorld;

                if (Mathf.Abs(local.x) > detectionHalfExtend.x || Mathf.Abs(local.y) > detectionHalfExtend.y)
                    continue;                                         // 박스 밖

                // 5) 파괴 처리
                if (!destroyedTileMapData.ContainsKey(tilemap))
                    destroyedTileMapData[tilemap] = new Dictionary<Vector3Int, TileBase>();

                destroyedTileMapData[tilemap][cellPos] = tile;
                SpawnFlyingTile(tilemap, cellPos);
                tilemap.SetTile(cellPos, null);
            }
        }
    }


    // 파괴된 타일 연출용 프리팹을 소환해 튕겨냄
    void SpawnFlyingTile(Tilemap tilemap, Vector3Int cellPos)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
        GameObject flyingTile = Instantiate(flyingTilePrefab, worldPos, Quaternion.identity);

        // 타일맵의 원래 타일 이미지 복사
        var sr = flyingTile.GetComponent<SpriteRenderer>();
        sr.sprite = tilemap.GetSprite(cellPos);

        // 물리적 튕김 및 회전
        var rb = flyingTile.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 force = new Vector2(
            moveDir.x * Random.Range(0.5f, sideJitter), // 이동 방향 기준으로 수평 튕김
            upwardForce                                 // 위로 튕김
        );
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(torqueMin, torqueMax), ForceMode2D.Impulse);

        Destroy(flyingTile, 3f); // 3초 후 제거
    }

    // 적 또는 플레이어와 부딪혔을 때 처리

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            collision.gameObject.GetComponent<MMove>().SetStatus(MStatus.die);
        }
            
    }
    void OnGUI()
    {
        // 디버그 정보 스타일 설정
        GUIStyle debugStyle = new GUIStyle();
        debugStyle.fontSize = 16;
        debugStyle.normal.textColor = Color.yellow;

        // 화면 좌측 상단에 정보 표시
        int y = 10;
        GUI.Label(new Rect(10, y, 400, 20), $"플레이어 위치: {transform.position}", debugStyle); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"이동 방향: {moveDir}", debugStyle); y += 20;
        //GUI.Label(new Rect(10, y, 400, 20), $"파괴된 타일 수: {GetTotalDestroyedTiles()}", debugStyle); y += 20;

        // 바운드 정보
        Bounds playerBounds = new Bounds(transform.position, new Vector3(detectionHalfExtend.x * 2f, detectionHalfExtend.y * 2f, 0f));
        GUI.Label(new Rect(10, y, 400, 20), $"감지 영역: {playerBounds.min} ~ {playerBounds.max}", debugStyle);
    }

    //private void OnDrawGizmos()
    //{
    //    var tilemapD = destructibleTilemaps[0];

    //    // 플레이어 감지 영역 계산
    //    Bounds playerBounds = new Bounds(transform.position, new Vector3(detectionHalfExtend.x * 2f, detectionHalfExtend.y * 2f, 0f));

    //    // ① 검사 Bounds의 좌하단/우상단을 셀 좌표로 변환
    //    Vector3Int minCell = tilemapD.WorldToCell(playerBounds.min);
    //    Vector3Int maxCell = tilemapD.WorldToCell(playerBounds.max);

    //    // ② 타일맵 검색 영역 생성 (이 부분이 잘못되었을 수 있음)
    //    // Vector3Int size = maxCell - minCell + Vector3Int.one; // 기존 코드

    //    // 수정된 코드: 좌하단과 우상단 좌표로 직접 영역 지정
    //    BoundsInt searchArea = new BoundsInt();
    //    searchArea.min = minCell;
    //    searchArea.max = maxCell + Vector3Int.one; // 최대값은 제외되므로 +1

    //    // 타일맵 영역과 교집합
    //    searchArea = ClipTo(tilemapD.cellBounds, searchArea);
    //    // 감지 영역 박스 그리기
    //    Gizmos.DrawCube(searchArea.center, searchArea.size);

    //    // 감지 영역 외곽선 색상 설정 (불투명 녹색)
    //    Gizmos.color = new Color(0f, 1f, 0f, 1f);

    //    // 감지 영역 외곽선 그리기
    //    Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);



    //    // 플레이어 이동 방향 표시 (빨간색 선)
    //    if (moveDir.sqrMagnitude > 0)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(transform.position, transform.position + new Vector3(moveDir.x, moveDir.y, 0) * 1.5f);
    //    }

    //    // 타일맵 보호 영역 표시 (파란색 선)
    //    if (destructibleTilemaps != null && destructibleTilemaps.Length > 0)
    //    {
    //        Gizmos.color = Color.blue;
    //        foreach (var tilemap in destructibleTilemaps)
    //        {
    //            if (tilemap != null)
    //            {
    //                // 타일맵의 경계를 얻음
    //                BoundsInt bounds = tilemap.cellBounds;

    //                // Y축 하한선(보호 영역) 표시
    //                Vector3 start = tilemap.CellToWorld(new Vector3Int(bounds.xMin, tilemapYMin, 0));
    //                Vector3 end = tilemap.CellToWorld(new Vector3Int(bounds.xMax, tilemapYMin, 0));

    //                // 타일 크기만큼 오프셋 적용
    //                Vector3 cellSize = tilemap.cellSize;
    //                start += new Vector3(0, cellSize.y * 0.5f, 0);
    //                end += new Vector3(cellSize.x, cellSize.y * 0.5f, 0);

    //                Gizmos.DrawLine(start, end);
    //            }
    //        }
    //    }
    //}

    //// 플레이 모드에서도 기즈모를 표시하기 위한 함수
    //private void OnDrawGizmosSelected()
    //{
    //    // 선택됐을 때도 동일한 기즈모 표시
    //    OnDrawGizmos();
    //}


    // 적을 튕겨내는 연출 (타일처럼)
    public void SpawnFlyingEnemy(Vector3 position, Sprite originalSprite)
    {
        GameObject flyingObj = Instantiate(flyingTilePrefab, position, Quaternion.identity);

        var sr = flyingObj.GetComponent<SpriteRenderer>();
        sr.sprite = originalSprite; // Enemy 스프라이트 복사할 경우 설정 가능

        var rb = flyingObj.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 force = new Vector2(
            moveDir.x * Random.Range(0.5f, sideJitter),
            upwardForce
        );
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-60, 60), ForceMode2D.Impulse); //얘는 따로처리함

        Destroy(flyingObj, 3f);
    }

    // 이전에 파괴한 타일들을 복구함 (외부 트리거나 UI에서 호출 가능)
    public void RestoreDestroyedTiles()
    {
        foreach (var tilemapEntry in destroyedTileMapData)
        {
            Tilemap map = tilemapEntry.Key;
            foreach (var kvp in tilemapEntry.Value)
            {
                map.SetTile(kvp.Key, kvp.Value); // 원래 타일 복구
            }
        }

        destroyedTileMapData.Clear(); // 복구 기록 초기화
    }
}
