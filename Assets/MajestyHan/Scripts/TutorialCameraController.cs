using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal; // URP의 PixelPerfectCamera를 위한 네임스페이스
using UnityEngine.UIElements;
using System.Collections;

public class TutorialCameraController : MonoBehaviour
{
    public Transform Player;

    public float yOffset = 2f;

    public float lerpMotionSpeed = 3f;

    public float minLerpSpeed = 2f;
    public float maxLerpSpeed = 15f;

    public Rect worldRect = Rect.zero;

    public float depthX = 2f;
    public float depthY = 1f;

    public float deadzoneX = 2f;
    public float deadzoneY = 1.5f;
    public float deadzoneThresold = 2f;

    public bool isMirrored;
    public float mirrorAxisX = 0.0f;

    Camera m_camera;
    PixelPerfectCamera m_pixelPerfectCamera; // URP의 PixelPerfectCamera
    int m_lastWidth;
    int m_lastHeight;
    float m_clampWidth;
    float m_clampHeight;
    Vector3 m_currentPos = Vector3.zero;
    Vector3 m_targetPos = Vector3.zero;
    Vector3 m_shakePos = Vector3.zero;
    float m_shakeAmount = 1.0f;
    int m_noiseSeed = 45;
    float m_shakeSpeed = 1.0f;
    float m_shakeRecovery = 1.0f;
    float m_shakeTrauma = 0.0f;
    bool m_nonTrauma = false;

    public bool IsZoomed => isZoomed;


    //bool m_flipWorldRect = false;

    public bool IsNonTrauma() => m_nonTrauma;



    //==============================================================================================
    //임의로 추가한 부분 -> 튜토리얼에서만 사용하는 카메라 연출
    //==============================================================================================

    [Header("튜토리얼 전용")]
    public Transform focusTarget;
    public float zoomedOrthographicSize = 3f;
    public float zoomDuration = 1f;

    [Header("Letterbox Timing")]
    public float letterboxEnterDuration = 1.0f; // 등장 속도
    public float letterboxExitDuration = 0.5f;  // 사라짐 속도

    private float originalOrthographicSize;
    private bool isZoomed = false;

    private bool isLetterboxOpen = false;
    public bool IsLetterboxOpen => isLetterboxOpen;
    public bool IsZoomFullyReady => IsZoomed && IsLetterboxOpen;


    public void ZoomToTarget(Transform target)
    {
        if (isZoomed) return;
        isZoomed = true;

        StopAllCoroutines();

        Vector3 moveTargetPos = new Vector3(
            target.position.x,
            target.position.y - yOffset, // offset 유지
            0f
        );

        AnimateLetterBox(true);
        originalOrthographicSize = m_camera.orthographicSize;
        StartCoroutine(ZoomAndMoveRoutine(zoomedOrthographicSize, moveTargetPos, zoomDuration, true));
    }

    public void ResetZoom()
    {
        if (!isZoomed) return;
        isZoomed = false;

        StopAllCoroutines();

        Vector3 moveTargetPos = new Vector3(
            Mathf.Clamp(Player.position.x, worldRect.xMin + m_clampWidth, worldRect.xMax - m_clampWidth),
            Mathf.Clamp(Player.position.y, worldRect.yMin + m_clampHeight - yOffset, worldRect.yMax - m_clampHeight - yOffset),
            0f
        );

        AnimateLetterBox(false);
        StartCoroutine(ZoomAndMoveRoutine(originalOrthographicSize, moveTargetPos, zoomDuration, false));
    }

    IEnumerator ZoomAndMoveRoutine(float targetSize, Vector3 moveTargetPos, float duration, bool zoomingIn)
    {
        float elapsed = 0f;
        float startSize = m_camera.orthographicSize;
        Vector3 startPos = m_currentPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            m_camera.orthographicSize = Mathf.Lerp(startSize, targetSize, easedT);
            m_targetPos = Vector3.Lerp(startPos, moveTargetPos, easedT);

            yield return null;
        }

        m_camera.orthographicSize = targetSize;
        m_targetPos = moveTargetPos;
    }

    //레터박스 관련
    public GameObject topBar, bottomBar;
    public void EnableLetterBox(bool on)
    {
        if (topBar == null || bottomBar == null)
        {
            Debug.LogWarning("Letterbox UI가 설정되지 않았습니다.");
            return;
        }

        topBar.SetActive(on);
        bottomBar.SetActive(on);
    }

    public void AnimateLetterBox(bool on)
    {
        if (topBar == null || bottomBar == null) return;

        StopCoroutine(nameof(LetterboxSlide));
        StartCoroutine(LetterboxSlide(on));
    }

    IEnumerator LetterboxSlide(bool enable)
    {
        isLetterboxOpen = false; // 슬라이드 시작할 때 false

        RectTransform topRect = topBar.GetComponent<RectTransform>();
        RectTransform bottomRect = bottomBar.GetComponent<RectTransform>();

        float duration = enable ? letterboxEnterDuration : letterboxExitDuration;
        float startTime = Time.time;

        Vector2 topStart = topRect.anchoredPosition;
        Vector2 bottomStart = bottomRect.anchoredPosition;

        Vector2 topTarget = enable ? new Vector2(0, 0) : new Vector2(0, 120);
        Vector2 bottomTarget = enable ? new Vector2(0, 0) : new Vector2(0, -120);

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Mathf.SmoothStep(0, 1, t);

            topRect.anchoredPosition = Vector2.Lerp(topStart, topTarget, t);
            bottomRect.anchoredPosition = Vector2.Lerp(bottomStart, bottomTarget, t);

            yield return null;
        }

        topRect.anchoredPosition = topTarget;
        bottomRect.anchoredPosition = bottomTarget;

        isLetterboxOpen = enable; // 애니메이션 끝나고 상태 반영
    }




    //==============================================================================================



    void Start()
    {
        //==============================================================================================
        //임의로 추가한 부분 -> 튜토리얼에서만 사용하는 카메라 연출
        //==============================================================================================
        isZoomed = false;
        //==============================================================================================




        m_camera = GetComponent<Camera>();
        m_pixelPerfectCamera = GetComponent<PixelPerfectCamera>();

        m_targetPos = Player.position;

        OnViewportSizeChanged();

        m_targetPos.x = Mathf.Clamp(
            m_targetPos.x,
            worldRect.xMin + m_clampWidth,
            worldRect.xMax - m_clampWidth
        );

        m_targetPos.y = Mathf.Clamp(
            m_targetPos.y,
            worldRect.yMin + m_clampHeight - yOffset,
            worldRect.yMax - m_clampHeight - yOffset
        );

        m_currentPos = m_targetPos;

        m_lastWidth = Screen.width;
        m_lastHeight = Screen.height;
    }

    void OnViewportSizeChanged()
    {
        // URP PixelPerfectCamera 컴포넌트에서 실제 렌더링 영역 가져오기
        if (m_pixelPerfectCamera != null)
        {
            // 픽셀 퍼펙트 카메라의 렌더링 영역 계산
            float unitsPerPixel = 1f / m_pixelPerfectCamera.assetsPPU;
            int screenHeight = m_pixelPerfectCamera.refResolutionY;
            int screenWidth = m_pixelPerfectCamera.refResolutionX;

            // 월드 유닛으로 변환
            m_clampHeight = (screenHeight * unitsPerPixel) / 2f;
            m_clampWidth = (screenWidth * unitsPerPixel) / 2f;

            // URP PixelPerfectCamera에서는 속성 이름이 다를 수 있음
            if (m_pixelPerfectCamera.gridSnapping == PixelPerfectCamera.GridSnapping.UpscaleRenderTexture)
            {
                // 업스케일 설정이 있는 경우, 카메라 컴포넌트의 orthographicSize를 사용하여 보정
                float actualSize = m_camera.orthographicSize;
                float pixelSize = m_clampHeight;

                // 실제 보여지는 크기와 픽셀 퍼펙트 크기 비율을 계산
                float ratio = actualSize / pixelSize;
                m_clampHeight = actualSize;
                m_clampWidth *= ratio;
            }
        }
        else
        {
            // 픽셀 퍼펙트 카메라가 없는 경우 기존 방법으로 계산
            float orthographicSize = m_camera.orthographicSize;
            float aspect = (float)Screen.width / Screen.height;

            m_clampHeight = orthographicSize;
            m_clampWidth = orthographicSize * aspect;
        }
    }

    private void OnValidate()
    {
        m_camera = GetComponent<Camera>();
        m_pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        if (m_camera == null || m_pixelPerfectCamera == null)
        {
            Debug.LogWarning("Camera or PixelPerfectCamera component is missing.");
            return;
        }

        // 카메라 크기 초기화는 PixelPerfectCamera의 속성을 따름
        OnViewportSizeChanged();
    }

    public void OnEnable()
    {
        m_targetPos = Player.position;

        OnViewportSizeChanged();

        m_targetPos.x = Mathf.Clamp(
            m_targetPos.x,
            worldRect.xMin + m_clampWidth,
            worldRect.xMax - m_clampWidth
        );

        m_targetPos.y = Mathf.Clamp(
            m_targetPos.y,
            worldRect.yMin + m_clampHeight - yOffset,
            worldRect.yMax - m_clampHeight - yOffset
        );

        m_currentPos = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!Player)
            return;

        //var lastFlippedState = m_flipWorldRect;
        if (isMirrored)
        {
            worldRect.x = Player.position.x > mirrorAxisX ? mirrorAxisX : mirrorAxisX - worldRect.width;
        }

        //스크린사이즈가 변경되었을 때
        if (Screen.width != m_lastWidth || Screen.height != m_lastHeight)
        {
            //카메라 경계 다시 계산
            m_lastWidth = Screen.width;
            m_lastHeight = Screen.height;
            OnViewportSizeChanged();
        }

        if (!isZoomed) //임의로 추가함 byMajestyHan        
        {
            if (Mathf.Abs(m_targetPos.x - Player.position.x) > depthX)
                m_targetPos.x = Player.position.x > m_targetPos.x ? Player.position.x - depthX : Player.position.x + depthX;

            if (Mathf.Abs(m_targetPos.y - Player.position.y) > depthY)
                m_targetPos.y = Player.position.y > m_targetPos.y ? Player.position.y - depthY : Player.position.y + depthY;

            m_targetPos.x = Mathf.Clamp(
                m_targetPos.x,
                worldRect.xMin + m_clampWidth,
                worldRect.xMax - m_clampWidth
            );

            m_targetPos.y = Mathf.Clamp(
                m_targetPos.y,
                worldRect.yMin + m_clampHeight - yOffset,
                worldRect.yMax - m_clampHeight - yOffset
            );
        }
        var setXLerpSpeed = minLerpSpeed + ((maxLerpSpeed - minLerpSpeed) * Mathf.Clamp01(Mathf.Max(0f, Mathf.Abs(Player.position.x - m_currentPos.x) - deadzoneX) / deadzoneThresold));
        var setYLerpSpeed = minLerpSpeed + ((maxLerpSpeed - minLerpSpeed) * Mathf.Clamp01(Mathf.Max(0f, Mathf.Abs(Player.position.y - m_currentPos.y) - deadzoneY) / deadzoneThresold));

        //거울반전 효과 시 속도유지
        if ((m_targetPos.x + deadzoneX + deadzoneThresold < m_currentPos.x + depthX && m_currentPos.x + depthX < worldRect.max.x)
            || (m_targetPos.x - deadzoneX - deadzoneThresold > m_currentPos.x - depthX && m_currentPos.x - depthX > worldRect.min.x)
            || (m_currentPos.x + depthX > worldRect.max.x)
            || (m_currentPos.x - depthX < worldRect.min.x))
        {
            setXLerpSpeed = maxLerpSpeed;
        }


        m_currentPos.x = Mathf.Lerp(m_currentPos.x, m_targetPos.x, 1 - Mathf.Exp(-setXLerpSpeed * Time.deltaTime));
        m_currentPos.y = Mathf.Lerp(m_currentPos.y, m_targetPos.y, 1 - Mathf.Exp(-setYLerpSpeed * Time.deltaTime));


        //카메라 쉐이킹
        m_shakePos = new Vector3(m_shakeAmount * (Mathf.PerlinNoise(m_noiseSeed, Time.time * m_shakeSpeed) * 2 - 1),
        m_shakeAmount * (Mathf.PerlinNoise(m_noiseSeed + 1, Time.time * m_shakeSpeed) * 2 - 1),
        m_shakeAmount * (Mathf.PerlinNoise(m_noiseSeed + 2, Time.time * m_shakeSpeed) * 2 - 1));
        m_shakeTrauma = Mathf.Lerp(m_shakeTrauma, 0, m_shakeRecovery * Time.deltaTime);

        transform.position = m_currentPos + Vector3.up * yOffset + Vector3.forward * -10f + (m_shakePos * (m_nonTrauma ? 1f : m_shakeTrauma));

    }

    public void ShakeCamera(float speed, float amount, float recovery)
    {
        m_shakeSpeed = speed;
        m_shakeAmount = amount;
        m_shakeRecovery = recovery;
        m_shakeTrauma = 1f;
    }

    public void SetShakeTrauma(bool trauma)
    {
        m_nonTrauma = !trauma;
    }


    private void OnDrawGizmos()
    {
        //Current View
        Gizmos.color = new Color(1, 0.5f, 0, 1);
        Gizmos.DrawLine(transform.position + new Vector3 { x = depthX, y = (-yOffset + depthY), z = 10 }, transform.position + new Vector3 { x = depthX, y = (-yOffset - depthY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = depthX, y = (-yOffset - depthY), z = 10 }, transform.position + new Vector3 { x = -depthX, y = (-yOffset - depthY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = -depthX, y = (-yOffset - depthY), z = 10 }, transform.position + new Vector3 { x = -depthX, y = (-yOffset + depthY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = -depthX, y = (-yOffset + depthY), z = 10 }, transform.position + new Vector3 { x = depthX, y = (-yOffset + depthY), z = 10 });

        //Target View
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(m_targetPos, new Vector3(depthX * 2f, depthY * 2f, 1));

        //Deadzone Min
        Gizmos.color = new Color(1, 0.5f, 0.75f, 1);
        Gizmos.DrawLine(transform.position + new Vector3 { x = deadzoneX, y = (-yOffset + deadzoneY), z = 10 }, transform.position + new Vector3 { x = deadzoneX, y = (-yOffset - deadzoneY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = deadzoneX, y = (-yOffset - deadzoneY), z = 10 }, transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset - deadzoneY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset - deadzoneY), z = 10 }, transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset + deadzoneY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset + deadzoneY), z = 10 }, transform.position + new Vector3 { x = deadzoneX, y = (-yOffset + deadzoneY), z = 10 });

        //Deadzone Max
        Gizmos.color = new Color(1, 0f, 1f);
        Gizmos.DrawWireCube(transform.position + new Vector3 { y = -yOffset }, new Vector3(deadzoneX * 2f + deadzoneThresold * 2f, deadzoneY * 2f + deadzoneThresold * 2, 1));

        //World Rect
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldRect.center, worldRect.size);

        //// PixelPerfect 카메라 영역 표시 (초록색)
        //if (m_camera != null && m_pixelPerfectCamera != null)
        //{
        //    Gizmos.color = Color.green;
        //    Vector3 cameraPos = transform.position;
        //    Vector3 ppCameraSize = new Vector3(m_clampWidth * 2f, m_clampHeight * 2f, 1f);
        //    Gizmos.DrawWireCube(new Vector3(cameraPos.x, cameraPos.y - yOffset, cameraPos.z), ppCameraSize);
        //}
    }
}