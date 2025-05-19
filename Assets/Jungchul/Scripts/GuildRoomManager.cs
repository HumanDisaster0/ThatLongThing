using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
//using static UnityEditor.Rendering.InspectorCurveEditor;
using System.Linq;

public class GuildRoomManager : MonoBehaviour
{
    public static GuildRoomManager Instance;

    public GameObject GuildRoomManagerPrefab;

    private bool isFirstTime = true;
    private bool isReturned = false;
    private bool isMissionSelected = false;

    public int selectedMission = 0;


    public enum viewState
    {
        IDLE,
        SETTLEMENT,
        MARKET,
        MISSIONBOARD,
        POKEDEX,
        DOOROUT,
    }
    public viewState curVstate;


    [SerializeField] AvatarController avatar;
    

    public GuildRoomObject MissionBoard;
    public GuildRoomObject Settlement;
    public GuildRoomObject Pokedex;
    public GuildRoomObject DoorOut;

    public GuildRoomObject[] guildObjects;


    public GuildRoomObject curObj;

    public float worldLeftLimit = -7.6f;
    public float worldRightLimit = 7.6f;

    public GameObject settlementPanel;
    public GameObject missionBoardPanel;
    public GameObject albumPanel;
       

    private void Awake()
    {
        
        Debug.Log("Awake called on GuildRoomManager: " + GetInstanceID());

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GuildRoomManager detected. Destroying new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        isFirstTime = true;
        isReturned = false;
        curVstate = viewState.IDLE;


        SceneManager.sceneLoaded += OnSceneLoaded;
                
        Scene current = SceneManager.GetActiveScene();
        OnSceneLoaded(current, LoadSceneMode.Single);


        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        //SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(this.gameObject);
        print("부수지마");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var temp = FindObjectsOfType<GuildRoomObject>();
        MissionBoard = temp.FirstOrDefault(p => p.code == "MB");
        Settlement = temp.FirstOrDefault(p => p.code == "SM");
        Pokedex = temp.FirstOrDefault(p => p.code == "PD");
        DoorOut = temp.FirstOrDefault(p => p.code == "DO");

        guildObjects[0] = MissionBoard;
        guildObjects[1] = Settlement;
        guildObjects[2] = Pokedex;
        guildObjects[3] = DoorOut;


        settlementPanel = GameObject.Find("SettlementPanel");        
        missionBoardPanel = GameObject.Find("MissionBoardPanel");
        albumPanel = GameObject.Find("MissionBoardPanel");

        if (settlementPanel != null) settlementPanel.SetActive(false);
        if (missionBoardPanel != null) missionBoardPanel.SetActive(false);
        if (albumPanel != null) albumPanel.SetActive(false);

        avatar = FindObjectOfType<AvatarController>();

        if (avatar != null)
        {
            if (isFirstTime && avatar)
            {
                print("신병받아라!");
                avatar.transform.position = new Vector3(7.5f, -1.65f, 0);
            }
            else if (!isFirstTime && isReturned)
            {
                avatar.transform.position = new Vector3(8.5f, -1.65f, 0);
                print("돌아와썹");
            }
            worldLeftLimit = -8.6f;
            worldRightLimit = 8.6f;
            avatar.SetLimits(worldLeftLimit, worldRightLimit);

            foreach (var obj in guildObjects)
            {
                obj.isHighlighted = false;
            }
        }

        DoorOutOff();        
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        print("폭파!");
    }

    void Start()
    {
        

    }

    void Update()
    {
        if (isReturned)
        {
            print("돌아온상태");
            ForceMoveOnRetrun();
            return;
        }
        Vector3 avatarPos = avatar.transform.position;
        //if (avatar != null)
        //{          
        //    avatarPos = avatar.transform.position;            
        //}

        foreach (var obj in guildObjects)
        {
            obj.UpdateState(avatarPos);

            if (obj.isHighlighted)
                curObj = obj;
        }

        if (curObj != null && !curObj.isHighlighted)
        {
            curObj = null;
        }

        switch (curVstate)
        {
            case viewState.IDLE:

                avatar.isMovable = true;

                break;

            case viewState.SETTLEMENT:

                avatar.isMovable = false;

                if (!settlementPanel.gameObject.activeSelf)
                {
                    settlementPanel.gameObject.SetActive(true);
                }

                break;

            case viewState.MISSIONBOARD:

                avatar.isMovable = false;
                if (!missionBoardPanel.gameObject.activeSelf)
                {
                    missionBoardPanel.gameObject.SetActive(true);
                }

                break;

            case viewState.POKEDEX:

                avatar.isMovable = false;
                if (!albumPanel.gameObject.activeSelf)
                {
                    missionBoardPanel.gameObject.SetActive(true);
                }

                break;

            case viewState.DOOROUT:

                avatar.isMovable = false;

                NotFirstNow();

                this.enabled = false;

                if (selectedMission == 1)
                {
                    SceneManager.LoadScene("DummyMission");
                    return;
                }

                break;

        }
    }
    public void SetRoomState(viewState vState)
    {
        curVstate = vState;
        Debug.Log($"GuildRoomManager: 상태 변경됨 → {vState}");
    }

    public void ForceMoveOnRetrun()
    {
        print("복귀신고!");
        avatar.isMovable = false;
        if (avatar.transform.position.x - guildObjects[1].transform.position.x <=0)
        {
            settlementPanel.gameObject.SetActive(true);

            isReturned = false;
            curVstate = viewState.IDLE;

            return;
        }

        avatar.transform.position += Vector3.right * -5.0f * Time.deltaTime;
    }

    public void SetReturned()
    {
        print("내가돌아왔다!");
        isReturned = true;
    }

    public void DoorOutOn()
    {
        guildObjects[3].isInteractable = true;
    }

    public void DoorOutOff()
    {
        if(guildObjects[3] != null)
        {
            guildObjects[3].isInteractable = false;
        }
        
    }

    public void NotFirstNow()
    {
        if (isFirstTime)
            isFirstTime = false;
    }

    public void SelectMission(int missionNum)
    {
        selectedMission = missionNum;
        isMissionSelected = true;
    }
}