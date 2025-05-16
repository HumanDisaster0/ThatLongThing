using UnityEngine;

public class RollingStoneMove : MonoBehaviour
{
    [Header("�̵� �ӵ� �� ����")]
    public Vector2 moveDirection = Vector2.right;
    public float moveSpeed = 2f;

    [Header("ȸ�� ����")]
    public float rotationMultiplier = 360f;

    private bool isActive = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;  // �ʱ� ��ġ ����
        gameObject.SetActive(false);           // ���� �� ��Ȱ��ȭ
    }

    void Update()
    {
        if (!isActive) return;

        // �̵�
        Vector3 deltaMove = moveDirection.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(deltaMove, Space.World);

        // �̵� ���⿡ ���� ȸ��
        float rotationAmount = rotationMultiplier * Time.deltaTime;
        float directionSign = Mathf.Sign(moveDirection.x);
        transform.Rotate(Vector3.forward, -rotationAmount * directionSign);
    }

    // �ܺο��� Ȱ��ȭ (����)
    public void Activate()
    {
        transform.position = initialPosition; // Ȥ�� �ٸ� ���� �־����� ����
        gameObject.SetActive(true);
        isActive = true;
    }
    // �ܺο��� ��Ȱ��ȭ (����� + ��ġ ����)
    public void Deactivate()
    {
        isActive = false;
        transform.position = initialPosition;
        gameObject.SetActive(false);
    }
}
