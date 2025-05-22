using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 엔딩 씬 전용 매니저 - 정규 컷씬, 선택지(분기), 깨지는 컷신(스프라이트), 엔딩 크레딧까지 통합 관리
/// </summary>
public class EndingSceneManager : MonoBehaviour
{
    [Header("오토모드 설정")]
    public bool autoPlayMode = false;
    public float autoPlayDelay = 1.0f;  // 한 컷당 대기 시간

    // ===== 컷씬 데이터 (정규 루트) =====
    public List<Sprite> cutSceneImages;           // 컷씬 이미지
    [TextArea]
    public List<string> cutSceneTexts;            // 컷씬 텍스트

    // ===== 선택지 등장 위치 =====
    public int choiceCutIndex = 3;                // 선택지 등장 인덱스
    public int shakeCutIndex = 5;                // 선택지 등장 인덱스

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

    private AudioSource bgmSrc; // BGM 소스

    private void Awake()
    {
        // 색수차(Chromatic Aberration) 효과 캐싱
        if (postProcessVolume != null)
            postProcessVolume.profile.TryGet(out chromaticAberration);

        postProcessVolume.gameObject.SetActive(false);

        // BGM 재생
        //bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending_BGM");
        //bgmSrc = BgmPlayer.instance.ChangeBgm("Ending_BGM");
    }

    private void Start()
    {
        autoPlayMode = true;
        autoPlayDelay = 2.0f;

        // 모든 UI/연출 기본 비활성화
        choicePanel.SetActive(false);
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(false);
        creditsPanel.SetActive(false);

        // 엔딩 컷씬 재생 시작!
        StartCoroutine(PlayCutScenes());

        // BGM 재생
        bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending2_BGM");
        DefaultSourceData data = bgmSrc.GetComponent<DefaultSourceData>();
        data.soundType = SoundType.Bg;
        data.volOverride = 0.5f;
        //bgmSrc = BgmPlayer.instance.ChangeBgm("Ending_BGM");
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
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, cutSceneTexts[i], 0.04f, accelerateInsteadOfSkip: true));

            if (i == choiceCutIndex)
            {
                // 2. 선택지 등장(분기)
                yield return ShowChoice();
            }

            if (i == (choiceCutIndex + 1))
            {
                yield return new WaitForSeconds(1.0f);  // 자동 대기
            }

            if (i == shakeCutIndex)
            {
                EndingSceneCamera camShake = Camera.main.GetComponent<EndingSceneCamera>();
                if (camShake != null)
                {
                    AudioSource src = SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
                    StartCoroutine(camShake.LerpShake(1.5f, 0.5f, 0.0f)); // 전체 지속시간, 시작세기, 종료세기  
                }
                      
            }

            if (i == (shakeCutIndex + 1))
            {
                yield return new WaitForSeconds(1.0f);  // 자동 대기
                SoundManager.instance.StopSound(bgmSrc);    // BGM 정지
            }

            if (i == shakeCutIndex + 2)
            {
                // 2번째 BGM 재생
                bgmSrc = SoundManager.instance?.PlayLoopBackSound("Ending_BGM");
                bgmSrc.GetComponent<DefaultSourceData>().soundType = SoundType.Bg;
            }
        }

        // 3. 분기(정규루트) 컷씬 출력
        for (int i = 0; i < branchImages.Count && i < branchTexts.Count; i++)
        {
            cutSceneImage.sprite = branchImages[i];
            yield return StartCoroutine(WaitForTextInputTyper(cutSceneText, branchTexts[i], 0.04f, accelerateInsteadOfSkip: true));
        }

        // 4. 엔딩 크레딧
        yield return ShowCredits();

        // 5. 메인메뉴로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// 타이핑 + 입력시 즉시완료(입력 대기)
    /// </summary>
    private IEnumerator WaitForTextInputTyper(TextMeshProUGUI textUI, string text, float baseSpeed, bool accelerateInsteadOfSkip = false)
    {
        yield return StartCoroutine(TextTyper.TypeText(textUI, text, baseSpeed, accelerateInsteadOfSkip));

        if (autoPlayMode)
        {
            yield return new WaitForSeconds(autoPlayDelay);  // 자동 대기
        }
        else
        {
            // 수동 입력 대기
            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));
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
        realChoiceBtn.onClick.AddListener(() => 
        { 
            if(!bgmSrc.isPlaying)
                bgmSrc.UnPause();

            SoundManager.instance.PlayNewBackSound("Glass_Break", SoundType.Se);
            realChosen = true; });

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
        // 0. BGM 일시정지
        bgmSrc.Pause();

        // 1. 아니요 버튼 비활성화(숨김)
        fakeChoiceBtn.interactable = false;
        fakeChoiceBtn.gameObject.SetActive(false);
        realChoiceBtn.interactable = false;

        // 2. 볼륨 오브젝트 ON
        if (postProcessVolume != null)
            postProcessVolume.gameObject.SetActive(true);

        EndingSceneCamera camShake = Camera.main.GetComponent<EndingSceneCamera>();

        // 흔들리는 사운드 출력
        SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);

        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(0.7f, 1.0f, 0.0f)); // 전체 지속시간, 시작세기, 종료세기    
        // 3. 금감 이미지 + 색수차 효과(Chromatic Aberration) ON
        yield return new WaitForSeconds(0.3f);
        SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
        cutSceneImage.sprite = brokenGlass1;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0.3f;
        if (cutSceneText != null) cutSceneText.text = ""; // 텍스트 숨김

        // 4. 조각남 이미지로 변경 (색수차 유지)
        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(1.2f, 2.0f, 0.0f)); // 전체 지속시간, 시작세기, 종료세기    

        SoundManager.instance?.PlayNewBackSound("Trex_Land", SoundType.Se);
        SoundManager.instance?.PlayNewBackSound("Glass_Crack", SoundType.Se);
        cutSceneImage.sprite = brokenGlass2;
        if (chromaticAberration != null) chromaticAberration.intensity.value = 1.0f;

        if (camShake != null)
            yield return StartCoroutine(camShake.LerpShake(0.3f, 0.7f, 0.0f)); // 전체 지속시간, 시작세기, 종료세기    
        yield return new WaitForSeconds(0.7f);


        // 5. 예 버튼 강조(Glow 등) ON
        if (realChoiceHighlight != null) realChoiceHighlight.SetActive(true);

        // 7. 연출 완료 콜백
        realChoiceBtn.interactable = true;
        onBrokenComplete?.Invoke();
    }

    /// <summary>
    /// 엔딩 크레딧 연출(스킵불가/누르면 빨라짐)
    /// </summary>
    private IEnumerator ShowCredits()
    {
        // 1. 크레딧 패널 아래로 배치
        RectTransform creditRect = creditsPanel.GetComponent<RectTransform>();
        Vector2 targetPos = creditRect.anchoredPosition;
        Vector2 startPos = new Vector2(targetPos.x, -Screen.height); // 아래로 보냄
        creditRect.anchoredPosition = startPos;

        Transform creditTextObj = creditsScroll.content.GetChild(0);
        creditTextObj.gameObject.SetActive(false);
        // 2. 활성화
        creditsPanel.SetActive(true);

        // 3. 위로 이동 트윈
        float duration = 3.0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.SmoothStep(0f, 1f, t / duration); // 부드럽게
            creditRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, lerpT);
            yield return null;
        }

        // 4. 정확히 도착 위치 고정
        creditRect.anchoredPosition = targetPos;
        creditTextObj.gameObject.SetActive(true);
        // 5. 스크롤 크레딧 시작
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
