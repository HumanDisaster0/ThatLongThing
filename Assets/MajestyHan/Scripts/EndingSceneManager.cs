using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering.LookDev;

/// <summary>
/// ���� �� ���� �Ŵ��� - ���� �ƾ�, ������(�б�), ������ �ƽ�(��������Ʈ), ���� ũ�������� ���� ����
/// </summary>
public class EndingSceneManager : MonoBehaviour
{
    // ===== �ƾ� ������ (���� ��Ʈ) =====
    public List<Sprite> cutSceneImages;           // �ƾ� �̹���
    [TextArea]
    public List<string> cutSceneTexts;            // �ƾ� �ؽ�Ʈ

    // ===== ������ ���� ��ġ =====
    public int choiceCutIndex = 3;                // ������ ���� �ε���

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

    private void Awake()
    {
        // ������(Chromatic Aberration) ȿ�� ĳ��
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out chromaticAberration);

        postProcessVolume.gameObject.SetActive(false);
    }

    private void Start()
    {
        // ��� UI/���� �⺻ ��Ȱ��ȭ
        choicePanel.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);

        // ���� �ƾ� ��� ����!
        StartCoroutine(PlayCutScenes());
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
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f));

            if (i == choiceCutIndex)
            {
                // 2. ������ ����(�б�)
                yield return ShowChoice();
            }
        }

        // 3. �б�(���Է�Ʈ) �ƾ� ���
        for (int i = 0; i < branchImages.Count && i < branchTexts.Count; i++)
        {
            cutSceneImage.sprite = branchImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, branchTexts[i], 0.04f));
        }

        // 4. ���� ũ����
        yield return ShowCredits();

        // 5. ���θ޴��� �̵�
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Ÿ���� + �Է½� ��ÿϷ�(�Է� ���)
    /// </summary>
    private IEnumerator WaitForTextInputTyper(TextMeshProUGUI textUI, string text, float speed)
    {
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
        realChoiceBtn.onClick.AddListener(() => { realChosen = true; });

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
        // 1. �ƴϿ� ��ư ��Ȱ��ȭ(����)
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);

        // 2. ���� ������Ʈ ON
        if (postProcessVolume != null)
            postProcessVolume.gameObject.SetActive(true);

        EndingSceneCamera camShake = Camera.main.GetComponent<EndingSceneCamera>();
        if (camShake != null)
            yield return StartCoroutine(camShake.Shake(0.2f, 0.3f)); // (���ӽð�, ����)

        // 3. �ݰ� �̹��� + ������ ȿ��(Chromatic Aberration) ON
        cutSceneImage.sprite = brokenGlass1;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0.3f;
        if (cutSceneText != null) cutSceneText.text = ""; // �ؽ�Ʈ ����

        yield return new WaitForSeconds(0.7f);

        // 4. ������ �̹����� ���� (������ ����)
        if (camShake != null)
            yield return StartCoroutine(camShake.Shake(0.7f, 0.5f)); // (���ӽð�, ����)
        cutSceneImage.sprite = brokenGlass2;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 1.0f;
        yield return new WaitForSeconds(0.7f);

        // 5. �� ��ư ����(Glow ��) ON
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);

        // 7. ���� �Ϸ� �ݹ�
        onBrokenComplete?.Invoke();
    }

    /// <summary>
    /// ���� ũ���� ����(��ŵ�Ұ�/������ ������)
    /// </summary>
    private IEnumerator ShowCredits()
    {
        creditsPanel.SetActive(true);
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
