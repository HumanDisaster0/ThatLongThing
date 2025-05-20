using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuildCounter : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    public TextMeshPro whatDidYouText;        // ment > whatdidyou
    public CustomClickable[] choices;         // choice1 ~ choice3
    public TextMeshPro[] choiceTexts;         // choice1~3 ���� text_c_1~3
    public GameObject answerBox;              // answer ������Ʈ
    public TextMeshPro answerText;            // answer > ansText

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;

    public List<QuestionResult> quizResults = new List<QuestionResult>(); // �ܺ� ���ٿ� ���

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
        whatDidYouText.text = "�׽�Ʈ ����";
        answerBox.SetActive(false);
    }

    void SetupClickEvents()
    {
        for (int i = 0; i < choices.Length; i++)
        {
            int index = i; // Ŭ���� ����
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

        // ��� ����
        quizResults.Add(new QuestionResult
        {
            questionIndex = currentQuestionIndex,
            isCorrect = (choiceIndex == q.correctIndex)
        });

        // ������ ��Ȱ��ȭ
        foreach (var btn in choices)
            btn.gameObject.SetActive(false);

        answerText.text = q.characterComment[choiceIndex];
        answerBox.SetActive(true);
        isAnswerRevealed = true;
    }

    void ShowNpcReply()
    {
        QuestionData q = questions[currentQuestionIndex];
        whatDidYouText.text = q.npcReplies[q.correctIndex]; // ���� ���� ���

        // ���� ������ �Ѿ�� �ʹٸ� ���⼭ index ����
        // currentQuestionIndex++;
        // ShowQuestion(currentQuestionIndex);
    }
}
