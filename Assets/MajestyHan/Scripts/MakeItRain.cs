using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeItRain : MonoBehaviour
{
    [System.Serializable]
    private struct RainSlot
    {
        public float x;
        public float delay;
    }

    [Header("Ǯ �Ŵ���")]
    public RainObjectPool pool;

    [Header("���� ���� (X: min~max, Y�� spawnY)")]
    public float spawnY = 10f;
    public float xMin = -5f;
    public float xMax = 5f;

    [Header("�Ҹ� Y ����")]
    public float destroyY = -5f;

    [Header("���� ����")]
    public float spawnInterval = 1f;
    public int spawnCount = 3;

    [Header("���� ���� �ð�")]
    public float delayMin = 0.1f;
    public float delayMax = 0.5f;

    [Header("���� �ӵ� (����/��)")]
    public float fallSpeed = 5f;

    private Coroutine rainRoutine;
    private List<RainSlot> rainSchedule = new List<RainSlot>();

    // ���� ���ذ���
    private float xThreshold = 0.4f;
    private float delayThreshold = 0.2f;
    private float fallDuration = 1f;

    private void Start()
    {
        // �����տ��� ��������Ʈ ũ�� ��� xThreshold ���
        if (pool != null && pool.rainPrefab != null)
        {
            var sr = pool.rainPrefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                xThreshold = sr.bounds.size.x * 1.1f; // 10% ����
            }
        }

        // ���� �ð� ���
        fallDuration = Mathf.Abs(spawnY - destroyY) / fallSpeed;
        delayThreshold = fallDuration * 0.5f; // ���� ���� ����
    }

    public void StartRain()
    {
        if (rainRoutine == null)
            rainRoutine = StartCoroutine(RainLoop());
    }

    public void StopRain()
    {
        if (rainRoutine != null)
        {
            StopCoroutine(rainRoutine);
            rainRoutine = null;
        }
    }

    private IEnumerator RainLoop()
    {
        while (true)
        {
            SpawnMultiple(spawnCount);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnMultiple(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(xMin, xMax);
            float delay = GetNonOverlappingDelay(x);

            Vector3 spawnPos = new Vector3(x, spawnY, 0);
            GameObject obj = pool.Get();
            obj.transform.position = spawnPos;

            StartCoroutine(FallRoutine(obj, delay, x));
        }
    }

    private float GetNonOverlappingDelay(float x)
    {
        float delay = Random.Range(delayMin, delayMax);
        int maxTries = 20;

        for (int i = 0; i < maxTries; i++)
        {
            bool overlaps = false;

            foreach (var slot in rainSchedule)
            {
                if (Mathf.Abs(slot.x - x) < xThreshold &&
                    Mathf.Abs(slot.delay - delay) < delayThreshold)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rainSchedule.Add(new RainSlot { x = x, delay = delay });
                return delay;
            }

            // ��ġ��: ���� ���� ~ �Ϸ� ���� ���̷� �߰� ����
            delay += Random.Range(fallDuration * 0.5f, fallDuration);
        }

        // �����ϸ� ������ �߰� ����
        delay += fallDuration;
        rainSchedule.Add(new RainSlot { x = x, delay = delay });
        return delay;
    }

    private IEnumerator FallRoutine(GameObject obj, float delay, float x)
    {
        yield return new WaitForSeconds(delay);

        // ���� ���� ���� ������ ����
        rainSchedule.RemoveAll(slot => Mathf.Approximately(slot.x, x) && Mathf.Approximately(slot.delay, delay));

        Vector3 endPos = new Vector3(x, destroyY, 0);

        while (obj != null && obj.transform.position.y > destroyY)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, endPos, fallSpeed * Time.deltaTime);
            yield return null;
        }

        if (obj != null)
            pool.Return(obj);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 topLeft = new Vector3(xMin, spawnY, 0);
        Vector3 topRight = new Vector3(xMax, spawnY, 0);
        Gizmos.DrawLine(topLeft, topRight);

        Vector3 bottomLeft = new Vector3(xMin, destroyY, 0);
        Vector3 bottomRight = new Vector3(xMax, destroyY, 0);
        Gizmos.DrawLine(bottomLeft, bottomRight);
    }
}
