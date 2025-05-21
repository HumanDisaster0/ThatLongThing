using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering.LookDev;

/// <summary>
/// 엔딩 씬 전용 매니저 - 정규 컷씬, 선택지(분기), 깨지는 컷신(스프라이트), 엔딩 크레딧까지 통합 관리
/// </summary>
public class EndingSceneManager : MonoBehaviour
{
    // ===== 컷씬 데이터 (정규 루트) =====
    public List<Sprite> cutSceneImages;           // 컷씬 이미지
    [TextArea]
    public List<string> cutSceneTexts;            // 컷씬 텍스트

    // ===== 선택지 등장 위치 =====
    public int choiceCutIndex = 3;                // 선택지 등장 인덱스

    // ===== 분기(정규루트) 컷씬 데이터 =====
    public List<Sprite> branchImages;             // 선택 후 이어지는 컷씬 이미지
    [TextArea]
    public List<string> branchTexts;              // 선택 후 이어지는 컷씬 텍스트

    // ===== UI/오브젝트 참조 =====
    public Image cutSceneImage;                   // 컷씬 이미지 출력용
    public TextMeshProUGUI cutSceneText;          // 컷씬 텍스트 출력용
    public GameObject choicePanel;                // 선택지(버튼) 패널
    public Button fakeChoiceBtn;                  // "아니요" 버튼
    public Button realChoiceBtn;                  // "예" 버튼
    public GameObject realChoiceHighlight;        // "예" 강조효과(Glow 등)

    // ===== 깨지는 컷신(금감/조각남) =====
    public Sprite brokenGlass1;                   // 금감(1단계)
    public Sprite brokenGlass2;                   // 조각남(2단계)

    // ===== 색수차(Chromatic Aberration) =====
    public Volume postProcessVolume;              // Global Volume (Inspector 연결)
    private ChromaticAberration chromaticAberration;

    // ===== 엔딩 크레딧 =====
    public GameObject creditsPanel;               // 크레딧 전체 패널
    public ScrollRect creditsScroll;              // 크레딧 스크롤뷰
    public float creditsNormalSpeed = 30f;        // 기본 스크롤 속도
    public float creditsFastSpeed = 100f;         // 버튼 누르면 빨라지는 속도

    public string mainMenuSceneName = "MainMenu"; // 엔딩 후 메인메뉴 씬 이름

    private void Awake()
    {
        // 색수차(Chromatic Aberration) 효과 캐싱
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out chromaticAberration);

        postProcessVolume.gameObject.SetActive(false);
    }

    private void Start()
    {
        // 모든 UI/연출 기본 비활성화
        choicePanel.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);

        // 엔딩 컷씬 재생 시작!
        StartCoroutine(PlayCutScenes());
    }

    /// <summary>
    /// 엔딩 컷씬 메인 진행 (정규컷→선택지→분기→크레딧)
    /// </summary>
    private IEnumerator PlayCutScenes()
    {
        // 1. 컷씬 리스트 돌며 출력(입력시 스킵)
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f));

            if (i == choiceCutIndex)
            {
                // 2. 선택지 등장(분기)
                yield return ShowChoice();
            }
        }

        // 3. 분기(정규루트) 컷씬 출력
        for (int i = 0; i < branchImages.Count && i < branchTexts.Count; i++)
        {
            cutSceneImage.sprite = branchImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, branchTexts[i], 0.04f));
        }

        // 4. 엔딩 크레딧
        yield return ShowCredits();

        // 5. 메인메뉴로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// 타이핑 + 입력시 즉시완료(입력 대기)
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
    /// 선택지 패널 + 분기 연출
    /// "아니요" 누르면 깨짐 컷신(금감→조각남→예강조)
    /// "예" 누르면 정상루트 진행
    /// </summary>
    private IEnumerator ShowChoice()
    {
        choicePanel.SetActive(true);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false); // 예 강조 OFF

        bool realChosen = false;
        bool fakeTriggered = false;

        // "예" 클릭 이벤트
        realChoiceBtn.onClick.AddListener(() => { realChosen = true; });

        // "아니요" 클릭 이벤트(깨짐 컷신 연출)
        fakeChoiceBtn.onClick.AddListener(() =>
        {
            if (!fakeTriggered)
            {
                StartCoroutine(FakeChoiceBrokenCutScenes(() => { fakeTriggered = true; }));
            }
        });

        // "예"가 눌릴 때까지 대기(깨짐 연출 중에는 fakeChoiceBtn이 사라지므로 무한루프X)
        while (!realChosen)
        {
            yield return null;
        }

        // =========== 여기서 끄기! ===========
        // 강조/색수차 OFF(혹시 남아있으면)
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (postProcessVolume != null) postProcessVolume.gameObject.SetActive(false);

        // 선택지 패널 OFF
        choicePanel.SetActive(false);

        // 리스너 해제
        realChoiceBtn.onClick.RemoveAllListeners();
        fakeChoiceBtn.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// "아니요" 선택 시 : 금감→조각남→예강조
    /// </summary>
    private IEnumerator FakeChoiceBrokenCutScenes(System.Action onBrokenComplete)
    {
        // 1. 아니요 버튼 비활성화(숨김)
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);

        // 2. 볼륨 오브젝트 ON
        if (postProcessVolume != null)
            postProcessVolume.gameObject.SetActive(true);

        EndingSceneCamera camShake = Camera.main.GetComponent<EndingSceneCamera>();
        if (camShake != null)
            yield return StartCoroutine(camShake.Shake(0.2f, 0.3f)); // (지속시간, 세기)

        // 3. 금감 이미지 + 색수차 효과(Chromatic Aberration) ON
        cutSceneImage.sprite = brokenGlass1;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0.3f;
        if (cutSceneText != null) cutSceneText.text = ""; // 텍스트 숨김

        yield return new WaitForSeconds(0.7f);

        // 4. 조각남 이미지로 변경 (색수차 유지)
        if (camShake != null)
            yield return StartCoroutine(camShake.Shake(0.7f, 0.5f)); // (지속시간, 세기)
        cutSceneImage.sprite = brokenGlass2;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 1.0f;
        yield return new WaitForSeconds(0.7f);

        // 5. 예 버튼 강조(Glow 등) ON
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);

        // 7. 연출 완료 콜백
        onBrokenComplete?.Invoke();
    }

    /// <summary>
    /// 엔딩 크레딧 연출(스킵불가/누르면 빨라짐)
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
