using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuildCounter : MonoBehaviour
{
    [Header("UI 오브젝트")]
    public TextMeshPro whatDidYouText;        // ment > whatdidyou
    public CustomClickable[] choices;         // choice1 ~ choice3
    public TextMeshPro[] choiceTexts;         // choice1~3 하위 text_c_1~3
    public GameObject answerBox;              // answer 오브젝트
    public TextMeshPro answerText;            // answer > ansText

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;

    public List<QuestionResult> quizResults = new List<QuestionResult>(); // 외부 접근용 결과

    void Start()
    {
        InitUI();
        questions = CSVLoader.LoadQuestions();
        ShowQuestion(currentQuestionIndex);
        SetupClickEvents();
    }

    void Update()
    {
        if (isAnswerRevealed && Input.GetMouseButtonDown(0))
        {
            ShowNpcReply();
            isAnswerRevealed = false;
        }
    }

    void InitUI()
    {
        whatDidYouText.text = "테스트 문제";
        answerBox.SetActive(false);
    }

    void SetupClickEvents()
    {
        for (int i = 0; i < choices.Length; i++)
        {
            int index = i; // 클로저 방지
            choices[i].SetClickAction(() => OnChoiceSelected(index));
        }
    }

    void ShowQuestion(int index)
    {
        if (index >= questions.Count) return;

        QuestionData q = questions[index];

        for (int i = 0; i < 3; i++)
        {
            choiceTexts[i].text = q.choices[i];
            choices[i].gameObject.SetActive(true);
        }

        answerBox.SetActive(false);
    }

    void OnChoiceSelected(int choiceIndex)
    {
        QuestionData q = questions[currentQuestionIndex];

        // 결과 저장
        quizResults.Add(new QuestionResult
        {
            questionIndex = currentQuestionIndex,
            isCorrect = (choiceIndex == q.correctIndex)
        });

        // 선택지 비활성화
        foreach (var btn in choices)
            btn.gameObject.SetActive(false);

        answerText.text = q.characterComment[choiceIndex];
        answerBox.SetActive(true);
        isAnswerRevealed = true;
    }

    void ShowNpcReply()
    {
        QuestionData q = questions[currentQuestionIndex];
        whatDidYouText.text = q.npcReplies[q.correctIndex]; // 정답 기준 출력

        // 다음 문제로 넘어가고 싶다면 여기서 index 증가
        // currentQuestionIndex++;
        // ShowQuestion(currentQuestionIndex);
    }
}
