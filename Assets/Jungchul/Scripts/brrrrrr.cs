using System.Collections;
using UnityEngine;

public class brrrrrr : MonoBehaviour
{
    public GameObject br;
    public float moveDistance = 0.03f;   // �̵��� �Ÿ�
    public float moveInterval = 0.1f;   // �̵� �ֱ� (��)

    private int direction = 1;          // 1�̸� ������, -1�̸� ����

    void Start()
    {
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            // ���⿡ ���� ��ġ �̵�
            Vector3 currentPos = br.transform.position;
            br.transform.position = new Vector3(currentPos.x + moveDistance * direction, currentPos.y, currentPos.z);

            // ���� ����
            direction *= -1;

            // 0.5�� ���
            yield return new WaitForSeconds(moveInterval);
        }
    }
}
