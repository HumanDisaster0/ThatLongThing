using UnityEngine;
using System.Collections;
public class EndingSceneCamera : MonoBehaviour
{

    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public IEnumerator LerpShake(float totalDuration, float startIntensity, float endIntensity)
    {
        float timer = 0f;

        while (timer < totalDuration)
        {
            float t = timer / totalDuration;
            float currentIntensity = Mathf.Lerp(startIntensity, endIntensity, t);

            // ���÷� ���� ó�� (�װ� ������ ������ ����ũ ��Ŀ� �°� ����)
            Vector3 offset = Random.insideUnitCircle * currentIntensity;
            transform.localPosition = originalPosition + offset;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos; // ��ġ ����
    }
}
