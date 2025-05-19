using UnityEngine;
using System.Collections;

public class CameraEffectManager : MonoBehaviour
{
    [Header("Camera & Targets")]
    public Camera cam;
    public Transform targetA;
    public Transform targetB;

    [Header("Zoom Settings")]
    public float zoomInSize = 3f;
    public float zoomDuration = 1f;

    [Header("Letterbox (optional)")]
    public RectTransform topBar;
    public RectTransform bottomBar;
    public float barSize = 100f;
    public float barSpeed = 500f;

    private Vector3 originalPosition;
    private float originalSize;
    private Vector3 offset;
    private Coroutine effectRoutine;

    private bool isControlling = false;

    void Awake()
    {
        if (cam == null) cam = Camera.main;

        topBar.anchoredPosition = new Vector2(0, barSize);
        bottomBar.anchoredPosition = new Vector2(0, -barSize);
    }

    public void ZoomInToMiddle()
    {
        if (targetA == null || targetB == null)
        {
            Debug.LogWarning("CameraEffectManager: 타겟이 할당되지 않음.");
            return;
        }

        if (effectRoutine != null) StopCoroutine(effectRoutine);

        originalPosition = cam.transform.position;
        originalSize = cam.orthographicSize;

        Vector3 midpoint = (targetA.position + targetB.position) / 2f;
        offset = new Vector3(midpoint.x - originalPosition.x, midpoint.y - originalPosition.y, 0f);
        Vector3 targetPos = originalPosition + offset;

        isControlling = true;
        effectRoutine = StartCoroutine(ZoomAndMove(originalSize, zoomInSize, originalPosition, targetPos));
        StartCoroutine(MoveBars(0f));
    }

    public void ZoomOut()
    {
        if (!isControlling) return;

        if (effectRoutine != null) StopCoroutine(effectRoutine);

        Vector3 currentPosition = cam.transform.position;
        Vector3 targetPos = currentPosition - offset;

        effectRoutine = StartCoroutine(ZoomAndMove(cam.orthographicSize, originalSize, currentPosition, targetPos));
        StartCoroutine(MoveBars(barSize));
        isControlling = false;
    }

    IEnumerator ZoomAndMove(float fromSize, float toSize, Vector3 fromPos, Vector3 toPos)
    {
        float t = 0f;
        while (t < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(fromSize, toSize, t / zoomDuration);
            cam.transform.position = Vector3.Lerp(fromPos, toPos, t / zoomDuration);
            t += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = toSize;
        cam.transform.position = toPos;
    }

    IEnumerator MoveBars(float targetY)
    {
        if (topBar == null || bottomBar == null)
            yield break;

        Vector2 topTarget = new Vector2(0, targetY);
        Vector2 bottomTarget = new Vector2(0, -targetY);

        while (true)
        {
            topBar.anchoredPosition = Vector2.MoveTowards(topBar.anchoredPosition, topTarget, barSpeed * Time.deltaTime);
            bottomBar.anchoredPosition = Vector2.MoveTowards(bottomBar.anchoredPosition, bottomTarget, barSpeed * Time.deltaTime);

            if (Vector2.Distance(topBar.anchoredPosition, topTarget) < 0.1f &&
                Vector2.Distance(bottomBar.anchoredPosition, bottomTarget) < 0.1f)
                break;

            yield return null;
        }
    }
}
