using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDuration = 2f; // 왕복 주기 절반 (ex: 2초 동안 한 방향)

    private Rigidbody2D rb;
    private float timer = 0f;
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // 일정 시간 지나면 방향 반전
        if (timer >= patrolDuration)
        {
            direction *= -1;
            timer = 0f;

            // 스프라이트 반전
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction);
            transform.localScale = scale;
        }

        rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
    }
}
