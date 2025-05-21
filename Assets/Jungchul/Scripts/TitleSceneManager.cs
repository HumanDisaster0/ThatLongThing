using UnityEngine;
using UnityEngine.SceneManagement;
using static NonePlaySceneManager;

public class TitleSceneManager : MonoBehaviour
{
    public static TitleSceneManager Instance;

    // 버튼 3개와 배경
    public GameObject startButton;
    public GameObject optionButton;
    public GameObject exitButton;
    public GameObject background;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject); // 선택사항

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        var start = startButton?.GetComponent<CustomClickable>();
        var opt = optionButton?.GetComponent<CustomClickable>();
        var exit = exitButton?.GetComponent<CustomClickable>();

        if (start != null)
            start.SetClickAction(() => StartGame()); // 게임 시작

        if (opt != null)
            opt.SetClickAction(() => Debug.Log("옵션 열기")); // 옵션 열기

        if (exit != null)
            exit.SetClickAction(() => ExitGame()); // 종료
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

    private void ActivateUI()
    {
        if (startButton != null) startButton.SetActive(true);
        if (optionButton != null) optionButton.SetActive(true);
        if (exitButton != null) exitButton.SetActive(true);
        if (background != null) background.SetActive(true);
    }

    private void DeactivateUI()
    {
        if (startButton != null) startButton.SetActive(false);
        if (optionButton != null) optionButton.SetActive(false);
        if (exitButton != null) exitButton.SetActive(false);
        if (background != null) background.SetActive(false);
    }

    public void StartGame()
    {
        print("Prologue로!");
        DeactivateUI();

        Invoke("LoadTutorialScene", 1f);
    }

    void LoadTutorialScene()
    {
        SceneManager.LoadScene("TutorialStage");
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
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 시 종료
#endif
    }
}
