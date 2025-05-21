using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseOverlay;
    //public GameObject pausePanel;

    //public GameObject pauseButtonPrefab;
    //public GameObject pauseButtonInstance;

    public GameObject pausePanelPrefab;
    private GameObject pausePanelInstance;

    public CustomClickable resumBtn;
    public CustomClickable returnToTitleBtn;
    public CustomClickable exitBtn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Scene current = SceneManager.GetActiveScene();
        OnSceneLoaded(current, LoadSceneMode.Single);

    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0f;
            pauseOverlay?.SetActive(true);
            pausePanelInstance?.SetActive(true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;
        

        bool shouldShowPause = !(name == "Title" || name == "Prologue");

        if (shouldShowPause)
        {       
            if(pausePanelInstance == null)
                CreatePausePanel();

            pausePanelInstance.SetActive(false);        

        }
        else
        {   
            if (pausePanelInstance != null)
                pausePanelInstance.SetActive(false);
        }

        //pausePanel?.SetActive(false);
        pauseOverlay?.SetActive(false);
    }
   

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseOverlay?.SetActive(true);
        pausePanelInstance?.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseOverlay?.SetActive(false);
        pausePanelInstance?.SetActive(false);
    }

    public void ReturnToTitle()
    {
        print("타이틀로돌아감요!");
        Time.timeScale = 1f;
        NonePlaySceneManager.Instance.SetSceneState(NonePlaySceneManager.npSceneState.TITLE);
        SceneManager.LoadScene("Title");        
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CreatePausePanel()
    {
        if (pausePanelPrefab == null)
        {
            Debug.LogError("PausePanelPrefab이 설정되지 않았습니다.");
            return;
        }

        pausePanelInstance = Instantiate(pausePanelPrefab);
        pausePanelInstance.name = "PausePanel";
        pausePanelInstance.SetActive(false);

        // 자식 버튼들 연결
        var btns = pausePanelInstance.GetComponentsInChildren<CustomClickable>(true);
        foreach (var btn in btns)
        {
            if (btn.name.Contains("Resume"))
                btn.SetClickAction(() => Resume());
            else if (btn.name.Contains("Title"))
                btn.SetClickAction(() => ReturnToTitle());
            else if (btn.name.Contains("Exit"))
                btn.SetClickAction(() => ExitGame());
        }
    }
}
