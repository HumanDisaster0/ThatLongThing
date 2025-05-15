using System.Collections.Generic;
using UnityEngine;
using static TrexMove;

public class TrexThink : MonoBehaviour
{
    public Transform player;           // 플레이어 참조
    public float chaseRange = 4f;      // 추적 시작 범위
    public float arriveNodeRange = 3f;      //노드 도착 판정
    public List<JumpNode> allJumpNodes; // 티라노가 인식하는 모든 노드 리스트

    private float yThreshold = 1.0f;     // 플레이어와 Y차이 임계값
    private TrexMove move;             // 실행기 참조
    private bool goToJump = false;
    private JumpNode bestNode = null;
    private BoxCollider2D trexCol;
    private BoxCollider2D playerCol;
    private PlayerController playerCont;

    void Awake()
    {
        move = GetComponent<TrexMove>();
        trexCol = move.GetComponent<BoxCollider2D>();
        playerCol = player.GetComponent<BoxCollider2D>();
        playerCont = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (move.state == TrexMove.MonsterState.Patrol)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= chaseRange)
            {
                move.SetTargetPosition(player.position);
                move.ChangeState(TrexMove.MonsterState.Chase);
            }
        }
        else if (move.state == TrexMove.MonsterState.Chase)
        {
            if (goToJump) //점프 지점으로 이동중인가?
            {
                float diff = Vector2.Distance(transform.position, bestNode.transform.position);

                if (diff < arriveNodeRange) //도착함
                {
                    move.ChangeState(TrexMove.MonsterState.ReadyToJump);
                    move.SetTargetPosition(bestNode.GetConnectedPosition());
                    goToJump = false;
                }
                return;
            }
            else
                ThinkToChase(); //추적 판단
        }
    }

    void ThinkToChase()
    {
        if (move.state != TrexMove.MonsterState.Chase)
            return;

        // 1. 플레이어가 점프 중이라면 추적 판단 중지
        bool playerIsJumping = (playerCont.GetCurrentState() == PlayerState.Jump
            || playerCont.GetCurrentState() == PlayerState.Fall);
        if (playerIsJumping)
        {
            Debug.Log("[TrexThink] 플레이어 점프 중 - 추적 보류");
            return;
        }

        float trexFootY = trexCol.bounds.min.y;
        float playerFootY = playerCol.bounds.min.y;
        float yDiff = Mathf.Abs(playerFootY - trexFootY);

        // 2. Y축 차이가 크지 않다면 그냥 직접 추적
        if (yDiff < yThreshold)
        {
            Debug.Log("[TrexThink] Y축 차이 무시 가능 - 직접 추적");
            move.SetTargetPosition(player.position);
            return;
        }

        Debug.Log("[TrexThink] Y축 차이 감지 - 점프 판단 시작");

        // Step 1: 플레이어와 같은 층에 있는 도착 노드들 수집
        List<JumpNode> candidateDestinations = new List<JumpNode>();
        foreach (var node in allJumpNodes)
        {
            Vector3 destPos = node.GetConnectedPosition();
            if (Mathf.Abs(destPos.y - playerFootY) < yThreshold)
            {
                candidateDestinations.Add(node);
                Debug.Log($"[TrexThink] 후보 도착노드 발견: {node.name}");
            }
        }

        if (candidateDestinations.Count == 0)
        {
            Debug.Log("[TrexThink] 도착 가능한 노드 없음 - Patrol 전환");
            move.ChangeState(TrexMove.MonsterState.Patrol);
            return;
        }

        // Step 2: BFS로 경유 노드를 통해 갈 수 있는 출발노드 탐색
        Queue<JumpNode> queue = new Queue<JumpNode>();
        Dictionary<JumpNode, JumpNode> cameFrom = new Dictionary<JumpNode, JumpNode>();

        foreach (var dest in candidateDestinations)
        {
            queue.Enqueue(dest);
            cameFrom[dest] = null;
        }

        JumpNode bestStart = null;
        float bestDist = Mathf.Infinity;

        while (queue.Count > 0)
        {
            JumpNode current = queue.Dequeue();
            Vector3 startPos = current.transform.position;

            // 2-1. 티라노가 같은 층에 있는 노드 발견
            if (Mathf.Abs(startPos.y - trexFootY) < yThreshold)
            {
                float dist = Vector2.Distance(startPos, transform.position);
                if (dist < bestDist)
                {
                    bestStart = current;
                    bestDist = dist;
                }
                continue;
            }

            // 2-2. 다른 노드에서 이 current로 이어질 수 있는지 확인 (Y값 기준 연결 허용)
            foreach (var node in allJumpNodes)
            {
                float yDelta = Mathf.Abs(node.GetConnectedPosition().y - current.transform.position.y);
                if (yDelta < yThreshold && !cameFrom.ContainsKey(node))
                {
                    queue.Enqueue(node);
                    cameFrom[node] = current;
                    Debug.Log($"[TrexThink] 계단식 연결 탐지: {node.name} → {current.name}");
                }
            }
        }

        // Step 3: 결과 반영
        if (bestStart != null)
        {
            move.SetTargetPosition(bestStart.transform.position);
            bestNode = bestStart;
            goToJump = true;
            Debug.Log($"[TrexThink] 최종 점프 시작 위치: {bestStart.name} 선택");
        }
        else
        {
            Debug.Log("[TrexThink] 경로 없음 - Patrol 전환");
            move.ChangeState(TrexMove.MonsterState.Patrol);
        }
    }



}
