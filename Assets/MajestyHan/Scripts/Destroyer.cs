using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Destroyer : MonoBehaviour
{
    [Header("���� �ݰ�")]
    public float detectionRadius = 0.6f; // �� �ݰ� ���� Ÿ�ϸ� �ı� ���

    [Header("���� ���� ��")]
    public float upwardForce = 12f;      // ���� ƨ�ܳ����� ��
    public float sideJitter = 1.5f;      // �¿�� ��鸮�� ƨ��� ����
    public float torqueMin = -180f;      // ȸ�� �ּҰ�
    public float torqueMax = 180f;       // ȸ�� �ִ밪

    [Header("�ı� Y�� ���Ѽ�")]
    public int tilemapYMin = 11; // �� Y�� ���Ͽ� ��ġ�� Ÿ���� ��ȣ(�ı����� ����)

    [Header("����")]
    public Tilemap[] destructibleTilemaps;    // �ı� ����� �Ǵ� Ÿ�ϸʵ�
    public GameObject flyingTilePrefab;       // �ı� ����� ���ư��� ������ (SpriteRenderer + Rigidbody2D �ʼ�)

    // �̵� ���� ����
    private Vector3 lastPosition;             // ���� ������ ��ġ
    private Vector2 moveDir = Vector2.right;  // ���� �̵� ���� (�ʱⰪ ������)

    // ���߿� ������ Ÿ�� ���� �����
    private Dictionary<Tilemap, Dictionary<Vector3Int, TileBase>> destroyedTileMapData = new();

    void Update()
    {
        // �� ������, ������ ��� Ÿ�ϸʿ� ���� �ı� ����� �ִ��� �˻�
        foreach (var tilemap in destructibleTilemaps)
        {
            TryDestroyNearbyTiles(tilemap);
        }
    }

    void LateUpdate()
    {
        // ���� �̵� ���� ��� (position ��ȭ�� ���)
        Vector3 delta = transform.position - lastPosition;
        if (delta.sqrMagnitude > 0.001f)
            moveDir = delta.normalized;

        lastPosition = transform.position;
    }

    // Ÿ�ϸ� �� ���� �ݰ� ���� Ÿ���� �˻��ϰ�, �ı� ����̸� ����
    void TryDestroyNearbyTiles(Tilemap tilemap)
    {
        Vector3 center = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(center); // ���� ��ġ�� Ÿ�ϸ� �� ��ǥ�� ��ȯ
        int radiusInCells = Mathf.CeilToInt(detectionRadius / tilemap.cellSize.x); // �� ���� �ݰ� ���

        // ���� ���� �� ��ȸ
        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cellPos = centerCell + offset;

                // Y ���Ѽ� ������ Ÿ���� �ǵ帮�� ����
                if (cellPos.y <= tilemapYMin) continue;

                // ���� ���� �Ÿ��� �ݰ� �ȿ� �ִ��� üũ
                Vector3 worldCenter = tilemap.GetCellCenterWorld(centerCell);
                Vector3 worldTile = tilemap.GetCellCenterWorld(cellPos);
                float dist = Vector3.Distance(worldCenter, worldTile);

                if (dist <= detectionRadius)
                {
                    TileBase tile = tilemap.GetTile(cellPos);
                    if (tile != null)
                    {
                        // ������ ������ ����
                        if (!destroyedTileMapData.ContainsKey(tilemap))
                            destroyedTileMapData[tilemap] = new Dictionary<Vector3Int, TileBase>();

                        destroyedTileMapData[tilemap][cellPos] = tile;

                        // ���ư��� ���� ���� + Ÿ�� ����
                        SpawnFlyingTile(tilemap, cellPos);
                        tilemap.SetTile(cellPos, null); // Ÿ�� ����
                    }
                }
            }
        }
    }

    // �ı��� Ÿ�� ����� �������� ��ȯ�� ƨ�ܳ�
    void SpawnFlyingTile(Tilemap tilemap, Vector3Int cellPos)
    {
        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
        GameObject flyingTile = Instantiate(flyingTilePrefab, worldPos, Quaternion.identity);

        // Ÿ�ϸ��� ���� Ÿ�� �̹��� ����
        var sr = flyingTile.GetComponent<SpriteRenderer>();
        sr.sprite = tilemap.GetSprite(cellPos);

        // ������ ƨ�� �� ȸ��
        var rb = flyingTile.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 force = new Vector2(
            moveDir.x * Random.Range(0.5f, sideJitter), // �̵� ���� �������� ���� ƨ��
            upwardForce                                 // ���� ƨ��
        );
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(torqueMin, torqueMax), ForceMode2D.Impulse);

        Destroy(flyingTile, 3f); // 3�� �� ����
    }

    // �� �Ǵ� �÷��̾�� �ε����� �� ó��

    // ���� ƨ�ܳ��� ���� (Ÿ��ó��)
    public void SpawnFlyingEnemy(Vector3 position, Sprite originalSprite)
    {
        GameObject flyingObj = Instantiate(flyingTilePrefab, position, Quaternion.identity);

        var sr = flyingObj.GetComponent<SpriteRenderer>();
        sr.sprite = originalSprite; // Enemy ��������Ʈ ������ ��� ���� ����

        var rb = flyingObj.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector2 force = new Vector2(
            moveDir.x * Random.Range(0.5f, sideJitter),
            upwardForce
        );
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-60, 60), ForceMode2D.Impulse); //��� ����ó����

        Destroy(flyingObj, 3f);
    }

    // ������ �ı��� Ÿ�ϵ��� ������ (�ܺ� Ʈ���ų� UI���� ȣ�� ����)
    public void RestoreDestroyedTiles()
    {
        foreach (var tilemapEntry in destroyedTileMapData)
        {
            Tilemap map = tilemapEntry.Key;
            foreach (var kvp in tilemapEntry.Value)
            {
                map.SetTile(kvp.Key, kvp.Value); // ���� Ÿ�� ����
            }
        }

        destroyedTileMapData.Clear(); // ���� ��� �ʱ�ȭ
    }

    // Scene �信�� ���� �ݰ� Ȯ�ο�
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
