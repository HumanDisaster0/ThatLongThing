using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ���� �� ���� �Ŵ��� - ���� �ƾ�, ������(�б�), ������ �ƽ�(��������Ʈ), ���� ũ�������� ���� ����
/// </summary>
public class EndingSceneManager : MonoBehaviour
{
    [Header("������ ����")]
    public bool autoPlayMode = false;
    public float autoPlayDelay = 1.0f;  // �� �ƴ� ��� �ð�

    // ===== �ƾ� ������ (���� ��Ʈ) =====
    public List<Sprite> cutSceneImages;           // �ƾ� �̹���
    [TextArea]
    public List<string> cutSceneTexts;            // �ƾ� �ؽ�Ʈ

    // ===== ������ ���� ��ġ =====
    public int choiceCutIndex = 3;                // ������ ���� �ε���
    public int shakeCutIndex = 5;                // ������ ���� �ε���

    // ===== �б�(���Է�Ʈ) �ƾ� ������ =====
    public List<Sprite> branchImages;             // ���� �� �̾����� �ƾ� �̹���
    [TextArea]
    public List<string> branchTexts;              // ���� �� �̾����� �ƾ� �ؽ�Ʈ

    // ===== UI/������Ʈ ���� =====
    public Image cutSceneImage;                   // �ƾ� �̹��� ��¿�
    public TextMeshProUGUI cutSceneText;          // �ƾ� �ؽ�Ʈ ��¿�
    public GameObject choicePanel;                // ������(��ư) �г�
    public Button fakeChoiceBtn;                  // "�ƴϿ�" ��ư
    public Button realChoiceBtn;                  // "��" ��ư
    public GameObject realChoiceHighlight;        // "��" ����ȿ��(Glow ��)

    // ===== ������ �ƽ�(�ݰ�/������) =====
    public Sprite brokenGlass1;                   // �ݰ�(1�ܰ�)
    public Sprite brokenGlass2;                   // ������(2�ܰ�)

    // ===== ������(Chromatic Aberration) =====
    public Volume postProcessVolume;              // Global Volume (Inspector ����)
    private ChromaticAberration chromaticAberration;

    // ===== ���� ũ���� =====
    public GameObject creditsPanel;               // ũ���� ��ü �г�
    public ScrollRect creditsScroll;              // ũ���� ��ũ�Ѻ�
    public float creditsNormalSpeed = 30f;        // �⺻ ��ũ�� �ӵ�
    public float creditsFastSpeed = 100f;         // ��ư ������ �������� �ӵ�

    public string mainMenuSceneName = "MainMenu"; // ���� �� ���θ޴� �� �̸�

    private AudioSource bgmSrc; // BGM �ҽ�

    private void Awake()
    {
        // ������(Chromatic Aberration) ȿ�� ĳ��
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out chromaticAberration);

        postProcessVolume.gameObject.SetActive(false);

        // BGM ���
        //bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending_BGM");
        //bgmSrc = BgmPlayer.instance.ChangeBgm("Ending_BGM");
    }

    private void Start()
    {
        autoPlayMode = true;
        autoPlayDelay = 2.0f;

        // ��� UI/���� �⺻ ��Ȱ��ȭ
        choicePanel.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);

        // ���� �ƾ� ��� ����!
        StartCoroutine(PlayCutScenes());

        // BGM ���
        bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending2_BGM");
        DefaultSourceData data = bgmSrc.GetComponent<DefaultSourceData>();
        data.soundType = SoundType.Bg;
        data.volOverride = 0.5f;
        //bgmSrc = BgmPlayer.instance.ChangeBgm("Ending_BGM");
    }

    /// <summary>
    /// ���� �ƾ� ���� ���� (�����ơ漱������б��ũ����)
    /// </summary>
    private IEnumerator PlayCutScenes()
    {
        // 1. �ƾ� ����Ʈ ���� ���(�Է½� ��ŵ)
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f, accelerateInsteadOfSkip: true));

            if (i == choiceCutIndex)
            {
                // 2. ������ ����(�б�)
                yield return ShowChoice();
            }

            if (i == (choiceCutIndex + 1))
            {
                yield return new WaitForSeconds(1.0f);  // �ڵ� ���
            }

            if (i == shakeCutIndex)
            {
                EndingSceneCamera camShake = Camera.main.GetComponent<EndingSceneCamera>();
                if (camShake != null)
                {
                    AudioSource src = SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
                    StartCoroutine(camShake.LerpShake(1.5f, 0.5f, 0.0f)); // ��ü ���ӽð�, ���ۼ���, ���Ἴ��  
                }
                      
            }

            if (i == (shakeCutIndex + 1))
            {
                yield return new WaitForSeconds(1.0f);  // �ڵ� ���
                SoundManager.instance.StopSound(bgmSrc);    // BGM ����
            }

            if (i == shakeCutIndex + 2)
            {
                // 2��° BGM ���
                bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending_BGM");
                bgmSrc.GetComponent<DefaultSourceData>().soundType = SoundType.Bg;
            }
        }

        // 3. �б�(���Է�Ʈ) �ƾ� ���
        for (int i = 0; i < branchImages.Count && i < branchTexts.Count; i++)
        {
            cutSceneImage.sprite = branchImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, branchTexts[i], 0.04f, accelerateInsteadOfSkip: true));
        }

        // 4. ���� ũ����
        yield return ShowCredits();

        // 5. ���θ޴��� �̵�
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Ÿ���� + �Է½� ��ÿϷ�(�Է� ���)
    /// </summary>
    private IEnumerator WaitForTextInputTyper(TextMeshProUGUI textUI, string text, float baseSpeed, bool accelerateInsteadOfSkip = false)
    {
        yield return StartCoroutine(TextTyper.TypeText(textUI, text, baseSpeed, accelerateInsteadOfSkip));

        if (autoPlayMode)
        {
            yield return new WaitForSeconds(autoPlayDelay);  // �ڵ� ���
        }
        else
        {
            // ���� �Է� ���
            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
        }
    }



    /// <summary>
    /// ������ �г� + �б� ����
    /// "�ƴϿ�" ������ ���� �ƽ�(�ݰ����������濹����)
    /// "��" ������ �����Ʈ ����
    /// </summary>
    private IEnumerator ShowChoice()
    {
        choicePanel.SetActive(true);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false); // �� ���� OFF

        bool realChosen = false;
        bool fakeTriggered = false;

        // "��" Ŭ�� �̺�Ʈ
        realChoiceBtn.onClick.AddListener(() => 
        { 
            if(!bgmSrc.isPlaying)
                bgmSrc.UnPause();

            SoundManager.instance.PlayNewBackSound("Glass_Break", SoundType.Se);
            realChosen = true; });

        // "�ƴϿ�" Ŭ�� �̺�Ʈ(���� �ƽ� ����)
        fakeChoiceBtn.onClick.AddListener(() =>
        {
            if (!fakeTriggered)
            {
                StartCoroutine(FakeChoiceBrokenCutScenes(() => { fakeTriggered = true; }));
            }
        });

        // "��"�� ���� ������ ���(���� ���� �߿��� fakeChoiceBtn�� ������Ƿ� ���ѷ���X)
        while (!realChosen)
        {
            yield return null;
        }

        // =========== ���⼭ ����! ===========
        // ����/������ OFF(Ȥ�� ����������)
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (postProcessVolume != null) postProcessVolume.gameObject.SetActive(false);

        // ������ �г� OFF
        choicePanel.SetActive(false);

        // ������ ����
        realChoiceBtn.onClick.RemoveAllListeners();
        fakeChoiceBtn.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// "�ƴϿ�" ���� �� : �ݰ����������濹����
    /// </summary>
    private IEnumerator FakeChoiceBrokenCutScenes(System.Action onBrokenComplete)
    {
        // 0. BGM �Ͻ�����
        bgmSrc.Pause();

        // 1. �ƴϿ� ��ư ��Ȱ��ȭ(����)
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);
        realChoiceBtn.interactable = false;

        // 2. ���� ������Ʈ ON
        if (postProcessVolume != null)
            postProcessVolume.gameObject.SetActive(true);

        EndingSceneCamera camShake = Camera.main.GetComponent<EndingSceneCamera>();

        // ��鸮�� ���� ���
        SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);

        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(0.7f, 1.0f, 0.0f)); // ��ü ���ӽð�, ���ۼ���, ���Ἴ��    
        // 3. �ݰ� �̹��� + ������ ȿ��(Chromatic Aberration) ON
        yield return new WaitForSeconds(0.3f);
        SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
        cutSceneImage.sprite = brokenGlass1;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0.3f;
        if (cutSceneText != null) cutSceneText.text = ""; // �ؽ�Ʈ ����

        // 4. ������ �̹����� ���� (������ ����)
        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(1.2f, 2.0f, 0.0f)); // ��ü ���ӽð�, ���ۼ���, ���Ἴ��    

        SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
        SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
        cutSceneImage.sprite = brokenGlass2;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 1.0f;

        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(0.3f, 0.7f, 0.0f)); // ��ü ���ӽð�, ���ۼ���, ���Ἴ��    
        yield return new WaitForSeconds(0.7f);


        // 5. �� ��ư ����(Glow ��) ON
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);

        // 7. ���� �Ϸ� �ݹ�
        realChoiceBtn.interactable = true;
        onBrokenComplete?.Invoke();
    }

    /// <summary>
    /// ���� ũ���� ����(��ŵ�Ұ�/������ ������)
    /// </summary>
    private IEnumerator ShowCredits()
    {
        // 1. ũ���� �г� �Ʒ��� ��ġ
        RectTransform creditRect = creditsPanel.GetComponent<RectTransform>();
        Vector2 targetPos = creditRect.anchoredPosition;
        Vector2 startPos = new Vector2(targetPos.x, -Screen.height); // �Ʒ��� ����
        creditRect.anchoredPosition = startPos;

        Transform creditTextObj = creditsScroll.content.GetChild(0);
        creditTextObj.gameObject.SetActive(false);
        // 2. Ȱ��ȭ
        creditsPanel.SetActive(true);

        // 3. ���� �̵� Ʈ��
        float duration = 3.0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.SmoothStep(0f, 1f, t / duration); // �ε巴��
            creditRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, lerpT);
            yield return null;
        }

        // 4. ��Ȯ�� ���� ��ġ ����
        creditRect.anchoredPosition = targetPos;
        creditTextObj.gameObject.SetActive(true);
        // 5. ��ũ�� ũ���� ����
        float y = 0f;
        creditsScroll.verticalNormalizedPosition = 1f;
        bool finished = false;

        while (!finished)
        {
            float speed = Input.anyKey ? creditsFastSpeed : creditsNormalSpeed;
            y += speed * Time.deltaTime / creditsScroll.content.rect.height;
            creditsScroll.verticalNormalizedPosition = 1f - y;
            if (y >= 1f) finished = true;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        creditsPanel.SetActive(false);
    }
}
