using UnityEngine;

public class RollingStoneMove : MonoBehaviour
{
    [Header("이동 속도 및 방향")]
    public Vector2 moveDirection = Vector2.right;
    public float moveSpeed = 2f;

    [Header("회전 연출")]
    public float rotationMultiplier = 360f;

    private bool isActive = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;  // 초기 위치 저장
        gameObject.SetActive(false);           // 시작 시 비활성화
    }

    void Update()
    {
        if (!isActive) return;

        // 이동
        Vector3 deltaMove = moveDirection.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(deltaMove, Space.World);

        // 이동 방향에 따라 회전
        float rotationAmount = rotationMultiplier * Time.deltaTime;
        float directionSign = Mathf.Sign(moveDirection.x);
        transform.Rotate(Vector3.forward, -rotationAmount * directionSign);
    }

    // 외부에서 활성화 (등장)
    public void Activate()
    {
        transform.position = initialPosition; // 혹시 다른 곳에 있었더라도 복귀
        gameObject.SetActive(true);
        isActive = true;
    }
    // 외부에서 비활성화 (사라짐 + 위치 복구)
    public void Deactivate()
    {
        isActive = false;
        transform.position = initialPosition;
        gameObject.SetActive(false);
    }
}
