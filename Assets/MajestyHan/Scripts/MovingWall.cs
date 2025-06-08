using UnityEngine;

public class MovingWall : MonoBehaviour
{
    // 투명벽 등록
    [Header("투명드래곤")] // 투명벽임
    public GameObject invisibleWall;

    [Header("플레이어")]
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

        if (tr == null || invisibleWall == null || col == null || wallCollider == null) // 감지 실패시 얼리리턴
        {
            Debug.Log("감지실패");
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
        if (invisibleWall.activeSelf) // 이미 꺼져 있으면 아무 것도 안 함
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