using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Rendering.InspectorCurveEditor;
using System.Linq;
using System;


public class GuildRoomManager : MonoBehaviour
{
    public static GuildRoomManager Instance;

    public GameObject GuildRoomManagerPrefab;

    private bool hideObjectActivated = false;
    private bool isFirstTime = true;
    private bool isReturned = false;
    public bool isGetRewardYet = true;
    public bool isReportYet = true;
    //private bool isMissionSelected = false;


    public int selectedMission = 0;

    public int day = 1;

    public int week = 1;

    public bool tempChecker = true;

    private bool trollCheck = false;

    private bool isPasan = false;


    public enum viewState
    {
        IDLE,
        COUNTER,
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

    public GameObject guildCounterPanel;
    public GameObject settlementPanel;
    public GameObject missionBoardPanel;
    public GameObject albumPanel;

    public GameObject checkResult;

    public TextDrawer textDrawer;



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

        trollCheck = false;

        isPasan = false;

        curVstate = viewState.IDLE;


        SceneManager.sceneLoaded += OnSceneLoaded;

        Scene current = SceneManager.GetActiveScene();

        if (current.name == "GuildMain")
        {
            OnSceneLoaded(current, LoadSceneMode.Single);
        }


        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        //SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(this.gameObject);
        print("부수지마");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hideObjectActivated && scene.name == "GuildMain")
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var go in allObjects)
            {
                // 이름으로 찾되, 현재 로드된 씬에 속한 오브젝트만 필터링
                if ((go.name == "SettlementPanel" || go.name == "MissionBoardPanel")
                    && go.scene == scene)
                {
                    go.SetActive(true);
                    Debug.Log($"[{go.name}] 강제로 활성화됨");
                }
            }

            //hideObjectActivated = true;
        }

        if (SceneManager.GetActiveScene().name == "GuildMain")
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


            guildCounterPanel = GameObject.Find("GuildCounter");
            albumPanel = GameObject.Find("MissionBoardPanel");
            checkResult = GameObject.Find("d_CheckResult");


            settlementPanel = GameObject.Find("SettlementPanel");
            if (settlementPanel == null)
            {
                Debug.Log("settlementPanel is null");
            }
            missionBoardPanel = GameObject.Find("MissionBoardPanel");
            if (missionBoardPanel == null)
            {
                Debug.Log("missionBoardPanel is null");
            }

            if (guildCounterPanel != null) guildCounterPanel.SetActive(false);
            if (missionBoardPanel != null) missionBoardPanel.SetActive(false);
            if (albumPanel != null) albumPanel.SetActive(false);
            if (settlementPanel != null) settlementPanel.SetActive(false);
            if (checkResult != null) checkResult.SetActive(false);

            textDrawer = FindObjectOfType<TextDrawer>();



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
                    tempChecker = true;
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

            //미션 클리어 처리     

            if (selectedMission != -1 && selectedMission != 0)
            {
                MissionSelectManager.Instance.SetMissionCleared(selectedMission);

                Debug.Log($"{name}" + selectedMission + "클리어!");

            }


            if (PostedMissionPanel.Instance != null)
            {
                PostedMissionPanel.Instance.enabled = true;

                // GuildRoom 진입마다 새로운 미션 받기
                var codes = MissionSelectManager.Instance.Generate3MissionCodes();
                PostedMissionPanel.Instance.InitPanel(codes);

                PostedMissionPanel.Instance.CardShowSet(false);

                print("카드 초기화 잘 됐어!");
            }


            DoorOutOff();
        }
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

                if (GoldManager.Instance.totalGold < 0)
                {
                    Debug.Log("천강파산뢰!");
                    Debug.Log("유브갓개털");
                }

                if (!avatar.isMovable)
                {
                    //
                    Debug.Log($"{name}:" + selectedMission + "선택됨");
                    PostedMissionPanel.Instance.CardShowSet(false);
                    PauseManager.Instance.pauseButtonInstance.SetActive(true);
                    GoldManager.Instance.ClearReward();
                }
                avatar.isMovable = true;

                break;

            case viewState.COUNTER:

                avatar.isMovable = false;
                var gcp = guildCounterPanel.GetComponent<GuildCounter>();
                if (!guildCounterPanel.gameObject.activeSelf)
                {
                    guildCounterPanel.gameObject.SetActive(true);

                }
                if (!isGetRewardYet && isReportYet && !trollCheck)
                {

                    checkResult.gameObject.SetActive(true);

                    gcp.closeButton.enabled = false;
                    gcp.cAlbum.isInteractable = false;                    
                    gcp.cMissionBoard.isInteractable = false;
                    gcp.cTalk.isInteractable = false;

                    if (checkResult)
                    {
                        Debug.Log("checkResult활성화");

                        //정산 패널에서 x버튼 누르는 순간 골드 정산 진행됨
                    }

                    gcp.StartQuiz();

                    isReportYet = false;
                }

                if (!isReportYet)
                {
                    selectedMission = 0;
                }

                if (Input.GetMouseButtonDown(0) && gcp.isEnd)
                {
                    Debug.Log($"isEnd! {gcp.isEnd}");

                    checkResult.gameObject.SetActive(false);

                    day++;
                    if (day == 4)
                    {
                        day = 1;
                        week++;
                    }

                    textDrawer.TextRefresh();

                    gcp.isEnd = false;

                }
                break;


            case viewState.SETTLEMENT:
                break;

            case viewState.MISSIONBOARD:
                PauseManager.Instance.pauseButtonInstance.SetActive(false);
                avatar.isMovable = false;
                if (!missionBoardPanel.gameObject.activeSelf)
                {
                    missionBoardPanel.gameObject.SetActive(true);
                    PostedMissionPanel.Instance.CardShowSet(true);
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

                int reCode = selectedMission % 1000;

                StageManager.instance.LoadStage(reCode);

                reCode = selectedMission % 1000;
                int stg = reCode / 100;
                int ano = reCode % 100;
                Debug.Log($"{name} 로드 스테이지: " + stg + "  / 이상현상: " + ano);

                //StageManager.instance.anomalyIdx = ano;

                SceneManager.LoadScene("DummyMission");

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
        if (tempChecker)
        {
            print("복귀신고!");
            avatar.isMovable = false;

            foreach (var obj in guildObjects)
            {
                obj.isInteractable = false;
            }

            tempChecker = false;
        }

        avatar.transform.position += Vector3.right * -5.0f * Time.deltaTime;

        if (avatar.transform.position.x - guildObjects[1].transform.position.x <= 0)
        {
            curVstate = viewState.COUNTER;

            settlementPanel.gameObject.SetActive(true);

            isReturned = false;

            foreach (var obj in guildObjects)
            {
                obj.isInteractable = true;
            }

            DoorOutOff();

            return;
        }
    }

    public void SetReturned()
    {
        print("내가돌아왔다!");
        isReturned = true;
        isGetRewardYet = true;
        isReportYet = true;


    }

    public void DoorOutOn()
    {
        guildObjects[3].isInteractable = true;
    }

    public void DoorOutOff()
    {
        if (guildObjects[3] != null)
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

        guildObjects[0].HLforceOff();
        guildObjects[0].isInteractable = false;
        //isMissionSelected = true;
    }

    public bool IsIdle()
    {
        if (curVstate == viewState.IDLE)
            return true;

        return false;
    }


}