using UnityEngine;

public class TrexMove : MonoBehaviour
{
    public enum MonsterState { Idle, Patrol, Chase, ReadyToJump, Jumping, Pause }
    public MonsterState state = MonsterState.Idle;
    private MonsterState prevState;


    [Header("속도 / 점프높이")]
    public float moveSpeed = 2f;            // X축 이동 속도
    public float chaseSpeed = 3f;           // 추적 속도
    public float apexHeight = 3f;           // 점프 정점 높이

    [Header("지속시간")]
    public float pauseDuration = 1.0f;      // 멈칫 상태 유지 시간
    public float jumpDuration = 2f;         // 점프 이동 총 시간
    public float readyDuration = 0.5f;      // 점프 준비 시간 (ReadyToJump 상태)

    [Header("정찰 중 점프 주기")]
    public float patrolJumpInterval = 10f;
    private float patrolJumpTimer = 0f;

    [Header("순찰 전환 최소/최대 시간")]
    public float minThinkTime = 5f;
    public float maxThinkTime = 10f;
    public float readyToChaseDuration = 2f;

    private float thinkDuration;            // 정찰 상태의 방향 재결정 주기

    private Vector3 jumpStart;              // 점프 시작 위치
    private Vector3? targetPosition = null; // 추적/점프 목표 위치

    private bool isJumping = false;         // 현재 점프 중인지 여부

    // 컴포넌트 참조
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D col;

    // 내부 타이머
    private float pauseTimer = 0f;
    private float thinkTimer = 0f;
    private float jumpTimer = 0f;
    private float readyTimer = 0f;
    private float chaseTimer = 0f;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        UpdateAnimator(); // 현재 속도 및 상태 기반 애니메이션 업데이트

        switch (state)
        {
            case MonsterState.Idle:
                rb.velocity = Vector2.zero;
                break;

            case MonsterState.Patrol:
                DoPatrol(); // 일정 시간마다 방향 변경 + 낙사 감지

                patrolJumpTimer += Time.deltaTime;
                if (patrolJumpTimer >= patrolJumpInterval)
                {
                    patrolJumpTimer = 0f;
                    TryPatrolJump(); // 랜덤 점프 시도
                }
                break;

            case MonsterState.Chase:
                chaseTimer += Time.deltaTime;
                if (chaseTimer >= readyToChaseDuration)
                {
                    if (targetPosition.HasValue)
                        MoveTowardsTarget(); // 플레이어를 향해 X축으로 이동
                    else
                        ChangeState(MonsterState.Patrol); // 추적 대상 없으면 순찰 복귀
                }
                break;

            case MonsterState.Pause:
                pauseTimer += Time.deltaTime;
                if (pauseTimer >= pauseDuration)
                {
                    pauseTimer = 0f;
                    ChangeState(MonsterState.Patrol);
                }
                break;

            case MonsterState.ReadyToJump:
                readyTimer += Time.deltaTime;
                if (readyTimer > readyDuration)
                {
                    readyTimer = 0;
                    StartJump();
                }
                break;

            case MonsterState.Jumping:
                DoJump(); // 선형 보간 + 포물선으로 위치 이동
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("JumpNode"))
        {
            JumpNode node = other.GetComponent<JumpNode>();
            if (node != null && node.isHorizontalJump)
            {
                float vx = rb.velocity.x;
                if (Mathf.Abs(vx) < 0.05f) return; // 멈춘 상태에서는 무시

                bool fromLeft = vx > 0;
                bool fromRight = vx < 0;

                bool reject = false;

                switch (node.allowedApproach)
                {
                    case JumpNode.JumpDirection.LeftOnly:
                        reject = fromRight;
                        break;
                    case JumpNode.JumpDirection.RightOnly:
                        reject = fromLeft;
                        break;
                    case JumpNode.JumpDirection.Any:
                        reject = false;
                        break;
                }

                if (!reject)
                {
                    SetTargetPosition(node.GetConnectedPosition());
                    ChangeState(MonsterState.ReadyToJump);
                    Debug.Log($"[TrexMove] 수평 노드 접근 감지 → {node.name} → {node.connectedNode.name}");
                }
            }

        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌!");
            // 여기에 죽이거나 리액션 넣기 !@#$!@#$!@#$
        }
    }

    // 플레이어 위치를 향해 X축 방향으로만 이동
    void MoveTowardsTarget()
    {
        if (!targetPosition.HasValue) return;

        float dirX = Mathf.Sign(targetPosition.Value.x - transform.position.x);
        rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
    }

    public void ResetChaseTimer()
    {
        chaseTimer = 0f;
    }

    // 순찰 중: 일정 주기마다 방향 전환 + 낙사 방지
    void DoPatrol()
    {
        thinkTimer += Time.deltaTime;

        if (thinkTimer >= thinkDuration)
        {
            thinkTimer = 0f;
            thinkDuration = Random.Range(minThinkTime, maxThinkTime);

            int dir = DoThink(); // -1, 0, 1 중 하나 결정
            if (dir != 0)
                rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        }

        // 앞에 낭떠러지가 있으면 방향 반전
        if (IsCliffAhead())
            rb.velocity = new Vector2(-1 * rb.velocity.x, rb.velocity.y);
    }

    // 정찰 방향 결정: -1, 0, 1 중 랜덤
    int DoThink()
    {
        int num = Random.Range(0, 3) - 1;
        if (num == 0)
        {
            ChangeState(MonsterState.Pause); // 멈칫 상태 전환
            return 0;
        }
        else
            return num;
    }

    // 점프 중: 포물선 이동 처리
    void DoJump()
    {
        if (!isJumping) return;

        jumpTimer += Time.deltaTime;

        spriteRenderer.flipX = (targetPosition.Value.x - jumpStart.x > 0);
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        // 도착 지점 역시 발 기준 보정
        Vector3 targetFootPos = targetPosition.Value;
        Vector3 flat = Vector3.Lerp(jumpStart, targetFootPos, t);
        float height = Mathf.Sin(t * Mathf.PI) * apexHeight;

        // 발 기준 위치 + 높이 => 중심 좌표
        transform.position = flat + Vector3.up * col.bounds.extents.y + Vector3.up * height;

        if (t >= 1f)
        {
            isJumping = false;
            col.isTrigger = false;
            ChangeState(prevState);
        }
    }


    // 점프 시작 시 초기 설정
    void StartJump()
    {
        jumpTimer = 0;
        float footOffset = col.bounds.extents.y;
        jumpStart = transform.position - Vector3.up * footOffset;
        isJumping = true;
        col.isTrigger = true; // 충돌 꺼줌 (벽 등에 걸리지 않도록)
        ChangeState(MonsterState.Jumping);
    }

    // 외부에서 목표 위치 설정
    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void ClearTargetPosition()
    {
        targetPosition = null;
    }

    // 상태 전이 처리
    public void ChangeState(MonsterState newState)
    {
        if (state == newState) return;

        if (newState == MonsterState.Pause)
        {
            pauseTimer = 0f;
            rb.velocity = Vector2.zero;
        }
        else if (newState == MonsterState.Patrol)
        {
            DoThink(); // 순찰 상태 들어갈 때 초기 방향 결정
        }
        else if (newState == MonsterState.ReadyToJump)
        {
            rb.velocity = Vector2.zero;
            float dirX = targetPosition.Value.x - transform.position.x;
            prevState = state; // 이전 상태 저장
            readyTimer = 0f;
        }


        state = newState;
    }

    // 낭떠러지 감지 (콜라이더 기준, 정면 아래 방향)
    bool IsCliffAhead()
    {
        Bounds bounds = col.bounds;

        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        float offsetX = spriteRenderer.flipX ? bounds.extents.x : -bounds.extents.x;
        Vector2 frontPos = origin + new Vector2(offsetX, 0f);

        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 0.8f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(frontPos, Vector2.down * 0.8f, Color.red);

        return (hit.collider == null);
    }

    // 애니메이션 bool 파라미터 갱신
    void UpdateAnimator()
    {
        bool walking = Mathf.Abs(rb.velocity.x) > 0.01f;
        bool jumping = (state == MonsterState.Jumping);

        if (walking)
            spriteRenderer.flipX = (rb.velocity.x > 0);

        anim.SetBool("isWalking", walking);
        anim.SetBool("isJumping", jumping);
    }
    void TryPatrolJump()
    {
        // 사용할 점프 노드는 TrexThink 쪽 allJumpNodes를 통해 받아야 하므로
        TrexThink thinker = GetComponent<TrexThink>();
        if (thinker == null || thinker.allJumpNodes == null || thinker.allJumpNodes.Count == 0) return;

        // 수직 점프 노드 중 연결된 노드가 있는 것만 필터링
        var candidates = thinker.allJumpNodes.FindAll(n => n.connectedNode != null);
        if (candidates.Count == 0) return;

        // 무작위 노드 선택
        int rand = Random.Range(0, candidates.Count);
        JumpNode selected = candidates[rand];

        // 점프 시작
        SetTargetPosition(selected.connectedNode.transform.position);
        ChangeState(MonsterState.ReadyToJump);

        Debug.Log($"[TrexMove] 정찰 점프 시도 → {selected.name} → {selected.connectedNode.name}");
    }


    public void ClearTimer()
    {
        pauseTimer = 0f;
        thinkTimer = 0f;
        jumpTimer = 0f;
        readyTimer = 0f;
        chaseTimer = 0f;
    }

    public void JumpNow(Vector3 target)
    {
        prevState = MonsterState.Patrol;  // 이후 돌아올 상태
        targetPosition = target;

        // 점프 직전 세팅
        jumpTimer = 0;
        float footOffset = col.bounds.extents.y;
        jumpStart = transform.position - Vector3.up * footOffset;
        isJumping = true;
        col.isTrigger = true;

        ChangeState(MonsterState.Jumping);  // 즉시 점프 상태 전환
    }


}
