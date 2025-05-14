using UnityEngine;
using UnityEngine.EventSystems;

public class MapZoom : MonoBehaviour
{
    public RectTransform mapContent; // 실제 지도
    public float zoomSpeed = 0.1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;
    public float GetScale => m_currentScale;

    float m_currentScale = 1.0f;

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) // UI 위일 때만 반응
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                Vector3 oldPos = Input.mousePosition;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mapContent, oldPos, null, out localPoint);

                Vector3 scale = mapContent.localScale;
                scale *= (1 + scroll * zoomSpeed);
                scale = ClampScale(scale);
                mapContent.localScale = scale;

                m_currentScale = scale.x;

                // 줌 포커스 유지 (줌인할 때 마우스 기준으로 당김)
                Vector2 newLocalPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mapContent, Input.mousePosition, null, out newLocalPoint);
                Vector3 diff = (Vector3)(newLocalPoint - localPoint);
                mapContent.localPosition -= Mathf.Sign(-scroll) * diff;
            }
        }
    }

    Vector3 ClampScale(Vector3 scale)
    {
        float clamped = Mathf.Clamp(scale.x, minScale, maxScale);
        return new Vector3(clamped, clamped, 1);
    }

}