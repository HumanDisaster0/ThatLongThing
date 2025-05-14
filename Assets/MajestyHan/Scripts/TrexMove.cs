using UnityEngine;

public class TrexMove : MonoBehaviour
{
    // 티라노의 상태들 정의
    public enum MonsterState { Idle, Chase, Kill, Pause, Patrol, Evaluate, PreJump, Jumping }
    public MonsterState state = MonsterState.Idle; // 시작 상태는 Idle

    [Header("AI Settings")]
    public float moveSpeed = 2f;                  // 이동 속도
    public float pauseDuration = 2f;              // Deactivate 후 멍때리는 시간
    public float yJumpThreshold = 1.0f;           // 플레이어와 Y축 차이 기준값
    public float preJumpDuration = 1.0f;          // 점프하기 전 준비 시간
    public float apexHeight = 3f;                 // 점프 정점 높이
    public float detectionRange = 4f;             // 순찰 중 플레이어 인식 거리
    public float fallGapThreshold = 1.0f;         // 낙사 위험 판단 거리
    public float horizontalApproachRange = 3f;    // X축 거리로 점프 조건 판별 기준

    [Header("References")]
    public Transform player;                      // 추적할 플레이어 오브젝트

    private Rigidbody2D rb;
    private Collider2D col;

    private float preJumpTimer = 0f;              // PreJump 상태일 때 경과 시간
    private float pauseTimer = 0f;                // Pause 상태일 때 경과 시간
    private Vector3 evaluatedPlayerPosition;      // 점프 타겟 위치 (플레이어 위치 or 앞쪽 착지 위치)
    private float colliderActivateThreshold = 0.3f; // 착지 판단 거리 기준
    private Vector3 initialPosition;              // 처음 위치 기억 (재소환 시 복귀)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        initialPosition = transform.position; // 재활성화할 때 돌아올 위치 저장
        gameObject.SetActive(false);          // 처음에는 비활성화
    }

    void Update()
    {
        switch (state)
        {
            case MonsterState.Chase:
                // 플레이어를 향해 이동
                ChasePlayer();

                float xDiff = Mathf.Abs(player.position.x - transform.position.x);
                float yDiff = Mathf.Abs(player.position.y - transform.position.y);
                var playerState = player.GetComponent<PlayerController>().GetCurrentState();
                bool isAirborne = (playerState == PlayerState.Jump || playerState == PlayerState.Fall);

                bool cliffBlocked = IsCliffAhead();
                bool wallBlocked = IsBlockedHorizontally();
                bool cannotReachPlayer = cliffBlocked || wallBlocked;

                // [조건 1] 막혀있고 X축으로 가까움
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
                // [조건 2] 충분히 가까운데 Y축 차이 큼
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
                // 점프 전 대기 시간 누적
                preJumpTimer += Time.deltaTime;

                // 지금 땅에 붙어있는지 체크
                bool isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

                // [조건 3] 낙사 위험 감지 → 플레이어 위치로 긴급 점프
                if (!isGrounded && transform.position.y < evaluatedPlayerPosition.y - fallGapThreshold)
                {
                    evaluatedPlayerPosition = player.position;
                    DoParabolicJump(evaluatedPlayerPosition);
                    break;
                }

                // 준비 시간 끝나면 점프 실행
                if (preJumpTimer >= preJumpDuration)
                {
                    DoParabolicJump(evaluatedPlayerPosition);
                }
                break;

            case MonsterState.Jumping:
                CheckLandingProximity(); // 착지 지점 근처에 오면 콜라이더 복구
                break;

            case MonsterState.Patrol:
                Patrol(); // 사인 곡선으로 움직임 (단순 움직임)

                // 착지 가능한 지점 탐색
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
                    // 착지할 곳 없으면 방향만 반전
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }

                // 플레이어 가까워지면 추적 상태로 전환
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
        // 플레이어 쪽으로 이동
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    void Patrol()
    {
        // 사인 곡선으로 흔들흔들 움직이기
        rb.velocity = new Vector2(Mathf.Sin(Time.time) * moveSpeed, rb.velocity.y);
    }

    bool IsBlockedHorizontally()
    {
        // 정면 벽 판정 (1m 거리 안)
        Vector2 origin = transform.position;
        Vector2 dir = transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    bool IsCliffAhead()
    {
        // 정면 앞에 바닥이 있는지 확인 (낭떠러지 여부)
        Vector2 frontPos = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, 0);
        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        return hit.collider == null;
    }

    void DoParabolicJump(Vector2 targetPosition)
    {
        // 목표 지점을 향한 포물선 점프 계산
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
        col.enabled = false; // 점프 중 충돌 방지
        state = MonsterState.Jumping;
    }

    void CheckLandingProximity()
    {
        // 점프 도중 착지 위치 근처에 오면 콜라이더 복구
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
            // TODO: 플레이어 사망 처리
            gameObject.SetActive(false);
            return;
        }

        if (state == MonsterState.Jumping && collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 착지 완료
            col.enabled = true;
            rb.velocity = new Vector2(0, rb.velocity.y);
            state = MonsterState.Chase;
        }
    }

    public void ActivateChase()
    {
        if (!gameObject.activeSelf)
        {
            // 처음 위치로 되돌리고 추적 시작
            transform.position = initialPosition;
            gameObject.SetActive(true);
            state = MonsterState.Chase;
        }
    }

    public void DeactivateChase()
    {
        if (state == MonsterState.Chase)
        {
            // 일시 멈춤 상태로 전환
            state = MonsterState.Pause;
            rb.velocity = Vector2.zero;
        }
    }
}