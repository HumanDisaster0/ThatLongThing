using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.CullingGroup;

public enum PlayerState
{
    Idle = 0,
    Walk,
    Jump,
    Fall,
    Die
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    #region Public Member

    [Header("참조 컴포넌트")]
    public SpriteRenderer playerSprite;
    public Animator playerAnimator;
    public Transform spriteRoot;
    public MagicAbility magic;
    public PreLifeCountIndicator preLifeIndicator;
    //public BoxCollider2D hitBoxCol;
    //public AudioSource audioSource;

    [Header("땅 레이어 마스크")]
    public LayerMask groundMask;

    [Header("좌우 이동 관련")]
    public float moveSpeed = 2.5f;
    public float moveSharpness = 12.0f;
    public float airAcceleration = 1.8f;
    public float airDrag = 0.1f;

    [Header("수직 이동 관련")]
    public float jumpForce = 6.0f;
    public float gravityForce = 15.0f;

    public int maxJumpCount = 2;
    public float coyoteJumpTime = 0.08f; //공중돌입 후 점프유예 시간
    public float jumpBufferTime = 0.15f; //착지 전 점프입력 유효 시간

    [Header("이상현상 관련")]
    public float playerScale = 1.0f;

    [Header("map")]
    public RectTransform minimap;

    public Action<PlayerState> OnStateChanged;
    public Action OnLand;

    public bool ForceInput;
    public int Dir;

    public bool SkipInput { get { return m_skipInput; } set { m_skipInput = value; } }
    public bool Invincibility { get { return m_invincibility; } set { m_invincibility = value; } }
    public bool Freeze { get { return m_freeze; } set { m_freeze = value; } }
    public bool IsGrounded => m_isGrounded;

    #endregion

    #region Private Member

    //내부 컴포넌트
    Rigidbody2D m_rb;
    BoxCollider2D m_col;

    //접지 관련
    bool m_isGrounded = true;
    Vector2 m_groundNormal;
    Rigidbody2D m_groundRB;
    RaycastHit2D[] m_hits = new RaycastHit2D[8];

    float m_footposY = 0.5f;

    //현재 속도
    Vector2 m_currentVel;

    //점프 관련
    float m_lastJumpTime;
    float m_coyoteJumpTimer;
    float m_jumpBufferTimer;
    int m_jumpCount;

    //입력 관련
    float m_hInput;
    float m_vInput;
    bool m_jumpInput;
    bool m_skipInput;

    bool m_freeze;
    bool m_invincibility;

    //플레이어 상태
    PlayerState m_currentState = PlayerState.Idle;


    //애니메이션 해쉬 값 - 이렇게 하면 빨라요
    readonly static int m_hash_idleAnim = Animator.StringToHash("anim_player_idle");
    readonly static int m_hash_walkAnim = Animator.StringToHash("anim_player_walk");
    readonly static int m_hash_jumpAnim = Animator.StringToHash("anim_player_jump");
    readonly static int m_hash_fallAnim = Animator.StringToHash("anim_player_fall");
    readonly static int m_hash_dieAnim = Animator.StringToHash("anim_player_die");

    readonly Vector2 PLAYER_DEFUALT_COL_OFFSET = new Vector2(0, -0.02f);
    readonly Vector2 PLAYER_DEFUALT_COL_SIZE = new Vector2(0.5f, 0.92f);
    readonly Vector3 PLAYER_DEFUALT_SPR_POS = new Vector3(0, 0.5f, 0);

    #endregion

    private void OnValidate()
    {
        ApplyScale();
    }

    public void ApplyScale()
    {
        if (!m_col)
            m_col = GetComponent<BoxCollider2D>();
        m_col.offset = PLAYER_DEFUALT_COL_OFFSET * playerScale;
        m_col.size = PLAYER_DEFUALT_COL_SIZE * playerScale;

        spriteRoot.transform.localScale = Vector3.one * playerScale;
        spriteRoot.transform.localPosition = PLAYER_DEFUALT_SPR_POS * playerScale;
    }

    // Start is called before the first frame update
    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<BoxCollider2D>();
        m_isGrounded = false;
        m_groundNormal = Vector2.zero;

        m_rb.gravityScale = 0.0f;
        m_rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        m_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        ApplyScale();

        minimap = GameObject.FindWithTag("Minimap")?.transform?.Find("Panel")?.Find("MapRect")?.GetComponent<RectTransform>();

        if (preLifeIndicator == null)
        {
            var indicator = FindObjectsByType<PreLifeCountIndicator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if(indicator.Length >0)
                preLifeIndicator = indicator[0];
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //충돌 시 속도 꺾기 (천장, 벽 부딪히는 경우 속도 제한)
        ProjectVelocity(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //충돌 시 속도 꺾기 (천장, 벽 부딪히는 경우 속도 제한)
        ProjectVelocity(collision);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(m_freeze) return;

        // 땅 체크
        var lastGrounded = m_isGrounded;
        GroundCheck();

        // 랜드
        if (!lastGrounded && m_isGrounded)
        {
            m_currentVel.y = 0.0f;
            m_rb.velocity = m_currentVel;
            OnLand?.Invoke();
        }

        // 상태 전이
        var lastState = m_currentState;
        CheckTransition();
        if (lastState != m_currentState)
        {
            OnStateChanged?.Invoke(m_currentState);
            ExitState(lastState);
            EnterState(m_currentState);
        }

        // 상태 실행
        UpdateState();
    }

    private void Update()
    {
        //---- 입력처리 ----//

        if(m_skipInput || (minimap && minimap.gameObject.activeInHierarchy))
        {
            m_hInput = 0;
            m_vInput = 0;
            m_jumpInput = false;
            return;
        }

        //-- 방향키 입력
        var lastHInput = m_hInput;

        m_hInput = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        m_vInput = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

        //이전 입력 비교 후 방향이 다른 경우
        if (m_currentState != PlayerState.Die && m_hInput != lastHInput && Mathf.Abs(m_hInput) > 0)
        {
            //스프라이트 좌우 반전
            playerSprite.flipX = m_hInput > 0 ? false : true;
        }


        //-- 점프 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpInput = true;
            m_jumpBufferTimer = jumpBufferTime; //점프 입력 버퍼 시간 갱신
        }
        else
        {
            m_jumpBufferTimer -= Time.deltaTime; //점프 입력이 없는 경우 계속 감소
        }

        //-- 코요테 시간(점프 유예 시간) 갱신
        if (m_isGrounded)
        {
            //땅인 경우 갱신
            m_coyoteJumpTimer = coyoteJumpTime;
        }
        else
        {
            //아닌 경우 감소
            var lastCoyoteJumpTimer = m_coyoteJumpTimer;
            m_coyoteJumpTimer -= Time.deltaTime;
            
            //공중 3단 점프 방지를 위한 공중돌입 판정
            if (lastCoyoteJumpTimer > 0.0f && m_coyoteJumpTimer <= 0.0f)
                m_jumpCount++;
        }

        if (Input.GetKeyDown(KeyCode.Q))
            magic.UseMagic();
    }

    void GroundCheck()
    {
        //점프 중이고 아직 위로 올라갈 수 있다면 땅체크 안함
        if (!m_isGrounded && m_currentVel.y > 0.025f)
            return;
        var lastGrounded = m_isGrounded;
        var checkDist = lastGrounded ? 0.5f : 0.025f;
        m_isGrounded = false;

        //바닥 체크 (박스 캐스트 - 너비 사이즈 큼)
        var isHit = CharacterSweepTest(transform.position + Vector3.down * (m_col.size.y * 0.25f - m_col.offset.y), new Vector2(0.55f * playerScale, m_col.size.y * 0.5f), Vector2.down, checkDist, groundMask, out RaycastHit2D hitInfo);

        //바닥 체크 실패 시 작은 너비로 재도전
        if (!isHit || hitInfo.normal.y <= 0.7f)
            isHit = CharacterSweepTest(transform.position + Vector3.down * (m_col.size.y * 0.25f - m_col.offset.y), new Vector2(0.5f * playerScale, m_col.size.y * 0.5f), Vector2.down, checkDist, groundMask, out hitInfo);


        //점프 중이 아니고 바닥체크에 성공한 경우
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
                var wallHitInfo = Physics2D.Raycast(transform.position + Vector3.down * (m_col.size.y * 0.5f + 0.005f - m_col.offset.y), -m_currentVel.normalized, 2.0f, groundMask);
                if (wallHitInfo.collider == null
                    || wallHitInfo.normal.y <= 0.7f)
                {
                    //절벽인 경우 달라 붙지 않고 isGrouded = false, 바닥이 Rigidbody였던 경우 Rigidbody의 속도 물려받기
                    if (m_groundRB != null)
                    {
                        m_currentVel += m_groundRB.velocity;
                    }

                    return;
                }
            }

            m_rb.position = snappedPos;
            m_isGrounded = true;
            m_groundNormal = hitInfo.normal;
            m_jumpCount = 0;
            m_groundRB = hitInfo.rigidbody;

            //바닥이 움직이는 경우
            if (m_groundRB && hitInfo.rigidbody.bodyType != RigidbodyType2D.Static)
            {
                m_rb.position += m_groundRB.velocity * Time.deltaTime;

                float angVel = m_groundRB.angularVelocity;         // deg/sec
                if (!Mathf.Approximately(angVel, 0f))
                {
                    float angVelRad = angVel * Mathf.Deg2Rad;
                    Vector2 pivot = m_groundRB.worldCenterOfMass;
                    Vector2 relPos = m_rb.position - pivot;
                    Vector2 rotVel = new Vector2(-angVelRad * relPos.y,
                                                   angVelRad * relPos.x);
                    m_rb.position += rotVel * Time.deltaTime;
                }
            }

            if(hitInfo.transform.TryGetComponent(out TrapTrigger traptrigger))
            {
                traptrigger.ForceActiveTrigger(gameObject);
            }
        }

        if (m_groundRB != null 
            && lastGrounded
            && !m_isGrounded)
        {
            m_currentVel += m_groundRB.velocity;

            // 회전에 의한 속도
            float angVel = m_groundRB.angularVelocity;
            if (!Mathf.Approximately(angVel, 0f))
            {
                float angVelRad = angVel * Mathf.Deg2Rad;
                Vector2 pivot = m_groundRB.worldCenterOfMass;
                Vector2 relPos = m_rb.position - pivot;
                Vector2 rotVel = new Vector2(-angVelRad * relPos.y,
                                               angVelRad * relPos.x);
                m_currentVel += rotVel;
            }
        }

        //완전 공중인 경우 땅 Rigidbody 없음 취급
        if (!lastGrounded && m_coyoteJumpTimer < 0.0f)
            m_groundRB = null;
    }


    public void CheckTransition()
    {
        //AnyState Transition->
        if (m_currentState == PlayerState.Die)
            return;

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
            ExcuteJump();
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
                        m_currentState = PlayerState.Walk;
                    return;
                }
                //->낙하
                if (!m_isGrounded)
                {
                    m_currentState = PlayerState.Fall;
                    return;
                }
                return;
            case PlayerState.Walk:
                if (m_isGrounded)
                {
                    //->이동
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
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
                        m_currentState = PlayerState.Walk;
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
                        m_currentState = PlayerState.Walk; 
                    //->앉기
                    return;
                }
                return;
        }
    }

    /// <summary>
    /// 외부에서 상태를 변경시키기 위한 함수
    /// </summary>
    /// <param name="state"></param>
    public void AnyState(PlayerState state, bool forceChange = false)
    {
        if (!forceChange && m_invincibility && state == PlayerState.Die)
            return;

        var lastState = m_currentState;
        m_currentState = state;
        if (lastState != m_currentState)
        {
            OnStateChanged?.Invoke(m_currentState);
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
            case PlayerState.Walk:
                SoundManager.instance.StopSound("Stone_Step", gameObject);
                return;
        }
    }

    public void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                playerAnimator.Play(m_hash_idleAnim);
                return;
            case PlayerState.Walk:
                SoundManager.instance?.PlayLoopSound("Stone_Step", gameObject);
                playerAnimator.Play(m_hash_walkAnim);
                return;
            case PlayerState.Jump:
                playerAnimator.Play(m_hash_jumpAnim);
                //SoundManager.instance.PlayNewSound("Jump", gameObject);
                return;
            case PlayerState.Fall:
                playerAnimator.Play(m_hash_fallAnim);
                return;
            case PlayerState.Die:
                SoundManager.instance.PlayBackSound("Die1");
                SoundManager.instance.SetMute(true, 1.0f);
                playerAnimator.Play(m_hash_dieAnim);
                return;
        }
    }

    public void UpdateState()
    {
        switch (m_currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Walk:
            case PlayerState.Jump:
            case PlayerState.Fall:
                DefaultMovement();
                break;
            case PlayerState.Die:
                m_hInput = 0;
                m_jumpInput = false;
                m_coyoteJumpTimer = 0f;
                m_jumpBufferTimer = 0f;
                m_jumpInput = false;
                DefaultMovement();
                if (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                {
                    //PlayerSpawnManager.instance.Respawn();
                    Freeze = true;
                    preLifeIndicator.PlayRespawnAnimation(StageManager.instance.deathCount + 1);
                }
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

            m_currentVel.x *= (1f / (1f + (airDrag * Time.deltaTime)));

            m_currentVel.y -= gravityForce * Time.deltaTime;
        }
        m_rb.velocity = m_currentVel;
    }

    public void AddForceToRB(Vector2 force)
    {
        m_rb.velocity += force;
        m_currentVel += force;
    }

    public void SetVelocity(Vector2 vel)
    {
        m_rb.velocity = vel;
        m_currentVel = vel;
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

    void ExcuteJump()
    {
        m_lastJumpTime = Time.time;
        m_currentVel.y = 0.0f;
        if (m_jumpCount < 1
            && m_groundRB != null
            && m_groundRB.velocity.y >= 0.0f)
        {
            m_currentVel += m_groundRB.velocity;

            // 회전에 의한 속도
            float angVel = m_groundRB.angularVelocity;
            if (!Mathf.Approximately(angVel, 0f))
            {
                float angVelRad = angVel * Mathf.Deg2Rad;
                Vector2 pivot = m_groundRB.worldCenterOfMass;
                Vector2 relPos = m_rb.position - pivot;
                Vector2 rotVel = new Vector2(-angVelRad * relPos.y,
                                               angVelRad * relPos.x);
                m_currentVel += rotVel;
            }
        }

        SoundManager.instance.PlayNewSound("Jump", gameObject);
        m_groundRB = null;
        m_currentVel += Vector2.up * jumpForce;
        m_isGrounded = false;
        m_currentState = PlayerState.Jump;
        m_coyoteJumpTimer = 0.0f;
        m_jumpBufferTimer = 0.0f;
        m_jumpInput = false;
        m_jumpCount++;
        return;
    }

    
    //커스텀 박스캐스트입니다. 정확한 레이어마스크 감지를 위해 만든 함수입니다.
    bool CharacterSweepTest(Vector2 origin, Vector2 size, Vector2 direction, float distance, LayerMask layerMask,out RaycastHit2D hitInfo)
    {
        //non alloc으로 해당 경로의 모든 충돌가능한 콜라이더 참조 가져오기
        var hitCount = Physics2D.BoxCastNonAlloc(origin, size, 0.0f, direction, m_hits, distance, layerMask);

        //hitcount가 하나라도 있는지 확인
        if (hitCount > 0)
        {
            //가장 가까운 콜라이더 찾기
            var nearestHit = new RaycastHit2D();
            var nearestDist = Mathf.Infinity;
            for(int i = 0; i < hitCount; i++)
            {
                var currentHit = m_hits[i];

                if (!currentHit)
                    continue;

                if (currentHit.collider == m_col
                    || currentHit.collider.isTrigger)
                    continue;

                if(currentHit.distance < nearestDist)
                {
                    nearestDist = currentHit.distance;
                    nearestHit = currentHit;
                }
            }

            //가장 가까운 콜라이더 정보 저장
            hitInfo = nearestHit;

            return true;
        }

        hitInfo = new RaycastHit2D();
        return false;
    }

    //private void OnGUI()
    //{

    //    // 텍스트 스타일 설정
    //    GUIStyle textStyle = new GUIStyle();
    //    textStyle.fontSize = 24;
    //    textStyle.normal.textColor = Color.white;

    //    // 화면 위치와 크기 설정 (x, y, width, height)
    //    GUI.Label(new Rect(10, 10, 300, 50), $"{m_currentState}", textStyle);
    //}
}
