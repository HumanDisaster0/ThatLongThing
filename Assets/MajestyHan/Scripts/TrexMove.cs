using UnityEngine;

public class TrexMove : MonoBehaviour
{
    // Ƽ����� ���µ� ����
    public enum MonsterState { Idle, Chase, Kill, Pause, Patrol, Evaluate, PreJump, Jumping }
    public MonsterState state = MonsterState.Idle; // ���� ���´� Idle

    [Header("AI Settings")]
    public float moveSpeed = 2f;                  // �̵� �ӵ�
    public float pauseDuration = 2f;              // Deactivate �� �۶����� �ð�
    public float yJumpThreshold = 1.0f;           // �÷��̾�� Y�� ���� ���ذ�
    public float preJumpDuration = 1.0f;          // �����ϱ� �� �غ� �ð�
    public float apexHeight = 3f;                 // ���� ���� ����
    public float detectionRange = 4f;             // ���� �� �÷��̾� �ν� �Ÿ�
    public float fallGapThreshold = 1.0f;         // ���� ���� �Ǵ� �Ÿ�
    public float horizontalApproachRange = 3f;    // X�� �Ÿ��� ���� ���� �Ǻ� ����

    [Header("References")]
    public Transform player;                      // ������ �÷��̾� ������Ʈ

    private Rigidbody2D rb;
    private Collider2D col;

    private float preJumpTimer = 0f;              // PreJump ������ �� ��� �ð�
    private float pauseTimer = 0f;                // Pause ������ �� ��� �ð�
    private Vector3 evaluatedPlayerPosition;      // ���� Ÿ�� ��ġ (�÷��̾� ��ġ or ���� ���� ��ġ)
    private float colliderActivateThreshold = 0.3f; // ���� �Ǵ� �Ÿ� ����
    private Vector3 initialPosition;              // ó�� ��ġ ��� (���ȯ �� ����)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        initialPosition = transform.position; // ��Ȱ��ȭ�� �� ���ƿ� ��ġ ����
        gameObject.SetActive(false);          // ó������ ��Ȱ��ȭ
    }

    void Update()
    {
        switch (state)
        {
            case MonsterState.Chase:
                // �÷��̾ ���� �̵�
                ChasePlayer();

                float xDiff = Mathf.Abs(player.position.x - transform.position.x);
                float yDiff = Mathf.Abs(player.position.y - transform.position.y);
                var playerState = player.GetComponent<PlayerController>().GetCurrentState();
                bool isAirborne = (playerState == PlayerState.Jump || playerState == PlayerState.Fall);

                bool cliffBlocked = IsCliffAhead();
                bool wallBlocked = IsBlockedHorizontally();
                bool cannotReachPlayer = cliffBlocked || wallBlocked;

                // [���� 1] �����ְ� X������ �����
                if (cannotReachPlayer && xDiff <= horizontalApproachRange && !isAirborne)
                {
                    RaycastHit2D hit = Physics2D.Raycast(player.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
                    if (hit.collider != null)
                    {
                        evaluatedPlayerPosition = hit.point + Vector2.up * 0.1f;
                        rb.velocity = Vector2.zero;
                        state = MonsterState.PreJump;
                        preJumpTimer = 0f;
                    }
                }
                // [���� 2] ����� ���� Y�� ���� ŭ
                else if (xDiff <= horizontalApproachRange && yDiff >= yJumpThreshold && !isAirborne)
                {
                    RaycastHit2D hit = Physics2D.Raycast(player.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
                    if (hit.collider != null)
                    {
                        evaluatedPlayerPosition = hit.point + Vector2.up * 0.1f;
                        rb.velocity = Vector2.zero;
                        state = MonsterState.PreJump;
                        preJumpTimer = 0f;
                    }
                }
                break;

            case MonsterState.PreJump:
                // ���� �� ��� �ð� ����
                preJumpTimer += Time.deltaTime;

                // ���� ���� �پ��ִ��� üũ
                bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

                // [���� 3] ���� ���� ���� �� �÷��̾� ��ġ�� ��� ����
                if (!isGrounded && transform.position.y < evaluatedPlayerPosition.y - fallGapThreshold)
                {
                    evaluatedPlayerPosition = player.position;
                    DoParabolicJump(evaluatedPlayerPosition);
                    break;
                }

                // �غ� �ð� ������ ���� ����
                if (preJumpTimer >= preJumpDuration)
                {
                    DoParabolicJump(evaluatedPlayerPosition);
                }
                break;

            case MonsterState.Jumping:
                CheckLandingProximity(); // ���� ���� ��ó�� ���� �ݶ��̴� ����
                break;

            case MonsterState.Patrol:
                Patrol(); // ���� ����� ������ (�ܼ� ������)

                // ���� ������ ���� Ž��
                bool foundLanding = false;
                Vector2 jumpTarget = transform.position;

                for (int i = 3; i <= 6; i++)
                {
                    Vector2 checkPos = transform.position + new Vector3(transform.localScale.x * i, 0);
                    RaycastHit2D groundCheck = Physics2D.Raycast(checkPos, Vector2.down, 3f, LayerMask.GetMask("Ground"));
                    if (groundCheck.collider != null)
                    {
                        jumpTarget = groundCheck.point + Vector2.up * 0.1f;
                        foundLanding = true;
                        break;
                    }
                }

                if (foundLanding)
                {
                    evaluatedPlayerPosition = jumpTarget;
                    rb.velocity = Vector2.zero;
                    state = MonsterState.PreJump;
                    preJumpTimer = 0f;
                }
                else if (IsCliffAhead() || IsBlockedHorizontally())
                {
                    // ������ �� ������ ���⸸ ����
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }

                // �÷��̾� ��������� ���� ���·� ��ȯ
                if (Vector2.Distance(transform.position, player.position) <= detectionRange)
                {
                    state = MonsterState.Chase;
                }
                break;

            case MonsterState.Pause:
                pauseTimer += Time.deltaTime;
                if (pauseTimer >= pauseDuration)
                {
                    pauseTimer = 0f;
                    state = MonsterState.Patrol;
                }
                break;
        }
    }

    void ChasePlayer()
    {
        // �÷��̾� ������ �̵�
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    void Patrol()
    {
        // ���� ����� ������ �����̱�
        rb.velocity = new Vector2(Mathf.Sin(Time.time) * moveSpeed, rb.velocity.y);
    }

    bool IsBlockedHorizontally()
    {
        // ���� �� ���� (1m �Ÿ� ��)
        Vector2 origin = transform.position;
        Vector2 dir = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    bool IsCliffAhead()
    {
        // ���� �տ� �ٴ��� �ִ��� Ȯ�� (�������� ����)
        Vector2 frontPos = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, 0);
        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        return hit.collider == null;
    }

    void DoParabolicJump(Vector2 targetPosition)
    {
        // ��ǥ ������ ���� ������ ���� ���
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float deltaY = targetPosition.y - transform.position.y;
        float deltaX = targetPosition.x - transform.position.x;

        float adjustedApexHeight = Mathf.Max(apexHeight, deltaY + 1f);
        float velocityY = Mathf.Sqrt(2 * gravity * adjustedApexHeight);
        float timeUp = velocityY / gravity;
        float timeDown = Mathf.Sqrt(2 * (adjustedApexHeight - deltaY) / gravity);
        float totalTime = timeUp + timeDown;
        float velocityX = deltaX / totalTime;

        rb.velocity = new Vector2(velocityX, velocityY);
        col.enabled = false; // ���� �� �浹 ����
        state = MonsterState.Jumping;
    }

    void CheckLandingProximity()
    {
        // ���� ���� ���� ��ġ ��ó�� ���� �ݶ��̴� ����
        float distanceToTarget = Vector2.Distance(transform.position, evaluatedPlayerPosition);

        if (rb.velocity.y < 0 && distanceToTarget <= colliderActivateThreshold)
        {
            col.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            // TODO: �÷��̾� ��� ó��
            gameObject.SetActive(false);
            return;
        }

        if (state == MonsterState.Jumping && collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // ���� �Ϸ�
            col.enabled = true;
            rb.velocity = new Vector2(0, rb.velocity.y);
            state = MonsterState.Chase;
        }
    }

    public void ActivateChase()
    {
        if (!gameObject.activeSelf)
        {
            // ó�� ��ġ�� �ǵ����� ���� ����
            transform.position = initialPosition;
            gameObject.SetActive(true);
            state = MonsterState.Chase;
        }
    }

    public void DeactivateChase()
    {
        if (state == MonsterState.Chase)
        {
            // �Ͻ� ���� ���·� ��ȯ
            state = MonsterState.Pause;
            rb.velocity = Vector2.zero;
        }
    }
}