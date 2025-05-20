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

    [Header("중간 결과 발표용")]
    public GameObject resultPanel;            // 중간 결과 발표용 패널 (비활성 상태로 시작)
    public TextMeshPro resultText;            // 결과 요약 텍스트
    public Sprite resultImage;  // 이미지 등 효과

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;
    private int totalSolvedCount = 0;         // 전체 푼 문제 수

    private int qIdx = 0;                     // GuildRoomManager.selectedMission / 1000
    private GuildRoomManager guildRoomManager;

    public List<QuestionResult> quizResults = new List<QuestionResult>();

    void Awake()
    {
        guildRoomManager = FindObjectOfType<GuildRoomManager>();
    }

    void OnEnable()
    {
        // 문제번호 계산
        int missionCode = guildRoomManager.selectedMission;
        qIdx = missionCode / 1000;

        // 중간결과 확인
        if ((totalSolvedCount + 1) % 3 == 0)
        {
            ShowMidResult();
            return;
        }

        StartQuiz();
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
        whatDidYouText.text = "테스트 문제";
        answerBox.SetActive(false);
        resultPanel.SetActive(false);

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

        answerBox.SetActive(false);
    }

    void OnChoiceSelected(int choiceIndex)
    {
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

        // 문제 번호 초기화
        guildRoomManager.selectedMission = 0;
    }

    void Update()
    {
        if (isAnswerRevealed && Input.GetMouseButtonDown(0))
        {
            whatDidYouText.text = questions[currentQuestionIndex].npcReplies[questions[currentQuestionIndex].correctIndex];
            isAnswerRevealed = false;
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
}
