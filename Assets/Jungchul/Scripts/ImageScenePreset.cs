using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ImageScenePreset : MonoBehaviour
{
    public Sprite[] cutsceneSprites; // 3장 이미지 등록
    public Image displayImage;       // 보여질 Image 컴포넌트
    
    public Image fadePanel;    // 검은 페이드용 패널
    public float fadeDuration = 0.2f;



    private int currentIndex = 0;

    void Start()
    {
        if (cutsceneSprites.Length > 0)
            displayImage.sprite = cutsceneSprites[0];
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentIndex++;

            if (currentIndex >= cutsceneSprites.Length)
            {
                // 컷씬 끝 -> 다음 씬으로 전환
                SceneManager.LoadScene("NextSceneName");
            }
            else
            {
                // 다음 이미지로 변경
                displayImage.sprite = cutsceneSprites[currentIndex];
            }
        }
    }
}