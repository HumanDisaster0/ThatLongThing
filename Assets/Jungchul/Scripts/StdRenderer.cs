using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StdRenderer : MonoBehaviour
{
    public Sprite[] StdSprites;
    public Image displayImage;       // ������ Image ������Ʈ

    //private int currentIndex = 0;

    void Start()
    {
        currentIndex = 0;
        if (StdSprites.Length > 0)
            displayImage.sprite = StdSprites[0];
    }

    void Update()
    {

        // ���ǿ� ���� ȭ�� �� ��������Ʈ ����


        //if (Input.GetMouseButtonDown(0))
        //{
        //    currentIndex++;

        //    if (currentIndex >= cutsceneSprites.Length)
        //    {
        //        // �ƾ� �� -> ���� ������ ��ȯ
        //        SceneManager.LoadScene("GuildMain");
        //    }
        //    else
        //    {
        //        // ���� �̹����� ����
        //        displayImage.sprite = cutsceneSprites[currentIndex];
        //    }
        //}
    }
}


