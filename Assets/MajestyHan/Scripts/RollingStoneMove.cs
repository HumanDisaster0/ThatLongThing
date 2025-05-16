using UnityEngine;

public class RollingStoneMove : MonoBehaviour
{
    [Header("이동 속도 및 방향")]
    public Vector2 moveDirection = Vector2.right; // 기본 이동 방향
    public float moveSpeed = 2f;

    [Header("회전 연출")]
    public float rotationMultiplier = 360f; // 1초에 360도 회전 기준

    private bool isActive = false;

    void Start()
    {
        gameObject.SetActive(false); // 시작 시 비활성화
    }

    void Update()
    {
        if (!isActive) return;

        // 이동
        Vector3 deltaMove = moveDirection.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(deltaMove, Space.World);

        // 회전: 방향에 따라 시계/반시계 조절
        float rotationAmount = rotationMultiplier * Time.deltaTime;

        // 오른쪽이면 반시계, 왼쪽이면 시계 방향
        float directionSign = Mathf.Sign(moveDirection.x);
        transform.Rotate(Vector3.forward, -rotationAmount * directionSign);
    }

    // 외부에서 호출해서 활성화
    public void Activate()
    {
        gameObject.SetActive(true);
        isActive = true;
    }
}
