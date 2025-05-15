using UnityEngine;

public class JumpNode : MonoBehaviour
{
    public JumpNode connectedNode;                 // 연결된 노드
    public bool isHorizontalJump = false;          // 수평 점프인지 여부
    public bool expectsRightApproach = true;       // 오른쪽에서 접근해야만 작동

    public Vector3 GetConnectedPosition() => connectedNode.transform.position;

    private void OnDrawGizmos()
    {
        if (connectedNode != null)
        {
            // 1. 선 색상: 수평이면 노랑, 아니면 파랑
            Gizmos.color = isHorizontalJump ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(transform.position, connectedNode.transform.position);

            // 2. 출발 노드 색
            Gizmos.color = isHorizontalJump ? Color.red : Color.blue;
            Gizmos.DrawSphere(transform.position, 0.15f);

            // 3. 도착 노드 색
            Gizmos.color = isHorizontalJump ? Color.green : Color.gray;
            Gizmos.DrawSphere(connectedNode.transform.position, 0.15f);

            // 4. 방향 화살표 (디버그용)
#if UNITY_EDITOR
            if (isHorizontalJump)
            {
                Vector3 dir = (connectedNode.transform.position - transform.position).normalized;
                Vector3 mid = (connectedNode.transform.position + transform.position) * 0.5f;
                UnityEditor.Handles.color = Color.magenta;
                UnityEditor.Handles.ArrowHandleCap(0, mid, Quaternion.LookRotation(Vector3.forward, dir), 1.0f, EventType.Repaint);
            }
#endif
        }
    }
}
