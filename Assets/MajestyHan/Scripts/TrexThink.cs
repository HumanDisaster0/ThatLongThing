using UnityEngine;
using System.Collections.Generic;
using static TrexMove;
using Unity.VisualScripting;

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

    [Header("���� ����")]
    public float stuckCheckDuration = 2f;
    public float stuckMovementThreshold = 0.02f;

    [Header("Y�Ӱ�����")]
    public float deathYThreshold = -10f; // ���⺸�� �Ʒ��� �������� ������

    private Vector2 prevPosition;
    private float stuckTimer = 0f;

    private TrexMove move;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D col;

    private Vector2 spawnPoint;

    private float stuckSampleInterval = 0.5f; // ���� ����
    private float stuckSampleTimer = 0f;
    private Vector2 lastSampledPosition;

    private bool isWallAhead = false;
    private bool isGrounded = false;
    private bool isCliffAhead = false;
    public bool isChaseSuppressed = false;

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

        if (!isChaseSuppressed && move.state == TrexMove.MonsterState.Patrol && dist < chaseRange)
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

    if (isThinking && isGrounded && (isCliffAhead || isWallAhead) && move.walkAfterLandingTimer <= 0)
    {
        ScanAndJump();
    }
    }

    private void LateUpdate()
    {
        stuckSampleTimer += Time.deltaTime;

        if (stuckSampleTimer >= stuckSampleInterval)
        {
            Vector2 currentPosition = transform.position;
            float velocityX = rb.velocity.x;
            float movedX = Mathf.Abs(currentPosition.x - lastSampledPosition.x);

            if (move.state == TrexMove.MonsterState.Patrol || move.state == TrexMove.MonsterState.Chase)
            {
                if (Mathf.Abs(velocityX) > 0.1f && movedX < stuckMovementThreshold)
                {
                    stuckTimer += stuckSampleTimer; // ���� �ð� �߰�
                    if (stuckTimer >= stuckCheckDuration)
                    {
                        Debug.Log("[TrexThink] ���� �� ���� �� ���� Ż�� �õ�");
                        ForceRespawn(move.state);
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                }
            }

            lastSampledPosition = currentPosition;
            stuckSampleTimer = 0f;
        }
    }


    void ForceRespawn(MonsterState returnState)
    {
        if (move.state == MonsterState.Jumping)
            return;

        move.prevState = returnState;
        move.JumpNow(spawnPoint);
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
                float playerDir = Mathf.Sign(player.position.x - transform.position.x);
                List<Vector2> directionalCandidates = new();

                foreach (var point in candidates)
                {
                    float pointDir = Mathf.Sign(point.x - transform.position.x);
                    if (Mathf.Approximately(pointDir, playerDir))
                        directionalCandidates.Add(point);
                }

                if (directionalCandidates.Count > 0)
                    candidates = directionalCandidates;

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
            else
            {
                best = candidates[Random.Range(0, candidates.Count)];
            }

            move.PrepareJump(best);
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
            float dirNow = spriteRenderer.flipX ? 1 : -1;
            rb.velocity = new Vector2(dirNow * move.moveSpeed, rb.velocity.y);
            Debug.Log("[TrexThink] ���� ���� �� ���� ����");
        }
    }


    private void OnDrawGizmos()
    {
        if (col == null) return;

        // ���� �Ÿ� �ð�ȭ (����� ��)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        float dir = GetComponent<SpriteRenderer>().flipX ? 1 : -1;
        float stepSize = scanStepSize;
        int steps = horizontalScanSteps;
        float height = verticalScanHeight;

        // ScanAndJump�� ���� ��ĵ ���� ��ġ�� ��ġ��Ŵ
        Vector2 horizontalStart = new Vector2(
            transform.position.x + dir * (col.bounds.extents.x + stepSize * 0.5f),
            transform.position.y
        );

        Vector2 size = new Vector2(stepSize * steps, height * 2f);
        Vector2 center = horizontalStart + new Vector2((stepSize * (steps - 1) * 0.5f) * dir, 0f);

        // �߽� ���� (���� �ø�)
        center.y += 0f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }


    public void ActiveTrex()
    {
        gameObject.SetActive(true);
        col.isTrigger = false;
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
        if (move.state == TrexMove.MonsterState.Jumping)
        {
            move.prevState = TrexMove.MonsterState.Pause;
            // Invoke ���� ȣ�� ���� (������ ���� �� prevState == Pause �� �ڿ������� ó����)
            return;
        }

        if (move.state == TrexMove.MonsterState.Pause || move.state == TrexMove.MonsterState.Patrol)
            return;

        move.prevState = TrexMove.MonsterState.Patrol;
        move.ChangeState(TrexMove.MonsterState.Pause);
        Invoke(nameof(ReturnToPatrol), 1.0f);
    }

    public void ChaseSuppressed()
    {
        isChaseSuppressed = true;
    }

    public void ChaseUnsuppressed()
    {
        isChaseSuppressed = false;
    }

    void ReturnToPatrol()
    {
        move.ChangeState(TrexMove.MonsterState.Patrol);
    }
}
