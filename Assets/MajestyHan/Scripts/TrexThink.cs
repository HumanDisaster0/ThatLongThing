using UnityEngine;
using System.Collections.Generic;
using static TrexMove;
using Unity.VisualScripting;

public class TrexThink : MonoBehaviour
{
    public Transform player;

    [Header("추적 범위")]
    public float chaseRange = 4f;
    public float chaseJumpDistance = 1.5f; // 가까우면 바로 점프

    [Header("점프스캔 범위")]
    public float verticalScanHeight = 6f;
    public int horizontalScanSteps = 6;
    public float scanStepSize = 0.5f;

    [Header("지형타일 레이어 등록")]
    public LayerMask groundMask;

    [Header("끼임 감지")]
    public float stuckCheckDuration = 2f;
    public float stuckMovementThreshold = 0.02f;

    [Header("Y임계지점")]
    public float deathYThreshold = -10f; // 여기보다 아래로 떨어지면 리스폰

    private Vector2 prevPosition;
    private float stuckTimer = 0f;

    private TrexMove move;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D col;

    private Vector2 spawnPoint;

    private float stuckSampleInterval = 0.5f; // 측정 간격
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
        RayAll(); // 벽/바닥/절벽 갱신

        float dist = Vector2.Distance(transform.position, player.position);

        if (!isChaseSuppressed && move.state == TrexMove.MonsterState.Patrol && dist < chaseRange)
        {
            move.ChangeState(TrexMove.MonsterState.Pause);
            Invoke(nameof(EnterChase), 1.0f); // 1초 뒤에 체이스 진입
            return;
        }

        if (move.state == TrexMove.MonsterState.Chase)
        {
            move.SetTargetPosition(player.position);

            float xDist = Mathf.Abs(player.position.x - transform.position.x);
            bool closeEnoughToJump = xDist < chaseJumpDistance;

            if (isGrounded && closeEnoughToJump)
            {
                Debug.Log("[TrexThink] 플레이어가 충분히 가까움 → 바로 점프 시도");
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
                    stuckTimer += stuckSampleTimer; // 누적 시간 추가
                    if (stuckTimer >= stuckCheckDuration)
                    {
                        Debug.Log("[TrexThink] 끼인 것 같음 → 점프 탈출 시도");
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
        // ▶ 벽 검사 (앞쪽 세 점)
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
        // ▶ 바닥 검사 (3개 → 중심 + 양끝)
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

        isGrounded = groundHitCount >= 1;          // 하나라도 맞으면 지상
        isCliffAhead = groundHitCount < 3;         // 하나라도 안 맞으면 절벽
    }


    private void ScanAndJump()
    {
        rb.velocity = Vector2.zero; // 일단 정지
        // 1. 바라보는 방향 계산 (flipX = true면 왼쪽을 보는 중이므로 dir = 1)
        float dir = spriteRenderer.flipX ? 1 : -1;

        // 2. 수평 스캔의 시작 지점 (캐릭터의 콜라이더 바깥에서 약간 떨어진 지점)
        Vector2 horizontalStart = new Vector2(
            transform.position.x + dir * (col.bounds.extents.x + scanStepSize * 0.5f),
            transform.position.y
        );

        // 3. 수직 스캔의 간격 설정 (한 층씩 아래로 얼마나 이동하며 검사할지)
        float stepY = 0.5f;

        // 4. 유효한 착지 지점을 저장할 리스트
        List<Vector2> candidates = new();


        // 5. 진행방향으로 일정 거리만큼 수평 스텝을 이동하며 검사
        for (int i = 0; i < horizontalScanSteps; i++)
        {

            Vector2 basePoint = horizontalStart + Vector2.right * dir * scanStepSize * i;

            // 이 수평 위치에서 위에서 아래 방향으로 검사할 범위 (탑 → 바닥)
            float topY = transform.position.y + verticalScanHeight;
            float bottomY = transform.position.y - verticalScanHeight;

            bool foundGround = false; // 첫 번째 땅 감지 여부
            bool skipping = false;    // 감지 이후 빈 공간 나올 때까지 스킵 중인지 여부

            // 6. 위에서 아래로 일정 간격으로 이동하며 지형을 감지
            for (float y = topY; y >= bottomY; y -= stepY)
            {
                Vector2 checkPoint = new Vector2(basePoint.x, y);

                // 반지름 0.05f의 원으로 검사
                Collider2D hit = Physics2D.OverlapCircle(checkPoint, 0.05f, groundMask);

                // 디버그용 점 찍기 (노란색: 땅 있음 / 회색: 없음)
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

        // 7. 착지 후보 중 가장 플레이어와 높이(y)가 비슷한 지점을 선택
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
            Debug.Log($"[TrexThink] 수직 스텝 스캔 점프 실행 → 착지점: {best}");
        }
        else
        {
            // 8. 유효한 착지 지점이 하나도 없을 경우

            if (move.state == TrexMove.MonsterState.Chase)
            {
                // 추적 중이었으면 포기하고 Patrol 상태로 전환
                move.ChangeState(TrexMove.MonsterState.Patrol);
                move.ClearTargetPosition();
                Debug.Log("[TrexThink] 점프 실패 → 추적 포기하고 Patrol로 복귀");
            }

            // 진행 방향 반대로 전환
            spriteRenderer.flipX = !spriteRenderer.flipX;
            float dirNow = spriteRenderer.flipX ? 1 : -1;
            rb.velocity = new Vector2(dirNow * move.moveSpeed, rb.velocity.y);
            Debug.Log("[TrexThink] 점프 실패 → 방향 반전");
        }
    }


    private void OnDrawGizmos()
    {
        if (col == null) return;

        // 추적 거리 시각화 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        float dir = GetComponent<SpriteRenderer>().flipX ? 1 : -1;
        float stepSize = scanStepSize;
        int steps = horizontalScanSteps;
        float height = verticalScanHeight;

        // ScanAndJump의 실제 스캔 시작 위치와 일치시킴
        Vector2 horizontalStart = new Vector2(
            transform.position.x + dir * (col.bounds.extents.x + stepSize * 0.5f),
            transform.position.y
        );

        Vector2 size = new Vector2(stepSize * steps, height * 2f);
        Vector2 center = horizontalStart + new Vector2((stepSize * (steps - 1) * 0.5f) * dir, 0f);

        // 중심 조정 (위로 올림)
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
            // Invoke 지연 호출 생략 (점프가 끝난 뒤 prevState == Pause → 자연스럽게 처리됨)
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
