using UnityEngine;

public class JumpNode : MonoBehaviour
{
    public JumpNode connectedNode;                 // ����� ���
    public bool isHorizontalJump = false;          // ���� �������� ����
    public bool expectsRightApproach = true;       // �����ʿ��� �����ؾ߸� �۵�

    public Vector3 GetConnectedPosition() => connectedNode.transform.position;

    private void OnDrawGizmos()
    {
        if (connectedNode != null)
        {
            // 1. �� ����: �����̸� ���, �ƴϸ� �Ķ�
            Gizmos.color = isHorizontalJump ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(transform.position, connectedNode.transform.position);

            // 2. ��� ��� ��
            Gizmos.color = isHorizontalJump ? Color.red : Color.blue;
            Gizmos.DrawSphere(transform.position, 0.15f);

            // 3. ���� ��� ��
            Gizmos.color = isHorizontalJump ? Color.green : Color.gray;
            Gizmos.DrawSphere(connectedNode.transform.position, 0.15f);

            // 4. ���� ȭ��ǥ (����׿�)
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
