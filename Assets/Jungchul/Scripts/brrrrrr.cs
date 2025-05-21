using System.Collections;
using UnityEngine;

public class brrrrrr : MonoBehaviour
{
    public GameObject br;
    public float moveDistance = 0.03f;   // 이동할 거리
    public float moveInterval = 0.1f;   // 이동 주기 (초)

    private int direction = 1;          // 1이면 오른쪽, -1이면 왼쪽

    void Start()
    {
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            // 방향에 따라 위치 이동
            Vector3 currentPos = br.transform.position;
            br.transform.position = new Vector3(currentPos.x + moveDistance * direction, currentPos.y, currentPos.z);

            // 방향 반전
            direction *= -1;

            // 0.5초 대기
            yield return new WaitForSeconds(moveInterval);
        }
    }
}
