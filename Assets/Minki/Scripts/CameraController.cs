using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    public Transform Player;

    public float yOffset = 2f;

    public float lerpMotionSpeed = 3f;

    public float minLerpSpeed = 2f;
    public float maxLerpSpeed = 15f;

    public Vector3 currentPos = Vector3.zero;
    public Vector3 targetPos = Vector3.zero;

    public Rect worldRect = Rect.zero;

    public float depthX = 2f;
    public float depthY = 1f;

    public float deadzoneX = 2f;
    public float deadzoneY = 1.5f;

    Camera m_camera;
    PixelPerfectCamera m_pixelPerfectCamera;
    int m_lastWidth;
    int m_lastHeight;
    float m_clampWidth;
    float m_clampHeight;
    const float DEADZONE_THRESHOLD = 2f;

    // Start is called before the first frame update
    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_pixelPerfectCamera = GetComponent<PixelPerfectCamera>();

        targetPos = Player.position;


        OnViewportSizeChanged();

        targetPos.x = Mathf.Clamp(
            targetPos.x,
            worldRect.xMin + m_clampWidth,
            worldRect.xMax - m_clampWidth
        );

        targetPos.y = Mathf.Clamp(
            targetPos.y,
            worldRect.yMin + m_clampHeight - yOffset,
            worldRect.yMax - m_clampHeight - yOffset
        );

        currentPos = targetPos;

        m_lastWidth = Screen.width;
        m_lastHeight = Screen.height;
    }

    void OnViewportSizeChanged()
    {
        // PixelPerfectCamera ������Ʈ���� ���� ������ ���� ��������
        if (m_pixelPerfectCamera != null)
        {
            // �ȼ� ����Ʈ ī�޶��� ������ ���� ���
            float unitsPerPixel = 1f / m_pixelPerfectCamera.assetsPPU;
            int screenHeight = m_pixelPerfectCamera.refResolutionY;
            int screenWidth = m_pixelPerfectCamera.refResolutionX;

            // ���� �������� ��ȯ
            m_clampHeight = (screenHeight * unitsPerPixel) / 2f;
            m_clampWidth = (screenWidth * unitsPerPixel) / 2f;

            // �ʿ��� ��� ���� ����
            if (m_pixelPerfectCamera.upscaleRT && m_pixelPerfectCamera.pixelSnapping)
            {
                // �������� ������ �ִ� ���, ī�޶� ������Ʈ�� orthographicSize�� ����Ͽ� ����
                float actualSize = m_camera.orthographicSize;
                float pixelSize = m_clampHeight;

                // ���� �������� ũ��� �ȼ� ����Ʈ ũ�� ������ ���
                float ratio = actualSize / pixelSize;
                m_clampHeight = actualSize;
                m_clampWidth *= ratio;
            }
        }
        else
        {
            // �ȼ� ����Ʈ ī�޶� ���� ��� ���� ������� ���
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

        // ī�޶� ũ�� �ʱ�ȭ�� PixelPerfectCamera�� �Ӽ��� ����
        OnViewportSizeChanged();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Screen.width != m_lastWidth || Screen.height != m_lastHeight)
        {
            m_lastWidth = Screen.width;
            m_lastHeight = Screen.height;
            OnViewportSizeChanged();
        }

        if (Mathf.Abs(targetPos.x - Player.position.x) > depthX)
            targetPos.x = Player.position.x > targetPos.x ? Player.position.x - depthX : Player.position.x + depthX;

        if (Mathf.Abs(targetPos.y - Player.position.y) > depthY)
            targetPos.y = Player.position.y > targetPos.y ? Player.position.y - depthY : Player.position.y + depthY;

        targetPos.x = Mathf.Clamp(
            targetPos.x,
            worldRect.xMin + m_clampWidth,
            worldRect.xMax - m_clampWidth
        );

        targetPos.y = Mathf.Clamp(
            targetPos.y,
            worldRect.yMin + m_clampHeight - yOffset,
            worldRect.yMax - m_clampHeight - yOffset
        );

        var setXLerpSpeed = minLerpSpeed + ((maxLerpSpeed - minLerpSpeed) * Mathf.Clamp01(Mathf.Max(0f, Mathf.Abs(Player.position.x - currentPos.x) - deadzoneX) / DEADZONE_THRESHOLD));
        var setYLerpSpeed = minLerpSpeed + ((maxLerpSpeed - minLerpSpeed) * Mathf.Clamp01(Mathf.Max(0f, Mathf.Abs(Player.position.y - currentPos.y) - deadzoneY) / DEADZONE_THRESHOLD));

        currentPos.x = Mathf.Lerp(currentPos.x, targetPos.x, 1 - Mathf.Exp(-setXLerpSpeed * Time.deltaTime));
        currentPos.y = Mathf.Lerp(currentPos.y, targetPos.y, 1 - Mathf.Exp(-setYLerpSpeed * Time.deltaTime));

        transform.position = currentPos + Vector3.up * yOffset + Vector3.forward * -10f;
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
        Gizmos.DrawWireCube(targetPos, new Vector3(depthX * 2f, depthY * 2f, 1));

        //Deadzone Min
        Gizmos.color = new Color(1, 0.5f, 0.75f, 1);
        Gizmos.DrawLine(transform.position + new Vector3 { x = deadzoneX, y = (-yOffset + deadzoneY), z = 10 }, transform.position + new Vector3 { x = deadzoneX, y = (-yOffset - deadzoneY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = deadzoneX, y = (-yOffset - deadzoneY), z = 10 }, transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset - deadzoneY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset - deadzoneY), z = 10 }, transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset + deadzoneY), z = 10 });
        Gizmos.DrawLine(transform.position + new Vector3 { x = -deadzoneX, y = (-yOffset + deadzoneY), z = 10 }, transform.position + new Vector3 { x = deadzoneX, y = (-yOffset + deadzoneY), z = 10 });

        //Deadzone Max
        Gizmos.color = new Color(1, 0f, 1f);
        Gizmos.DrawWireCube(transform.position + new Vector3 { y = -yOffset }, new Vector3(deadzoneX * 2f + DEADZONE_THRESHOLD * 2f, deadzoneY * 2f + DEADZONE_THRESHOLD * 2, 1));

        //World Rect
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldRect.center, worldRect.size);

        //// PixelPerfect ī�޶� ���� ǥ�� (�ʷϻ�)
        //if (m_camera != null && m_pixelPerfectCamera != null)
        //{
        //    Gizmos.color = Color.green;
        //    Vector3 cameraPos = transform.position;
        //    Vector3 ppCameraSize = new Vector3(m_clampWidth * 2f, m_clampHeight * 2f, 1f);
        //    Gizmos.DrawWireCube(new Vector3(cameraPos.x, cameraPos.y - yOffset, cameraPos.z), ppCameraSize);
        //}
    }
}