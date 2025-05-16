using UnityEngine;

public class RollingStoneMove : MonoBehaviour
{
    [Header("�̵� �ӵ� �� ����")]
    public Vector2 moveDirection = Vector2.right; // �⺻ �̵� ����
    public float moveSpeed = 2f;

    [Header("ȸ�� ����")]
    public float rotationMultiplier = 360f; // 1�ʿ� 360�� ȸ�� ����

    private bool isActive = false;

    void Start()
    {
        gameObject.SetActive(false); // ���� �� ��Ȱ��ȭ
    }

    void Update()
    {
        if (!isActive) return;

        // �̵�
        Vector3 deltaMove = moveDirection.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(deltaMove, Space.World);

        // ȸ��: ���⿡ ���� �ð�/�ݽð� ����
        float rotationAmount = rotationMultiplier * Time.deltaTime;

        // �������̸� �ݽð�, �����̸� �ð� ����
        float directionSign = Mathf.Sign(moveDirection.x);
        transform.Rotate(Vector3.forward, -rotationAmount * directionSign);
    }

    // �ܺο��� ȣ���ؼ� Ȱ��ȭ
    public void Activate()
    {
        gameObject.SetActive(true);
        isActive = true;
    }
}
