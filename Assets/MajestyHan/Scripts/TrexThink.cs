using UnityEngine;

public class TrexThink : MonoBehaviour
{
    public Transform player;           // �÷��̾� ����
    public float chaseRange = 4f;      // ���� ���� ����

    private TrexMove move;             // ����� ����

    void Awake()
    {
        move = GetComponent<TrexMove>();
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
        } else if(move.state == TrexMove.MonsterState.Chase)
        {
            move.SetTargetPosition(player.position);
        }
    }
}
