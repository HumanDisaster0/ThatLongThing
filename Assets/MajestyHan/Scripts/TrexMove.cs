//using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class TrexMove : MonoBehaviour
{

    public CameraController cam;
    public enum MonsterState { Idle, Patrol, Chase, ReadyToJump, Jumping, Pause }
    [Header("�������ͽ�")]
    public MonsterState state = MonsterState.Idle;
    public MonsterState prevState;

    [Header("�ӵ� / ��������")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float apexHeight = 3f;

    [Header("���ӽð�")]
    public float pauseDuration = 1.0f;
    public float jumpDuration = 2f;
    public float readyDuration = 0.5f;

    [Header("���� ��ȯ �ּ�/�ִ� �ð�")]
    public float minThinkTime = 5f;
    public float maxThinkTime = 10f;


    private float thinkDuration;
    private Vector3 jumpStart;
    private Vector3? targetPosition = null;
    private Vector3? preJumpTargetPosition = null;

    private bool isJumping = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D col;
    private TrexThink think;

    private float pauseTimer = 0f;
    private float thinkTimer = 0f;
    private float jumpTimer = 0f;
    private float readyTimer = 0f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        think = GetComponent<TrexThink>();
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
                DoPatrol();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
                pc.AnyState(PlayerState.Die);

            if (state == MonsterState.Jumping)
                prevState = MonsterState.Pause;
            else
                ChangeState(MonsterState.Pause);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {

            PlayerController pc = other.gameObject.GetComponent<PlayerController>();
            if (pc != null)
                pc.AnyState(PlayerState.Die);

            if (state == MonsterState.Jumping)
                prevState = MonsterState.Pause;
            else
                ChangeState(MonsterState.Pause);

        }
    }



    void MoveTowardsTarget()
    {
        if (!targetPosition.HasValue) return;

        float dirX = Mathf.Sign(targetPosition.Value.x - transform.position.x);
        rb.velocity = new Vector2(dirX * chaseSpeed, rb.velocity.y);
    }



    void DoPatrol()
    {
        thinkTimer += Time.deltaTime;

        if (thinkTimer >= thinkDuration)
        {
            thinkTimer = 0f;
            thinkDuration = Random.Range(minThinkTime, maxThinkTime);

            int dir = DoThink();
            if (dir != 0)
                rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);
        }
    }

    int DoThink()
    {

        int num = Random.Range(0, 3) - 1;
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

        spriteRenderer.flipX = (targetPosition.Value.x - jumpStart.x > 0);
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        Vector3 targetFootPos = targetPosition.Value;
        Vector3 flat = Vector3.Lerp(jumpStart, targetFootPos, t);
        float height = Mathf.Sin(t * Mathf.PI) * apexHeight;

        transform.position = flat + Vector3.up * col.bounds.extents.y + Vector3.up * height;

        if (t >= 1f) //���� �Ϸ� ���� - ī�޶� ��鸮�� ȿ�� ��������
        {
            isJumping = false;
            col.isTrigger = false;
            if (preJumpTargetPosition.HasValue)
            {
                SetTargetPosition(preJumpTargetPosition.Value);
                preJumpTargetPosition = null;
            }

            if (cam != null)
            {
                cam.ShakeCamera(45f, 0.4f, 3f);
            }
            else
            {
                Debug.Log("ī�޶� ����");
            }

            SoundManager.instance?.PlayNewBackSound("Trex_Land");
            ChangeState(prevState);
        }
    }
    public void OnTrexFootstep()
    {
        if (cam != null && state == MonsterState.Chase) // �޸��� �߿���
        {
            cam.ShakeCamera(25f, 0.1f, 5f); // ������ ������ ��鸲
        }
    }


    void StartJump()
    {
        jumpTimer = 0;
        float footOffset = col.bounds.extents.y;
        jumpStart = transform.position - Vector3.up * footOffset;
        isJumping = true;
        col.isTrigger = true;
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
            // DoThink(); // �� �Ǵ��� TrexThink���� ó��
        }
        else if (newState == MonsterState.ReadyToJump)
        {
            rb.velocity = Vector2.zero;
            readyTimer = 0f;
        }

        state = newState;
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

    public void ClearTimer()
    {
        pauseTimer = 0f;
        thinkTimer = 0f;
        jumpTimer = 0f;
        readyTimer = 0f;

    }
    public void PrepareJump(Vector3 target)
    {
        if (state == MonsterState.Chase)
        {
            preJumpTargetPosition = think.player.position; //
        }

        prevState = state;
        targetPosition = target;
        ChangeState(MonsterState.ReadyToJump); // �� �غ� �� ����
    }

    public void JumpNow(Vector3 target)
    {
        if (state == MonsterState.ReadyToJump || state == MonsterState.Jumping)
            return;

        if (state == MonsterState.Chase)
        {
            preJumpTargetPosition = think.player.position;
        }

        prevState = state;
        targetPosition = target;
        StartJump();
    }
}
