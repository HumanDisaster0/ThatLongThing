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

    [Header("풀 매니저")]
    public RainObjectPool pool;

    [Header("생성 범위 (X: min~max, Y는 spawnY)")]
    public float spawnY = 10f;
    public float xMin = -5f;
    public float xMax = 5f;

    [Header("소멸 Y 기준")]
    public float destroyY = -5f;

    [Header("생성 설정")]
    public float spawnInterval = 1f;
    public int spawnCount = 3;

    [Header("낙하 지연 시간")]
    public float delayMin = 0.1f;
    public float delayMax = 0.5f;

    [Header("낙하 시간 (초)")]
    public float fallDuration = 1f;

    [Header("겹침 허용 계수 (스프라이트 너비 × 계수)")]
    public float xThresholdFactor = 1.1f;

    private Coroutine rainRoutine;
    private List<RainSlot> rainSchedule = new List<RainSlot>();
    private List<GameObject> activePrefabs = new List<GameObject>();



    private float xThreshold = 0.4f;
    private float delayThreshold = 0.2f;

    private void Start()
    {
        if (pool != null && pool.rainPrefab != null)
        {
            var sr = pool.rainPrefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                xThreshold = sr.bounds.size.x * xThresholdFactor;
            }
        }

        delayThreshold = fallDuration * 0.5f;
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

        // 현재 떠 있는 모든 프리팹 강제 리턴
        foreach (var obj in activePrefabs)
        {
            if (obj != null)
                pool.Return(obj);
        }

        activePrefabs.Clear();
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
            float delay = Random.Range(delayMin, delayMax);

            if (IsOverlapping(x, delay))
            {
                GameObject skipped = pool.Get();
                skipped.transform.position = new Vector3(x, spawnY, 0);
                pool.Return(skipped);
                continue;
            }

            rainSchedule.Add(new RainSlot { x = x, delay = delay });

            Vector3 spawnPos = new Vector3(x, spawnY, 0);
            GameObject obj = pool.Get();
            activePrefabs.Add(obj);
            obj.transform.position = spawnPos;            

            StartCoroutine(FallRoutine(obj, delay, x));
            StartCoroutine(RemoveFromScheduleAfterDelay(x, delay, delayMin));
        }
    }

    private bool IsOverlapping(float x, float delay)
    {
        foreach (var slot in rainSchedule)
        {
            if (Mathf.Abs(slot.x - x) < xThreshold &&
                Mathf.Abs(slot.delay - delay) < delayThreshold)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator FallRoutine(GameObject obj, float delay, float x)
    {
        yield return new WaitForSeconds(delay);

        Vector3 startPos = obj.transform.position;
        Vector3 endPos = new Vector3(x, destroyY, 0);
        float t = 0f;

        while (obj != null && t < fallDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fallDuration);
            obj.transform.position = Vector3.Lerp(startPos, endPos, normalized);
            yield return null;
        }

        if (obj != null)
            pool.Return(obj);
    }

    private IEnumerator RemoveFromScheduleAfterDelay(float x, float delay, float extraDelay)
    {
        yield return new WaitForSeconds(delay + extraDelay);
        rainSchedule.RemoveAll(slot => Mathf.Approximately(slot.x, x) && Mathf.Approximately(slot.delay, delay));
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
