using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfactPanel : MonoBehaviour
{
    public Camera pixelPerfectCamera; // Pixel Perfect Camera 지정

    void Start()
    {
        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = Camera.main;
        }

        RectTransform panel = GetComponent<RectTransform>();
        Canvas canvas = panel.GetComponentInParent<Canvas>();

        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogError("이 스크립트는 Screen Space - Overlay 모드에서만 동작합니다.");
            return;
        }

        float scaleFactor = canvas.scaleFactor;
        Rect gameRect = pixelPerfectCamera.pixelRect;

        panel.anchorMin = new Vector2(0, 0); // Stretch 상태 유지
        panel.anchorMax = new Vector2(1, 1);

        // 검은 프레임 영역을 제외한 offset 계산
        panel.offsetMin = new Vector2(gameRect.xMin / scaleFactor, gameRect.yMin / scaleFactor);
        panel.offsetMax = new Vector2(-(Screen.width - gameRect.xMax) / scaleFactor,
                                      -(Screen.height - gameRect.yMax) / scaleFactor);
    }
}
