using UnityEngine;

public class TrexMove : MonoBehaviour
{
    public enum MonsterState { Idle, Patrol, Chase, ReadyToJump, Jumping, Pause }
    public MonsterState state = MonsterState.Idle;

    private MonsterState prevState;

    [Header("�ӵ� / ��������")]
    public float moveSpeed = 2f;            // �̵��ӵ�
    public float apexHeight = 3f;           // ���� ����
    [Header("���ӽð�")]
    public float pauseDuration = 1.0f;      // ��ĩ �ð�
    public float jumpDuration = 2f;         // �����ð�
    public float readyDuration = 0.5f;
    
    public float minThinkTime = 5f;
    public float maxThinkTime = 10f;

    private float thinkDuration;

    private Vector3 jumpStart;
    private Vector3? targetPosition = null; //Ƽ��밡 ��ǥ����

    bool isJumping = false;                 // ���� ���� �� ����


    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D col;
    private float pauseTimer = 0f;          // ������ ������
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
                DoPatrol(); // ���� �ð� ��, ������ ������
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
        rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y); //X�������θ� �߰��̵�
    }

    void DoPatrol()
    {
        thinkTimer += Time.deltaTime;

        if (thinkTimer >= thinkDuration)
        {
            thinkTimer = 0f;
            thinkDuration = Random.Range(minThinkTime, maxThinkTime); // ���� �Ͽ� �ð� ����

            int dir = DoThink(); // -1, 0, 1 �� �ϳ�

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
            ChangeState(prevState); // or Chase ��
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

    bool IsCliffAhead() // !@#$ �̺κ� ���� �պ����ҵ� - ��������ϴ°�
    {
        Bounds bounds = col.bounds;

        // �ݶ��̴� �Ʒ��� �߽�
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);

        // X�� ������ ���� �ٶ󺸴� ���� �������� �ణ ������
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