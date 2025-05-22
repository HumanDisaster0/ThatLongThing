using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeInOut : MonoBehaviour
{
    public static FadeInOut instance;

    public bool exeFadeOut = true;  // 시작 시 페이드 아웃 여부
    public float fadeTime = 1.0f;   // 페이드 지속 시간
    public float delay = 1.0f;      // 씬 변경 전 대기 시간
    public string sceneName;        // 로드할 씬 이름

    [SerializeField] RectTransform fadeObjTrans;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (exeFadeOut)
        {
            fadeObjTrans.sizeDelta = new Vector2(1920, fadeObjTrans.sizeDelta.y);
            StartCoroutine(FadeOut());
        }
        else
            fadeObjTrans.sizeDelta = Vector2.zero;
    }

    public void ExeFadeIn()
    {
        StartCoroutine(FadeInAndLoadScene());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSecondsRealtime(delay);
        float elapsedTime = 0;
        Vector2 startPos = new Vector2(0f, fadeObjTrans.position.y);
        Vector2 targetPos = new Vector2(1920, startPos.y);

        while (elapsedTime < fadeTime)
        {
            fadeObjTrans.position = Vector2.Lerp(startPos, targetPos, elapsedTime / fadeTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeObjTrans.sizeDelta = Vector2.zero;
    }

    IEnumerator FadeInAndLoadScene()
    {
        float elapsedTime = 0;
        Vector2 startSize = new Vector2(0, fadeObjTrans.sizeDelta.y);
        Vector2 targetSize = new Vector2(1920, startSize.y);
        fadeObjTrans.sizeDelta = startSize;
        fadeObjTrans.position = new Vector2(0f, fadeObjTrans.position.y);

        yield return new WaitForSecondsRealtime(delay); // 지정된 시간만큼 대기

        while (elapsedTime < fadeTime)
        {
            fadeObjTrans.sizeDelta = Vector2.Lerp(startSize, targetSize, elapsedTime / fadeTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeObjTrans.sizeDelta = targetSize;

        yield return new WaitForSecondsRealtime(0.5f); // 추가 대기 후 씬 변경
        LoadScene();
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("씬 이름이 설정되지 않았습니다!");
        }
    }
}