using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ImageScenePreset : MonoBehaviour
{
    public Sprite[] cutsceneSprites; // 3�� �̹��� ���
    public Image displayImage;       // ������ Image ������Ʈ
    
    public Image fadePanel;    // ���� ���̵�� �г�
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
                // �ƾ� �� -> ���� ������ ��ȯ
                SceneManager.LoadScene("NextSceneName");
            }
            else
            {
                // ���� �̹����� ����
                displayImage.sprite = cutsceneSprites[currentIndex];
            }
        }
    }
}