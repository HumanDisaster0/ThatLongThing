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
    private SpriteRenderer spriteRenderer;
    private AnimatorController animCon;
    private MRange range;
    private MStatus currStatus = MStatus.idle;
    private MStatus prevStatus = MStatus.end;

    private float idleTime = 0f;
    private float destX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        range = GetComponentInParent<MRange>();
        animCon = GetComponent<AnimatorController>();
    }

    void FixedUpdate()
    {
        CheckStatus();

        //timer += Time.fixedDeltaTime;

        //if (timer >= patrolDuration)
        //{
        //    direction *= -1;
        //    timer = 0f;

        //    spriteRenderer.flipX = (direction < 0);
        //}

        //rb.velocity = new Vector2(moveSpeed * direction, rb.velocity.y);
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

                    break;
                case MStatus.move:
                    MoveFunc();
                    break;
                case MStatus.die:
                    DieFunc();
                    break;
            }
        }
    }

    private void MoveFunc()
    {
        throw new NotImplementedException();
    }

    private void DieFunc()
    {
        throw new NotImplementedException();
    }
}
