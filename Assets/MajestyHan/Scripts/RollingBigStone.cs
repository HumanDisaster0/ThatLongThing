using UnityEngine;
using UnityEngine.Tilemaps;

public class RollingBigStone : MonoBehaviour
{
    // === [�̵� ���� ����] ===
    [Header("�̵� �ӵ�")]
    public float moveSpeed = 1.5f; // �� ������ ���������� �̵��� �ӵ�

    // === [���� ���� ����] ===
    [Header("���� �ݰ� (World ����)")]
    public float detectionRadius = 0.6f; // �� �ݰ� ���� Ÿ�ϸ� �ı� ������� ������

    // === [���� ���ư��� �� ����] ===
    [Header("���ư��� �� ����")]
    public float upwardForce = 12f;        // ���� ƨ������ �⺻ ��
    public float sideJitter = 1.5f;        // �¿� ���� ���� (���� ������)
    public float torqueMin = -360f;        // ȸ�� �ּҰ�
    public float torqueMax = 360f;         // ȸ�� �ִ밪

    // === [������ ������Ʈ��] ===
    [Header("����")]
    public Tilemap tilemap;               // �μ� Ÿ�ϸ� (Ground Ÿ�ϸ�)
    public GameObject flyingTilePrefab;   // �ı��� Ÿ��ó�� ���ư� ������

    void Update()
    {
        // [1] ���������� ���� �̵�
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);

        // [2] ���� ��ġ�� �� ��ǥ �������� ���� Ž��
        Vector3 center = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(center);

        // [3] detectionRadius ���� �� ������ ��ȯ
        int radiusInCells = Mathf.CeilToInt(detectionRadius / tilemap.cellSize.x);

        // [4] ���� �� �� Ž��
        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cellPos = centerCell + offset;

                // [5] ���� ���� �߽� �� ���� �Ÿ� Ȯ�� (���� ���� ������ üũ)
                Vector3 worldCenter = tilemap.GetCellCenterWorld(centerCell);
                Vector3 worldTile = tilemap.GetCellCenterWorld(cellPos);
                float dist = Vector3.Distance(worldCenter, worldTile);

                if (dist <= detectionRadius)
                {
                    // [6] Ÿ���� �����ϸ� ���� + ���� ����
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

    // === [���� ���� �Լ�] ===
    void SpawnFlyingTile(TileBase tile, Vector3Int cellPos)
    {
        // [1] �� ��ġ�� ���� ��ǥ ����
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

        // [2] ������ ����
        GameObject flyingTile = Instantiate(flyingTilePrefab, worldPos, Quaternion.identity);

        // [3] Ÿ�� ��������Ʈ �����ؼ� ���̱�
        var sr = flyingTile.GetComponent<SpriteRenderer>();
        sr.sprite = tilemap.GetSprite(cellPos);

        // [4] ������ ����: ���� ƨ�ܳ� + �ణ �¿� ����
        var rb = flyingTile.GetComponent<Rigidbody2D>();
        Vector2 force = new Vector2(
            Random.Range(-sideJitter, sideJitter), // ���� ���� ��
            upwardForce                            // ���� ���� ��
        );
        rb.AddForce(force, ForceMode2D.Impulse);

        // [5] ȸ���� ����
        float torque = Random.Range(torqueMin, torqueMax);
        rb.AddTorque(torque, ForceMode2D.Impulse);

        // [6] ���� �ð� �� �ڵ� ����
        Destroy(flyingTile, 3f);
    }

    // === [���� �ݰ� �ð�ȭ - Scene �信�� Ȯ�ο�] ===
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
