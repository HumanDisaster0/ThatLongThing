using UnityEngine;
using System.Collections.Generic;

public class TrexMove : MonoBehaviour
{
    // Ƽ����� ���� ����
    public enum MonsterState { Idle, Chase, Kill, Pause, Patrol, Evaluate, PreJump, Jumping }
    public MonsterState state = MonsterState.Idle;

    [Header("AI Settings")]
    public float moveSpeed = 2f;                    // �Ϲ� �̵� �ӵ�
    public float pauseDuration = 2f;                // ���� �ߴ� �� �۶����� �ð�
    public float yJumpThreshold = 1.0f;             // Y�� ���̷� ���� �Ǵ� ����
    public float preJumpDuration = 1.0f;            // �����ϱ� �� �غ� �ð�
    public float apexHeight = 3f;                   // ���� ��� ���� ����
    public float detectionRange = 4f;               // ���� �� �÷��̾� �ν� �Ÿ�
    public float fallGapThreshold = 1.0f;           // ���� �������� ���� �����ϴ� ����
    public float horizontalApproachRange = 3f;      // X������ ����� �����ߴ��� �Ǵ� ����
    public float jumpDuration = 0.5f;               // �������� ���� ���� �ð�
    public float jumpChance = 0.5f;                 // ���� �� ���̳� �������� ������ Ȯ��

    [Header("References")]
    public Transform player;                        // ���� ��� �÷��̾�
    public List<Transform> patrolJumpTargets;       // ���� �� ������ �� �ִ� ��ġ�� (Empty ������Ʈ ��)

    // ������Ʈ ĳ��
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    // ���� ���� ������
    private float preJumpTimer = 0f;
    private float pauseTimer = 0f;
    private float jumpTimer = 0f;
    private Vector3 evaluatedPlayerPosition;        // ���� ��ǥ ��ġ
    private float colliderActivateThreshold = 0.3f; // ������ ��ǥ �Ÿ� ��� ����
    private Vector3 initialPosition;                // Ʈ���ŷ� Ȱ��ȭ �� �ʱ� ��ġ ����
    private Vector3 jumpStart;                      // ���� ���� ��ġ
    private bool isJumpingLerp = false;             // ���� ���� �� ����
    private MonsterState prevState;                 // ���� ���� ����
    private bool initializedPatrolDirection = false; // ���� ���� �����ߴ��� ����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        gameObject.SetActive(false); // Ʈ���ŷ� Ȱ��ȭ�� ������ ��Ȱ��ȭ
    }

    void Update()
    {
        UpdateAnimator(); // ���� ��� �ִϸ��̼� ����

        // �������� ���̸� �̵� ó���� �ϰ� ����
        if (isJumpingLerp)
        {
            jumpTimer += Time.deltaTime;
            float t = Mathf.Clamp01(jumpTimer / jumpDuration);
            float height = Mathf.Sin(t * Mathf.PI) * apexHeight;
            Vector3 flat = Vector3.Lerp(jumpStart, evaluatedPlayerPosition, t);
            transform.position = flat + Vector3.up * height;

            if (t >= 1f)
            {
                isJumpingLerp = false;
                col.enabled = true;
                state = (prevState == MonsterState.Patrol) ? MonsterState.Patrol : MonsterState.Chase;
            }
            return;
        }

        switch (state)
        {
            case MonsterState.Chase:
                ChasePlayer(); // x������ �÷��̾�� �̵�

                float xDiff = Mathf.Abs(player.position.x - transform.position.x);
                float yDiff = Mathf.Abs(player.position.y - transform.position.y);
                var playerState = player.GetComponent<PlayerController>().GetCurrentState();
                bool isAirborne = (playerState == PlayerState.Jump || playerState == PlayerState.Fall);

                bool cliffBlocked = IsCliffAhead();           // �տ� �ٴ� ����
                bool wallBlocked = IsBlockedHorizontally();   // �տ� ��
                bool cannotReachPlayer = cliffBlocked || wallBlocked;

                // ��/������ �����ְų� Y���̰� Ŭ �� �� ����
                if (cannotReachPlayer)
                {
                    SetJumpTargetToPlayerFeet();
                }
                else if (xDiff <= horizontalApproachRange && yDiff >= yJumpThreshold && !isAirborne)
                {
                    SetJumpTargetToPlayerFeet();
                }
                break;

            case MonsterState.PreJump:
                preJumpTimer += Time.deltaTime;

                if (preJumpTimer >= preJumpDuration)
                {
                    StartLerpJump(evaluatedPlayerPosition);
                }
                break;

            case MonsterState.Patrol:
                if (!initializedPatrolDirection)
                {
                    initializedPatrolDirection = true;

                    // ó�� ���� ���� ��, �÷��̾� �ݴ� �������� �Ȱ� ��
                    float dirX = Mathf.Sign(transform.position.x - player.position.x);
                    spriteRenderer.flipX = dirX > 0;
                }

                Patrol(); // ���� ���� �����ؼ� �ȱ�

                // ���̳� ������ ������
                if (IsCliffAhead() || IsBlockedHorizontally())
                {
                    // Ȯ�������� ����
                    if (Random.value < jumpChance && patrolJumpTargets.Count > 0)
                    {
                        Transform closest = patrolJumpTargets[0];
                        float minDist = Vector2.Distance(transform.position, closest.position);

                        foreach (var t in patrolJumpTargets)
                        {
                            float dist = Vector2.Distance(transform.position, t.position);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                closest = t;
                            }
                        }

                        evaluatedPlayerPosition = closest.position;
                        rb.velocity = Vector2.zero;
                        prevState = MonsterState.Patrol;
                        state = MonsterState.PreJump;
                        preJumpTimer = 0f;
                    }
                    else
                    {
                        // �׳� ���⸸ �ٲ㼭 �Ȱ� �ϱ�
                        rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
                    }
                }

                // �÷��̾ ��������� ���� ���� ��ȯ
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
                    initializedPatrolDirection = false;
                    state = MonsterState.Patrol;
                }
                break;
        }

        // x�� �ӵ��� ���� ��������Ʈ ���� ó��
        if (!isJumpingLerp && Mathf.Abs(rb.velocity.x) > 0.01f)
        {
            spriteRenderer.flipX = rb.velocity.x < 0f;
        }
    }

    // �÷��̾� �߹��� ���� ���� ���� ����
    void SetJumpTargetToPlayerFeet()
    {
        var playerCol = player.GetComponent<Collider2D>();
        Vector2 groundTarget = player.position - new Vector3(0, playerCol.bounds.extents.y, 0);
        RaycastHit2D hit = Physics2D.Raycast(groundTarget, Vector2.down, 0.2f, LayerMask.GetMask("Ground"));
        evaluatedPlayerPosition = hit.collider != null ? hit.point + Vector2.up * 0.1f : groundTarget;
        rb.velocity = Vector2.zero;
        state = MonsterState.PreJump;
        preJumpTimer = 0f;
    }

    // ���� �� �̵�
    void ChasePlayer()
    {
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
    }

    // ���� �� ���� ����
    void Patrol()
    {
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
    }

    // �� �浹 �˻�
    bool IsBlockedHorizontally()
    {
        Vector2 origin = transform.position;
        Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    // �������� �˻�
    bool IsCliffAhead()
    {
        Vector2 frontPos = (Vector2)transform.position + new Vector2((spriteRenderer.flipX ? -0.5f : 0.5f), 0);
        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        return hit.collider == null;
    }

    // ���� ����
    void StartLerpJump(Vector3 target)
    {
        jumpStart = transform.position;
        evaluatedPlayerPosition = target;
        jumpTimer = 0f;
        isJumpingLerp = true;
        col.enabled = false;
        prevState = state;
        state = MonsterState.Jumping;
    }

    // �÷��̾�� �浹 �� ����
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    // Ʈ���ŷ� Ȱ��ȭ
    public void ActivateChase()
    {
        if (!gameObject.activeSelf)
        {
            transform.position = initialPosition;
            gameObject.SetActive(true);
            state = MonsterState.Chase;
        }
    }

    // ���� �ߴ� �� ����ٰ� ������ ��ȯ
    public void DeactivateChase()
    {
        if (state == MonsterState.Chase)
        {
            rb.velocity = Vector2.zero;
            state = MonsterState.Pause;
        }
    }

    // �ִϸ��̼� bool �Ķ���� ����
    void UpdateAnimator()
    {
        bool walking = (state == MonsterState.Chase || state == MonsterState.Patrol);
        bool jumping = (state == MonsterState.Jumping);

        anim.SetBool("isWalking", walking);
        anim.SetBool("isJumping", jumping);
    }
}
