using UnityEngine;

public class TrexMove : MonoBehaviour
{
    public enum MonsterState { Idle, Patrol, Chase, ReadyToJump, Jumping, Pause }
    public MonsterState state = MonsterState.Idle;

    private MonsterState prevState;

    [Header("속도 / 점프높이")]
    public float moveSpeed = 2f;            // 이동속도
    public float apexHeight = 3f;           // 정점 높이
    [Header("지속시간")]
    public float pauseDuration = 1.0f;      // 멈칫 시간
    public float jumpDuration = 2f;         // 점프시간
    public float readyDuration = 0.5f;
    
    public float minThinkTime = 5f;
    public float maxThinkTime = 10f;

    private float thinkDuration;

    private Vector3 jumpStart;
    private Vector3? targetPosition = null; //티라노가 목표지점

    bool isJumping = false;                 // 현재 점프 중 여부


    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D col;
    private float pauseTimer = 0f;          // 누적할 변수들
    private float thinkTimer = 0f;
    public float jumpTimer = 0f;
    private float readyTimer = 0f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        UpdateAnimator();

        switch (state)
        {
            case MonsterState.Idle:
                rb.velocity = Vector2.zero;
                break;

            case MonsterState.Patrol:
                DoPatrol(); // 일정 시간 후, 방향을 변경함
                break;

            case MonsterState.Chase:
                if (targetPosition.HasValue)
                    MoveTowardsTarget();
                else
                    ChangeState(MonsterState.Patrol);
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
                DoJump();
                break;
        }
    }

    void MoveTowardsTarget()
    {
        if (!targetPosition.HasValue) return;

        Vector2 dir = (targetPosition.Value - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y); //X방향으로만 추가이동
    }

    void DoPatrol()
    {
        thinkTimer += Time.deltaTime;

        if (thinkTimer >= thinkDuration)
        {
            thinkTimer = 0f;
            thinkDuration = Random.Range(minThinkTime, maxThinkTime); // 다음 턴용 시간 설정

            int dir = DoThink(); // -1, 0, 1 중 하나

            if (dir != 0)
            {
                rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
            }
        }

        if (!IsCliffAhead())        
            rb.velocity = new Vector2(-1 * rb.velocity.x, rb.velocity.y);        
    }

    int DoThink()
    {
        int num = Random.Range(0, 3) - 1; // -1 0 1
        if (num == 0)
        {
            ChangeState(MonsterState.Pause);
            return 0;
        }
        else
            return num;
    }


    void DoJump()
    {
        if (!isJumping) return;

        jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        Vector3 flat = Vector3.Lerp(jumpStart, targetPosition.Value, t);
        float height = Mathf.Sin(t * Mathf.PI) * apexHeight;

        transform.position = flat + Vector3.up * height;

        if (t >= 1f)
        {
            isJumping = false;
            col.enabled = true;
            ChangeState(prevState); // or Chase 등
        }
    }

    void StartJump()
    {
        jumpTimer = 0;
        jumpStart = transform.position;
        isJumping = true;
        col.enabled = false;
        ChangeState(MonsterState.Jumping);
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void ClearTargetPosition()
    {
        targetPosition = null;
    }

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
            DoThink();
        }
        else if (newState == MonsterState.ReadyToJump)
        {
            prevState = state;
            readyTimer = 0f;
        }

        state = newState;
    }

    bool IsCliffAhead() // !@#$ 이부분 좀더 손봐야할듯 - 낙사방지하는거
    {
        Bounds bounds = col.bounds;

        // 콜라이더 아래쪽 중심
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);

        // X축 방향은 현재 바라보는 방향 기준으로 약간 옆으로
        float offsetX = spriteRenderer.flipX ? bounds.extents.x : -bounds.extents.x;
        Vector2 frontPos = origin + new Vector2(offsetX, 0f);

        RaycastHit2D hit = Physics2D.Raycast(frontPos, Vector2.down, 1.5f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(frontPos, Vector2.down * 1.5f, Color.red);

        return hit.collider == null;
    }

    void UpdateAnimator()
    {
        bool walking = Mathf.Abs(rb.velocity.x) > 0.01f;
        bool jumping = (state == MonsterState.Jumping);
        if (walking)
            spriteRenderer.flipX = (rb.velocity.x > 0);

        anim.SetBool("isWalking", walking);
        anim.SetBool("isJumping", jumping);
    }
}