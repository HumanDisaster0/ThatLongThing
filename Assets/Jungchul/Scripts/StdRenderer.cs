using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StdRenderer : MonoBehaviour
{
    public Sprite[] StdSprites;
    public Image displayImage;       // 보여질 Image 컴포넌트

    //private int currentIndex = 0;

    void Start()
    {
        currentIndex = 0;
        if (StdSprites.Length > 0)
            displayImage.sprite = StdSprites[0];
    }

    void Update()
    {

        // 조건에 따라 화면 내 스프라이트 렌더


        //if (Input.GetMouseButtonDown(0))
        //{
        //    currentIndex++;

        //    if (currentIndex >= cutsceneSprites.Length)
        //    {
        //        // 컷씬 끝 -> 다음 씬으로 전환
        //        SceneManager.LoadScene("GuildMain");
        //    }
        //    else
        //    {
        //        // 다음 이미지로 변경
        //        displayImage.sprite = cutsceneSprites[currentIndex];
        //    }
        //}
    }
}


