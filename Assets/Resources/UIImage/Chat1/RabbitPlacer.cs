using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RabbitPlacer : MonoBehaviour
{
    public RectTransform container; // �θ� �����̳�
    public List<GameObject> images;  // ���� ������ �̹���
    public int rabbitCount = 3; // �䳢 ����
    public float padding = 150f; // �¿� �е�����

#if UNITY_EDITOR
    private void OnValidate()
    {
        container = GetComponent<RectTransform>();
    }
#endif

    void Start()
    {
        container = GetComponent<RectTransform>();
        GenerateSprites(); // ��������Ʈ ��ġ
    }

    void GenerateSprites()
    {
        float spacing = container.sizeDelta.x - (padding * 2) / rabbitCount;

        for(int i = 0; i < rabbitCount; i++)
        {
            int j = 0;
            GameObject obj = Instantiate(images[j], transform);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(padding + spacing * i, 0, 0);
            j++;
            if (j >= images.Count)
                j = 0;
        }

        //for (int i = 0; i < count; i++)
        //{
        //    GameObject newSpriteObj = this.gameObject;
        //    Image imageComponent = newSpriteObj.GetComponent<Image>();

        //    if (imageComponent != null && i < sprites.Length)
        //    {
        //        imageComponent.sprite = sprites[i]; // ��������Ʈ ����
        //    }

        //    RectTransform rt = newSpriteObj.GetComponent<RectTransform>();
        //    float xPos = (i * widthFactor) + (i * spacing); // ��ġ ���
        //    rt.anchoredPosition = new Vector2(xPos, 0); // X ��ġ ����

        //    Debug.Log($"��������Ʈ {i + 1} ��ġ = {xPos}");
        //}
    }
}
