using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GuildCounter : MonoBehaviour
{
    //public static GuildCounter Instance;

    [Header("UI 오브젝트")]
    public TextMeshPro whatDidYouText;
    public CustomClickable[] choices;
    public TextMeshPro[] choiceTexts;
    public GameObject answerBox;
    public TextMeshPro answerText;

    public CustomClickable itemA;
    public CustomClickable itemB;
    public CustomClickable itemC;


    public CustomClickable closeButton;
    //public CustomClickable cMissionBoard;
    //public CustomClickable cAlbum;
    //public CustomClickable cTalk;

    [Header("중간 결과 발표용")]
    public GameObject trollPanel;            // 중간 결과 발표용 패널 (비활성 상태로 시작)
    public TextMeshPro trollText;            // 결과 요약 텍스트
    public GameObject trollImage;  // 이미지 등 효과

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;


    private int qIdx = 0;
    public bool isEnd = false;

    public int sIndex = 0;

    private int totalSolvedCount = 0;         // 전체 푼 문제 수


    //private GuildRoomManager guildRoomManager;



    void Awake()
    {

        //guildRoomManager = FindObjectOfType<GuildRoomManager>();
        Transform closeBtnTransform = transform.Find("Counter_exit");
        if (closeBtnTransform != null)
        {
            closeButton = closeBtnTransform.GetComponent<CustomClickable>();
            closeButton.SetClickAction(OnCloseButtonClicked);
        }

        //Transform mbBtnTransform = transform.Find("a_mboard");
        //if (mbBtnTransform != null)
        //{
        //    cMissionBoard = mbBtnTransform.GetComponent<CustomClickable>();
        //    cMissionBoard.SetClickAction(OnCloseButtonClicked);
        //}

        //Transform abBtnTransform = transform.Find("b_album");
        //if (abBtnTransform != null)
        //{
        //    cAlbum = abBtnTransform.GetComponent<CustomClickable>();
        //    cAlbum.SetClickAction(OnCloseButtonClicked);
        //}

        //Transform tkBtnTransform = transform.Find("c_talk");
        //if (tkBtnTransform != null)
        //{
        //    cTalk = tkBtnTransform.GetComponent<CustomClickable>();
        //    cTalk.SetClickAction(OnCloseButtonClicked);
        //}


    }

    public void Start()
    {

        // 문제번호 계산
        int missionCode = GuildRoomManager.Instance.selectedMission;
        qIdx = missionCode / 1000;

        //Debug.Log($"문제번호: {qIdx}  {missionCode}");

        // 중간결과 확인       
        isEnd = false;

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
        {
            ResetAll();
        }
    }

    private void ResetAll()
    {
        questions = null;

        currentQuestionIndex = 0;
        isAnswerRevealed = false;


        qIdx = 0;
        isEnd = false;

        sIndex = 0;
    }

    public void StartQuiz(int pIdx)
    {
        Debug.Log($"pIdx = {pIdx}");
        InitUI();

        if (questions == null)
            questions = CSVLoader.LoadQuestions();

        currentQuestionIndex = pIdx;
        ShowQuestion(currentQuestionIndex);
        SetupClickEvents();
    }

    void InitUI()
    {
        whatDidYouText.text = "이번 의뢰 수고 많으셨습니다.\n의뢰의 대상은 결국 무엇이었나요?";
        answerBox.SetActive(false);
        trollPanel.SetActive(false);
        answerBox.SetActive(false);

        foreach (var c in choices)
            c.gameObject.SetActive(true);
    }

    void SetupClickEvents()
    {

        for (int i = 0; i < choices.Length; i++)
        {
            int index = i;
            choices[i].SetClickAction(() => OnChoiceSelected(index));
        }
    }

    void ShowQuestion(int index)
    {
        var q = questions[index];

        for (int i = 0; i < 3; i++)
        {
            choiceTexts[i].text = q.choices[i];
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        answerBox.SetActive(true);
        sIndex = choiceIndex;
        var q = questions[currentQuestionIndex];

        GuildRoomManager.Instance.quizResults.Add(new QuestionResult
        {
            questionIndex = currentQuestionIndex,
            isCorrect = choiceIndex == q.correctIndex
        });

        Debug.Log($"답안지 길이 {GuildRoomManager.Instance.quizResults.Count}");

        foreach (var btn in choices)
            btn.gameObject.SetActive(false);

        answerText.text = q.characterComment[choiceIndex];
        answerBox.SetActive(true);
        isAnswerRevealed = true;

        totalSolvedCount++;

        for (int i = Mathf.Max(GuildRoomManager.Instance.quizResults.Count - 3, 0); i < GuildRoomManager.Instance.quizResults.Count; i++)
        {
            Debug.Log($"{i + 1}번째 {GuildRoomManager.Instance.quizResults[i].isCorrect}");

        }

        GoldManager.Instance.PlusGold(30);

        // 문제 번호 초기화        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isAnswerRevealed)
            {
                whatDidYouText.text = questions[currentQuestionIndex].npcReplies[sIndex];
                isAnswerRevealed = false;
                isEnd = true;
            }
        }
    }

    public void ShowMidResult(int wrong)
    {
        trollPanel.SetActive(true);

        trollText.text = $"데비씨... 당신이 보고한 이상 현상 중 {wrong}개가 잘못 보고되어, 대비하지 못한 모험가들의 피해가 속출하고 있습니다.\n길드의 규칙에 따라 벌금 {wrong * 40}골드 부과하겠습니다!";
        // resultImage.sprite = ...; // 이미지 변경도 가능

        // 퀴즈 시작은 외부에서 resultPanel 종료 이후 StartQuiz() 수동 호출
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
        GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);

        GuildRoomManager.Instance.cState = GuildRoomManager.counterState.NONE;
        GuildRoomManager.Instance.preCState = GuildRoomManager.counterState.NONE;
    }

    public void btnOnOff(bool onoff)
    {
        if (onoff)
        {
            MarketManager.Instance.UpdateItemStatus();

            closeButton.isInteractable = true;
        }
        else
        {            
            itemA.isInteractable = false;
            itemB.isInteractable = false;
            itemC.isInteractable = false;
            closeButton.isInteractable = false;
        }
    }
}
