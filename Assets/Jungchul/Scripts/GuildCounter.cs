using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GuildCounter : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    public TextMeshPro whatDidYouText;
    public CustomClickable[] choices;
    public TextMeshPro[] choiceTexts;
    public GameObject answerBox;
    public TextMeshPro answerText;

    [Header("�߰� ��� ��ǥ��")]
    public GameObject resultPanel;            // �߰� ��� ��ǥ�� �г� (��Ȱ�� ���·� ����)
    public TextMeshPro resultText;            // ��� ��� �ؽ�Ʈ
    public Sprite resultImage;  // �̹��� �� ȿ��

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;
    private int totalSolvedCount = 0;         // ��ü Ǭ ���� ��

    private int qIdx = 0;                     // GuildRoomManager.selectedMission / 1000
    private GuildRoomManager guildRoomManager;

    public List<QuestionResult> quizResults = new List<QuestionResult>();

    void Awake()
    {
        guildRoomManager = FindObjectOfType<GuildRoomManager>();
    }

    void OnEnable()
    {
        // ������ȣ ���
        int missionCode = guildRoomManager.selectedMission;
        qIdx = missionCode / 1000;

        // �߰���� Ȯ��
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
        whatDidYouText.text = "�׽�Ʈ ����";
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

        // ���� ��ȣ �ʱ�ȭ
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

        resultText.text = $"�ֱ� 3���� �� {correctCount}�� ����!";
        // resultImage.sprite = ...; // �̹��� ���浵 ����

        // ���� ������ �ܺο��� resultPanel ���� ���� StartQuiz() ���� ȣ��
    }
}
