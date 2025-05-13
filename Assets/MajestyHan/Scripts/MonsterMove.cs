using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDuration = 2f; // �պ� �ֱ� ���� (ex: 2�� ���� �� ����)

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

        // ���� �ð� ������ ���� ����
        if (timer >= patrolDuration)
        {
            direction *= -1;
            timer = 0f;

            // ��������Ʈ ����
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction);
            transform.localScale = scale;
        }

        rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
    }
}
