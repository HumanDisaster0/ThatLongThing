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

    [Header("���� ������Ʈ")]
    public SpriteRenderer playerSprite;
    public Animator playerAnimator;
    public Transform spriteRoot;
    public MagicAbility magic;
    public PreLifeCountIndicator preLifeIndicator;
    //public BoxCollider2D hitBoxCol;
    //public AudioSource audioSource;

    [Header("�� ���̾� ����ũ")]
    public LayerMask groundMask;

    [Header("�¿� �̵� ����")]
    public float moveSpeed = 2.5f;
    public float moveSharpness = 12.0f;
    public float airAcceleration = 1.8f;
    public float airDrag = 0.1f;

    [Header("���� �̵� ����")]
    public float jumpForce = 6.0f;
    public float gravityForce = 15.0f;

    public int maxJumpCount = 2;
    public float coyoteJumpTime = 0.08f; //���ߵ��� �� �������� �ð�
    public float jumpBufferTime = 0.15f; //���� �� �����Է� ��ȿ �ð�

    [Header("�̻����� ����")]
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

    //���� ������Ʈ
    Rigidbody2D m_rb;
    BoxCollider2D m_col;

    //���� ����
    bool m_isGrounded = true;
    Vector2 m_groundNormal;
    Rigidbody2D m_groundRB;
    RaycastHit2D[] m_hits = new RaycastHit2D[8];

    float m_footposY = 0.5f;

    //���� �ӵ�
    Vector2 m_currentVel;

    //���� ����
    float m_lastJumpTime;
    float m_coyoteJumpTimer;
    float m_jumpBufferTimer;
    int m_jumpCount;

    //�Է� ����
    float m_hInput;
    float m_vInput;
    bool m_jumpInput;
    bool m_skipInput;

    bool m_freeze;
    bool m_invincibility;

    //�÷��̾� ����
    PlayerState m_currentState = PlayerState.Idle;


    //�ִϸ��̼� �ؽ� �� - �̷��� �ϸ� �����
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
        //�浹 �� �ӵ� ���� (õ��, �� �ε����� ��� �ӵ� ����)
        ProjectVelocity(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        //�浹 �� �ӵ� ���� (õ��, �� �ε����� ��� �ӵ� ����)
        ProjectVelocity(collision);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(m_freeze) return;

        // �� üũ
        var lastGrounded = m_isGrounded;
        GroundCheck();

        // ����
        if (!lastGrounded && m_isGrounded)
        {
            m_currentVel.y = 0.0f;
            m_rb.velocity = m_currentVel;
            OnLand?.Invoke();
        }

        // ���� ����
        var lastState = m_currentState;
        CheckTransition();
        if (lastState != m_currentState)
        {
            OnStateChanged?.Invoke(m_currentState);
            ExitState(lastState);
            EnterState(m_currentState);
        }

        // ���� ����
        UpdateState();
    }

    private void Update()
    {
        //---- �Է�ó�� ----//

        if(m_skipInput || (minimap && minimap.gameObject.activeInHierarchy))
        {
            m_hInput = 0;
            m_vInput = 0;
            m_jumpInput = false;
            return;
        }

        //-- ����Ű �Է�
        var lastHInput = m_hInput;

        m_hInput = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        m_vInput = (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0);

        //���� �Է� �� �� ������ �ٸ� ���
        if (m_currentState != PlayerState.Die && m_hInput != lastHInput && Mathf.Abs(m_hInput) > 0)
        {
            //��������Ʈ �¿� ����
            playerSprite.flipX = m_hInput > 0 ? false : true;
        }


        //-- ���� �Է�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_jumpInput = true;
            m_jumpBufferTimer = jumpBufferTime; //���� �Է� ���� �ð� ����
        }
        else
        {
            m_jumpBufferTimer -= Time.deltaTime; //���� �Է��� ���� ��� ��� ����
        }

        //-- �ڿ��� �ð�(���� ���� �ð�) ����
        if (m_isGrounded)
        {
            //���� ��� ����
            m_coyoteJumpTimer = coyoteJumpTime;
        }
        else
        {
            //�ƴ� ��� ����
            var lastCoyoteJumpTimer = m_coyoteJumpTimer;
            m_coyoteJumpTimer -= Time.deltaTime;
            
            //���� 3�� ���� ������ ���� ���ߵ��� ����
            if (lastCoyoteJumpTimer > 0.0f && m_coyoteJumpTimer <= 0.0f)
                m_jumpCount++;
        }

        if (Input.GetKeyDown(KeyCode.Q))
            magic.UseMagic();
    }

    void GroundCheck()
    {
        //���� ���̰� ���� ���� �ö� �� �ִٸ� ��üũ ����
        if (!m_isGrounded && m_currentVel.y > 0.025f)
            return;
        var lastGrounded = m_isGrounded;
        var checkDist = lastGrounded ? 0.5f : 0.025f;
        m_isGrounded = false;

        //�ٴ� üũ (�ڽ� ĳ��Ʈ - �ʺ� ������ ŭ)
        var isHit = CharacterSweepTest(transform.position + Vector3.down * (m_col.size.y * 0.25f - m_col.offset.y), new Vector2(0.55f * playerScale, m_col.size.y * 0.5f), Vector2.down, checkDist, groundMask, out RaycastHit2D hitInfo);

        //�ٴ� üũ ���� �� ���� �ʺ�� �絵��
        if (!isHit || hitInfo.normal.y <= 0.7f)
            isHit = CharacterSweepTest(transform.position + Vector3.down * (m_col.size.y * 0.25f - m_col.offset.y), new Vector2(0.5f * playerScale, m_col.size.y * 0.5f), Vector2.down, checkDist, groundMask, out hitInfo);


        //���� ���� �ƴϰ� �ٴ�üũ�� ������ ���
        if (Time.time >= m_lastJumpTime + 0.2f
            && hitInfo.collider != null
            && hitInfo.normal.y > 0.7f)
        {
            //������(���� �޶�ٱ�)
            var snappedPos = m_rb.position + Vector2.down * (hitInfo.distance - 0.015f);

            if (hitInfo.transform.TryGetComponent(out PlatformEffector2D effector)
                && m_rb.position.y - m_footposY - hitInfo.point.y < 0.0f)
            {
                return;
            }

            //���� üũ
            if (lastGrounded
                && hitInfo.distance > 0.05f
                && !Mathf.Approximately(0.0f, m_currentVel.x))
            {
                var wallHitInfo = Physics2D.Raycast(transform.position + Vector3.down * (m_col.size.y * 0.5f + 0.005f - m_col.offset.y), -m_currentVel.normalized, 2.0f, groundMask);
                if (wallHitInfo.collider == null
                    || wallHitInfo.normal.y <= 0.7f)
                {
                    //������ ��� �޶� ���� �ʰ� isGrouded = false, �ٴ��� Rigidbody���� ��� Rigidbody�� �ӵ� �����ޱ�
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

            //�ٴ��� �����̴� ���
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

            // ȸ���� ���� �ӵ�
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

        //���� ������ ��� �� Rigidbody ���� ���
        if (!lastGrounded && m_coyoteJumpTimer < 0.0f)
            m_groundRB = null;
    }


    public void CheckTransition()
    {
        //AnyState Transition->
        if (m_currentState == PlayerState.Die)
            return;

        // ���� ó��
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
                    //->�̵�
                    if (Mathf.Abs(m_hInput) > 0)
                        m_currentState = PlayerState.Walk;
                    return;
                }
                //->����
                if (!m_isGrounded)
                {
                    m_currentState = PlayerState.Fall;
                    return;
                }
                return;
            case PlayerState.Walk:
                if (m_isGrounded)
                {
                    //->�̵�
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
                    return;
                }
                //->����
                if (!m_isGrounded)
                {
                    m_currentState = PlayerState.Fall;
                    return;
                }
                return;
            case PlayerState.Jump:
                if (m_isGrounded)
                {
                    //->�̵�
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
                    else
                        m_currentState = PlayerState.Walk;
                    return;
                }
                else
                {
                    //->����
                    if (m_rb.velocity.y < 0.0f)
                        m_currentState = PlayerState.Fall;
                }
                return;
            case PlayerState.Fall:
                if (m_isGrounded)
                {
                    //->�̵�
                    if (m_hInput == 0)
                        m_currentState = PlayerState.Idle;
                    else
                        m_currentState = PlayerState.Walk; 
                    //->�ɱ�
                    return;
                }
                return;
        }
    }

    /// <summary>
    /// �ܺο��� ���¸� �����Ű�� ���� �Լ�
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

        // �⺻ �̵� ����
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
        //�÷��� �������� ��� ����
        if (collision.gameObject.TryGetComponent(out PlatformEffector2D effecter))
            return;

        foreach (var contact in collision.contacts)
        {
            // �浹�� �븻 ����
            Vector2 normal = contact.normal;

            // ���� ��� ����
            if ((groundMask.value & (1 << collision.gameObject.layer)) != 0
                && normal.y > 0.7f)
                continue;

            // ���� ��� (������ �浹 ����)
            float dot = Vector2.Dot(m_currentVel, normal);

            // �ӵ� ���Ͱ� �浹���� ���� �����ϴ� ��츸 ó��
            if (dot < 0)
            {
                // �浹�鿡 ������ �ӵ� ���
                Vector2 projection = m_currentVel - (dot * normal);

                m_currentVel = projection; // �浹 �� �ӵ��� ������ �ӵ��� ����
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

            // ȸ���� ���� �ӵ�
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

    
    //Ŀ���� �ڽ�ĳ��Ʈ�Դϴ�. ��Ȯ�� ���̾��ũ ������ ���� ���� �Լ��Դϴ�.
    bool CharacterSweepTest(Vector2 origin, Vector2 size, Vector2 direction, float distance, LayerMask layerMask,out RaycastHit2D hitInfo)
    {
        //non alloc���� �ش� ����� ��� �浹������ �ݶ��̴� ���� ��������
        var hitCount = Physics2D.BoxCastNonAlloc(origin, size, 0.0f, direction, m_hits, distance, layerMask);

        //hitcount�� �ϳ��� �ִ��� Ȯ��
        if (hitCount > 0)
        {
            //���� ����� �ݶ��̴� ã��
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

            //���� ����� �ݶ��̴� ���� ����
            hitInfo = nearestHit;

            return true;
        }

        hitInfo = new RaycastHit2D();
        return false;
    }

    //private void OnGUI()
    //{

    //    // �ؽ�Ʈ ��Ÿ�� ����
    //    GUIStyle textStyle = new GUIStyle();
    //    textStyle.fontSize = 24;
    //    textStyle.normal.textColor = Color.white;

    //    // ȭ�� ��ġ�� ũ�� ���� (x, y, width, height)
    //    GUI.Label(new Rect(10, 10, 300, 50), $"{m_currentState}", textStyle);
    //}
}
