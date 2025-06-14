using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PauseManager;

public class PostedMissionPanel : MonoBehaviour
{
    public static PostedMissionPanel Instance;

    [Header("미션 프리팹 및 인스턴스")]
    public GameObject missionCardPrefab;

    private List<GameObject> missionInstances = new List<GameObject>();

    public Sprite[] missionSprites;
    public Sprite[] missionSpritesHover;
    public Sprite emptyMissionSprite; // 미션 없음 표시용

    public Sprite[] missionDetailSprites;


    private readonly Vector3[] defaultPositions = new Vector3[]
    {
        new Vector3(-5.08f, -0.25f, 0f),
        new Vector3( 0.00f, -0.25f, 0f),
        new Vector3( 5.08f, -0.25f, 0f)
    };

    public GameObject missionDetailPrefab;
    public GameObject currentPopup; // 현재 활성화된 팝업
    private List<int> currentMissionCodes = new List<int>();

    public int mIdx = 0;


    public int selCnt = 0;


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

        enabled = false;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GuildMain")
        {
            DestroyMissionsCards();
            Destroy(currentPopup);
        }
        else
        {
            CreateMissions();
        }

    }

    private void Update()
    {
        if (GuildRoomManager.Instance.IsIdle())
        {
            Destroy(currentPopup);
        }
        if (SceneManager.GetActiveScene().name == "GuildMain" && GuildRoomManager.Instance.curVstate == GuildRoomManager.viewState.MISSIONBOARD)
        {
            DisableOnPause();
        }

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void InitPanel(List<int> codes)
    {
        currentMissionCodes = codes;

        for (int i = 0; i < 3; i++)
        {
            var instance = missionInstances[i];
            int code = (i < codes.Count) ? codes[i] : 0;

            Debug.Log($"i , code: {i}  {code}");
            ApplyCodeToMission(i, code);

            int capturedIndex = code / 1000;
            int ci = i;

            var clickable = instance.GetComponent<CustomClickable>();
            if (clickable != null)
            {
                clickable.SetClickAction(() => ShowPopupCard(capturedIndex, ci));
            }
        }
    }

    private void CreateMissions()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject instance = Instantiate(missionCardPrefab, transform);
            instance.name = $"PostedMission_{i + 1}";
            instance.transform.localPosition = defaultPositions[i];
            missionInstances.Add(instance);
        }
    }


    private void ApplyCodeToMission(int index, int code)
    {
        if (index < 0 || index >= missionInstances.Count)
            return;

        var instance = missionInstances[index];
        var clickable = instance.GetComponent<CustomClickable>();

        if (clickable == null)
            return;

        Sprite sprite, hSprite;

        if (code == 0)
        {

            ResizeBoxColliderToSprite(gameObject);
            clickable.SetSprites(emptyMissionSprite, emptyMissionSprite);
            clickable.isInteractable = false;
            return;
        }
        else
        {
            sprite = missionSprites[code / 1000];
            hSprite = missionSpritesHover[code / 1000];
        }

        clickable.SetSprites(sprite, hSprite);
        clickable.AdjustColliderToSprite();

    }

    public void DestroyMissionsCards()
    {
        foreach (var instance in missionInstances)
        {
            if (instance != null)
                Destroy(instance);
        }
        missionInstances.Clear();
    }

    public void DisableOnPause()
    {
        if (PauseManager.Instance.pState == isPause.PAUSE)
        {
            for (int i = 0; i < 3; i++)
            {
                if (missionInstances[i] != null)
                    missionInstances[i].GetComponent<CustomClickable>().isInteractable = false;
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (missionInstances[i] != null)
                    missionInstances[i].GetComponent<CustomClickable>().isInteractable = true;
            }
        }
    }


    private int ResolveCodeToIndex(int code)
    {
        int stage = code / 100;
        int anomaly = code % 100;

        if (stage == 1)
            return anomaly;
        else if (stage == 2)
            return 2 + anomaly;
        else if (stage == 3)
            return 7 + anomaly;
        else
            return -1;
    }

    public void SetMissionPosition(int index, float x, float y)
    {
        if (index < 0 || index >= missionInstances.Count) return;
        Vector3 newPosition = new Vector3(x, y, missionInstances[index].transform.position.z);
        missionInstances[index].transform.position = newPosition;
    }

    public void ShowPopupCard(int index, int pidx)
    {
        for (int i = 0; i < 3; i++)
        {
            missionInstances[i].GetComponent<CustomClickable>().isInteractable = false;
        }

        int code = currentMissionCodes[pidx];
        int spriteIndex = index;

        mIdx = currentMissionCodes[pidx];

        currentPopup = Instantiate(missionDetailPrefab, Vector3.zero, Quaternion.identity);

        // SpriteRenderer 세팅
        var spriteRenderer = currentPopup.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteIndex >= 0 && spriteIndex < missionDetailSprites.Length)
        {
            spriteRenderer.sprite = missionDetailSprites[spriteIndex];
        }

        // 버튼 처리
        var popupComp = currentPopup.GetComponent<MissionDetailPopup>();
        if (popupComp != null)
        {
            //Debug.Log("팝업 컴포넌트 찾음");

            if (popupComp.acceptButton == null) Debug.LogError("acceptButton 연결 안 됨!");
            if (popupComp.rejectButton == null) Debug.LogError("rejectButton 연결 안 됨!");

            popupComp.acceptButton.SetClickAction(() =>
            {
                PostedMissionPanel.Instance.PopupBtnAccept();
            });

            popupComp.rejectButton.SetClickAction(() =>
            {
                PostedMissionPanel.Instance.PopupBtnReject();
            });
        }
    }

    public void PopupBtnAccept()
    {
        //의뢰 수락

        GuildRoomManager.Instance.missionBoardPanel.SetActive(false);
        GuildRoomManager.Instance.DoorOutOn();

        GuildRoomManager.Instance.SelectMission(mIdx);
        for (int i = 0; i < 3; i++)
        {
            missionInstances[i].GetComponent<CustomClickable>().isInteractable = true;
        }
        CardShowSet(false);

        Destroy(currentPopup);


        if (GuildRoomManager.Instance.preMissionBoardVstate == GuildRoomManager.viewState.COUNTER)
        {
            var gc = GuildRoomManager.Instance.guildCounterPanel.GetComponent<GuildCounter>();
            gc.OnCloseButtonClicked();
        }
        else
        {
            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);
        }

    }

    public void PopupBtnReject()
    {
        //의뢰 거절

        mIdx = 0;
        for (int i = 0; i < 3; i++)
        {
            missionInstances[i].GetComponent<CustomClickable>().isInteractable = true;
        }
        Destroy(currentPopup);
    }


    public void CardShowSet(bool onoff)
    {
        foreach (var card in missionInstances)
        {           
            card.SetActive(onoff);
        }
    }

    public static void ResizeBoxColliderToSprite(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        BoxCollider2D bc = obj.GetComponent<BoxCollider2D>();

        if (sr == null || bc == null)
        {
            Debug.LogWarning("SpriteRenderer 또는 BoxCollider2D가 없습니다.");
            return;
        }

        Sprite sprite = sr.sprite;
        if (sprite == null)
        {
            Debug.LogWarning("Sprite가 비어 있습니다.");
            return;
        }

        // 스프라이트의 픽셀 단위 크기 -> 유닛 단위로 변환
        Vector2 size = sprite.bounds.size;

        bc.size = size;
        bc.offset = Vector2.zero;
    }

}