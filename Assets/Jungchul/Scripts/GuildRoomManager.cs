using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;


public class GuildRoomManager : MonoBehaviour
{
    public static GuildRoomManager Instance;

    public GameObject GuildRoomManagerPrefab;

    private bool hideObjectActivated = false;
    private bool isFirstTime = true;
    private bool isReturned = false;
    public bool isGetRewardYet = false;
    public bool isReportYet = true;
    //private bool isMissionSelected = false;

    private bool isEscHandling = false;

    public bool isPauseAble = true;
    public bool isAlbumMemoryActive = false;

    public int selectedMission = 0;

    public int day = 1;

    public int week = 1;

    public bool tempChecker = true;

    public bool trollCheck = false;

    public bool isPasan = false;

    public int wrongCnt = 0;


    public enum viewState
    {
        IDLE,
        COUNTER,
        SETTLEMENT,
        MARKET,
        MISSIONBOARD,
        POKEDEX,
        DOOROUT,
        NONE,
    }
    public viewState curVstate;
    public viewState preCurVstate;
    public viewState preMissionBoardVstate;
    public viewState prePokedexVstate;


    public enum counterState
    {
        SETTlE,
        TROLL,
        TROLL_P,
        QUIZ,
        QUIZ_P,
        QUIZ_PP,
        C_IDLE,
        NONE,
    }
    public counterState cState;
    public counterState preCState;
    public counterState tState;


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

    public List<QuestionResult> quizResults = new List<QuestionResult>();

    //    104, 1111, 2202, 
    //    3203, 4209, 5212, 
    //    6207, 7301, 8305, 
    //    9314, 10306, 11308, 
    //    12310, 13313, 14315, 


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


        isGetRewardYet = false;
        trollCheck = false;

        isPasan = false;

        curVstate = viewState.IDLE;
        preCurVstate = viewState.NONE;
        preMissionBoardVstate = viewState.IDLE;
        prePokedexVstate = viewState.IDLE;

        cState = counterState.C_IDLE;
        preCState = counterState.NONE;

        isPauseAble = true;



        SceneManager.sceneLoaded += OnSceneLoaded;

        Scene current = SceneManager.GetActiveScene();

        if (current.name == "GuildMain")
        {
            OnSceneLoaded(current, LoadSceneMode.Single);
        }


        //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        //SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(this.gameObject);

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

            albumPanel = GameObject.Find("MemoryCanvas");
            if (albumPanel == null)
            {
                Debug.Log("albumPanel is null");
            }

            if (guildCounterPanel != null) guildCounterPanel.SetActive(false);
            if (missionBoardPanel != null) missionBoardPanel.SetActive(false);
            if (albumPanel != null) albumPanel.SetActive(false);
            if (settlementPanel != null) settlementPanel.SetActive(false);
            if (checkResult != null) checkResult.SetActive(false);

            textDrawer = FindObjectOfType<TextDrawer>();


            //1 - 미션 보고 2 미션 보고 3 미션 보고 4
            avatar = FindObjectOfType<AvatarController>();

            if (avatar != null)
            {
                if (isFirstTime && avatar)
                {

                    //avatar.transform.position = new Vector3(7.5f, -1.65f, 0);
                }
                else if (!isFirstTime && isReturned)
                {
                    tempChecker = true;
                    //avatar.transform.position = new Vector3(8.5f, -1.65f, 0);

                }
                worldLeftLimit = -8.6f;
                worldRightLimit = 8.6f;
                //avatar.SetLimits(worldLeftLimit, worldRightLimit);

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


            }

            DoorOutOff();
        }


        //타이틀 진입시 리셋
        if (scene.name == "Title")
        {
            hideObjectActivated = false;
            isFirstTime = true;
            isReturned = false;
            isGetRewardYet = false;
            isReportYet = true;
            selectedMission = 0;
            trollCheck = false;
            isPasan = false;
            isPauseAble = true;
            isEscHandling = false;

            curVstate = viewState.IDLE;
            preCurVstate = viewState.NONE;
            preMissionBoardVstate = viewState.IDLE;
            prePokedexVstate = viewState.IDLE;

            cState = counterState.C_IDLE;
            preCState = counterState.NONE;

            day = 1;
            week = 1;
            tempChecker = true;
            wrongCnt = 0;
            quizResults.Clear();
        }
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

    }

    void Update()
    {
        if (!avatar)
            return;

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

        CloseWithESC();

        if (preCurVstate != curVstate)
        {
            switch (curVstate)
            {
                case viewState.IDLE:

                    isPauseAble = true;

                    if (!avatar.isMovable)
                    {
                        avatar.isMovable = true;
                    }  
                   
                    preCurVstate = curVstate;

                    break;

                case viewState.COUNTER:

                    isPauseAble = false;

                    if (cState != preCState || cState == counterState.TROLL_P || cState == counterState.QUIZ_P)
                    {
                        var gcp = guildCounterPanel.GetComponent<GuildCounter>();


                        //preCState = cState;


                        switch (cState)
                        {
                            case counterState.SETTlE:
                                preCState = cState;

                                avatar.isMovable = false;

                                gcp.btnOnOff(false);

                                day++;


                                //gcp.btnOnOff(false);
                                guildCounterPanel.gameObject.SetActive(true);
                                //sp.RefreshTexts();

                            
                                GoldManager.Instance.calRewardGold();
                                GoldManager.Instance.getRewardGold();

                                settlementPanel.gameObject.SetActive(true);
                                gcp.trollPanel.SetActive(false);

                                if (day == 4)
                                {

                                    int last3Start = Mathf.Max(quizResults.Count - 3, 0);

                                    for (int i = last3Start; i < quizResults.Count; i++)
                                    {
                                        if (!quizResults[i].isCorrect)
                                        {
                                            wrongCnt++;
                                        }
                                    }

                                    if (wrongCnt > 0)
                                    {
                                        trollCheck = true;
                                    }
                                    else
                                    {
                                        cState = counterState.SETTlE;
                                    }
                                    GoldManager.Instance.MinusGold(60);
                                    day = 1;
                                    week++;
                                }

                                break;

                            case counterState.TROLL:

                                gcp.ShowMidResult(wrongCnt);
                                preCState = cState;
                                gcp.btnOnOff(false);

                                cState = counterState.TROLL_P;

                                break;


                            case counterState.TROLL_P:
                                if (Input.GetMouseButtonDown(0))
                                {
                                    preCState = cState;

                                    gcp.btnOnOff(false);

                                    GoldManager.Instance.MinusGold(wrongCnt * 40);
                                    wrongCnt = 0;
                                    cState = counterState.QUIZ;
                                }


                                break;

                            case counterState.QUIZ:
                                checkResult.gameObject.SetActive(true);
                                gcp.StartQuiz(selectedMission / 1000);

                                gcp.btnOnOff(false);

                                cState = counterState.QUIZ_P;
                                break;

                            case counterState.QUIZ_P:

                                gcp.btnOnOff(false);

                                if (gcp.isEnd)
                                {
                                    //SoundManager.instance.PlayNewBackSound("Map_Check2", SoundType.Se);
                                    preCState = cState;
                                    gcp.btnOnOff(false);
                                    cState = counterState.QUIZ_PP;
                                }
                                break;


                            case counterState.QUIZ_PP:

                                if (Input.GetMouseButtonDown(0))
                                {
                                    //SoundManager.instance.PlayNewBackSound("Map_Check2", SoundType.Se);

                                    checkResult.gameObject.SetActive(false);

                                    textDrawer.TextRefresh();

                                    selectedMission = 0;

                                    cState = counterState.C_IDLE;
                                }
                                break;

                            case counterState.C_IDLE:

                                preCurVstate = curVstate;

                                // 효과음 재생
                                SoundManager.instance?.PlayNewBackSound("Map_Check2");

                                preCState = cState;
                                avatar.isMovable = false;

                                gcp.btnOnOff(true);

                                textDrawer.TextRefresh();
                                guildCounterPanel.gameObject.SetActive(true);
                                gcp.trollPanel.SetActive(false);


                                //gcp.btnOnOff(true);
                                break;
                            default:
                                gcp.btnOnOff(true);
                                break;
                        }
                    }

                    break;


                case viewState.SETTLEMENT:

                    preCurVstate = curVstate;

                    curVstate = viewState.COUNTER;

                    cState = counterState.C_IDLE;

                    break;

                case viewState.MISSIONBOARD:

                    preMissionBoardVstate = preCurVstate;
                    preCState = counterState.NONE;

                    preCurVstate = curVstate;

                    avatar.isMovable = false;
                    if (!missionBoardPanel.gameObject.activeSelf)
                    {
                        // 효과음 재생
                        SoundManager.instance?.PlayNewBackSound("Map_Check2");

                        missionBoardPanel.gameObject.SetActive(true);
                        PostedMissionPanel.Instance.CardShowSet(true);
                    }

                    break;

                case viewState.POKEDEX:

                    prePokedexVstate = preCurVstate;
                    preCState = counterState.NONE;

                    preCurVstate = curVstate;

                    avatar.isMovable = false;
                    if (!albumPanel.gameObject.activeSelf)
                    {
                        // 효과음 재생
                        SoundManager.instance?.PlayNewBackSound("Album_Click");

                        albumPanel.gameObject.SetActive(true);
                        var ap = albumPanel.GetComponent<MemoryCanvas>();
                        ap.UnlockMemories();

                    }


                    break;

                case viewState.DOOROUT:

                    preCurVstate = curVstate;

                    isPauseAble = true;

                    // 문 열리는 소리 재생
                    SoundManager.instance?.PlayNewBackSound("Door_Open");

                    avatar.isMovable = false;

                    NotFirstNow();

                    this.enabled = false;

                    int reCode = selectedMission % 1000;

                    if (FadeInOut.instance)
                    {
                        StageManager.instance.LoadStage(reCode);
                        FadeInOut.instance.ExeFadeIn();
                    }
                    else
                        StageManager.instance.LoadStage(reCode);
                    //SceneManager.LoadScene("DummyMission");

                    break;
            }
        }

    }
    public void SetRoomState(viewState vState)
    {
        curVstate = vState;

    }

    public void ForceMoveOnRetrun()
    {
        if (tempChecker)
        {

            avatar.isMovable = false;

            foreach (var obj in guildObjects)
            {
                obj.isInteractable = false;
            }

            tempChecker = false;
        }

        var pc = avatar.GetComponent<PlayerController>();
        pc.ForceInput = true;
        pc.Dir = -1;

        //avatar.transform.position += Vector3.right * -5.0f * Time.deltaTime;

        if (avatar.transform.position.x - guildObjects[1].transform.position.x <= 0)
        {
            curVstate = viewState.COUNTER;
            cState = counterState.SETTlE;

            isReturned = false;

            foreach (var obj in guildObjects)
            {
                obj.isInteractable = true;
            }

            pc.ForceInput = false;

            DoorOutOff();

            return;
        }
    }

    public void SetReturned()
    {

        isReturned = true;
        //isGetRewardYet = true;
        //isReportYet = true;
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

        //guildObjects[0].HLforceOff();
        //guildObjects[0].isInteractable = false;
        //isMissionSelected = true;
    }

    public bool IsIdle()
    {
        if (curVstate == viewState.IDLE)
            return true;

        return false;
    }

    public void CloseWithESC()
    {
        if (isEscHandling) 
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (curVstate)
            {
                case viewState.POKEDEX:

                    if (!isAlbumMemoryActive)
                    {
                        albumPanel.gameObject.SetActive(false);
                    }

                    break;


                case viewState.MISSIONBOARD:
                    
                    if(PostedMissionPanel.Instance.currentPopup)
                    {
                        PostedMissionPanel.Instance.PopupBtnReject();
                        break;
                    }

                    var mbp = missionBoardPanel.GetComponent<MissionBoardPannel>();
                    mbp.MissionBoardClose();

                    break;


                case viewState.COUNTER:
                    
                    switch(cState)
                    {
                        case counterState.SETTlE:
                            if(settlementPanel.activeSelf)
                            {
                                var sp = settlementPanel.GetComponent<SettlementPanelScript>();
                                sp.OnCloseButtonClicked();
                            }

                            break;

                        case counterState.C_IDLE:
                            var gcp = guildCounterPanel.GetComponent<GuildCounter>();

                            gcp.OnCloseButtonClicked();
                            break;
                    }

                    break;

            }

            StartCoroutine(ResetEscFlag());

        }
    }

    IEnumerator ResetEscFlag()
    {
        yield return new WaitForSeconds(0.1f);
        isEscHandling = false;
    }


}