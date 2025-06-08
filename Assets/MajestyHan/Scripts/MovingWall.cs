using UnityEngine;

public class MovingWall : MonoBehaviour
{
    // ���� ���
    [Header("����巡��")] // ������
    public GameObject invisibleWall;

    [Header("�÷��̾�")]
    public PlayerController player;

    //==========================================================
    private Transform tr;
    private BoxCollider2D col;
    private BoxCollider2D wallCollider;

    private bool saftyFlag = false;

    private void Start()
    {
        tr = player.transform;
        col = player.GetComponent<BoxCollider2D>();
        wallCollider = invisibleWall.GetComponent<BoxCollider2D>();

        if (tr == null || invisibleWall == null || col == null || wallCollider == null) // ���� ���н� �󸮸���
        {
            Debug.Log("��������");
            return;
        }
        saftyFlag = true;
    }

    public void MoveInvisibleWall()
    {
        if (saftyFlag)
        {
            float playerLeft = col.size.x * 0.5f * tr.lossyScale.x;
            float wallRight = wallCollider.size.x * 0.5f * wallCollider.transform.lossyScale.x;

            invisibleWall.transform.position = new Vector2(tr.position.x - playerLeft - wallRight, tr.position.y);
        }
    }
    
    public void WallActiveFalse()
    {
        if (invisibleWall.activeSelf) // �̹� ���� ������ �ƹ� �͵� �� ��
        {
            invisibleWall.SetActive(false);
        }
    }

    public void WallActiveTrue()
    {
        if (!invisibleWall.activeSelf)
        {
            invisibleWall.SetActive(true);
        }
    }
}