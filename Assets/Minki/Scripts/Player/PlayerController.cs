using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public enum PlayerState
{
    Idle = 0,
    Run,
    Jump,
    Fall,
    Crouch,
    Hit,
    Carried
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Public Member

    public SpriteRenderer playerSprite;
    //public Animator playerAnimator;
    public Transform spriteRoot;
    //public BoxCollider2D hitBoxCol;
    //public AudioSource audioSource;
    public LayerMask groundMask;

    public float moveSpeed = 2.5f;
    public float moveSharpness = 12.0f;
    public float airAcceleration = 1.8f;
    public float ariDrag = 0.1f;
    public float jumpForce = 6.0f;
    public float gravityForce = 15.0f;

    public int maxJumpCount = 2;
    public float coyoteJumpTime = 0.08f;
    public float jumpBufferTime = 0.15f;

    #endregion

    #region Private Member

    Rigidbody2D m_rb;

    bool m_isGrounded = true;
    Vector2 m_groundNormal;

    Vector2 m_currentVel;

    float m_lastJumpTime;
    float m_coyoteJumpTimer;
    float m_jumpBufferTimer;
    int m_jumpCount;

    float m_hInput;
    float m_vInput;
    bool m_jumpInput;
    float m_footposY = 0.5f;
    PlayerState m_currentState = PlayerState.Idle;

    //readonly static int m_hash_idleAnim = Animator.StringToHash("anim_knight_idle");
    //readonly static int m_hash_runAnim = Animator.StringToHash("anim_knight_run");
    //readonly static int m_hash_jumpAnim = Animator.StringToHash("anim_knight_jump");
    //readonly static int m_hash_fallAnim = Animator.StringToHash("anim_knight_fall");
    //readonly static int m_hash_crouchAnim = Animator.StringToHash("anim_knight_crouch");

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_isGrounded = false;
        m_groundNormal = Vector2.zero;

        m_rb.gravityScale = 0.0f;
        m_rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        m_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProjectVelocity(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProjectVelocity(collision);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // 땅 체크
        var lastGrounded = m_isGrounded;
        GroundCheck();

        // 랜드
        if (!lastGrounded && m_isGrounded)
        {
            m_currentVel.y = 0.0f;
            m_rb.velocity = m_currentVel;
        }

        // 상태 전이
        var lastState = m_currentState;
        CheckTransition();
        if (lastState != m_currentState)
        {
            ExitState(lastState);
            EnterState(m_currentState);
        }

        UpdateState();
    }

    private void Update()
    {
        var lastHInput = m_hInput;

        m_hInput = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        m_vInput = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

        if (m_hInput != lastHInput && Mathf.Abs(m_hInput) > 0)
        {
            playerSprite.flipX = m_hInput > 0 ? false : true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpInput = true;
            m_jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            m_jumpBufferTimer -= Time.deltaTime;
        }

        if (m_isGrounded)
        {
            m_coyoteJumpTimer = coyoteJumpTime;
        }
        else
        {
            var lastCoyoteJumpTimer = m_coyoteJumpTimer;
            m_coyoteJumpTimer -= Time.deltaTime;
            if (lastCoyoteJumpTimer > 0.0f && m_coyoteJumpTimer <= 0.0f)
                m_jumpCount++;
        }
    }

    void GroundCheck()
    {
        //점프 중이고 아직 위로 올라갈 수 있다면 땅체크 안함
        if (!m_isGrounded && m_currentVel.y > 0.025f)
            return;
        var lastGrounded = m_isGrounded;
        var checkDist = lastGrounded ? 0.5f : 0.025f;
        m_isGrounded = false;

        var hitInfo = Physics2D.BoxCast(transform.position + Vector3.down * 0.25f, new Vector2(0.55f, 0.5f), 0.0f, Vector2.down, checkDist, groundMask);

        if (hitInfo.collider == null
            || hitInfo.normal.y <= 0.7f)
            hitInfo = Physics2D.BoxCast(transform.position + Vector3.down * 0.25f, new Vector2(0.5f, 0.5f), 0.0f, Vector2.down, checkDist, groundMask);

        if (Time.time >= m_lastJumpTime + 0.2f
            && hitInfo.collider != null
            && hitInfo.normal.y > 0.7f)
        {
            //스냅핑(지면 달라붙기)
            var snappedPos = m_rb.position + Vector2.down * (hitInfo.distance - 0.015f);

            if (hitInfo.transform.TryGetComponent(out PlatformEffector2D effector)
                && m_rb.position.y - m_footposY - hitInfo.point.y < 0.0f)
            {
                return;
            }

            //절벽 체크
            if (lastGrounded
                && hitInfo.distance > 0.05f
                && !Mathf.Approximately(0.0f, m_currentVel.x))
            {
                var wallHitInfo = Physics2D.Raycast(transform.position + Vector3.down * 0.505f, -m_currentVel.normalized, 2.0f, groundMask);
                if (wallHitInfo.collider == null
                    || wallHitInfo.normal.y <= 0.7f)
                {
                    return;
                }
            }

            m_rb.position = snappedPos;
            m_isGrounded = true;
            m_groundNormal = hitInfo.normal;
            m_jumpCount = 0;
        }
    }


    public void CheckTransition()
    {
        //AnyState Transition->
        // 점프 처리

        if ((m_jumpCount < 1
            && m_jumpBufferTimer > 0f
            && m_coyoteJumpTimer > 0f
            && m_jumpInput)
            ||
            (m_jumpCount > 0
            && m_jumpCount < maxJumpCount
            && m_jumpInput))
        {
            m_lastJumpTime = Time.time;
            m_currentVel.y = 0.0f;
            m_currentVel += Vector2.up * jumpForce;
            m_isGrounded = false;
            m_currentState = PlayerState.Jump;
            //audioSource.Play();
            m_coyoteJumpTimer = 0.0f;
            m_jumpBufferTimer = 0.0f;
            m_jumpInput = false;
            m_jumpCount++;
            return;
        }

        //Normal Transition->
        switch (m_currentState)
        {
            case PlayerState.Idle:
                if (m_isGrounded)
                {
                    //->이동
                    if (Mathf.Abs(m_hInput) > 0)
                        m_currentState = PlayerState.Run;
                    //->앉기
                    if (m_vInput < 0)
                        m_currentState = PlayerState.Crouch;
                    return;
                }
                //->낙하
                if (!m_isGrounded)
                {
                    m_currentState = PlayerState.Fall;
                    return;
                }
                return;
            case PlayerState.Run:
                if (m_isGrounded)
                {
                    //->이동
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
                    //->앉기
                    if (m_vInput < 0)
                        m_currentState = PlayerState.Crouch;
                    return;
                }
                //->낙하
                if (!m_isGrounded)
                {
                    m_currentState = PlayerState.Fall;
                    return;
                }
                return;
            case PlayerState.Jump:
                if (m_isGrounded)
                {
                    //->이동
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
                    else
                        m_currentState = PlayerState.Run;
                    //->앉기
                    if (m_vInput < 0)
                        m_currentState = PlayerState.Crouch;
                    return;
                }
                else
                {
                    //->낙하
                    if (m_rb.velocity.y < 0.0f)
                        m_currentState = PlayerState.Fall;
                }
                return;
            case PlayerState.Fall:
                if (m_isGrounded)
                {
                    //->이동
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
                    else
                        m_currentState = PlayerState.Run;
                    //->앉기
                    if (m_vInput < 0)
                        m_currentState = PlayerState.Crouch;
                    return;
                }
                return;
            case PlayerState.Crouch:
                if (m_isGrounded)
                {
                    //->앉기
                    if (m_vInput >= 0)
                    {
                        if (Mathf.Abs(m_hInput) > 0)
                            m_currentState = PlayerState.Run;
                        else
                            m_currentState = PlayerState.Idle;
                    }
                    return;
                }
                //->낙하
                if (!m_isGrounded)
                {
                    m_currentState = PlayerState.Fall;
                    return;
                }
                return;
        }
    }

    public void AnyState(PlayerState state)
    {
        var lastState = m_currentState;
        m_currentState = state;
        if (lastState != m_currentState)
        {
            ExitState(lastState);
            EnterState(m_currentState);
        }
    }

    public PlayerState GetCurrentState()
    {
        return m_currentState;
    }

    public void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Crouch:
                //hitBoxCol.offset = Vector2.zero;
                //hitBoxCol.size = new Vector2 { x = 0.5f, y = 1.0f };
                return;
        }
    }

    public void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                //playerAnimator.Play(m_hash_idleAnim);
                return;
            case PlayerState.Run:
                //playerAnimator.Play(m_hash_runAnim);
                return;
            case PlayerState.Jump:
                //playerAnimator.Play(m_hash_jumpAnim);
                return;
            case PlayerState.Fall:
                //playerAnimator.Play(m_hash_fallAnim);
                return;
            case PlayerState.Crouch:
                //playerAnimator.Play(m_hash_crouchAnim);
                //hitBoxCol.offset = new Vector2 { x = 0.0f, y = -0.375f };
                //hitBoxCol.size = new Vector2 { x = 0.5f, y = 0.25f };
                return;
            case PlayerState.Carried:
                //playerAnimator.Play(m_hash_crouchAnim);
                m_rb.velocity = Vector2.zero;
                m_currentVel.x = 0.0f;
                return;
        }
    }

    public void UpdateState()
    {
        switch (m_currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Run:
            case PlayerState.Jump:
            case PlayerState.Fall:
                DefaultMovement();
                break;
            case PlayerState.Crouch:
                m_hInput = 0.0f;
                DefaultMovement();
                break;
            case PlayerState.Carried:
                break;
        }

        if (m_jumpBufferTimer <= 0.0f)
            m_jumpInput = false;
    }

    void DefaultMovement()
    {
        var targetVel = new Vector2(m_hInput * moveSpeed, 0f);

        // 기본 이동 로직
        if (m_isGrounded)
        {
            var crossNormal = new Vector2(-m_groundNormal.y, m_groundNormal.x);
            targetVel = Mathf.Sign(Vector2.Dot(targetVel, crossNormal)) * crossNormal * targetVel.magnitude;
            m_currentVel = Vector2.Lerp(m_currentVel, targetVel, Time.deltaTime * moveSharpness);
        }
        else
        {
            if (m_hInput != 0)
            {
                var velDiff = new Vector2(targetVel.x - m_currentVel.x, 0.0f);
                m_currentVel.x += velDiff.x * airAcceleration * Time.deltaTime;
            }

            m_currentVel.x *= (1f / (1f + (ariDrag * Time.deltaTime)));

            m_currentVel.y -= gravityForce * Time.deltaTime;
        }
        m_rb.velocity = m_currentVel;
    }

    public void AddForceToRB(Vector2 force)
    {
        m_rb.velocity += force;
        m_currentVel += force;
    }

    void ProjectVelocity(Collision2D collision)
    {
        //플랫폼 이펙터인 경우 무시
        if (collision.gameObject.TryGetComponent(out PlatformEffector2D effecter))
            return;

        foreach (var contact in collision.contacts)
        {
            // 충돌면 노말 벡터
            Vector2 normal = contact.normal;

            // 땅인 경우 무시
            if ((groundMask.value & (1 << collision.gameObject.layer)) != 0
                && normal.y > 0.7f)
                continue;

            // 내적 계산 (음수면 충돌 방향)
            float dot = Vector2.Dot(m_currentVel, normal);

            // 속도 벡터가 충돌면을 향해 접근하는 경우만 처리
            if (dot < 0)
            {
                // 충돌면에 투영된 속도 계산
                Vector2 projection = m_currentVel - (dot * normal);

                m_currentVel = projection; // 충돌 후 속도를 투영된 속도로 설정
            }
        }
    }
}
