using System.Collections.Generic;
using UnityEngine;
using static TrexMove;

public class TrexThink : MonoBehaviour
{
    public Transform player;           // �÷��̾� ����
    public float chaseRange = 4f;      // ���� ���� ����
    public float arriveNodeRange = 3f;      //��� ���� ����
    public List<JumpNode> allJumpNodes; // Ƽ��밡 �ν��ϴ� ��� ��� ����Ʈ

    private float yThreshold = 1.0f;     // �÷��̾�� Y���� �Ӱ谪
    private TrexMove move;             // ����� ����
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
            if (goToJump) //���� �������� �̵����ΰ�?
            {
                float diff = Vector2.Distance(transform.position, bestNode.transform.position);

                if (diff < arriveNodeRange) //������
                {
                    move.ChangeState(TrexMove.MonsterState.ReadyToJump);
                    move.SetTargetPosition(bestNode.GetConnectedPosition());
                    goToJump = false;
                }
                return;
            }
            else
                ThinkToChase(); //���� �Ǵ�
        }
    }

    void ThinkToChase()
    {
        if (move.state != TrexMove.MonsterState.Chase)
            return;

        // 1. �÷��̾ ���� ���̶�� ���� �Ǵ� ����
        bool playerIsJumping = (playerCont.GetCurrentState() == PlayerState.Jump
            || playerCont.GetCurrentState() == PlayerState.Fall);
        if (playerIsJumping)
        {
            Debug.Log("[TrexThink] �÷��̾� ���� �� - ���� ����");
            return;
        }

        float trexFootY = trexCol.bounds.min.y;
        float playerFootY = playerCol.bounds.min.y;
        float yDiff = Mathf.Abs(playerFootY - trexFootY);

        // 2. Y�� ���̰� ũ�� �ʴٸ� �׳� ���� ����
        if (yDiff < yThreshold)
        {
            Debug.Log("[TrexThink] Y�� ���� ���� ���� - ���� ����");
            move.SetTargetPosition(player.position);
            return;
        }

        Debug.Log("[TrexThink] Y�� ���� ���� - ���� �Ǵ� ����");

        // Step 1: �÷��̾�� ���� ���� �ִ� ���� ���� ����
        List<JumpNode> candidateDestinations = new List<JumpNode>();
        foreach (var node in allJumpNodes)
        {
            Vector3 destPos = node.GetConnectedPosition();
            if (Mathf.Abs(destPos.y - playerFootY) < yThreshold)
            {
                candidateDestinations.Add(node);
                Debug.Log($"[TrexThink] �ĺ� ������� �߰�: {node.name}");
            }
        }

        if (candidateDestinations.Count == 0)
        {
            Debug.Log("[TrexThink] ���� ������ ��� ���� - Patrol ��ȯ");
            move.ChangeState(TrexMove.MonsterState.Patrol);
            return;
        }

        // Step 2: BFS�� ���� ��带 ���� �� �� �ִ� ��߳�� Ž��
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

            // 2-1. Ƽ��밡 ���� ���� �ִ� ��� �߰�
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

            // 2-2. �ٸ� ��忡�� �� current�� �̾��� �� �ִ��� Ȯ�� (Y�� ���� ���� ���)
            foreach (var node in allJumpNodes)
            {
                float yDelta = Mathf.Abs(node.GetConnectedPosition().y - current.transform.position.y);
                if (yDelta < yThreshold && !cameFrom.ContainsKey(node))
                {
                    queue.Enqueue(node);
                    cameFrom[node] = current;
                    Debug.Log($"[TrexThink] ��ܽ� ���� Ž��: {node.name} �� {current.name}");
                }
            }
        }

        // Step 3: ��� �ݿ�
        if (bestStart != null)
        {
            move.SetTargetPosition(bestStart.transform.position);
            bestNode = bestStart;
            goToJump = true;
            Debug.Log($"[TrexThink] ���� ���� ���� ��ġ: {bestStart.name} ����");
        }
        else
        {
            Debug.Log("[TrexThink] ��� ���� - Patrol ��ȯ");
            move.ChangeState(TrexMove.MonsterState.Patrol);
        }
    }



}
