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

    public CustomClickable closeButton;

    [Header("�߰� ��� ��ǥ��")]
    public GameObject resultPanel;            // �߰� ��� ��ǥ�� �г� (��Ȱ�� ���·� ����)
    public TextMeshPro resultText;            // ��� ��� �ؽ�Ʈ
    public Sprite resultImage;  // �̹��� �� ȿ��

    private List<QuestionData> questions;
    private int currentQuestionIndex = 0;
    private bool isAnswerRevealed = false;
    private int totalSolvedCount = 0;         // ��ü Ǭ ���� ��

    private int qIdx = 0;
    public bool isEnd = false;

    public int sIndex = 0;


    //private GuildRoomManager guildRoomManager;

    public List<QuestionResult> quizResults = new List<QuestionResult>();

    void Awake()
    {
        //guildRoomManager = FindObjectOfType<GuildRoomManager>();
       

    }

    void OnEnable()
    {
        Transform closeBtnTransform = transform.Find("Counter_exit");
        if (closeBtnTransform != null)
        {
            closeButton = closeBtnTransform.GetComponent<CustomClickable>();
            closeButton.SetClickAction(OnCloseButtonClicked);
        }

        // ������ȣ ���
        int missionCode = GuildRoomManager.Instance.selectedMission;
        qIdx = missionCode / 1000;

        //Debug.Log($"������ȣ: {qIdx}  {missionCode}");

        // �߰���� Ȯ��
        if ((totalSolvedCount + 1) % 3 == 0)
        {
            ShowMidResult();
            return;
        }
        isEnd = false;

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

        whatDidYouText.text = "�̹� �Ƿ� ���� �����̽��ϴ�.\n�Ƿ��� ����� �ᱹ �����̾�����?";
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

        // ���� ��ȣ �ʱ�ȭ        
    }

    void Update()
    {

        if (isAnswerRevealed && Input.GetMouseButtonDown(0))
        {
            //Debug.Log($"�����ε��� {currentQuestionIndex}");
            //Debug.Log($"������ �亯 {questions[currentQuestionIndex].npcReplies[questions[currentQuestionIndex].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[0].npcReplies[questions[0].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[1].npcReplies[questions[1].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[2].npcReplies[questions[2].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[3].npcReplies[questions[3].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[4].npcReplies[questions[4].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[5].npcReplies[questions[5].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[6].npcReplies[questions[6].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[7].npcReplies[questions[7].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[8].npcReplies[questions[8].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[9].npcReplies[questions[9].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[10].npcReplies[questions[10].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[11].npcReplies[questions[11].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[12].npcReplies[questions[12].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[13].npcReplies[questions[13].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[14].npcReplies[questions[14].correctIndex]}");
            //Debug.Log($"������ �亯 {questions[15].npcReplies[questions[15].correctIndex]}");

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

        resultText.text = $"�ֱ� 3���� �� {correctCount}�� ����!";
        // resultImage.sprite = ...; // �̹��� ���浵 ����

        // ���� ������ �ܺο��� resultPanel ���� ���� StartQuiz() ���� ȣ��
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false); 
        GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);

    }
}
