using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuildCounter : MonoBehaviour
{
    [Header("UI 오브젝트")]
    public TextMeshPro whatDidYouText;
    public CustomClickable[] choices;
    public TextMeshPro[] choiceTexts;
    public GameObject answerBox;
    public TextMeshPro answerText;


    public CustomClickable closeButton;
    public CustomClickable cMissionBoard;
    public CustomClickable cAlbum;
    public CustomClickable cTalk;

    [Header("중간 결과 발표용")]
    public GameObject resultPanel;            // 중간 결과 발표용 패널 (비활성 상태로 시작)
    public TextMeshPro resultText;            // 결과 요약 텍스트
    public Sprite resultImage;  // 이미지 등 효과

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;
    private int totalSolvedCount = 0;         // 전체 푼 문제 수

    private int qIdx = 0;
    public bool isEnd = false;

    public int sIndex = 0;


    //private GuildRoomManager guildRoomManager;

    public List<QuestionResult> quizResults = new List<QuestionResult>();

    void Awake()
    {
        //guildRoomManager = FindObjectOfType<GuildRoomManager>();
        Transform closeBtnTransform = transform.Find("Counter_exit");
        if (closeBtnTransform != null)
        {
            closeButton = closeBtnTransform.GetComponent<CustomClickable>();
            closeButton.SetClickAction(OnCloseButtonClicked);
        }

        Transform mbBtnTransform = transform.Find("a_mboard");
        if (mbBtnTransform != null)
        {
            cMissionBoard = mbBtnTransform.GetComponent<CustomClickable>();
            cMissionBoard.SetClickAction(OnCloseButtonClicked);
        }

        Transform abBtnTransform = transform.Find("b_album");
        if (abBtnTransform != null)
        {
            cAlbum = abBtnTransform.GetComponent<CustomClickable>();
            cAlbum.SetClickAction(OnCloseButtonClicked);
        }

        Transform tkBtnTransform = transform.Find("c_talk");
        if (tkBtnTransform != null)
        {
            cTalk = tkBtnTransform.GetComponent<CustomClickable>();
            cTalk.SetClickAction(OnCloseButtonClicked);
        }


    }

    public void Start()
    {

        // 문제번호 계산
        int missionCode = GuildRoomManager.Instance.selectedMission;
        qIdx = missionCode / 1000;

        //Debug.Log($"문제번호: {qIdx}  {missionCode}");

        // 중간결과 확인
        if ((totalSolvedCount + 1) % 3 == 0)
        {
            ShowMidResult();
            return;
        }
        isEnd = false;

    }

    void OnEnable()
    {


    }

    public void StartQuiz()
    {
        InitUI();

        if (questions == null)
            questions = CSVLoader.LoadQuestions();

        currentQuestionIndex = qIdx;
        ShowQuestion(currentQuestionIndex);
        SetupClickEvents();
    }

    void InitUI()
    {
        whatDidYouText.text = "이번 의뢰 수고 많으셨습니다.\n의뢰의 대상은 결국 무엇이었나요?";
        answerBox.SetActive(false);
        resultPanel.SetActive(false);
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

        quizResults.Add(new QuestionResult
        {
            questionIndex = currentQuestionIndex,
            isCorrect = choiceIndex == q.correctIndex
        });

        foreach (var btn in choices)
            btn.gameObject.SetActive(false);

        answerText.text = q.characterComment[choiceIndex];
        answerBox.SetActive(true);
        isAnswerRevealed = true;
        totalSolvedCount++;

        GoldManager.Instance.PlusGold(30);

        // 문제 번호 초기화        
    }

    void Update()
    {

        if (isAnswerRevealed && Input.GetMouseButtonDown(0))
        {
            whatDidYouText.text = questions[currentQuestionIndex].npcReplies[sIndex];
            isAnswerRevealed = false;
            isEnd = true;
        }
    }

    void ShowMidResult()
    {
        resultPanel.SetActive(true);

        int last3Start = Mathf.Max(quizResults.Count - 3, 0);
        int correctCount = 0;
        for (int i = last3Start; i < quizResults.Count; i++)
        {
            if (quizResults[i].isCorrect)
                correctCount++;
        }

        resultText.text = $"최근 3문제 중 {correctCount}개 정답!";
        // resultImage.sprite = ...; // 이미지 변경도 가능

        // 퀴즈 시작은 외부에서 resultPanel 종료 이후 StartQuiz() 수동 호출
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
        GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);

    }

    public void btnOnOff(bool onoff)
    {
        if (onoff)
        {
            Debug.Log("버튼 켜기");
            closeButton.isInteractable = true;
            cMissionBoard.isInteractable = true;
            cAlbum.isInteractable = true;
            cTalk.isInteractable = true;
        }
        else
        {
            Debug.Log("버튼 끄기");
            closeButton.isInteractable = false;
            cMissionBoard.isInteractable = false;
            cAlbum.isInteractable = false;
            cTalk.isInteractable = false;
        }
    }
}
