using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Animations;
using UnityEngine;

public enum MStatus
{
    idle = 0,
    move,
    die,
    end
}

public enum MonsterType
{
    Mole = 0,
    Rabbit
}

public class MMove : MonoBehaviour
{
    #region Serialized Private Member
    [Header("몬스터옵션")]
    [SerializeField] MonsterType MType = 0;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float maxidleDuration = 4f;
    [SerializeField] float minMoveLength = 1f;
    [SerializeField] float maxMoveLength = 3f;
    [SerializeField] bool respawn = true;
    [SerializeField] int respawnPointIndex = 0;
    #endregion

    #region Private Member
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animCon;
    private MRange range;
    private MStatus currStatus = MStatus.idle;
    private MStatus prevStatus = MStatus.end;
    private GameManager manager;

    private float idleTime = 0f;
    private float destX = 0f;
    private float timer = 0f;
    #endregion

    private void OnValidate()
    {
        switch (MType)
        {
            default:
            case MonsterType.Mole:
                GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Monster/Mole/Mole_AnimCon");
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Monster/Mole/00");
                GetComponent<CapsuleCollider2D>().excludeLayers = LayerMask.GetMask("Enemy");
                for(int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(true);
                break;
            case MonsterType.Rabbit:
                GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Monster/Rabbit/Rabbit_AnimCon");
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Monster/Rabbit/00");
                GetComponent<CapsuleCollider2D>().excludeLayers = LayerMask.GetMask("Enemy", "Player");
                for (int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(false);
                break;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        range = GetComponentInParent<MRange>();
        animCon = GetComponent<Animator>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void FixedUpdate()
    {
        CheckStatus();
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        switch(currStatus)
        {
            case MStatus.idle:
                timer += Time.fixedDeltaTime;
                if (timer > idleTime)
                {
                    timer = 0f;
                    currStatus = MStatus.move;
                }
                break;
            case MStatus.move:
                float dir = transform.position.x < destX ? 1f : -1f;
                rb.velocity = new Vector2(dir * moveSpeed, rb.velocity.y);

                if (Mathf.Abs(transform.position.x - destX) < 0.1f)
                    currStatus = MStatus.idle;
                break;
            case MStatus.die:
                break;
            default:
                currStatus = MStatus.idle;
                break;
        }
    }

    private void CheckStatus()
    {
        if(currStatus != prevStatus)
        {
            switch (currStatus)
            {
                case MStatus.idle:
                    idleTime = UnityEngine.Random.Range(maxidleDuration / 2f, maxidleDuration);
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                    animCon.SetBool("isWalking" , false);
                    break;
                case MStatus.move:
                    do
                    {
                        destX = range.GetRandomPosX();
                        //Debug.Log("움직임 길이 : " + MathF.Abs(transform.position.x - destX));
                    }
                    while (MathF.Abs(transform.position.x - destX) <= minMoveLength || MathF.Abs(transform.position.x - destX) >= maxMoveLength);
                    float dir = transform.position.x < destX ? 1f : -1f;
                    FlipSprite(dir > 0f ? true : false);
                    animCon.SetBool("isWalking", true);
                    break;
                case MStatus.die:
                    rb.velocity = Vector2.zero;
                    rb.simulated = false;
                    GetComponent<CapsuleCollider2D>().enabled = false;
                    animCon.SetBool("isDead", true);
                    StartCoroutine(SetActivision(3.0f));
                    break;
                default:
                    currStatus = MStatus.idle;
                    break;
            }
            prevStatus = currStatus;
        }
    }

    IEnumerator SetActivision(float _time)
    {
        yield return new WaitForSeconds(_time);
        
        manager.MonsterRespawn(this.gameObject, respawnPointIndex);
        if(respawn)
            Respawn();
    }

    void FlipSprite(bool flag)
    {
        if(flag != sr.flipX)
        {
            Transform ct = transform.GetChild(1);
            ct.localScale = new Vector3(ct.localScale.x * -1f, ct.localScale.y, ct.localScale.z);
        }

        sr.flipX = flag;
    }
    public void SetStatus(MStatus _stat)
    {
        currStatus = _stat;
    }

    public void Respawn()
    {
        gameObject.SetActive(true);
        rb.simulated = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        animCon.SetBool("isDead", false);

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.GetComponent<BoxCollider2D>().enabled = true;

        SetStatus(MStatus.move);
    }
}
