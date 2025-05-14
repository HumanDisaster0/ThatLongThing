using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("�̵��ӵ�")]
    public float moveSpeed = 2f;
    [Header("�� �̵��ð�")]
    public float patrolDuration = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float timer = 0f;
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= patrolDuration)
        {
            direction *= -1;
            timer = 0f;

            spriteRenderer.flipX = (direction < 0);
        }

        rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
    }
}
