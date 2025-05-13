using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class TrexMove : MonoBehaviour
{
    public enum MonsterState { Idle, Chase, Kill, Pause, Patrol, Evaluate, PreJump, Jumping }
    public MonsterState state = MonsterState.Idle;

    [Header("AI Settings")]
    public float moveSpeed = 2f;
    public float pauseDuration = 2f;
    public float yJumpThreshold = 1.0f;
    public float preJumpDuration = 1.0f; // 점프 전 대기시간
    public float apexHeight = 3f;        // 포물선 점프 정점 높이 (조정 가능)

    [Header("Platform Settings")]
    public Tilemap groundTilemap;
    public Transform player;
    public GameObject movingPlatform;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;

    private List<Vector3> jumpTargets = new List<Vector3>();
    private float pauseTimer = 0f;
    private float preJumpTimer = 0f;
    private Vector3 evaluatedPlayerPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        CacheJumpableTiles();
    }

    void Update()
    {
        switch (state)
        {
            case MonsterState.Idle:
                break;

            case MonsterState.Chase:
                ChasePlayer();
                float yDiff = Mathf.Abs(player.position.y - transform.position.y);
                bool isPlayerJumping = player.GetComponent<PlayerController>().GetCurrentState() == PlayerState.Jump;

                if (yDiff >= yJumpThreshold && !isPlayerJumping)
                {
                    evaluatedPlayerPosition = GetPlayerGroundPosition(); // 바닥 위치 기억
                    rb.velocity = Vector2.zero;
                    state = MonsterState.PreJump;
                    preJumpTimer = 0f;
                }
                break;

            case MonsterState.PreJump:
                preJumpTimer += Time.deltaTime;
                if (preJumpTimer >= preJumpDuration)
                {
                    DoParabolicJump(evaluatedPlayerPosition);
                }
                break;

            case MonsterState.Jumping:
                // 추가 처리 없음 (착지는 OnCollisionEnter2D 처리)
                break;

            case MonsterState.Kill:
                // TODO: 플레이어 사망 처리
                state = MonsterState.Idle;
                gameObject.SetActive(false);
                break;

            case MonsterState.Pause:
                pauseTimer += Time.deltaTime;
                if (pauseTimer >= pauseDuration)
                {
                    pauseTimer = 0f;
                    state = MonsterState.Patrol;
                }
                break;

            case MonsterState.Patrol:
                Patrol();
                break;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    void Patrol()
    {
        rb.velocity = new Vector2(Mathf.Sin(Time.time) * moveSpeed, rb.velocity.y);
    }

    void CacheJumpableTiles()
    {
        Vector3Int origin = groundTilemap.WorldToCell(transform.position);
        for (int dx = -10; dx <= 10; dx++)
        {
            for (int dy = 1; dy <= 6; dy++)
            {
                Vector3Int checkCell = new Vector3Int(origin.x + dx, origin.y + dy, 0);
                if (groundTilemap.HasTile(checkCell))
                {
                    Vector3 worldPos = groundTilemap.CellToWorld(checkCell) + new Vector3(0.5f, 0.5f);
                    if (movingPlatform != null && Vector3.Distance(worldPos, movingPlatform.transform.position) < 0.5f)
                        continue;
                    jumpTargets.Add(worldPos);
                }
            }
        }
    }

    Vector3 GetPlayerGroundPosition()
    {
        RaycastHit2D hit = Physics2D.Raycast(player.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            return hit.point + Vector2.up * 0.1f;
        }
        return player.position;
    }

    void DoParabolicJump(Vector2 targetPosition)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float deltaY = targetPosition.y - transform.position.y;
        float deltaX = targetPosition.x - transform.position.x;

        float velocityY = Mathf.Sqrt(2 * gravity * apexHeight);
        float timeUp = velocityY / gravity;
        float timeDown = Mathf.Sqrt(2 * (apexHeight - deltaY) / gravity);
        float totalTime = timeUp + timeDown;

        float velocityX = deltaX / totalTime;

        rb.velocity = new Vector2(velocityX, velocityY);
        col.enabled = false;
        state = MonsterState.Jumping;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == MonsterState.Jumping && collision.collider.CompareTag("Ground"))
        {
            col.enabled = true;
            state = MonsterState.Chase;
        }
    }

    public void ActivateChase()
    {
        if (state == MonsterState.Idle)
        {
            gameObject.SetActive(true);
            state = MonsterState.Chase;
        }
    }

    public void DeactivateChase()
    {
        if (state == MonsterState.Chase)
        {
            state = MonsterState.Pause;
            rb.velocity = Vector2.zero;
        }
    }
}
