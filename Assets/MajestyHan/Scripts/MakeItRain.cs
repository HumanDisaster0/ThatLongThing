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

    [Header("낙하 속도 (유닛/초)")]
    public float fallSpeed = 5f;

    private Coroutine rainRoutine;
    private List<RainSlot> rainSchedule = new List<RainSlot>();

    // 계산된 기준값들
    private float xThreshold = 0.4f;
    private float delayThreshold = 0.2f;
    private float fallDuration = 1f;

    private void Start()
    {
        // 프리팹에서 스프라이트 크기 기반 xThreshold 계산
        if (pool != null && pool.rainPrefab != null)
        {
            var sr = pool.rainPrefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                xThreshold = sr.bounds.size.x * 1.1f; // 10% 여유
            }
        }

        // 낙하 시간 계산
        fallDuration = Mathf.Abs(spawnY - destroyY) / fallSpeed;
        delayThreshold = fallDuration * 0.5f; // 절반 지점 기준
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

            // 겹치면: 낙하 절반 ~ 완료 시점 사이로 추가 지연
            delay += Random.Range(fallDuration * 0.5f, fallDuration);
        }

        // 실패하면 강제로 추가 지연
        delay += fallDuration;
        rainSchedule.Add(new RainSlot { x = x, delay = delay });
        return delay;
    }

    private IEnumerator FallRoutine(GameObject obj, float delay, float x)
    {
        yield return new WaitForSeconds(delay);

        // 낙하 시작 전에 스케줄 제거
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
