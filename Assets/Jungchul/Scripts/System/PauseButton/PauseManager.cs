using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseCanvasPrefab;
    private GameObject pauseCanvasInstance;

    private Button resumBtn;
    private Button returnToTitleBtn;
    private Button exitBtn;

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
            Debug.LogWarning("�ߺ� PauseManager ���ŵ�");
            Destroy(gameObject);
            return;
        }
        Debug.Log("PauseManager ������");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        pState = isPause.NOTPAUSE;
        prePState = isPause.NOTPAUSE;
        Debug.Log("PauseManager Awake - �̺�Ʈ ���");

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name != "Title")
        {
            pState = (pState == isPause.NOTPAUSE) ? isPause.PAUSE : isPause.NOTPAUSE;
        }

        if (prePState != pState)
        {
            prePState = pState;
            if (pState == isPause.PAUSE)
                Pause();
            else
                Resume();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pauseCanvasInstance != null)
        {
            Camera mainCam = Camera.main;
            Canvas canvas = pauseCanvasInstance.GetComponent<Canvas>();
            if (canvas != null && mainCam != null)
            {
                canvas.worldCamera = mainCam;
                Debug.Log($"[PauseManager] Canvas ī�޶� ������: {mainCam.name}");
            }
            else
            {
                Debug.LogWarning("[PauseManager] Canvas�� Main Camera�� ã�� ���߽��ϴ�.");
            }
        }

        // 2. �Ͻ����� ���� ó��
        
        Resume();
    }

    private void CreatePauseUI()
    {
        if (pauseCanvasInstance != null) return;

        pauseCanvasInstance = Instantiate(pauseCanvasPrefab);
        DontDestroyOnLoad(pauseCanvasInstance);

        var btns = pauseCanvasInstance.GetComponentsInChildren<Button>(true);
        foreach (var btn in btns)
        {
            if (btn.name.Contains("Resume"))
            {
                resumBtn = btn;
                resumBtn.onClick.AddListener(Resume);
            }
            else if (btn.name.Contains("Title"))
            {
                returnToTitleBtn = btn;
                returnToTitleBtn.onClick.AddListener(ReturnToTitle);
            }
            else if (btn.name.Contains("Exit"))
            {
                exitBtn = btn;
                exitBtn.onClick.AddListener(ExitGame);
            }
        }
    }

    public void Pause()
    {
        if (pauseCanvasInstance == null)
            CreatePauseUI();

        Time.timeScale = 0f;
        pauseCanvasInstance.SetActive(true);
    }

    public void Resume()
    {
        SoundManager.instance?.GetComponent<SettingCanvasController>().SetSettingCanvas(false);
        Time.timeScale = 1f;
        if (pauseCanvasInstance != null)
            pauseCanvasInstance.SetActive(false);
        pState = isPause.NOTPAUSE;
    }

    public void ReturnToTitle()
    {
        Resume();
        Destroy(pauseCanvasInstance);
        pauseCanvasInstance = null;

        SceneManager.LoadScene("Title");
    }

    public void ExitGame()
    {
        SoundManager.instance?.GetComponent<SettingCanvasController>().SetSettingCanvas(true);
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit();
//#endif
    }

    private void LateUpdate()
    {
        if (pauseCanvasInstance != null)
        {
            Canvas canvas = pauseCanvasInstance.GetComponent<Canvas>();
            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
                if (canvas.worldCamera != null)
                    Debug.Log("[PauseManager] LateUpdate���� ī�޶� ���� �Ϸ�");
            }
        }
    }
}
