using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeInOut : MonoBehaviour
{
    public static FadeInOut instance;

    public bool exeFadeOut = true;  // ���� �� ���̵� �ƿ� ����
    public float fadeTime = 1.0f;   // ���̵� ���� �ð�
    public float delay = 1.0f;      // �� ���� �� ��� �ð�
    public string sceneName;        // �ε��� �� �̸�

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

        yield return new WaitForSecondsRealtime(delay); // ������ �ð���ŭ ���

        while (elapsedTime < fadeTime)
        {
            fadeObjTrans.sizeDelta = Vector2.Lerp(startSize, targetSize, elapsedTime / fadeTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeObjTrans.sizeDelta = targetSize;

        yield return new WaitForSecondsRealtime(0.5f); // �߰� ��� �� �� ����
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
            Debug.LogError("�� �̸��� �������� �ʾҽ��ϴ�!");
        }
    }
}