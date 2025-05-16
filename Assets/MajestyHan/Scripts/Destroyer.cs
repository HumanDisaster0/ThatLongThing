using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Destroyer : MonoBehaviour
{
    [Header("감지 반경")]
    public float detectionRadius = 0.6f; // 이 반경 안의 타일만 파괴 대상

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

    // 타일맵 내 감지 반경 안의 타일을 검사하고, 파괴 대상이면 제거
    void TryDestroyNearbyTiles(Tilemap tilemap)
    {
        Vector3 center = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(center); // 현재 위치를 타일맵 셀 좌표로 변환
        int radiusInCells = Mathf.CeilToInt(detectionRadius / tilemap.cellSize.x); // 셀 단위 반경 계산

        // 감지 범위 셀 순회
        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cellPos = centerCell + offset;

                // Y 하한선 이하인 타일은 건드리지 않음
                if (cellPos.y <= tilemapYMin) continue;

                // 실제 월드 거리로 반경 안에 있는지 체크
                Vector3 worldCenter = tilemap.GetCellCenterWorld(centerCell);
                Vector3 worldTile = tilemap.GetCellCenterWorld(cellPos);
                float dist = Vector3.Distance(worldCenter, worldTile);

                if (dist <= detectionRadius)
                {
                    TileBase tile = tilemap.GetTile(cellPos);
                    if (tile != null)
                    {
                        // 복구용 데이터 저장
                        if (!destroyedTileMapData.ContainsKey(tilemap))
                            destroyedTileMapData[tilemap] = new Dictionary<Vector3Int, TileBase>();

                        destroyedTileMapData[tilemap][cellPos] = tile;

                        // 날아가는 연출 생성 + 타일 제거
                        SpawnFlyingTile(tilemap, cellPos);
                        tilemap.SetTile(cellPos, null); // 타일 제거
                    }
                }
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

    // Scene 뷰에서 감지 반경 확인용
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
