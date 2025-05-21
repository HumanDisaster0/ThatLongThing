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
           
            Vector3 currentPos = br.transform.position;
            br.transform.position = new Vector3(currentPos.x + moveDistance * direction, currentPos.y, currentPos.z);

           
            direction *= -1;

           
            yield return new WaitForSeconds(moveInterval);
        }
    }
}
