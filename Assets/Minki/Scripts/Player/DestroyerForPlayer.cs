using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DestroyerForPlayer : MonoBehaviour
{
    [Header("���� �ݰ�")]
    public Vector2 detectionHalfExtend = new Vector2(0.5f,1.0f) * 6f * 0.5f + new Vector2(0.5f,0.5f);

    [Header("���� ���� ��")]
    public float upwardForce = 12f;      // ���� ƨ�ܳ����� ��
    public float sideJitter = 1.5f;      // �¿�� ��鸮�� ƨ��� ����
    public float torqueMin = -90f;      // ȸ�� �ּҰ�
    public float torqueMax = 90f;       // ȸ�� �ִ밪

    [Header("�ı� Y�� ���Ѽ�")]
    public int tilemapYMin = 11; // �� Y�� ���Ͽ� ��ġ�� Ÿ���� ��ȣ(�ı����� ����)

    [Header("����")]
    public Tilemap[] destructibleTilemaps;    // �ı� ����� �Ǵ� Ÿ�ϸʵ�
    public GameObject flyingTilePrefab;       // �ı� ����� ���ư��� ������ (SpriteRenderer + Rigidbody2D �ʼ�)

    [Header("ī�޶� ����")]
    public Vector3 shakeForce;
    public float shakeRate = 0.5f;

    // �̵� ���� ����
    private Vector3 lastPosition;             // ���� ������ ��ġ
    private Vector2 moveDir = Vector2.right;  // ���� �̵� ���� (�ʱⰪ ������)
    private CameraController m_camCon;
    private PlayerController m_pc;

    private float m_moveAmount;

    // ���߿� ������ Ÿ�� ���� �����
    private Dictionary<Tilemap, Dictionary<Vector3Int, TileBase>> destroyedTileMapData = new();

    private List<GameObject> m_destroyedEnemy = new List<GameObject>();

    private void Awake()
    {
        m_camCon = Camera.main.GetComponent<CameraController>();
        m_pc = GetComponent<PlayerController>();

        m_pc.OnStateChanged += (currentState) => { if (currentState == PlayerState.Jump) m_camCon.ShakeCamera(shakeForce.x, shakeForce.y, shakeForce.z); };
        m_pc.OnLand += () => { m_camCon.ShakeCamera(shakeForce.x, shakeForce.y, shakeForce.z); };
    }

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
        if (delta.sqrMagnitude > 0.0001f)
            moveDir = delta.normalized;
        else
            moveDir = Vector2.zero;

        lastPosition = transform.position;

        if (m_pc.GetCurrentState() == PlayerState.Walk)
        {
            m_moveAmount += Time.deltaTime;
        }
        else
        {
            m_moveAmount = 0;
        }

        if (m_moveAmount > shakeRate)
        {
            m_camCon.ShakeCamera(shakeForce.x, shakeForce.y, shakeForce.z);
            m_moveAmount = 0;
        }
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
        int layer = collision.gameObject.layer;

        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            var move = collision.gameObject.GetComponent<MMove>();
            var sr = collision.gameObject.GetComponentInChildren<SpriteRenderer>();

            if (move != null && sr != null)
            {
                SpawnFlyingEnemy(collision.gameObject.transform.position, sr.sprite);
                m_destroyedEnemy.Add(collision.gameObject);
                collision.gameObject.SetActive(false);
            }
        }
    }

    public void RespawnDestoryedEnemy()
    {
        foreach (var go in m_destroyedEnemy)
            go.SetActive(true);

        m_destroyedEnemy.Clear();
    }

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
        rb.AddTorque(Random.Range(-10, 10), ForceMode2D.Impulse); //��� ����ó����

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
