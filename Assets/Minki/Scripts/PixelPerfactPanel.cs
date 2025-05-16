using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfactPanel : MonoBehaviour
{
    public Camera pixelPerfectCamera; // Pixel Perfect Camera ����

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
            Debug.LogError("�� ��ũ��Ʈ�� Screen Space - Overlay ��忡���� �����մϴ�.");
            return;
        }

        float scaleFactor = canvas.scaleFactor;
        Rect gameRect = pixelPerfectCamera.pixelRect;

        panel.anchorMin = new Vector2(0, 0); // Stretch ���� ����
        panel.anchorMax = new Vector2(1, 1);

        // ���� ������ ������ ������ offset ���
        panel.offsetMin = new Vector2(gameRect.xMin / scaleFactor, gameRect.yMin / scaleFactor);
        panel.offsetMax = new Vector2(-(Screen.width - gameRect.xMax) / scaleFactor,
                                      -(Screen.height - gameRect.yMax) / scaleFactor);
    }
}
