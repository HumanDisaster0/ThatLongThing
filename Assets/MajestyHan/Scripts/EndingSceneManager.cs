// ���� �۵��ϴ� ���� �Ŵ��� ��ũ��Ʈ (2�ܰ� ��/�ƴϿ� �б� ����)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [Header("������ ����")]
    public bool autoPlayMode = false;
    public float autoPlayDelay = 1.0f;

    [Header("�ƾ� ������")]
    public List<Sprite> cutSceneImages; //���� 4�� 1112
    [TextArea] public List<string> cutSceneTexts;

    public List<Sprite> branchImages; // �� ������ 12�� 34_567788999
    [TextArea] public List<string> branchTexts;

    public List<Sprite> fakeBranchImages; // �ƴϿ� ������ 8�� _2_34567
    [TextArea] public List<string> fakeBranchTexts;

    [Header("������ ����")]
    public int choiceCutIndex = 3;
    public int shakeCutIndex = 5;

    [Header("UI ����")]
    public Image cutSceneImage;
    public TextMeshProUGUI cutSceneText;
    public GameObject choicePanel;
    public Button fakeChoiceBtn;
    public Button realChoiceBtn;
    public GameObject realChoiceHighlight;

    [Header("���� �ƾ�")]
    public Sprite brokenGlass1;
    public Sprite brokenGlass2;

    [Header("��ó�� ȿ��")]
    public Volume postProcessVolume;
    private ChromaticAberration chromaticAberration;
    private EndingSceneCamera cam;

    [Header("ũ����")]
    public GameObject creditsPanel;
    public ScrollRect creditsScroll;
    public float creditsNormalSpeed = 50f;
    public float creditsFastSpeed = 150f;
    public enum CutScenePhase
    {
        Intro,      // ������ ����
        YesRoute,   // ���� ��Ʈ
        NoRoute     // ���� ��Ʈ
    }

    private CutScenePhase currentPhase = CutScenePhase.Intro;

    public string mainMenuSceneName = "MainMenu";

    private AudioSource bgmSrc;
    private bool isRealFinal = true;
    //=================================================================================================

    private void Awake()
    {
        postProcessVolume.profile.TryGet(out chromaticAberration);
        postProcessVolume.gameObject.SetActive(false);
    }

    //=================================================================================================

    private void Start()
    {
        autoPlayMode = true;
        autoPlayDelay = 1.0f;
        choicePanel.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);
        bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending4_BGM");
        cam = Camera.main.GetComponent<EndingSceneCamera>();
        var data = bgmSrc.GetComponent<DefaultSourceData>();
        data.soundType = SoundType.Bg;
        data.volOverride = 0.5f;

        StartCoroutine(PlayCutScenes());
    }

    //=================================================================================================
    //�б����
    //=================================================================================================
    private IEnumerator PlayCutScenes()
    {
        List<Sprite> images = new();
        List<string> texts = new();

        currentPhase = CutScenePhase.Intro;

        /////////////////////////////////////////////////////////////////////////
        // 1. ������ ���� �ƾ�
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            if (i == 0) //ù �ƽſ� ���ؼ� 1�� ��ٷ��ֱ�
            {
                yield return new WaitForSeconds(1.0f);
            }

            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f));


            if (i == choiceCutIndex)
            {
                bool? firstChoice = null;
                yield return StartCoroutine(ShowChoice(result => firstChoice = result));

                /* 
                if (firstChoice == false)
                {
                    yield return StartCoroutine(FakeChoiceBrokenCutScenes());

                    bool? secondChoice = null;
                    yield return StartCoroutine(ShowChoice(result => secondChoice = result));

                    isRealFinal = secondChoice == true;
                }
                else
                {
                    isRealFinal = true;
                }
                */

                isRealFinal = firstChoice == true;
                currentPhase = isRealFinal ? CutScenePhase.YesRoute : CutScenePhase.NoRoute; //�б� �� / �ƴϿ�
            }
            /*
            if (i == shakeCutIndex)
            {
                var camShake = Camera.main.GetComponent<EndingSceneCamera>();
                if (camShake != null)
                {
                    SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
                    StartCoroutine(camShake.LerpShake(1.5f, 0.5f, 0.0f));
                }
            }

            if (i == shakeCutIndex + 1)
            {
                yield return new WaitForSeconds(1.0f);
                SoundManager.instance.StopSound(bgmSrc);
            }

            if (i == shakeCutIndex + 2)
            {
                bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending_BGM");
                bgmSrc.GetComponent<DefaultSourceData>().soundType = SoundType.Bg;
            }
            */


        }


        /////////////////////////////////////////////////////////////////////////
        // 2. �б� �ƾ� �غ�
        if (currentPhase == CutScenePhase.YesRoute)
        {
            images = branchImages;
            texts = branchTexts;
        }
        else if (currentPhase == CutScenePhase.NoRoute)
        {
            images = fakeBranchImages;
            texts = fakeBranchTexts;
        }

        // 3. ���� �ƾ� ����
        for (int i = 0; i < images.Count && i < texts.Count; i++)
        {
            cutSceneImage.sprite = images[i];

            if (i == 0 && currentPhase == CutScenePhase.NoRoute)
            {
                SoundManager.instance.StopSound(bgmSrc);
            }

            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, texts[i], 0.04f));

            /////////////////////////////////////////////////////////////////////////
            //������[��]

            if (currentPhase == CutScenePhase.YesRoute)
            {
                switch (i)
                {
                    case 0: yield return new WaitForSeconds(0.8f); break; //¸�׶�
                    case 1:
                        yield return new WaitForSeconds(0.5f); //�������� ����                        
                        if (cam != null)
                        {
                            SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
                            StartCoroutine(cam.LerpShake(1.5f, 0.5f, 0.0f));
                        }
                        break;

                    case 2: //��� off                        
                        yield return new WaitForSeconds(0.5f);
                        SoundManager.instance.StopSound(bgmSrc);
                        break;

                    case 3: //��� ����
                        yield return new WaitForSeconds(0.5f);
                        bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending_BGM");
                        bgmSrc.GetComponent<DefaultSourceData>().soundType = SoundType.Bg;
                        break;

                    default:
                        break;
                }
            }
            /////////////////////////////////////////////////////////////////////////
            //������[�ƴϿ�]
            if (currentPhase == CutScenePhase.NoRoute)
            {
                switch (i)
                {
                    case 0: //��� ����                  
                        bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending3_BGM");
                        bgmSrc.GetComponent<DefaultSourceData>().soundType = SoundType.Bg;
                        break;
                    case 1: yield return new WaitForSeconds(0.8f); Debug.Log("1�� ���"); break;
                    case 3:
                        yield return new WaitForSeconds(0.5f);
                        if (cam != null)
                        {
                            SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
                            StartCoroutine(cam.LerpShake(0.7f, 0.5f, 0.0f));
                        }
                        break;

                    case 4:
                        yield return new WaitForSeconds(0.5f);
                        if (cam != null)
                        {
                            SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
                            StartCoroutine(cam.LerpShake(1.5f, 0.5f, 0.0f));
                        }
                        break;
                    case 5:
                        yield return new WaitForSeconds(0.5f);
                        if (cam != null)
                        {
                            //SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
                            StartCoroutine(cam.LerpShake(0.6f, 1.0f, 0.0f));
                        }
                        break;
                    case 6:
                        yield return new WaitForSeconds(0.5f);
                        break;
                    default:
                        break;
                }
            }
        }

        // 4. ũ����
        yield return ShowCredits();
        SceneManager.LoadScene(mainMenuSceneName);
    }


    //=================================================================================================
    private IEnumerator ShowChoice(System.Action<bool> onResult)
    {
        choicePanel.SetActive(true);
        realChoiceBtn.interactable = true;
        fakeChoiceBtn.interactable = true;
        fakeChoiceBtn.gameObject.SetActive(true);
        if (realChoiceHighlight != null && !realChoiceHighlight.activeSelf)
            realChoiceHighlight.SetActive(false);

        bool chosen = false;
        bool tempResult = true;

        realChoiceBtn.onClick.RemoveAllListeners();
        fakeChoiceBtn.onClick.RemoveAllListeners();

        realChoiceBtn.onClick.AddListener(() =>
        {
            SoundManager.instance.PlayNewBackSound("Glass_Break", SoundType.Se);
            tempResult = true;
            chosen = true;
        });
        fakeChoiceBtn.onClick.AddListener(() =>
        {
            tempResult = false;
            chosen = true;
        });

        yield return new WaitUntil(() => chosen);

        onResult?.Invoke(tempResult);

        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (postProcessVolume != null) postProcessVolume.gameObject.SetActive(false);
        choicePanel.SetActive(false);
    }

    //=================================================================================================
    private IEnumerator FakeChoiceBrokenCutScenes()
    {
        bgmSrc.Pause();
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);
        realChoiceBtn.interactable = false;

        postProcessVolume.gameObject.SetActive(true);
        var camShake = Camera.main.GetComponent<EndingSceneCamera>();
        SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(0.7f, 1.0f, 0.0f));

        yield return new WaitForSeconds(0.3f);
        SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
        cutSceneImage.sprite = brokenGlass1;
        chromaticAberration.intensity.value = 0.3f;
        cutSceneText.text = "";

        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(1.2f, 2.0f, 0.0f));
        SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
        SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
        cutSceneImage.sprite = brokenGlass2;
        chromaticAberration.intensity.value = 1.0f;

        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(0.3f, 0.7f, 0.0f));
        yield return new WaitForSeconds(0.7f);

        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);
    }

    //=================================================================================================
    private IEnumerator WaitForTextInputTyper(TextMeshProUGUI textUI, string text, float baseSpeed)
    {
        bool skipRequested = false;

        IEnumerator SkipChecker()
        {
            while (!skipRequested)
            {
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                    skipRequested = true;
                yield return null;
            }
        }
        StartCoroutine(SkipChecker());

        yield return StartCoroutine(TextTyper.TypeText(textUI, text, baseSpeed, () => skipRequested));

        if (autoPlayMode)
            yield return new WaitForSeconds(autoPlayDelay);
        else
            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
    }

    //=================================================================================================
    private IEnumerator ShowCredits()
    {
        RectTransform creditRect = creditsPanel.GetComponent<RectTransform>();
        Vector2 targetPos = creditRect.anchoredPosition;
        Vector2 startPos = new Vector2(targetPos.x, -Screen.height);
        creditRect.anchoredPosition = startPos;

        Transform creditTextObj = creditsScroll.content.GetChild(0);
        creditTextObj.gameObject.SetActive(false);
        creditsPanel.SetActive(true);

        float duration = 1.5f, t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.SmoothStep(0f, 1f, t / duration);
            creditRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, lerpT);
            yield return null;
        }
        creditRect.anchoredPosition = targetPos;
        creditTextObj.gameObject.SetActive(true);

        float y = 0f;
        creditsScroll.verticalNormalizedPosition = 1f;
        while (y < 1f)
        {
            float speed = Input.anyKey ? creditsFastSpeed : creditsNormalSpeed;
            y += speed * Time.deltaTime / creditsScroll.content.rect.height;
            creditsScroll.verticalNormalizedPosition = 1f - y;
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);
        creditsPanel.SetActive(false);
    }
}
