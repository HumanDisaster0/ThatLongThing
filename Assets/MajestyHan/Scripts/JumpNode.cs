using UnityEngine;

public class JumpNode : MonoBehaviour
{
    public JumpNode connectedNode;         // 연결된 점프 노드
    public bool isHorizontalJump = false;  // 수평 점프인지 여부

    public enum JumpDirection
    {
        Any,
        LeftOnly,
        RightOnly
    }

    [Header("접근 방향 제한")]
    public JumpDirection allowedApproach = JumpDirection.Any;

    public Vector3 GetConnectedPosition() => connectedNode.transform.position;

    private void OnValidate()
    {
        if (connectedNode == null) return;

        if (isHorizontalJump) // 수직 점프일 경우
        {
            float diff = connectedNode.transform.position.x - transform.position.x;
            allowedApproach = diff > 0 ? JumpDirection.LeftOnly :
                              diff < 0 ? JumpDirection.RightOnly : JumpDirection.Any;
        }
        else
        {
            allowedApproach = JumpDirection.Any;
        }
    }



    private void OnDrawGizmos()
    {
        if (connectedNode != null)
        {
            // 선 색상: 수평 노드는 노랑, 포물선은 파랑
            Gizmos.color = isHorizontalJump ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(transform.position, connectedNode.transform.position);

            // 출발 노드 색
            Gizmos.color = isHorizontalJump ? Color.red : Color.blue;
            Gizmos.DrawSphere(transform.position, 0.15f);

            // 도착 노드 색
            Gizmos.color = isHorizontalJump ? Color.green : Color.gray;
            Gizmos.DrawSphere(connectedNode.transform.position, 0.15f);

#if UNITY_EDITOR
            // 화살표 표시
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
