using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal; // URP�� PixelPerfectCamera�� ���� ���ӽ����̽�
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
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
    PixelPerfectCamera m_pixelPerfectCamera; // URP�� PixelPerfectCamera
    int m_lastWidth;
    int m_lastHeight;
    float m_clampWidth;
    float m_clampHeight;
    Vector3 m_currentPos = Vector3.zero;
    Vector3 m_targetPos = Vector3.zero;

    //bool m_flipWorldRect = false;

    // Start is called before the first frame update
    void Start()
    {
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
        // URP PixelPerfectCamera ������Ʈ���� ���� ������ ���� ��������
        if (m_pixelPerfectCamera != null)
        {
            // �ȼ� ����Ʈ ī�޶��� ������ ���� ���
            float unitsPerPixel = 1f / m_pixelPerfectCamera.assetsPPU;
            int screenHeight = m_pixelPerfectCamera.refResolutionY;
            int screenWidth = m_pixelPerfectCamera.refResolutionX;

            // ���� �������� ��ȯ
            m_clampHeight = (screenHeight * unitsPerPixel) / 2f;
            m_clampWidth = (screenWidth * unitsPerPixel) / 2f;

            // URP PixelPerfectCamera������ �Ӽ� �̸��� �ٸ� �� ����
            if (m_pixelPerfectCamera.gridSnapping == PixelPerfectCamera.GridSnapping.UpscaleRenderTexture)
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
        if (!Player)
            return;

        //var lastFlippedState = m_flipWorldRect;
        if (isMirrored)
        {
            worldRect.x = Player.position.x > mirrorAxisX ? mirrorAxisX : mirrorAxisX - worldRect.width;
        }

        //��ũ������� ����Ǿ��� ��
        if (Screen.width != m_lastWidth || Screen.height != m_lastHeight)
        {
            //ī�޶� ��� �ٽ� ���
            m_lastWidth = Screen.width;
            m_lastHeight = Screen.height;
            OnViewportSizeChanged();
        }

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

        var setXLerpSpeed = minLerpSpeed + ((maxLerpSpeed - minLerpSpeed) * Mathf.Clamp01(Mathf.Max(0f, Mathf.Abs(Player.position.x - m_currentPos.x) - deadzoneX) / deadzoneThresold));
        var setYLerpSpeed = minLerpSpeed + ((maxLerpSpeed - minLerpSpeed) * Mathf.Clamp01(Mathf.Max(0f, Mathf.Abs(Player.position.y - m_currentPos.y) - deadzoneY) / deadzoneThresold));

        //�ſ���� ȿ�� �� �ӵ�����
        if ((m_targetPos.x + deadzoneX + deadzoneThresold < m_currentPos.x + depthX && m_currentPos.x + depthX < worldRect.max.x )
            || (m_targetPos.x - deadzoneX - deadzoneThresold > m_currentPos.x - depthX && m_currentPos.x - depthX > worldRect.min.x)
            || (m_currentPos.x + depthX > worldRect.max.x) 
            || (m_currentPos.x - depthX < worldRect.min.x))
        {
            setXLerpSpeed = maxLerpSpeed;
        }
           

        m_currentPos.x = Mathf.Lerp(m_currentPos.x, m_targetPos.x, 1 - Mathf.Exp(-setXLerpSpeed * Time.deltaTime));
        m_currentPos.y = Mathf.Lerp(m_currentPos.y, m_targetPos.y, 1 - Mathf.Exp(-setYLerpSpeed * Time.deltaTime));

        transform.position = m_currentPos + Vector3.up * yOffset + Vector3.forward * -10f;
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