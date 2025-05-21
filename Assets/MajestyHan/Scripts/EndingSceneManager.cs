using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EndingSceneManager : MonoBehaviour
{
    // ========== �ƾ� ������ ==========
    public List<Sprite> cutSceneImages; // ���� �ƾ� �̹��� ����Ʈ
    [TextArea]
    public List<string> cutSceneTexts;  // ���� �ƾ� �ؽ�Ʈ ����Ʈ

    // ========== ������ ���� �ε��� ==========
    public int choiceCutIndex = 3; // ���° �ƿ��� ������ ��������

    // ========== �б�(���Է�Ʈ) �ƾ� ������ ==========
    public List<Sprite> branchImages;   // ���� ���� �̾����� �ƾ� �̹��� ����Ʈ
    [TextArea]
    public List<string> branchTexts;    // ���� ���� �̾����� �ƾ� �ؽ�Ʈ ����Ʈ

    // ========== UI ���� ==========
    public Image cutSceneImage;             // �ƾ��� �̹���
    public TextMeshProUGUI cutSceneText;    // �ƾ��� �ؽ�Ʈ
    public GameObject choicePanel;          // ������ �г� (��/�ƴϿ� ��ư ����)
    public Button fakeChoiceBtn;            // "�ƴϿ�" ��ư
    public Button realChoiceBtn;            // "��" ��ư
    public GameObject realChoiceHighlight;  // "��" ��ư ���� ����(Glow ��)
    public GameObject glitchEffect;         // �۸�ġ/����Ʈ ������Ʈ(�Ⱦ��� �����)

    // ========== ������ �ƽ�(���� �ݰ�/������) ==========
    public Sprite brokenGlass1; // �ݰ�(1�ܰ�)
    public Sprite brokenGlass2; // ������(2�ܰ�)

    // ========== ������ ȿ��(Chromatic Aberration) ==========
    public Volume postProcessVolume; // Global Volume (Inspector���� ����)
    private ChromaticAberration chromaticAberration;

    // ========== ���� ũ���� ==========
    public GameObject creditsPanel;        // ũ���� ��ü �г�
    public ScrollRect creditsScroll;       // ũ���� ��ũ��
    public float creditsNormalSpeed = 30f; // �⺻ ��ũ�� �ӵ�
    public float creditsFastSpeed = 100f;  // ���� ��ũ�� �ӵ�

    public string mainMenuSceneName = "MainMenu"; // ���� �� ���ư� �� �̸�

    private void Awake()
    {
        // Volume���� ������ ȿ�� ã�Ƽ� ������ ����
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out chromaticAberration);
    }

    private void Start()
    {
        // ������ �� ��� UI, ���� ��Ȱ��ȭ
        choicePanel.SetActive(false);
        if (glitchEffect != null) glitchEffect.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);

        // �ƾ� ��� ����
        StartCoroutine(PlayCutScenes());
    }

    // ======= ���� �ƾ� ���� �ڷ�ƾ =======
    private IEnumerator PlayCutScenes()
    {
        // �ƾ� ����Ʈ ���鼭 ��� (�Է� �� ��ŵ)
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f));
            if (i == choiceCutIndex)
            {
                // ������ ���� Ÿ�̹�
                yield return ShowChoice();
            }
        }

        // �б�(���� ��Ʈ) �ƾ� ���
        for (int i = 0; i < branchImages.Count && i < branchTexts.Count; i++)
        {
            cutSceneImage.sprite = branchImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, branchTexts[i], 0.04f));
        }

        // ���� ũ���� ����
        yield return ShowCredits();

        // ���θ޴��� �̵�
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    // ======= Ÿ���� + �Է� �� ��� �Ϸ� (�Է� ���) =======
    private IEnumerator WaitForTextInputTyper(TextMeshProUGUI textUI, string text, float speed)
    {
        // TextTyper���� done �ݹ� ������ ���� ������
        bool done = false;
        StartCoroutine(TextTyper.TypeText(textUI, text, speed, () => done));
        while (!done)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                done = true;
            }
            yield return null;
        }
    }

    // ======= ������ ���� + �б� ó�� =======
    private IEnumerator ShowChoice()
    {
        choicePanel.SetActive(true);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false); // �� ��ư ���� �̸� OFF

        bool realChosen = false;
        bool fakeTriggered = false;

        // �� ��ư Ŭ�� �̺�Ʈ
        realChoiceBtn.onClick.AddListener(() => { realChosen = true; });

        // �ƴϿ� ��ư Ŭ�� �̺�Ʈ (���� ����)
        fakeChoiceBtn.onClick.AddListener(() => { if (!fakeTriggered) StartCoroutine(FakeChoiceBrokenCutScenes(() => { fakeTriggered = true; })); });

        // ��� : (���� ��Ʈ���) �� ������ ���� / �ƴϿ�� ������ �ݰ�-����-������ ��Ʈ��
        while (!realChosen)
        {
            yield return null;
        }

        // �� ��ư ���� ����, ����/������ OFF (Ȥ�� ����������)
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;

        // ������ �г�/���� ��� OFF
        choicePanel.SetActive(false);

        // ��ư ������ ����
        realChoiceBtn.onClick.RemoveAllListeners();
        fakeChoiceBtn.onClick.RemoveAllListeners();
    }

    // ======= "�ƴϿ�" ��ư Ŭ�� �� �ݰ����������濹���� ���� =======
    private IEnumerator FakeChoiceBrokenCutScenes(System.Action onGlitchComplete)
    {
        // 1. �ƴϿ� ��ư ��� ��Ȱ��ȭ
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);

        // 2. �ݰ� �̹��� + ������ ȿ�� ON
        cutSceneImage.sprite = brokenGlass1;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 1.0f;
        if (glitchEffect != null) glitchEffect.SetActive(true); // �ʿ�� ����/������ �� �߰�
        if (cutSceneText != null) cutSceneText.text = ""; // �ؽ�Ʈ�� ���� �� ����

        yield return new WaitForSeconds(0.7f);

        // 3. ������ �̹����� ���� (������ ����)
        cutSceneImage.sprite = brokenGlass2;
        if (glitchEffect != null) glitchEffect.SetActive(false); // 2�ܰ� ������ off ����

        yield return new WaitForSeconds(0.7f);

        // 4. �� ��ư ����(Glow ��) ON
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);

        // 5. �ݹ����� ���� �� �˸�
        onGlitchComplete?.Invoke();
    }

    // ======= ���� ũ���� (��ŵ �Ұ�, ������ ������ ������) =======
    private IEnumerator ShowCredits()
    {
        creditsPanel.SetActive(true);
        float y = 0f;
        creditsScroll.verticalNormalizedPosition = 0f;
        bool finished = false;

        while (!finished)
        {
            float speed = Input.anyKey ? creditsFastSpeed : creditsNormalSpeed;
            y += speed * Time.deltaTime / creditsScroll.content.rect.height;
            creditsScroll.verticalNormalizedPosition = y;
            if (y >= 1f) finished = true;
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);
        creditsPanel.SetActive(false);
    }
}
