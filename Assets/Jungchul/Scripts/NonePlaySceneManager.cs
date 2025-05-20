using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MissionSelectManager;

public class NonePlaySceneManager : MonoBehaviour
{
    public static NonePlaySceneManager Instance;
    public GuildRoomManager gRoomManagerPrefab;
    public MissionSelectManager missionSelectManagerPrefab;

    public bool isMissionPosted;

    public int curMissionCode;
    

    public enum npSceneState
    {
        TITLE,
        PROLOGUE,
        GUILDMAIN,
        MISSION,

    }

    npSceneState state;

    public int prologueCnt;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        string currentScene = SceneManager.GetActiveScene().name;

        isMissionPosted = false;

    }


    IEnumerator Start()
    {

        yield return null; // 1 프레임 대기 → 씬 완전히 로드됨


        if (GuildRoomManager.Instance == null)
        {
            var go = Instantiate(gRoomManagerPrefab);
            Debug.Log("[NonePlaySceneManager] GuildRoomManager 인스턴스 생성됨: " + go.GetInstanceID());
        }
        else
        {
            Debug.Log("[NonePlaySceneManager] 이미 인스턴스 존재함: " + GuildRoomManager.Instance.GetInstanceID());
        }

        if (missionSelectManagerPrefab == null)
        {
            var ob = Instantiate(missionSelectManagerPrefab);
            Debug.Log("[NonePlaySceneManager] MissionSelectManager 인스턴스 생성됨: " + ob.GetInstanceID());
        }
        else
        {
            Debug.Log("[NonePlaySceneManager] 이미 인스턴스 존재함: " + MissionSelectManager.Instance.GetInstanceID());
        }


        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "GuildMain")
        {
            GuildRoomManager.Instance.enabled = false;
            Debug.Log("GuildRoomManager disabled (현재 씬: " + currentScene + ")");
        }

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (GuildRoomManager.Instance == null)
            return;

        if (newScene.name == "GuildMain")
        {
            GuildRoomManager.Instance.enabled = true;
            Debug.Log("GuildRoomManager enabled (씬 진입)");
        }
        if (oldScene.name == "GuildMain")
        {
            GuildRoomManager.Instance.enabled = false;
            Debug.Log("GuildRoomManager disabled (씬 이탈)");
        }

        if (newScene.name == "Prologue")
        {
            prologueCnt = 0;
        }
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // Update is called once per frame
    void Update()
    {

        switch (state)
        {
            case npSceneState.TITLE:
                break;


            case npSceneState.PROLOGUE:

                if (Input.GetMouseButtonDown(0))
                {
                    prologueCnt++;
                    print("하나둘셋");
                }

                if (Input.GetKeyDown(KeyCode.Space) || prologueCnt == 3)
                {
                    SceneManager.LoadScene("GuildMain");
                    state = npSceneState.GUILDMAIN;
                    return;
                }

                break;

            case npSceneState.GUILDMAIN:                



                break;

            case npSceneState.MISSION:
                break;

        }
    }

    public void StartGame()
    {
        state = npSceneState.PROLOGUE;
        print("Prologue로!");
        SceneManager.LoadScene("Prologue");
    }

    public void EnterOption()
    {
        state = npSceneState.PROLOGUE;
        SceneManager.LoadScene("TitleScene");
    }


    public void SetSceneState(npSceneState pstate)
    {
        state = pstate;
    }
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 시 종료
#endif
    }


    // 참고용: NonePlaySceneManager 내부에 이런 함수가 있어야 함

    public List<int> GetCurrentMissionCodes()
    {
        int[] missionCodes = new int[3];

        return new List<int>(missionCodes);
    }

}
