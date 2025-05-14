using UnityEngine;

public class TrexMove : MonoBehaviour
{
    public enum MonsterState { Idle, Chase, Kill, Pause, Patrol, Evaluate, PreJump, Jumping }
    public MonsterState state = MonsterState.Idle;

    [Header("AI Settings")]
    public float moveSpeed = 2f;
    public float pauseDuration = 2f;
    public float yJumpThreshold = 1.0f;
    public float preJumpDuration = 1.0f;
    public float apexHeight = 3f;
    public float detectionRange = 4f;
    public float fallGapThreshold = 1.0f;
    public float horizontalApproachRange = 3f;
    public float jumpDuration = 0.5f; // 선형 점프 시간

    [Header("References")]
    public Transform player;

    private Rigidbody2D rb;
    private Collider2D col;

    private float preJumpTimer = 0f;
    private float pauseTimer = 0f;
    private float jumpTimer = 0f;
    private Vector3 evaluatedPlayerPosition;
    private float colliderActivateThreshold = 0.3f;
    private Vector3 initialPosition;
    private Vector3 jumpStart;
    private bool isJumpingLerp = false;
    private MonsterState prevState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        initialPosition = transform.position;
        gameObject.SetActive(false);
    }

    void Update()
    {
        //Debug.Log(state);

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
                ChasePlayer(); // X방향으로만 벡터 이동

                float xDiff = Mathf.Abs(player.position.x - transform.position.x); //X차이 절대값
                float yDiff = Mathf.Abs(player.position.y - transform.position.y); //Y차이 절대값
                var playerState = player.GetComponent<PlayerController>().GetCurrentState(); //v플레이어 상태 받아옴
                bool isAirborne = (playerState == PlayerState.Jump || playerState == PlayerState.Fall); //플레이어는 점프중인가?

                bool cliffBlocked = IsCliffAhead(); //앞에 절벽인가?
                bool wallBlocked = IsBlockedHorizontally(); //앞에 벽인가?
                bool cannotReachPlayer = cliffBlocked || wallBlocked; //벽이든, 절벽인가?

                if (cannotReachPlayer) //티라노가 도달 못할때
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
                Patrol();

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
                    prevState = MonsterState.Patrol;
                    state = MonsterState.PreJump;
                    preJumpTimer = 0f;
                }
                else if (IsCliffAhead() || IsBlockedHorizontally())
                {
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }

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


    void SetJumpTargetToPlayerFeet() // 점프
    {
        var playerCol = player.GetComponent<Collider2D>();
        Vector2 groundTarget = player.position - new Vector3(0, playerCol.bounds.extents.y, 0);
        RaycastHit2D hit = Physics2D.Raycast(groundTarget, Vector2.down, 0.2f, LayerMask.GetMask("Ground"));
        evaluatedPlayerPosition = hit.collider != null ? hit.point + Vector2.up * 0.1f : groundTarget;
        rb.velocity = Vector2.zero;
        state = MonsterState.PreJump;
        preJumpTimer = 0f;
    }



    void ChasePlayer()
    {
        Debug.Log("작동중");
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    void Patrol()
    {
        rb.velocity = new Vector2(Mathf.Sin(Time.time) * moveSpeed, rb.velocity.y);
    }

    bool IsBlockedHorizontally()
    {
        Vector2 origin = transform.position;
        Vector2 dir = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    bool IsCliffAhead()
    {
        Vector2 frontPos = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, 0);
        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        return hit.collider == null;
    }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    public void ActivateChase()
    {
        if (!gameObject.activeSelf)
        {
            transform.position = initialPosition;
            gameObject.SetActive(true);
            state = MonsterState.Chase;
        }
    }

    public void DeactivateChase()
    {
        if (state == MonsterState.Chase)
        {
            state = MonsterState.Pause;
            rb.velocity = Vector2.zero;
        }
    }
}