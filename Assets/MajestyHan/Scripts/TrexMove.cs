using UnityEngine;

public class TrexMove : MonoBehaviour
{
    public enum MonsterState { Idle, Patrol, Chase, ReadyToJump, Jumping, Pause }
    public MonsterState state = MonsterState.Idle;
    private MonsterState prevState;


    [Header("�ӵ� / ��������")]
    public float moveSpeed = 2f;            // X�� �̵� �ӵ�
    public float chaseSpeed = 3f;           // ���� �ӵ�
    public float apexHeight = 3f;           // ���� ���� ����

    [Header("���ӽð�")]
    public float pauseDuration = 1.0f;      // ��ĩ ���� ���� �ð�
    public float jumpDuration = 2f;         // ���� �̵� �� �ð�
    public float readyDuration = 0.5f;      // ���� �غ� �ð� (ReadyToJump ����)

    [Header("���� �� ���� �ֱ�")]
    public float patrolJumpInterval = 10f;
    private float patrolJumpTimer = 0f;

    [Header("���� ��ȯ �ּ�/�ִ� �ð�")]
    public float minThinkTime = 5f;
    public float maxThinkTime = 10f;
    public float readyToChaseDuration = 2f;

    private float thinkDuration;            // ���� ������ ���� ����� �ֱ�

    private Vector3 jumpStart;              // ���� ���� ��ġ
    private Vector3? targetPosition = null; // ����/���� ��ǥ ��ġ

    private bool isJumping = false;         // ���� ���� ������ ����

    // ������Ʈ ����
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D col;

    // ���� Ÿ�̸�
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
        UpdateAnimator(); // ���� �ӵ� �� ���� ��� �ִϸ��̼� ������Ʈ

        switch (state)
        {
            case MonsterState.Idle:
                rb.velocity = Vector2.zero;
                break;

            case MonsterState.Patrol:
                DoPatrol(); // ���� �ð����� ���� ���� + ���� ����

                patrolJumpTimer += Time.deltaTime;
                if (patrolJumpTimer >= patrolJumpInterval)
                {
                    patrolJumpTimer = 0f;
                    TryPatrolJump(); // ���� ���� �õ�
                }
                break;

            case MonsterState.Chase:
                chaseTimer += Time.deltaTime;
                if (chaseTimer >= readyToChaseDuration)
                {
                    if (targetPosition.HasValue)
                        MoveTowardsTarget(); // �÷��̾ ���� X������ �̵�
                    else
                        ChangeState(MonsterState.Patrol); // ���� ��� ������ ���� ����
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
                DoJump(); // ���� ���� + ���������� ��ġ �̵�
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
                if (Mathf.Abs(vx) < 0.05f) return; // ���� ���¿����� ����

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
                    Debug.Log($"[TrexMove] ���� ��� ���� ���� �� {node.name} �� {node.connectedNode.name}");
                }
            }

        }
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("�÷��̾�� �浹!");
            // ���⿡ ���̰ų� ���׼� �ֱ� !@#$!@#$!@#$
        }
    }

    // �÷��̾� ��ġ�� ���� X�� �������θ� �̵�
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

    // ���� ��: ���� �ֱ⸶�� ���� ��ȯ + ���� ����
    void DoPatrol()
    {
        thinkTimer += Time.deltaTime;

        if (thinkTimer >= thinkDuration)
        {
            thinkTimer = 0f;
            thinkDuration = Random.Range(minThinkTime, maxThinkTime);

            int dir = DoThink(); // -1, 0, 1 �� �ϳ� ����
            if (dir != 0)
                rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        }

        // �տ� ���������� ������ ���� ����
        if (IsCliffAhead())
            rb.velocity = new Vector2(-1 * rb.velocity.x, rb.velocity.y);
    }

    // ���� ���� ����: -1, 0, 1 �� ����
    int DoThink()
    {
        int num = Random.Range(0, 3) - 1;
        if (num == 0)
        {
            ChangeState(MonsterState.Pause); // ��ĩ ���� ��ȯ
            return 0;
        }
        else
            return num;
    }

    // ���� ��: ������ �̵� ó��
    void DoJump()
    {
        if (!isJumping) return;

        jumpTimer += Time.deltaTime;

        spriteRenderer.flipX = (targetPosition.Value.x - jumpStart.x > 0);
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        // ���� ���� ���� �� ���� ����
        Vector3 targetFootPos = targetPosition.Value;
        Vector3 flat = Vector3.Lerp(jumpStart, targetFootPos, t);
        float height = Mathf.Sin(t * Mathf.PI) * apexHeight;

        // �� ���� ��ġ + ���� => �߽� ��ǥ
        transform.position = flat + Vector3.up * col.bounds.extents.y + Vector3.up * height;

        if (t >= 1f)
        {
            isJumping = false;
            col.isTrigger = false;
            ChangeState(prevState);
        }
    }


    // ���� ���� �� �ʱ� ����
    void StartJump()
    {
        jumpTimer = 0;
        float footOffset = col.bounds.extents.y;
        jumpStart = transform.position - Vector3.up * footOffset;
        isJumping = true;
        col.isTrigger = true; // �浹 ���� (�� � �ɸ��� �ʵ���)
        ChangeState(MonsterState.Jumping);
    }

    // �ܺο��� ��ǥ ��ġ ����
    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
    }

    public void ClearTargetPosition()
    {
        targetPosition = null;
    }

    // ���� ���� ó��
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
            DoThink(); // ���� ���� �� �� �ʱ� ���� ����
        }
        else if (newState == MonsterState.ReadyToJump)
        {
            rb.velocity = Vector2.zero;
            float dirX = targetPosition.Value.x - transform.position.x;
            prevState = state; // ���� ���� ����
            readyTimer = 0f;
        }


        state = newState;
    }

    // �������� ���� (�ݶ��̴� ����, ���� �Ʒ� ����)
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

    // �ִϸ��̼� bool �Ķ���� ����
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
        // ����� ���� ���� TrexThink �� allJumpNodes�� ���� �޾ƾ� �ϹǷ�
        TrexThink thinker = GetComponent<TrexThink>();
        if (thinker == null || thinker.allJumpNodes == null || thinker.allJumpNodes.Count == 0) return;

        // ���� ���� ��� �� ����� ��尡 �ִ� �͸� ���͸�
        var candidates = thinker.allJumpNodes.FindAll(n => n.connectedNode != null);
        if (candidates.Count == 0) return;

        // ������ ��� ����
        int rand = Random.Range(0, candidates.Count);
        JumpNode selected = candidates[rand];

        // ���� ����
        SetTargetPosition(selected.connectedNode.transform.position);
        ChangeState(MonsterState.ReadyToJump);

        Debug.Log($"[TrexMove] ���� ���� �õ� �� {selected.name} �� {selected.connectedNode.name}");
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
        prevState = MonsterState.Patrol;  // ���� ���ƿ� ����
        targetPosition = target;

        // ���� ���� ����
        jumpTimer = 0;
        float footOffset = col.bounds.extents.y;
        jumpStart = transform.position - Vector3.up * footOffset;
        isJumping = true;
        col.isTrigger = true;

        ChangeState(MonsterState.Jumping);  // ��� ���� ���� ��ȯ
    }


}
