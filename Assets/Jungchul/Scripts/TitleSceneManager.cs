using UnityEngine;
using UnityEngine.SceneManagement;
using static NonePlaySceneManager;

public class TitleSceneManager : MonoBehaviour
{
    public static TitleSceneManager Instance;

    [SerializeField] bool isGoGuildMain = false; // GuildMain���� �ٷ� ����

    // ��ư 3���� ���
    public CustomClickable startButton;
    public CustomClickable optionButton;
    public CustomClickable exitButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject); // ���û���

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        var start = startButton;
        var opt = optionButton;
        var exit = exitButton;

        if (start != null)
            start.SetClickAction(() => StartGame()); // ���� ����

        if (opt != null)
            opt.SetClickAction(() => Debug.Log("�ɼ� ����")); // �ɼ� ����

        if (exit != null)
            exit.SetClickAction(() => ExitGame()); // ����
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
        {
            ActivateUI();
        }
        else
        {
            DeactivateUI();
        }
    }

    public void ActivateUI()
    {
        if (startButton != null) startButton.isInteractable = true;
        if (optionButton != null) optionButton.isInteractable = true;
        if (exitButton != null) exitButton.isInteractable = true;
    }

    public void DeactivateUI()
    {
        if (startButton != null) startButton.isInteractable = false;
        if (optionButton != null) optionButton.isInteractable = false;
        if (exitButton != null) exitButton.isInteractable = false;
    }

    public void StartGame()
    {
        print("Prologue��!");
        DeactivateUI();

        if(isGoGuildMain)
        {
            //Invoke("LoadGuildMain", 0.1f);
            FadeInOut.instance.sceneName = "GuildMain";
            FadeInOut.instance.ExeFadeIn();
            return;
        }
        else
        {
            //Invoke("LoadTutorialScene", 1f);
            FadeInOut.instance.sceneName = "TutorialScene";
            FadeInOut.instance.ExeFadeIn();
        }
            
    }

    void LoadTutorialScene()
    {
        SceneManager.LoadScene("TutorialStage");
    }
    void LoadGuildMain()
    {
        SceneManager.LoadScene("GuildMain");
    }


    public void EnterOption()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void ExitGame()
    {
        DeactivateUI();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // �����Ϳ��� �׽�Ʈ �� ����
#endif
    }
}
