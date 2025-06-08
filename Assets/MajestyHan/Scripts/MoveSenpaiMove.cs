using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoveSenpaiMove : MonoBehaviour
{
    public NPCController npc;
    public PlayerController player;

    //    public void SetJumpInput() << ������Ű�� 1ȸ��
    //    public void SetHorizontalInput(int dir) << -1 ���� / 1 ������ / 0 �߸�

    public enum SenpaiState
    {
        SenpaiIsNowIdle,
        MoveRight,
        MoveLeft,
        SenpaiIsNowMoving
    };

    [Header("���� ���̾�")]
    public LayerMask groundLayer;
    [Header("�˻� ����")]
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
            Debug.LogError("NPC �Ǵ� Player�� ������� �ʾҽ��ϴ�!");
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
        npc.SetHorizontalInput(0); // �߸����� ����
    }
    public void senpaiMoveRight()
    {
        MovingWall.WallActiveFalse();
        npc.SetHorizontalInput(1); // ����������
    }
    public void senpaiMoveLeft()
    {
        MovingWall.WallActiveFalse();
        npc.SetHorizontalInput(-1); // ��������
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

    public void senpaiMoving() // �� �� ����ġ�°� �־��ٲ���
    {
        npc.SetHorizontalInput(1); // ���������� ����
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

        // 3�� �� 2�� �̻� ������ ���������� �Ǵ�
        return groundCount >= 2;
    }


    public bool IsRightWallAhead() // �����ʸ� �˻� << ������ �������� �Ȱ��ϱ� �̷��� ó����
    {
        Vector2 topRayOrigin = new Vector2(col.bounds.max.x, col.bounds.max.y - 0.05f);
        Vector2 bottomRayOrigin = new Vector2(col.bounds.max.x, col.bounds.min.y + 0.05f);

        bool topHit = Physics2D.Raycast(topRayOrigin, Vector2.right, rayLength, groundLayer);
        bool bottomHit = Physics2D.Raycast(bottomRayOrigin, Vector2.right, rayLength, groundLayer);

        Debug.DrawRay(topRayOrigin, Vector2.right * rayLength, Color.blue);
        Debug.DrawRay(bottomRayOrigin, Vector2.right * rayLength, Color.blue);

        return topHit || bottomHit; //���� �����ϴ� ��� ��
    }

    private void Update()
    {
        switch (state) //���ٷ� ������
        {
            case SenpaiState.SenpaiIsNowIdle: senpaiIdle(); break;
            case SenpaiState.MoveRight: senpaiMoveRight(); break;
            case SenpaiState.MoveLeft: senpaiMoveLeft(); break;
            case SenpaiState.SenpaiIsNowMoving: senpaiMoving(); break; //�󸮸���
        }


        bool isGrounded = IsGroundAhead();
        bool isWall = IsRightWallAhead();

        
        if (!senpaiIsNowJumping) //�����̰� �������� �ƴ� ���
        {
            if (!isGrounded || isWall) //�ٴ��� ���ų�, ���� �ִ°��
            {
                npc.SetJumpInput();
                senpaiIsNowJumping = true;
            }
        }

        if (state == SenpaiState.SenpaiIsNowIdle || state == SenpaiState.SenpaiIsNowMoving)
            return; //Idle, �����̵� �󸮸���


        if (isGrounded)
            senpaiIsNowJumping = false; //�����̴� ������ �ѹ��� �ٽŴ�


        float xDistance = Mathf.Abs(senpaiPos.position.x - playerPos.position.x);

        if (xDistance < 1f && !senpaiIsNowJumping)
        {
            state = SenpaiState.SenpaiIsNowIdle; // ���� �Ÿ� ���Ϸ� ���Դµ�, �������� �ƴ϶�� �����·� ��ȯ                        
        }


    }
}
