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

        yield return null; // 1 ������ ��� �� �� ������ �ε��


        if (GuildRoomManager.Instance == null)
        {
            var go = Instantiate(gRoomManagerPrefab);
            Debug.Log("[NonePlaySceneManager] GuildRoomManager �ν��Ͻ� ������: " + go.GetInstanceID());
        }
        else
        {
            Debug.Log("[NonePlaySceneManager] �̹� �ν��Ͻ� ������: " + GuildRoomManager.Instance.GetInstanceID());
        }

        if (missionSelectManagerPrefab == null)
        {
            var ob = Instantiate(missionSelectManagerPrefab);
            Debug.Log("[NonePlaySceneManager] MissionSelectManager �ν��Ͻ� ������: " + ob.GetInstanceID());
        }
        else
        {
            Debug.Log("[NonePlaySceneManager] �̹� �ν��Ͻ� ������: " + MissionSelectManager.Instance.GetInstanceID());
        }


        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "GuildMain")
        {
            GuildRoomManager.Instance.enabled = false;
            Debug.Log("GuildRoomManager disabled (���� ��: " + currentScene + ")");
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
            Debug.Log("GuildRoomManager enabled (�� ����)");
        }
        if (oldScene.name == "GuildMain")
        {
            GuildRoomManager.Instance.enabled = false;
            Debug.Log("GuildRoomManager disabled (�� ��Ż)");
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
                    print("�ϳ��Ѽ�");
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
        print("Prologue��!");
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
        UnityEditor.EditorApplication.isPlaying = false; // �����Ϳ��� �׽�Ʈ �� ����
#endif
    }


    // �����: NonePlaySceneManager ���ο� �̷� �Լ��� �־�� ��

    public List<int> GetCurrentMissionCodes()
    {
        int[] missionCodes = new int[3];

        return new List<int>(missionCodes);
    }

}
