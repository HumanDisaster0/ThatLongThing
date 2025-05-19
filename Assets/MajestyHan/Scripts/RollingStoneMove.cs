using UnityEngine;

public class RollingStoneMove : MonoBehaviour
{
    public CameraController cam;

    [Header("�̵� �ӵ� �� ����")]
    public Vector2 moveDirection = Vector2.right;
    public float moveSpeed = 2f;

    [Header("ȸ�� ����")]
    public float rotationMultiplier = 360f;

    [Header("ȭ�� ��鸲 ���� ����")]
    public float shakeInterval = 0.7f;

    private float shakeTimer;

    private bool isActive = false;
    private Vector3 initialPosition;
    private Destroyer dest;


    void Start()
    {
        initialPosition = transform.position;  // �ʱ� ��ġ ����
        gameObject.SetActive(false);           // ���� �� ��Ȱ��ȭ
        dest = GetComponent<Destroyer>();
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

        shakeTimer += Time.deltaTime;
        if (shakeTimer >= shakeInterval)
        {
            cam?.ShakeCamera(35f, 0.18f, 6f);
            shakeTimer = 0f;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        int layer = other.gameObject.layer;

        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            var move = other.GetComponent<MMove>();
            var rb = other.GetComponent<Rigidbody2D>();

            if (move != null)
                move.SetStatus(MStatus.die);
            //   else
            //      Debug.LogWarning($"[RollingStone] Enemy ������Ʈ�� MMove ����: {other.name}");

            if (rb != null)
                rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
            // else
            //    Debug.LogWarning($"[RollingStone] Enemy ������Ʈ�� Rigidbody2D ����: {other.name}");
        }
        else if (layer == LayerMask.NameToLayer("Player"))
        {
            var pc = other.GetComponent<PlayerController>();

            if (pc != null)
                pc.AnyState(PlayerState.Die);
            // else
            //       Debug.LogWarning($"[RollingStone] Player ������Ʈ�� PlayerController ����: {other.name}");
        }
    }


    // �ܺο��� Ȱ��ȭ (����)
    public void Activate()
    {
        transform.position = initialPosition; // Ȥ�� �ٸ� ���� �־����� ����
        gameObject.SetActive(true);
        isActive = true;

        cam?.ShakeCamera(100f, 1f, 0.01f);
    }
    // �ܺο��� ��Ȱ��ȭ (����� + ��ġ ����)
    public void Deactivate()
    {
        isActive = false;
        transform.position = initialPosition;
        gameObject.SetActive(false);
    }
}
