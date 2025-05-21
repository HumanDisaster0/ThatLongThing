using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseOverlay;
    public GameObject pausePanel;

    //public GameObject pauseButtonPrefab;
    //public GameObject pauseButtonInstance;

    //public GameObject pausePanelPrefab;
    //private GameObject pausePanelInstance;

    public CustomClickable resumBtn;
    public CustomClickable returnToTitleBtn;
    public CustomClickable exitBtn;

    //public bool isPause = false;

    public enum isPause
    {
        PAUSE,
        NOTPAUSE,
    }
    public isPause pState;
    public isPause prePState;

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

        pState = isPause.NOTPAUSE;
        prePState = isPause.NOTPAUSE;


    }

    private void Start()
    {
        //if (resumBtn != null)
        //    resumBtn.SetClickAction(() => Resume());

        //if (returnToTitleBtn != null)
        //    returnToTitleBtn.SetClickAction(() => ReturnToTitle());

        //if (exitBtn != null)
        //    exitBtn.SetClickAction(() => ExitGame());
    }

    private void Update()
    {
        if (resumBtn == null)
        {
            Debug.Log("리섬버튼 안달렸어");

        }
        if (Input.GetKeyDown(KeyCode.Escape) && !(SceneManager.GetActiveScene().name == "Title"))// || SceneManager.GetActiveScene().name == "Prologue") )
        {
            if (pState == isPause.NOTPAUSE)
            {
                pState = isPause.PAUSE;
            }
            else
            {
                pState = isPause.NOTPAUSE;
            }
        }

        if (prePState != pState)
        {
            prePState = pState;
            switch (pState)
            {
                case isPause.PAUSE:
                    Pause();
                    
                    break;

                case isPause.NOTPAUSE:
                    Resume();

                    break;
            }
        }

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pausePanel = GameObject.Find("PausePanel");
        pauseOverlay = GameObject.Find("PauseOverlay");

        // 버튼을 다시 찾아서 연결
        if (pausePanel != null)
        {
            var btns = pausePanel.GetComponentsInChildren<CustomClickable>(true);
            foreach (var btn in btns)
            {
                if (btn.name.Contains("Resume"))
                {
                    resumBtn = btn;
                    resumBtn.SetClickAction(() => Resume());
                }
                else if (btn.name.Contains("Title"))
                {
                    returnToTitleBtn = btn;
                    returnToTitleBtn.SetClickAction(() => ReturnToTitle());
                }
                else if (btn.name.Contains("Exit"))
                {
                    exitBtn = btn;
                    exitBtn.SetClickAction(() => ExitGame());
                }
            }

            pausePanel.SetActive(false);
        }

        if (pauseOverlay != null)
            pauseOverlay.SetActive(false);

        pausePanel?.SetActive(false);
        pauseOverlay?.SetActive(false);
    }


    public void Pause()
    {
        Time.timeScale = 0f;
        pauseOverlay?.SetActive(true);
        pausePanel?.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseOverlay?.SetActive(false);
        pausePanel?.SetActive(false);
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

    //private void CreatePausePanel()
    //{
    //    if (pausePanelPrefab == null)
    //    {
    //        Debug.LogError("PausePanelPrefab이 설정되지 않았습니다.");
    //        return;
    //    }

    //    pausePanelInstance = Instantiate(pausePanelPrefab);
    //    pausePanelInstance.name = "PausePanel";
    //    pausePanelInstance.SetActive(false);

    //    // 자식 버튼들 연결
    //    var btns = pausePanelInstance.GetComponentsInChildren<CustomClickable>(true);
    //    foreach (var btn in btns)
    //    {
    //        if (btn.name.Contains("Resume"))
    //            btn.SetClickAction(() => Resume());
    //        else if (btn.name.Contains("Title"))
    //            btn.SetClickAction(() => ReturnToTitle());
    //        else if (btn.name.Contains("Exit"))
    //            btn.SetClickAction(() => ExitGame());
    //    }
    //}
}
