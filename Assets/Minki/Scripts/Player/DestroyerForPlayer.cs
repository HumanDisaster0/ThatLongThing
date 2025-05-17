using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestroyerForPlayer : MonoBehaviour
{
    [Header("���� �ݰ�")]
    public Vector2 detectionHalfExtend = new Vector2(0.5f,1.0f) * 6f * 0.5f + new Vector2(0.5f,0.5f);

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

    void TryDestroyNearbyTiles(Tilemap tilemap)
    {
        // 1) �ڽ� �߽� �� �� �߽�
        Vector3 centerWorld = transform.position;
        Vector3Int centerCell = tilemap.WorldToCell(centerWorld);

        // 2) halfExtents(����) �� �� ��ǥ�� ��
        int halfX = Mathf.CeilToInt(detectionHalfExtend.x / tilemap.cellSize.x);
        int halfY = Mathf.CeilToInt(detectionHalfExtend.y / tilemap.cellSize.y);

        // 3) �� ���� ���� (�ڽ� �� �ĺ� ����)
        for (int dx = -halfX; dx <= halfX; ++dx)
        {
            for (int dy = -halfY; dy <= halfY; ++dy)
            {
                Vector3Int cellPos = centerCell + new Vector3Int(dx, dy, 0);

                if (cellPos.y <= tilemapYMin) continue;               // Y ���� ����

                TileBase tile = tilemap.GetTile(cellPos);
                if (tile == null) continue;                           // �� ĭ skip

                // 4) ���� ��ǥ ���� �ڽ� �������� ���� Ȯ�� (�� ��Ŀ ���� ����)
                Vector3 tileCenter = tilemap.GetCellCenterWorld(cellPos);
                Vector3 local = tileCenter - centerWorld;

                if (Mathf.Abs(local.x) > detectionHalfExtend.x || Mathf.Abs(local.y) > detectionHalfExtend.y)
                    continue;                                         // �ڽ� ��

                // 5) �ı� ó��
                if (!destroyedTileMapData.ContainsKey(tilemap))
                    destroyedTileMapData[tilemap] = new Dictionary<Vector3Int, TileBase>();

                destroyedTileMapData[tilemap][cellPos] = tile;
                SpawnFlyingTile(tilemap, cellPos);
                tilemap.SetTile(cellPos, null);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            collision.gameObject.GetComponent<MMove>().SetStatus(MStatus.die);
        }
            
    }
    void OnGUI()
    {
        // ����� ���� ��Ÿ�� ����
        GUIStyle debugStyle = new GUIStyle();
        debugStyle.fontSize = 16;
        debugStyle.normal.textColor = Color.yellow;

        // ȭ�� ���� ��ܿ� ���� ǥ��
        int y = 10;
        GUI.Label(new Rect(10, y, 400, 20), $"�÷��̾� ��ġ: {transform.position}", debugStyle); y += 20;
        GUI.Label(new Rect(10, y, 400, 20), $"�̵� ����: {moveDir}", debugStyle); y += 20;
        //GUI.Label(new Rect(10, y, 400, 20), $"�ı��� Ÿ�� ��: {GetTotalDestroyedTiles()}", debugStyle); y += 20;

        // �ٿ�� ����
        Bounds playerBounds = new Bounds(transform.position, new Vector3(detectionHalfExtend.x * 2f, detectionHalfExtend.y * 2f, 0f));
        GUI.Label(new Rect(10, y, 400, 20), $"���� ����: {playerBounds.min} ~ {playerBounds.max}", debugStyle);
    }

    //private void OnDrawGizmos()
    //{
    //    var tilemapD = destructibleTilemaps[0];

    //    // �÷��̾� ���� ���� ���
    //    Bounds playerBounds = new Bounds(transform.position, new Vector3(detectionHalfExtend.x * 2f, detectionHalfExtend.y * 2f, 0f));

    //    // �� �˻� Bounds�� ���ϴ�/������ �� ��ǥ�� ��ȯ
    //    Vector3Int minCell = tilemapD.WorldToCell(playerBounds.min);
    //    Vector3Int maxCell = tilemapD.WorldToCell(playerBounds.max);

    //    // �� Ÿ�ϸ� �˻� ���� ���� (�� �κ��� �߸��Ǿ��� �� ����)
    //    // Vector3Int size = maxCell - minCell + Vector3Int.one; // ���� �ڵ�

    //    // ������ �ڵ�: ���ϴܰ� ���� ��ǥ�� ���� ���� ����
    //    BoundsInt searchArea = new BoundsInt();
    //    searchArea.min = minCell;
    //    searchArea.max = maxCell + Vector3Int.one; // �ִ밪�� ���ܵǹǷ� +1

    //    // Ÿ�ϸ� ������ ������
    //    searchArea = ClipTo(tilemapD.cellBounds, searchArea);
    //    // ���� ���� �ڽ� �׸���
    //    Gizmos.DrawCube(searchArea.center, searchArea.size);

    //    // ���� ���� �ܰ��� ���� ���� (������ ���)
    //    Gizmos.color = new Color(0f, 1f, 0f, 1f);

    //    // ���� ���� �ܰ��� �׸���
    //    Gizmos.DrawWireCube(playerBounds.center, playerBounds.size);



    //    // �÷��̾� �̵� ���� ǥ�� (������ ��)
    //    if (moveDir.sqrMagnitude > 0)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(transform.position, transform.position + new Vector3(moveDir.x, moveDir.y, 0) * 1.5f);
    //    }

    //    // Ÿ�ϸ� ��ȣ ���� ǥ�� (�Ķ��� ��)
    //    if (destructibleTilemaps != null && destructibleTilemaps.Length > 0)
    //    {
    //        Gizmos.color = Color.blue;
    //        foreach (var tilemap in destructibleTilemaps)
    //        {
    //            if (tilemap != null)
    //            {
    //                // Ÿ�ϸ��� ��踦 ����
    //                BoundsInt bounds = tilemap.cellBounds;

    //                // Y�� ���Ѽ�(��ȣ ����) ǥ��
    //                Vector3 start = tilemap.CellToWorld(new Vector3Int(bounds.xMin, tilemapYMin, 0));
    //                Vector3 end = tilemap.CellToWorld(new Vector3Int(bounds.xMax, tilemapYMin, 0));

    //                // Ÿ�� ũ�⸸ŭ ������ ����
    //                Vector3 cellSize = tilemap.cellSize;
    //                start += new Vector3(0, cellSize.y * 0.5f, 0);
    //                end += new Vector3(cellSize.x, cellSize.y * 0.5f, 0);

    //                Gizmos.DrawLine(start, end);
    //            }
    //        }
    //    }
    //}

    //// �÷��� ��忡���� ����� ǥ���ϱ� ���� �Լ�
    //private void OnDrawGizmosSelected()
    //{
    //    // ���õ��� ���� ������ ����� ǥ��
    //    OnDrawGizmos();
    //}


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
}
