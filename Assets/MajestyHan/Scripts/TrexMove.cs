using UnityEngine;
using System.Collections.Generic;

public class TrexMove : MonoBehaviour
{
    // 티라노의 상태 정의
    public enum MonsterState { Idle, Chase, Kill, Pause, Patrol, Evaluate, PreJump, Jumping }
    public MonsterState state = MonsterState.Idle;

    [Header("AI Settings")]
    public float moveSpeed = 2f;                    // 일반 이동 속도
    public float pauseDuration = 2f;                // 추적 중단 후 멍때리는 시간
    public float yJumpThreshold = 1.0f;             // Y축 차이로 점프 판단 기준
    public float preJumpDuration = 1.0f;            // 점프하기 전 준비 시간
    public float apexHeight = 3f;                   // 점프 곡선의 정점 높이
    public float detectionRange = 4f;               // 순찰 중 플레이어 인식 거리
    public float fallGapThreshold = 1.0f;           // 낙사 위험으로 공중 점프하는 기준
    public float horizontalApproachRange = 3f;      // X축으로 충분히 접근했는지 판단 기준
    public float jumpDuration = 0.5f;               // 선형보간 점프 지속 시간
    public float jumpChance = 0.5f;                 // 순찰 중 벽이나 절벽에서 점프할 확률

    [Header("References")]
    public Transform player;                        // 추적 대상 플레이어
    public List<Transform> patrolJumpTargets;       // 순찰 중 점프할 수 있는 위치들 (Empty 오브젝트 등)

    // 컴포넌트 캐싱
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    // 내부 상태 변수들
    private float preJumpTimer = 0f;
    private float pauseTimer = 0f;
    private float jumpTimer = 0f;
    private Vector3 evaluatedPlayerPosition;        // 점프 목표 위치
    private float colliderActivateThreshold = 0.3f; // 착지와 목표 거리 허용 오차
    private Vector3 initialPosition;                // 트리거로 활성화 시 초기 위치 복귀
    private Vector3 jumpStart;                      // 점프 시작 위치
    private bool isJumpingLerp = false;             // 선형 점프 중 여부
    private MonsterState prevState;                 // 점프 이전 상태
    private bool initializedPatrolDirection = false; // 순찰 방향 설정했는지 여부

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        gameObject.SetActive(false); // 트리거로 활성화할 때까지 비활성화
    }

    void Update()
    {
        UpdateAnimator(); // 상태 기반 애니메이션 갱신

        // 선형보간 중이면 이동 처리만 하고 리턴
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
                ChasePlayer(); // x축으로 플레이어에게 이동

                float xDiff = Mathf.Abs(player.position.x - transform.position.x);
                float yDiff = Mathf.Abs(player.position.y - transform.position.y);
                var playerState = player.GetComponent<PlayerController>().GetCurrentState();
                bool isAirborne = (playerState == PlayerState.Jump || playerState == PlayerState.Fall);

                bool cliffBlocked = IsCliffAhead();           // 앞에 바닥 없음
                bool wallBlocked = IsBlockedHorizontally();   // 앞에 벽
                bool cannotReachPlayer = cliffBlocked || wallBlocked;

                // 벽/절벽에 막혀있거나 Y차이가 클 때 → 점프
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

                    // 처음 순찰 시작 시, 플레이어 반대 방향으로 걷게 함
                    float dirX = Mathf.Sign(transform.position.x - player.position.x);
                    spriteRenderer.flipX = dirX > 0;
                }

                Patrol(); // 기존 방향 유지해서 걷기

                // 벽이나 절벽을 만나면
                if (IsCliffAhead() || IsBlockedHorizontally())
                {
                    // 확률적으로 점프
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
                        // 그냥 방향만 바꿔서 걷게 하기
                        rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
                    }
                }

                // 플레이어가 가까워지면 추적 상태 전환
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

        // x축 속도에 따라 스프라이트 반전 처리
        if (!isJumpingLerp && Mathf.Abs(rb.velocity.x) > 0.01f)
        {
            spriteRenderer.flipX = rb.velocity.x < 0f;
        }
    }

    // 플레이어 발밑을 향해 점프 지점 설정
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

    // 추적 시 이동
    void ChasePlayer()
    {
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
    }

    // 순찰 시 방향 유지
    void Patrol()
    {
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
    }

    // 벽 충돌 검사
    bool IsBlockedHorizontally()
    {
        Vector2 origin = transform.position;
        Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 1f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    // 낭떠러지 검사
    bool IsCliffAhead()
    {
        Vector2 frontPos = (Vector2)transform.position + new Vector2((spriteRenderer.flipX ? -0.5f : 0.5f), 0);
        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        return hit.collider == null;
    }

    // 점프 시작
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

    // 플레이어와 충돌 시 제거
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    // 트리거로 활성화
    public void ActivateChase()
    {
        if (!gameObject.activeSelf)
        {
            transform.position = initialPosition;
            gameObject.SetActive(true);
            state = MonsterState.Chase;
        }
    }

    // 추적 중단 → 멈췄다가 순찰로 전환
    public void DeactivateChase()
    {
        if (state == MonsterState.Chase)
        {
            rb.velocity = Vector2.zero;
            state = MonsterState.Pause;
        }
    }

    // 애니메이션 bool 파라미터 갱신
    void UpdateAnimator()
    {
        bool walking = (state == MonsterState.Chase || state == MonsterState.Patrol);
        bool jumping = (state == MonsterState.Jumping);

        anim.SetBool("isWalking", walking);
        anim.SetBool("isJumping", jumping);
    }
}
