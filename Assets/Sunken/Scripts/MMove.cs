using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public enum MBehavior
{
    Move = 0,
    Stop
}

public enum MStatus
{
    idle = 0,
    move,
    attack,
    die,
    end
}

public enum MonsterType
{
    Mole = 0,
    Rabbit,
    None
}

public class MMove : MonoBehaviour
{
    #region Serialized Private Member
    [Header("몬스터옵션")]
    [SerializeField] MonsterType MType = 0;
    [SerializeField] MBehavior mBehavior = 0;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpForce = 15f;
    [SerializeField] float maxidleDuration = 4f;
    [SerializeField] float minMoveLength = 1f;
    [SerializeField] float maxMoveLength = 3f;
    [SerializeField] float moveSharpness = 8f;
    [SerializeField] bool respawn = true;
    [SerializeField] Transform respawnPoint;
    [SerializeField] Vector2 spriteOffset = new Vector2(-0.5f, 0.5f);
    #endregion

    #region Private Member
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animCon;
    private MRange range;
    private MStatus currStatus = MStatus.idle;
    private MStatus prevStatus = MStatus.end;

    private float idleTime = 0f;
    private float destX = 0f;
    private float timer = 0f;
    private bool isGrounded = true;
    private bool flipX = false;
    private Vector2 initPos = Vector2.zero;
    #endregion

    private void OnValidate()
    {
        ExeType();
    }

    void Start()
    {
        ExeType();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        range = GetComponentInParent<MRange>();
        animCon = GetComponentInChildren<Animator>();
        initPos = transform.position;
    }

    void FixedUpdate()
    {
        if(mBehavior == MBehavior.Move)
        {
            CheckStatus();
            MoveEnemy();
        }
        else if(mBehavior == MBehavior.Stop)
        {
            StopAnim();
        }
    }

    private void GetGrounded()
    {
        CapsuleCollider2D m_col = GetComponent<CapsuleCollider2D>();

        RaycastHit2D hit = Physics2D.BoxCast(transform.position + Vector3.down * (m_col.size.y * 0.5f - m_col.offset.y), new Vector2(0.5f, m_col.size.y * 0.5f), 0f, Vector2.down);
        if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            isGrounded = true;
    }

    private void MoveEnemy()
    {
        switch(currStatus)
        {
            case MStatus.idle:
                timer += Time.fixedDeltaTime;
                rb.velocity =  new Vector2(Mathf.Lerp(rb.velocity.x,0f, moveSharpness * Time.deltaTime), rb.velocity.y);
                if (timer > idleTime)
                {
                    timer = 0f;
                    currStatus = MStatus.move;
                }
                break;
            case MStatus.move:
                float dir = transform.position.x < destX ? 1f : -1f;
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x,dir * moveSpeed,moveSharpness * Time.deltaTime), rb.velocity.y);

                Vector2 rdir = flipX ? Vector2.right : Vector2.left;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, rdir, 0.6f, LayerMask.GetMask("Ground"));

                if (hit.collider != null)
                {
                    //Jump();
                    range.SetRange(hit.point, this.gameObject);
                    currStatus = MStatus.idle;
                }

                if (Mathf.Abs(transform.position.x - destX) < 0.1f)
                {
                    Vector3 currpos = transform.position;
                    currpos.x = destX;
                    transform.position = currpos;
                    currStatus = MStatus.idle;
                }
                    
                break;
            case MStatus.die:
                timer += Time.fixedDeltaTime;
                //if (timer > 3.0f)
                //{
                //    timer = 0f;
                //    SetActivision();
                //}
                break;
            case MStatus.attack:
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, moveSharpness * Time.deltaTime);
                break;
            default:
                currStatus = MStatus.idle;
                break;
        }
    }

    private void Jump()
    {
        if(isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }  
    }

    private void CheckStatus()
    {
        if (currStatus != prevStatus)
        {
            switch (currStatus)
            {
                case MStatus.idle:
                    SoundManager.instance?.StopSound("Mole_FootStep", this.gameObject);
                    animCon.SetBool("isWalking", false);
                    animCon.SetBool("isAttack", false);
                    idleTime = UnityEngine.Random.Range(maxidleDuration / 2f, maxidleDuration);
                    break;
                case MStatus.move:
                    animCon.SetBool("isWalking", true);

                    switch(MType)
                    {
                        case MonsterType.Mole:
                            SoundManager.instance?.PlayLoopSound("Mole_FootStep", this.gameObject);
                            break;
                        case MonsterType.Rabbit:
                            SoundManager.instance?.PlayLoopSound("Rabbit_Footstep", this.gameObject);
                            break;
                    }

                    if (transform.position.x >= range.GetMinX() && transform.position.x <= range.GetMaxX())
                    {
                        int idx = 0;
                        do
                        {
                            if (idx > 10)
                            {
                                destX = (range.GetMinX() + range.GetMaxX()) / 2f;
                                break;
                            }
                            else
                                destX = range.GetRandomPosX();
                            idx++;
                            //Debug.Log("움직임 길이 : " + MathF.Abs(transform.position.x - destX));
                        }
                        while (MathF.Abs(transform.position.x - destX) <= minMoveLength || MathF.Abs(transform.position.x - destX) >= maxMoveLength);
                    }
                    else
                    {
                        if (Mathf.Abs(transform.position.x - range.GetMinX()) > Mathf.Abs(transform.position.x - range.GetMaxX()))
                            destX = range.GetMaxX();
                        else
                            destX = range.GetMinX();
                    }
                    float dir = transform.position.x < destX ? 1f : -1f;
                    FlipSprite(dir > 0f ? true : false);
                    break;
                case MStatus.die:
                    animCon.SetBool("isDead", true);

                    switch (MType)
                    {
                        case MonsterType.Mole:
                            SoundManager.instance?.StopSound("Mole_FootStep", this.gameObject);
                            break;
                        case MonsterType.Rabbit:
                            SoundManager.instance?.StopSound("Rabbit_Footstep", this.gameObject);
                            break;
                    }
                    SoundManager.instance?.PlaySound("Mole_Die", this.gameObject);
                    rb.velocity = Vector2.zero;
                    rb.simulated = false;
                    GetComponent<CapsuleCollider2D>().enabled = false;
                    StartCoroutine(SetActivision(3.0f));
                    break;
                case MStatus.attack:
                    animCon.SetBool("isAttack", true);
                    SoundManager.instance?.StopSound("Mole_FootStep", this.gameObject);
                    SoundManager.instance?.PlaySound("Mole_Attack", this.gameObject);
                    StartCoroutine(SetAttack(1.0f));
                    break;
                default:
                    currStatus = MStatus.idle;
                    break;
            }
            prevStatus = currStatus;
        }
    }

    //void SetActivision()
    //{
    //    manager.MonsterRespawn(this.gameObject, respawnPointIndex);
    //    if (respawn)
    //        Respawn();
    //}

    IEnumerator SetActivision(float _time)
    {
        yield return new WaitForSeconds(_time);

        if (respawnPoint == null || respawnPoint?.gameObject.activeSelf == false)
            respawn = false;

        if (respawn)
            Respawn();
        else
            gameObject.SetActive(false);
    }


    IEnumerator SetAttack(float _time)
    {   
        yield return new WaitForSeconds(_time);
        currStatus = MStatus.idle;
    }

    void FlipSprite(bool flag)
    {
        if(flag != flipX)
        {
            Vector3 currScale = transform.localScale;
            currScale.x *= -1f;
            transform.localScale = currScale;
        }

        flipX = flag;

        //if(flag != sr.flipX)
        //{
        //    Transform ct = transform.GetChild(1);
        //    ct.localScale = new Vector3(ct.localScale.x * -1f, ct.localScale.y, ct.localScale.z);
        //}

        //sr.flipX = flag;
    }
    public void SetStatus(MStatus _stat)
    {
        currStatus = _stat;
    }

    public void SetType(MonsterType _type)
    {
        MType = _type;
        ExeType();
    }

    public void ExeType()
    {
        gameObject.SetActive(true);

        switch (MType)
        {
            default:
            case MonsterType.Mole:
                GetComponentInChildren<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Monster/Mole/Mole_AnimCon");
                GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Monster/Mole/00");
                GetComponent<CapsuleCollider2D>().excludeLayers = LayerMask.GetMask("Enemy");
                gameObject.layer = 7;
                transform.GetChild(3).localPosition = spriteOffset;
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(true);
                break;
            case MonsterType.Rabbit:
                GetComponentInChildren<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Monster/Rabbit/Rabbit_AnimCon");
                GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Monster/Rabbit/00");
                GetComponent<CapsuleCollider2D>().excludeLayers = LayerMask.GetMask("Friendly", "Enemy", "Player", "Ignore Raycast");
                gameObject.layer = 9;
                transform.GetChild(3).localPosition = Vector2.zero;
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject.name != "Sprite")
                        transform.GetChild(i).gameObject.SetActive(false);
                }
                break;
            case MonsterType.None:
                gameObject.SetActive(false);
                break;
        }

        if (!Mathf.Approximately(transform.parent.localScale.y, 1f))
        {
            //Debug.Log(transform.Find("StepCollider").gameObject);
            transform.Find("StepCollider").gameObject.SetActive(false);
        }
    }
    public void Respawn()
    {
        if (respawnPoint == null || !respawnPoint.gameObject.activeSelf)
            Spawn(initPos);
        else
            Spawn(respawnPoint.position);

        
    }

    void Spawn(Vector2 pos)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = pos;
        rb.simulated = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        animCon.SetBool("isDead", false);

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                BoxCollider2D bc = transform.GetChild(i).gameObject.GetComponent<BoxCollider2D>();
                if (bc != null)
                    bc.enabled = true;
            }
        }
        range.InitRange();
        SetStatus(MStatus.move);
    }

    public void SetBehavior(MBehavior behavior)
    {
        mBehavior = behavior;
    }

    private void StopAnim()
    {
        StopAllCoroutines();
        transform.Find("Sprite").GetComponent<Animator>().enabled = false;
        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        transform.Find("StepCollider").gameObject.SetActive(false);

        //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, LayerMask.GetMask("Default", "Ground"));
        CapsuleCollider2D cc = gameObject.GetComponent<CapsuleCollider2D>();
        RaycastHit2D hit = Physics2D.CapsuleCast(transform.position, cc.size, cc.direction, 0f, Vector2.down, 10f, LayerMask.GetMask("Default", "Ground"));
        if(hit)
        {
            transform.position += Vector3.down * hit.distance;
        }
    }

}
