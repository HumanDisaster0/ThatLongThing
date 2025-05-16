using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseOverlay;
    public GameObject pausePanel;

    public GameObject pauseButtonPrefab;
    private GameObject pauseButtonInstance;

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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;

        bool shouldShowPause = !(name == "Title" || name == "Prologue");

        if (shouldShowPause)
        {
            if (pauseButtonInstance == null)
                CreatePauseButton();

            pauseButtonInstance.SetActive(true);
        }
        else
        {
            if (pauseButtonInstance != null)
                pauseButtonInstance.SetActive(false);
        }

        pausePanel?.SetActive(false);
        pauseOverlay?.SetActive(false);
    }

    private void CreatePauseButton()
    {
        if (pauseButtonPrefab == null)
        {
            Debug.LogError("PauseButtonPrefab이 인스펙터에서 설정되지 않았습니다.");
            return;
        }

        pauseButtonInstance = Instantiate(pauseButtonPrefab);
        pauseButtonInstance.name = "PauseButton";
        pauseButtonInstance.transform.position = new Vector3(8.5f, 4.5f, 0);
        print("일시정지버튼 생성!");

        var btn = pauseButtonInstance.GetComponent<CustomClickable>();
        if (btn != null)
            btn.onClick += Pause;
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
        Time.timeScale = 1f;
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
}
