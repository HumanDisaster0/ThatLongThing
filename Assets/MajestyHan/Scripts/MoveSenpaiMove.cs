using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoveSenpaiMove : MonoBehaviour
{
    public NPCController npc;
    public PlayerController player;

    //    public void SetJumpInput() << 점프시키기 1회성
    //    public void SetHorizontalInput(int dir) << -1 왼쪽 / 1 오른쪽 / 0 중립

    public enum SenpaiState
    {
        SenpaiIsNowIdle,
        MoveRight,
        MoveLeft,
        SenpaiIsNowMoving
    };

    [Header("지상 레이어")]
    public LayerMask groundLayer;
    [Header("검사 범위")]
    public float rayLength = 0.2f;

    private bool senpaiIsNowJumping = false;
    private bool senpaiIsCanMoving = false;
    private BoxCollider2D col;

    private Transform senpaiPos;
    private Transform playerPos;

    private MovingWall MovingWall;

    SenpaiState state;

    private void Awake()
    {
        if (npc == null || player == null)
        {
            Debug.LogError("NPC 또는 Player가 연결되지 않았습니다!");
            enabled = false;
            return;
        }
        col = npc.GetComponent<BoxCollider2D>();
        senpaiPos = npc.GetComponent<Transform>();
        playerPos = player.GetComponent<Transform>();
        MovingWall = GetComponent<MovingWall>();

        senpaiIsNowJumping = false;
    }

    public void ChangeSenpaiState(SenpaiState newState)
    {
        state = newState;
    }

    public void senpaiIdle()
    {
        npc.SetHorizontalInput(0); // 중립상태 유지
    }
    public void senpaiMoveRight()
    {
        MovingWall.WallActiveFalse();
        npc.SetHorizontalInput(1); // 오른쪽으로
    }
    public void senpaiMoveLeft()
    {
        MovingWall.WallActiveFalse();
        npc.SetHorizontalInput(-1); // 왼쪽으로
    }

    public void senpaiNowGoLeftWillReturn()
    {
        state = SenpaiState.MoveLeft;
        Invoke("senpaiPlzStop", 1.0f);
    }

    public void senpaiNowGoRight()
    {
        state = SenpaiState.MoveRight;
    }

    public void senpaiPlzStop()
    {
        state = SenpaiState.SenpaiIsNowIdle;
    }

    public void senpaiMoving() // 좌 우 무빙치는거 넣어줄꺼임
    {
        npc.SetHorizontalInput(1); // 오른쪽으로 진격
    }

    public bool IsGroundAhead()
    {
        float centerX = col.bounds.center.x;
        float bottomY = col.bounds.min.y;

        Vector2 leftRayOrigin = new Vector2(col.bounds.min.x, bottomY);
        Vector2 centerRayOrigin = new Vector2(centerX, bottomY);
        Vector2 rightRayOrigin = new Vector2(col.bounds.max.x, bottomY);

        bool leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, rayLength, groundLayer);
        bool centerHit = Physics2D.Raycast(centerRayOrigin, Vector2.down, rayLength, groundLayer);
        bool rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, rayLength, groundLayer);

        Debug.DrawRay(leftRayOrigin, Vector2.down * rayLength, Color.red);
        Debug.DrawRay(centerRayOrigin, Vector2.down * rayLength, Color.red);
        Debug.DrawRay(rightRayOrigin, Vector2.down * rayLength, Color.red);

        int groundCount = 0;
        if (leftHit) groundCount++;
        if (centerHit) groundCount++;
        if (rightHit) groundCount++;

        // 3개 중 2개 이상 없으면 낭떠러지로 판단
        return groundCount >= 2;
    }


    public bool IsRightWallAhead() // 오른쪽만 검사 << 어차피 왼쪽으로 안가니까 이렇게 처리함
    {
        Vector2 topRayOrigin = new Vector2(col.bounds.max.x, col.bounds.max.y - 0.05f);
        Vector2 bottomRayOrigin = new Vector2(col.bounds.max.x, col.bounds.min.y + 0.05f);

        bool topHit = Physics2D.Raycast(topRayOrigin, Vector2.right, rayLength, groundLayer);
        bool bottomHit = Physics2D.Raycast(bottomRayOrigin, Vector2.right, rayLength, groundLayer);

        Debug.DrawRay(topRayOrigin, Vector2.right * rayLength, Color.blue);
        Debug.DrawRay(bottomRayOrigin, Vector2.right * rayLength, Color.blue);

        return topHit || bottomHit; //벽이 존재하는 경우 참
    }

    private void Update()
    {
        switch (state) //간바레 센빠이
        {
            case SenpaiState.SenpaiIsNowIdle: senpaiIdle(); break;
            case SenpaiState.MoveRight: senpaiMoveRight(); break;
            case SenpaiState.MoveLeft: senpaiMoveLeft(); break;
            case SenpaiState.SenpaiIsNowMoving: senpaiMoving(); break; //얼리리턴
        }


        bool isGrounded = IsGroundAhead();
        bool isWall = IsRightWallAhead();

        
        if (!senpaiIsNowJumping) //센빠이가 점프중이 아닌 경우
        {
            if (!isGrounded || isWall) //바닥이 없거나, 벽이 있는경우
            {
                npc.SetJumpInput();
                senpaiIsNowJumping = true;
            }
        }

        if (state == SenpaiState.SenpaiIsNowIdle || state == SenpaiState.SenpaiIsNowMoving)
            return; //Idle, 강제이동 얼리리턴


        if (isGrounded)
            senpaiIsNowJumping = false; //센빠이는 점프를 한번만 뛰신다


        float xDistance = Mathf.Abs(senpaiPos.position.x - playerPos.position.x);

        if (xDistance < 1f && !senpaiIsNowJumping)
        {
            state = SenpaiState.SenpaiIsNowIdle; // 일정 거리 이하로 들어왔는데, 점프중이 아니라면 대기상태로 전환                        
        }


    }
}
