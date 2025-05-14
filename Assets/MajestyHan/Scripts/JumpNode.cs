//======================================================
// JumpNode.cs
//======================================================
using UnityEngine;

public enum JumpNodeType
{
    Vertical,   // �� �� �̵�
    Horizontal  // ��ֹ� �ѱ�
}

public class JumpNode : MonoBehaviour
{
    public JumpNode destination;
    public JumpNodeType type = JumpNodeType.Vertical;

    private void OnDrawGizmos()
    {
        if (destination != null)
        {
            Gizmos.color = (type == JumpNodeType.Vertical) ? Color.cyan : Color.yellow;
            Gizmos.DrawLine(transform.position, destination.transform.position);
            Gizmos.DrawSphere(transform.position, 0.1f);
        }
    }
}

