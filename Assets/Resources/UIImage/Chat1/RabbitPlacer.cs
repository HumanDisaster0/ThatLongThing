using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RabbitPlacer : MonoBehaviour
{
    public RectTransform container; // 부모 컨테이너
    public List<GameObject> images;  // 패턴 설정할 이미지
    public int rabbitCount = 3; // 토끼 갯수
    public float padding = 150f; // 좌우 패딩간격

#if UNITY_EDITOR
    private void OnValidate()
    {
        container = GetComponent<RectTransform>();
    }
#endif

    void Start()
    {
        container = GetComponent<RectTransform>();
        GenerateSprites(); // 스프라이트 배치
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
        //        imageComponent.sprite = sprites[i]; // 스프라이트 적용
        //    }

        //    RectTransform rt = newSpriteObj.GetComponent<RectTransform>();
        //    float xPos = (i * widthFactor) + (i * spacing); // 위치 계산
        //    rt.anchoredPosition = new Vector2(xPos, 0); // X 위치 설정

        //    Debug.Log($"스프라이트 {i + 1} 위치 = {xPos}");
        //}
    }
}
