using UnityEngine;
using UnityEngine.Tilemaps;

public class RollingBigStone : MonoBehaviour
{
    // === [이동 관련 설정] ===
    [Header("이동 속도")]
    public float moveSpeed = 1.5f; // 매 프레임 오른쪽으로 이동할 속도

    // === [감지 범위 설정] ===
    [Header("감지 반경 (World 단위)")]
    public float detectionRadius = 0.6f; // 이 반경 안의 타일만 파괴 대상으로 감지함

    // === [파편 날아가는 힘 조정] ===
    [Header("날아가는 힘 조정")]
    public float upwardForce = 12f;        // 위로 튕겨지는 기본 힘
    public float sideJitter = 1.5f;        // 좌우 퍼짐 정도 (수평 랜덤성)
    public float torqueMin = -360f;        // 회전 최소값
    public float torqueMax = 360f;         // 회전 최대값

    // === [참조할 오브젝트들] ===
    [Header("참조")]
    public Tilemap tilemap;               // 부술 타일맵 (Ground 타일맵)
    public GameObject flyingTilePrefab;   // 파괴된 타일처럼 날아갈 프리팹

    void Update()
    {
        // [1] 오른쪽으로 지속 이동
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        // [2] 현재 위치의 셀 좌표 기준으로 범위 탐색
        Vector3 center = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(center);

        // [3] detectionRadius 값을 셀 단위로 변환
        int radiusInCells = Mathf.CeilToInt(detectionRadius / tilemap.cellSize.x);

        // [4] 범위 내 셀 탐색
        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cellPos = centerCell + offset;

                // [5] 현재 셀과 중심 셀 간의 거리 확인 (원형 범위 내인지 체크)
                Vector3 worldCenter = tilemap.GetCellCenterWorld(centerCell);
                Vector3 worldTile = tilemap.GetCellCenterWorld(cellPos);
                float dist = Vector3.Distance(worldCenter, worldTile);

                if (dist <= detectionRadius)
                {
                    // [6] 타일이 존재하면 삭제 + 파편 생성
                    TileBase tile = tilemap.GetTile(cellPos);
                    if (tile != null)
                    {
                        SpawnFlyingTile(tile, cellPos);
                        tilemap.SetTile(cellPos, null);
                    }
                }
            }
        }
    }

    // === [파편 생성 함수] ===
    void SpawnFlyingTile(TileBase tile, Vector3Int cellPos)
    {
        // [1] 셀 위치의 월드 좌표 구함
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

        // [2] 프리팹 생성
        GameObject flyingTile = Instantiate(flyingTilePrefab, worldPos, Quaternion.identity);

        // [3] 타일 스프라이트 복사해서 붙이기
        var sr = flyingTile.GetComponent<SpriteRenderer>();
        sr.sprite = tilemap.GetSprite(cellPos);

        // [4] 물리력 적용: 위로 튕겨냄 + 약간 좌우 퍼짐
        var rb = flyingTile.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(
            Random.Range(-sideJitter, sideJitter), // 수평 방향 힘
            upwardForce                            // 수직 방향 힘
        );
        rb.AddForce(force, ForceMode2D.Impulse);

        // [5] 회전력 적용
        float torque = Random.Range(torqueMin, torqueMax);
        rb.AddTorque(torque, ForceMode2D.Impulse);

        // [6] 일정 시간 뒤 자동 삭제
        Destroy(flyingTile, 3f);
    }

    // === [감지 반경 시각화 - Scene 뷰에서 확인용] ===
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
