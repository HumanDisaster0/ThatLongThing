using System;
using System.Collections;
using System.Collections.Generic;
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
    [Header("몬스터옵션")]
    [SerializeField] MonsterType type = 0;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float maxidleDuration = 4f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animCon;
    private MRange range;
    [SerializeField] private MStatus currStatus = MStatus.idle;
    [SerializeField] private MStatus prevStatus = MStatus.end;

    [SerializeField] private float idleTime = 0f;
    [SerializeField] private float destX = 0f;
    [SerializeField] private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        range = GetComponentInParent<MRange>();
        animCon = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        CheckStatus();
        MoveEnemy();
        //timer += Time.fixedDeltaTime;

        //if (timer >= patrolDuration)
        //{
        //    direction *= -1;
        //    timer = 0f;

        //    spriteRenderer.flipX = (direction < 0);
        //}

        //rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
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
                    idleTime = UnityEngine.Random.Range(maxidleDuration / 2, maxidleDuration);
                    rb.velocity = Vector2.zero;
                    animCon.SetBool("isWalking" , false);
                    break;
                case MStatus.move:
                    destX = range.GetRandomPosX();
                    float dir = transform.position.x < destX ? 1f : -1f;
                    FlipSprite(dir > 0f ? true : false);
                    animCon.SetBool("isWalking", true);
                    break;
                case MStatus.die:
                    rb.velocity = Vector2.zero;
                    animCon.SetBool("isDead", true);
                    break;
                default:
                    currStatus = MStatus.idle;
                    break;
            }
            prevStatus = currStatus;
        }
    }

    void FlipSprite(bool flag)
    {
        sr.flipX = flag;
        Transform ct = transform.GetChild(1);
        ct.localScale = new Vector3(ct.localScale.x * -1f, ct.localScale.y, ct.localScale.z);
    }
    public void SetStatus(MStatus _stat)
    {
        currStatus = _stat;
        GetComponent<CapsuleCollider2D>().enabled = false;
        Destroy(this, 3.0f);
    }
}
