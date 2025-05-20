using UnityEngine;

public class AvatarController : MonoBehaviour
{
    public float moveSpeed = 10f;
    private float leftLimit;
    private float rightLimit;

    public bool isMovable;

    private void Start()
    {
        isMovable = true;
        //SetLimits(-7.4f, 7.4f);
    }
    public void SetLimits(float left, float right)
    {
        moveSpeed = 10f;
        leftLimit = left;
        rightLimit = right;
    }

    void Update()
    {
        if (isMovable)
        {
            Move();            
        }        
    }

    private void Move()
    {
        float move = 0f;

        if (Input.GetKey(KeyCode.A))
            move = -1f;
        else if (Input.GetKey(KeyCode.D))
            move = 1f;

        Vector3 newPos = transform.position + Vector3.right * move * moveSpeed * Time.deltaTime;

        // 화면 경계 제한
        newPos.x = Mathf.Clamp(newPos.x, leftLimit, rightLimit);
        transform.position = newPos;
    }
}