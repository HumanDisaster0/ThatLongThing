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
    public CustomClickable cMissionBoard;
    public CustomClickable cAlbum;
    public CustomClickable cTalk;

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
        whatDidYouText.text = "�̹� �Ƿ� ���� �����̽��ϴ�.\n�Ƿ��� ����� �ᱹ �����̾�����?";
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

        // ���� ��ȣ �ʱ�ȭ        
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

        resultText.text = $"�ֱ� 3���� �� {correctCount}�� ����!";
        // resultImage.sprite = ...; // �̹��� ���浵 ����

        // ���� ������ �ܺο��� resultPanel ���� ���� StartQuiz() ���� ȣ��
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
            Debug.Log("��ư �ѱ�");
            closeButton.isInteractable = true;
            cMissionBoard.isInteractable = true;
            cAlbum.isInteractable = true;
            cTalk.isInteractable = true;
        }
        else
        {
            Debug.Log("��ư ����");
            closeButton.isInteractable = false;
            cMissionBoard.isInteractable = false;
            cAlbum.isInteractable = false;
            cTalk.isInteractable = false;
        }
    }
}
