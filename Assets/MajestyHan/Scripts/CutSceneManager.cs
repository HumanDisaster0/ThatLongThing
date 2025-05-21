using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutSceneManager : MonoBehaviour
{
    private void Start()
    {
        //PlayRegisteredCutScene();
        //�׽�Ʈ��
        blackOutImage.gameObject.SetActive(false);
    }

    public static CutSceneManager Instance { get; private set; }


    [Header("�ƾ� ���� �� �̵��� ��")]
    public string nextSceneName;   // �����Ϳ��� ���� �Է�

    [Header("�ƾ� ������")]
    public List<Sprite> cutSceneImages;
    public List<string> cutSceneTexts;
    public float autoDelay = 1.0f;
    public Image blackOutImage;

    [Header("UI ����")]
    public Image cutSceneImage;
    public TextMeshProUGUI cutSceneText;
    public Canvas cutSceneCanvas;

    private List<GameObject> objectsToDisable = new List<GameObject>();
    private Coroutine runningCutscene = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // ���ϸ� ��ü ����
        cutSceneCanvas.gameObject.SetActive(false);
    }

    // �ܺο��� ȣ���� �޼���
    public void PlayCutScene(List<Sprite> images, List<string> texts, float? delay = null)
    {        
        if (runningCutscene != null)
        {
            StopCoroutine(runningCutscene);
        }
        cutSceneImages = images;
        cutSceneTexts = texts;
        if (delay.HasValue) autoDelay = delay.Value;
        runningCutscene = StartCoroutine(RunCutScene());
    }

    private IEnumerator RunCutScene()
    {
        // ��� ������Ʈ ��Ȱ��ȭ (ī�޶�, �ƾ�ĵ����, �ڱ��ڽ� ����)
        objectsToDisable.Clear();
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj == null) continue;
            if (obj == gameObject) continue;
            if (obj == cutSceneCanvas.gameObject) continue;
            if (obj.GetComponent<Camera>()) continue;
            // ���� ó�� �߰� ���� (�±� ��)
            if (obj.activeInHierarchy)
            {
                obj.SetActive(false);
                objectsToDisable.Add(obj);
            }
        }

        cutSceneCanvas.gameObject.SetActive(true);

        for (int i = 0; i < Mathf.Min(cutSceneImages.Count, cutSceneTexts.Count); i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(TypeTextAuto(cutSceneText, cutSceneTexts[i], 0.04f, autoDelay));
        }

        // �ƾ� ���� ó��
        cutSceneCanvas.gameObject.SetActive(false);
        foreach (var obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(true);
        }
        objectsToDisable.Clear();

        blackOutImage.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void PlayRegisteredCutScene()
    {
        PlayCutScene(cutSceneImages, cutSceneTexts, autoDelay);
    }
    private IEnumerator TypeTextAuto(TextMeshProUGUI target, string message, float typingSpeed, float waitAfter = 1f)
    {
        target.text = "";
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int i = 0;

        while (i < message.Length)
        {
            if (message[i] == '<')
            {
                int tagCloseIndex = message.IndexOf('>', i);
                if (tagCloseIndex != -1)
                {
                    builder.Append(message.Substring(i, tagCloseIndex - i + 1));
                    target.text = builder.ToString();
                    i = tagCloseIndex + 1;
                    continue;
                }
            }
            builder.Append(message[i]);
            target.text = builder.ToString();
            i++;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(waitAfter);
    }
}
