using UnityEngine;

public class JumpNode : MonoBehaviour
{
    public JumpNode connectedNode;         // ����� ���� ���
    public bool isHorizontalJump = false;  // ���� �������� ����

    public enum JumpDirection
    {
        Any,
        LeftOnly,
        RightOnly
    }

    [Header("���� ���� ����")]
    public JumpDirection allowedApproach = JumpDirection.Any;

    public Vector3 GetConnectedPosition() => connectedNode.transform.position;

    private void OnValidate()
    {
        if (connectedNode == null) return;

        if (isHorizontalJump) // ���� ������ ���
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
            // �� ����: ���� ���� ���, �������� �Ķ�
            Gizmos.color = isHorizontalJump ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(transform.position, connectedNode.transform.position);

            // ��� ��� ��
            Gizmos.color = isHorizontalJump ? Color.red : Color.blue;
            Gizmos.DrawSphere(transform.position, 0.15f);

            // ���� ��� ��
            Gizmos.color = isHorizontalJump ? Color.green : Color.gray;
            Gizmos.DrawSphere(connectedNode.transform.position, 0.15f);

#if UNITY_EDITOR
            // ȭ��ǥ ǥ��
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
