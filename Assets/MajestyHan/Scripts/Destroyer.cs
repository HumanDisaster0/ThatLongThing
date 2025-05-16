using UnityEngine;
using UnityEngine.Tilemaps;

public class Destroyer : MonoBehaviour
{
    [Header("감지 반경")]
    public float detectionRadius = 0.6f;

    [Header("파편 물리 힘")]
    public float upwardForce = 12f;
    public float sideJitter = 1.5f;
    public float torqueMin = -180f;
    public float torqueMax = 180f;

    [Header("파괴할 타일맵 미리 지정해줘야함")]
    public Tilemap[] destructibleTilemaps;      // 파괴 가능한 타일맵들
    public GameObject flyingTilePrefab;         // 파편 프리팹

    private Vector3 lastPosition;
    private Vector2 moveDir = Vector2.right;    // 초기 이동 방향은 오른쪽

    void Update()
    {
        foreach (var tilemap in destructibleTilemaps)
        {
            TryDestroyNearbyTiles(tilemap);
        }
    }

    void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;
        if (delta.sqrMagnitude > 0.001f)
            moveDir = delta.normalized;

        lastPosition = transform.position;
    }

    void TryDestroyNearbyTiles(Tilemap tilemap)
    {
        Vector3 center = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(center);
        int radiusInCells = Mathf.CeilToInt(detectionRadius / tilemap.cellSize.x);

        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cellPos = centerCell + offset;

                Vector3 worldCenter = tilemap.GetCellCenterWorld(centerCell);
                Vector3 worldTile = tilemap.GetCellCenterWorld(cellPos);
                float dist = Vector3.Distance(worldCenter, worldTile);

                if (dist <= detectionRadius)
                {
                    TileBase tile = tilemap.GetTile(cellPos);
                    if (tile != null)
                    {
                        SpawnFlyingTile(tilemap, cellPos);
                        tilemap.SetTile(cellPos, null);
                    }
                }
            }
        }
    }

    void SpawnFlyingTile(Tilemap tilemap, Vector3Int cellPos)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
        GameObject flyingTile = Instantiate(flyingTilePrefab, worldPos, Quaternion.identity);

        var sr = flyingTile.GetComponent<SpriteRenderer>();
        sr.sprite = tilemap.GetSprite(cellPos);

        var rb = flyingTile.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 force = new Vector2(
            moveDir.x * Random.Range(0.5f, sideJitter),
            upwardForce
        );
        rb.AddForce(force, ForceMode2D.Impulse);

        float torque = Random.Range(torqueMin, torqueMax);
        rb.AddTorque(torque, ForceMode2D.Impulse);

        Destroy(flyingTile, 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            // 적이 닿았을 때 처리
            // collision.collider.GetComponent<Enemy>()?.Disable();

            SpawnFlyingEnemy(collision.collider.transform.position);
            Destroy(collision.collider.gameObject);
        }

        if (collision.collider.CompareTag("Player"))
        {
            // 플레이어와 닿았을 때 처리
            // collision.collider.GetComponent<Player>()?.Die();
        }
    }

    void SpawnFlyingEnemy(Vector3 position)
    {
        GameObject flyingObj = Instantiate(flyingTilePrefab, position, Quaternion.identity);
        var sr = flyingObj.GetComponent<SpriteRenderer>();
        sr.sprite = null; // 필요시 enemy 스프라이트로 대체

        var rb = flyingObj.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(
            moveDir.x * Random.Range(0.5f, sideJitter),
            upwardForce
        );
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(torqueMin, torqueMax), ForceMode2D.Impulse);

        Destroy(flyingObj, 3f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
