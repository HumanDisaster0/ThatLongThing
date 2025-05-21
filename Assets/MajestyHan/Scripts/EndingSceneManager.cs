using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EndingSceneManager : MonoBehaviour
{
    // ========== 컷씬 데이터 ==========
    public List<Sprite> cutSceneImages; // 정규 컷씬 이미지 리스트
    [TextArea]
    public List<string> cutSceneTexts;  // 정규 컷씬 텍스트 리스트

    // ========== 선택지 등장 인덱스 ==========
    public int choiceCutIndex = 3; // 몇번째 컷에서 선택지 등장할지

    // ========== 분기(정규루트) 컷씬 데이터 ==========
    public List<Sprite> branchImages;   // 선택 이후 이어지는 컷씬 이미지 리스트
    [TextArea]
    public List<string> branchTexts;    // 선택 이후 이어지는 컷씬 텍스트 리스트

    // ========== UI 참조 ==========
    public Image cutSceneImage;             // 컷씬용 이미지
    public TextMeshProUGUI cutSceneText;    // 컷씬용 텍스트
    public GameObject choicePanel;          // 선택지 패널 (예/아니요 버튼 포함)
    public Button fakeChoiceBtn;            // "아니요" 버튼
    public Button realChoiceBtn;            // "예" 버튼
    public GameObject realChoiceHighlight;  // "예" 버튼 강조 연출(Glow 등)
    public GameObject glitchEffect;         // 글리치/이펙트 오브젝트(안쓰면 비워둠)

    // ========== 깨지는 컷신(유리 금감/조각남) ==========
    public Sprite brokenGlass1; // 금감(1단계)
    public Sprite brokenGlass2; // 조각남(2단계)

    // ========== 색수차 효과(Chromatic Aberration) ==========
    public Volume postProcessVolume; // Global Volume (Inspector에서 연결)
    private ChromaticAberration chromaticAberration;

    // ========== 엔딩 크레딧 ==========
    public GameObject creditsPanel;        // 크레딧 전체 패널
    public ScrollRect creditsScroll;       // 크레딧 스크롤
    public float creditsNormalSpeed = 30f; // 기본 스크롤 속도
    public float creditsFastSpeed = 100f;  // 빠른 스크롤 속도

    public string mainMenuSceneName = "MainMenu"; // 엔딩 후 돌아갈 씬 이름

    private void Awake()
    {
        // Volume에서 색수차 효과 찾아서 변수에 담음
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out chromaticAberration);
    }

    private void Start()
    {
        // 시작할 때 모든 UI, 연출 비활성화
        choicePanel.SetActive(false);
        if (glitchEffect != null) glitchEffect.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);

        // 컷씬 재생 시작
        StartCoroutine(PlayCutScenes());
    }

    // ======= 메인 컷씬 진행 코루틴 =======
    private IEnumerator PlayCutScenes()
    {
        // 컷씬 리스트 돌면서 출력 (입력 시 스킵)
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f));
            if (i == choiceCutIndex)
            {
                // 선택지 등장 타이밍
                yield return ShowChoice();
            }
        }

        // 분기(정규 루트) 컷씬 출력
        for (int i = 0; i < branchImages.Count && i < branchTexts.Count; i++)
        {
            cutSceneImage.sprite = branchImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, branchTexts[i], 0.04f));
        }

        // 엔딩 크레딧 연출
        yield return ShowCredits();

        // 메인메뉴로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    // ======= 타이핑 + 입력 시 즉시 완료 (입력 대기) =======
    private IEnumerator WaitForTextInputTyper(TextMeshProUGUI textUI, string text, float speed)
    {
        // TextTyper에서 done 콜백 받으면 다음 컷으로
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

    // ======= 선택지 연출 + 분기 처리 =======
    private IEnumerator ShowChoice()
    {
        choicePanel.SetActive(true);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false); // 예 버튼 강조 미리 OFF

        bool realChosen = false;
        bool fakeTriggered = false;

        // 예 버튼 클릭 이벤트
        realChoiceBtn.onClick.AddListener(() => { realChosen = true; });

        // 아니요 버튼 클릭 이벤트 (깨짐 연출)
        fakeChoiceBtn.onClick.AddListener(() => { if (!fakeTriggered) StartCoroutine(FakeChoiceBrokenCutScenes(() => { fakeTriggered = true; })); });

        // 대기 : (정상 루트라면) 예 누르면 진행 / 아니요면 강제로 금감-깨짐-예강조 루트로
        while (!realChosen)
        {
            yield return null;
        }

        // 예 버튼 눌린 직후, 강조/색수차 OFF (혹시 남아있으면)
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;

        // 선택지 패널/연출 모두 OFF
        choicePanel.SetActive(false);

        // 버튼 리스너 해제
        realChoiceBtn.onClick.RemoveAllListeners();
        fakeChoiceBtn.onClick.RemoveAllListeners();
    }

    // ======= "아니요" 버튼 클릭 시 금감→조각남→예강조 연출 =======
    private IEnumerator FakeChoiceBrokenCutScenes(System.Action onGlitchComplete)
    {
        // 1. 아니요 버튼 즉시 비활성화
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);

        // 2. 금감 이미지 + 색수차 효과 ON
        cutSceneImage.sprite = brokenGlass1;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 1.0f;
        if (glitchEffect != null) glitchEffect.SetActive(true); // 필요시 깨짐/노이즈 등 추가
        if (cutSceneText != null) cutSceneText.text = ""; // 텍스트는 연출 중 숨김

        yield return new WaitForSeconds(0.7f);

        // 3. 조각남 이미지로 변경 (색수차 유지)
        cutSceneImage.sprite = brokenGlass2;
        if (glitchEffect != null) glitchEffect.SetActive(false); // 2단계 연출은 off 가능

        yield return new WaitForSeconds(0.7f);

        // 4. 예 버튼 강조(Glow 등) ON
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);

        // 5. 콜백으로 연출 끝 알림
        onGlitchComplete?.Invoke();
    }

    // ======= 엔딩 크레딧 (스킵 불가, 누르고 있으면 빨라짐) =======
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
