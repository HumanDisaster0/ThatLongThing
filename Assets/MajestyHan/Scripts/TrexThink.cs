using UnityEngine;

public class TrexThink : MonoBehaviour
{
    public Transform player;           // 플레이어 참조
    public float chaseRange = 4f;      // 추적 시작 범위

    private TrexMove move;             // 실행기 참조

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
