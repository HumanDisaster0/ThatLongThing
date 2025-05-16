using UnityEngine;
using System.Collections.Generic;

public class TrexThink : MonoBehaviour
{
    public Transform player;

    [Header("���� ����")]
    public float chaseRange = 4f;
    public float chaseJumpDistance = 1.5f; // ������ �ٷ� ����

    [Header("������ĵ ����")]
    public float verticalScanHeight = 6f;
    public int horizontalScanSteps = 6;
    public float scanStepSize = 0.5f;
    
    [Header("����Ÿ�� ���̾� ���")]
    public LayerMask groundMask;

    private TrexMove move;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D col;

    private Vector2 spawnPoint;

    private bool isWallAhead = false;
    private bool isGrounded = false;
    private bool isCliffAhead = false;

    private void Awake()
    {
        move = GetComponent<TrexMove>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPoint = transform.position;
        DeactiveTrex();
    }

    private void Update()
    {
        RayAll(); // ��/�ٴ�/���� ����


        float dist = Vector2.Distance(transform.position, player.position);

        if (move.state == TrexMove.MonsterState.Patrol && dist < chaseRange)
        {
            move.ChangeState(TrexMove.MonsterState.Pause);
            Invoke(nameof(EnterChase), 1.0f); // 1�� �ڿ� ü�̽� ����
            return;
        }

        if (move.state == TrexMove.MonsterState.Chase)
        {
            move.SetTargetPosition(player.position);

            float xDist = Mathf.Abs(player.position.x - transform.position.x);
            bool closeEnoughToJump = xDist < chaseJumpDistance;

            if (isGrounded && closeEnoughToJump)
            {
                Debug.Log("[TrexThink] �÷��̾ ����� ����� �� �ٷ� ���� �õ�");
                move.JumpNow(player.position);
                return;
            }
        }

        bool isThinking = move.state == TrexMove.MonsterState.Patrol || move.state == TrexMove.MonsterState.Chase;

        if (isThinking && isGrounded && (isCliffAhead || isWallAhead))
        {
            ScanAndJump();
        }
    }


    void EnterChase()
    {
        move.SetTargetPosition(player.position);
        move.ChangeState(TrexMove.MonsterState.Chase);
    }

    private void RayAll()
    {
        Bounds bounds = col.bounds;
        float rayLength = 0.2f;
        float dir = spriteRenderer.flipX ? 1 : -1;

        // ==============================
        // �� �� �˻� (���� �� ��)
        // ==============================
        Vector2 top = new Vector2(bounds.center.x + dir * bounds.extents.x, bounds.max.y - 0.05f);
        Vector2 middle = new Vector2(bounds.center.x + dir * bounds.extents.x, bounds.center.y);
        Vector2 bottom = new Vector2(bounds.center.x + dir * bounds.extents.x, bounds.min.y + 0.05f);

        RaycastHit2D hitTop = Physics2D.Raycast(top, Vector2.right * dir, rayLength, groundMask);
        RaycastHit2D hitMiddle = Physics2D.Raycast(middle, Vector2.right * dir, rayLength, groundMask);
        RaycastHit2D hitBottom = Physics2D.Raycast(bottom, Vector2.right * dir, rayLength, groundMask);

        Debug.DrawRay(top, Vector2.right * dir * rayLength, Color.blue);
        Debug.DrawRay(middle, Vector2.right * dir * rayLength, Color.blue);
        Debug.DrawRay(bottom, Vector2.right * dir * rayLength, Color.blue);

        isWallAhead = hitTop.collider != null || hitMiddle.collider != null || hitBottom.collider != null;

        // ==============================
        // �� �ٴ� �˻� (3�� �� �߽� + �糡)
        // ==============================
        float centerX = bounds.center.x;
        float leftX = bounds.min.x + 0.05f;
        float rightX = bounds.max.x - 0.05f;
        float y = bounds.min.y + 0.01f;

        Vector2 left = new Vector2(leftX, y);
        Vector2 center = new Vector2(centerX, y);
        Vector2 right = new Vector2(rightX, y);

        RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, rayLength, groundMask);
        RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, rayLength, groundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, rayLength, groundMask);

        Debug.DrawRay(left, Vector2.down * rayLength, Color.green);
        Debug.DrawRay(center, Vector2.down * rayLength, Color.green);
        Debug.DrawRay(right, Vector2.down * rayLength, Color.green);

        int groundHitCount = 0;
        if (hitLeft.collider != null) groundHitCount++;
        if (hitCenter.collider != null) groundHitCount++;
        if (hitRight.collider != null) groundHitCount++;

        isGrounded = groundHitCount >= 1;          // �ϳ��� ������ ����
        isCliffAhead = groundHitCount < 3;         // �ϳ��� �� ������ ����
    }


    private void ScanAndJump()
    {
        rb.velocity = Vector2.zero; // �ϴ� ����
        // 1. �ٶ󺸴� ���� ��� (flipX = true�� ������ ���� ���̹Ƿ� dir = 1)
        float dir = spriteRenderer.flipX ? 1 : -1;

        // 2. ���� ��ĵ�� ���� ���� (ĳ������ �ݶ��̴� �ٱ����� �ణ ������ ����)
        Vector2 horizontalStart = new Vector2(
            transform.position.x + dir * (col.bounds.extents.x + scanStepSize * 0.5f),
            transform.position.y
        );

        // 3. ���� ��ĵ�� ���� ���� (�� ���� �Ʒ��� �󸶳� �̵��ϸ� �˻�����)
        float stepY = 0.5f;

        // 4. ��ȿ�� ���� ������ ������ ����Ʈ
        List<Vector2> candidates = new();


        // 5. ����������� ���� �Ÿ���ŭ ���� ������ �̵��ϸ� �˻�
        for (int i = 0; i < horizontalScanSteps; i++)
        {
            
            Vector2 basePoint = horizontalStart + Vector2.right * dir * scanStepSize * i;

            // �� ���� ��ġ���� ������ �Ʒ� �������� �˻��� ���� (ž �� �ٴ�)
            float topY = transform.position.y + verticalScanHeight;
            float bottomY = transform.position.y - verticalScanHeight;

            bool foundGround = false; // ù ��° �� ���� ����
            bool skipping = false;    // ���� ���� �� ���� ���� ������ ��ŵ ������ ����

            // 6. ������ �Ʒ��� ���� �������� �̵��ϸ� ������ ����
            for (float y = topY; y >= bottomY; y -= stepY)
            {
                Vector2 checkPoint = new Vector2(basePoint.x, y);

                // ������ 0.05f�� ������ �˻�
                Collider2D hit = Physics2D.OverlapCircle(checkPoint, 0.05f, groundMask);

                // ����׿� �� ��� (�����: �� ���� / ȸ��: ����)
                Debug.DrawRay(checkPoint, Vector2.right * 0.1f, hit ? Color.yellow : Color.gray, 0.5f);

                if (!foundGround && hit != null)
                {
                    candidates.Add(new Vector2(checkPoint.x, checkPoint.y + stepY));

                    foundGround = true;
                    skipping = true;
                    y -= stepY;
                    continue;
                }

                if (skipping && hit == null)
                {
                    skipping = false;
                }
            }

        }

        // 7. ���� �ĺ� �� ���� �÷��̾�� ����(y)�� ����� ������ ����
        if (candidates.Count > 0)
        {

            Vector2 best;

            if (move.state == TrexMove.MonsterState.Chase)
            {
                // �÷��̾�� ���� ����� �ĺ� ���� (X+Y �Ÿ� ����)
                float bestScore = float.MaxValue;
                best = candidates[0];

                foreach (var point in candidates)
                {
                    float xDiff = Mathf.Abs(point.x - player.position.x);
                    float yDiff = Mathf.Abs(point.y - player.position.y);
                    float score = xDiff + yDiff;

                    if (score < bestScore)
                    {
                        best = point;
                        bestScore = score;
                    }
                }
            }
            else // Patrol ���� ��
            {
                // �����ϰ� �ϳ� ����
                best = candidates[Random.Range(0, candidates.Count)];
            }



            // ������ ���� �������� ���� ����
            move.PrepareJump(best); // �� �غ� ���º��� ����
            Debug.Log($"[TrexThink] ���� ���� ��ĵ ���� ���� �� ������: {best}");
        }
        else
        {
            // 8. ��ȿ�� ���� ������ �ϳ��� ���� ���

            if (move.state == TrexMove.MonsterState.Chase)
            {
                // ���� ���̾����� �����ϰ� Patrol ���·� ��ȯ
                move.ChangeState(TrexMove.MonsterState.Patrol);
                move.ClearTargetPosition();
                Debug.Log("[TrexThink] ���� ���� �� ���� �����ϰ� Patrol�� ����");
            }

            // ���� ���� �ݴ�� ��ȯ
            spriteRenderer.flipX = !spriteRenderer.flipX;
            Debug.Log("[TrexThink] ���� ���� �� ���� ����");
        }
    }


    private void OnDrawGizmos()
    {
        if (col == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        float dir = GetComponent<SpriteRenderer>().flipX ? 1 : -1;
        float stepSize = scanStepSize;
        int steps = horizontalScanSteps;
        float height = verticalScanHeight;

        Vector2 start = new Vector2(
            transform.position.x + dir * (col.bounds.extents.x + stepSize * 0.5f),
            transform.position.y + height
        );

        Vector2 size = new Vector2(stepSize * steps, height * 2f);
        Vector2 center = start - new Vector2(-size.x * 0.5f, size.y * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }

    public void ActiveTrex()
    {
        gameObject.SetActive(true);
        move.SetTargetPosition(player.position);
        move.ChangeState(TrexMove.MonsterState.Chase);
        move.ClearTimer();
    }

    public void DeactiveTrex()
    {
        move.transform.position = spawnPoint;
        gameObject.SetActive(false);
    }

    public void DechasedTrex()
    {
        //move.ClearTargetPosition(); << ���ص� ��
        move.ChangeState(TrexMove.MonsterState.Pause);
        Invoke(nameof(ReturnToPatrol), 1.0f);
        move.ClearTimer();
    }

    void ReturnToPatrol()
    {
        move.ChangeState(TrexMove.MonsterState.Patrol);
    }
}
