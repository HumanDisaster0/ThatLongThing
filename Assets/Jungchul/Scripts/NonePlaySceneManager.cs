using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NonePlaySceneManager : MonoBehaviour
{
    public static NonePlaySceneManager Instance;
    public GuildRoomManager gRoomManagerPrefab;
    

    public enum npSceneState
    {
        TITLE,
        PROLOGUE,
        GUILDMAIN,
        DMISSION1, DMISSION2, DMISSION3,

    }

    npSceneState state;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);


        state = npSceneState.TITLE;
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
        else
        {
            GuildRoomManager.Instance.enabled = false;
            Debug.Log("GuildRoomManager disabled (�� ��Ż)");
        }
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
        switch(state)
        {
            case npSceneState.TITLE:
                break;


            case npSceneState.PROLOGUE:
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    SceneManager.LoadScene("GuildMain");
                }
                break;

            case npSceneState.GUILDMAIN:   
               
                break;

        }
        
    }

    public void StartGame()
    {
        state = npSceneState.PROLOGUE;
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
}
