using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostedMissionPanel : MonoBehaviour
{
    public static PostedMissionPanel Instance;

    [Header("�̼� ������ �� �ν��Ͻ�")]
    public GameObject missionCardPrefab;
   
    private List<GameObject> missionInstances = new List<GameObject>();

    public Sprite[] missionSprites;
    public Sprite[] missionSpritesHover;
    public Sprite emptyMissionSprite; // �̼� ���� ǥ�ÿ�

    public Sprite[] missionDetailSprites;


    private readonly Vector3[] defaultPositions = new Vector3[]
    {
        new Vector3(-5.08f, -0.25f, 0f),
        new Vector3( 0.00f, -0.25f, 0f),
        new Vector3( 5.08f, -0.25f, 0f)
    };

    public GameObject missionDetailPrefab;
    private GameObject currentPopup; // ���� Ȱ��ȭ�� �˾�
    private List<int> currentMissionCodes = new List<int>();

    public int mIdx = 0;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateMissions();        

        enabled = false;

        print("�̼Ǻ��� ����");
    }

    private void Update()
    {
        if(GuildRoomManager.Instance.IsIdle())
        {
            Destroy(currentPopup);
        }
    }


    public void InitPanel(List<int> codes)
    {
        currentMissionCodes = codes;

        Debug.Log(" currentMissionCodes ����Ʈ ����: " + string.Join(", ", currentMissionCodes));

        for (int i = 0; i < 3; i++)
        {
            var instance = missionInstances[i];
            int code = (i < codes.Count) ? codes[i] : 0;
            ApplyCodeToMission(i, code);

            int capturedIndex = i;
            var clickable = instance.GetComponent<CustomClickable>();
            if (clickable != null)
            {
                clickable.SetClickAction(() => ShowPopupCard(capturedIndex));
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
            clickable.SetSprites(emptyMissionSprite, emptyMissionSprite);
            return;
        }
        else
        {
            int tNum = ResolveCodeToIndex(code);
            if (tNum < 0 || tNum >= missionSprites.Length)
                return;

            sprite = missionSprites[tNum];
            hSprite = missionSpritesHover[tNum];
        }

        clickable.SetSprites(sprite, hSprite);
        clickable.AdjustColliderToSprite();

    }
    

    private int ResolveCodeToIndex(int code)
    {
        int stage = code / 100;
        int anomaly = code % 10;

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

    public void ShowPopupCard(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            missionInstances[i].GetComponent<CustomClickable>().isInteractable = false;
        }

        int code = currentMissionCodes[index];
        int spriteIndex = ResolveCodeToIndex(code);

        mIdx = code;

        currentPopup = Instantiate(missionDetailPrefab, Vector3.zero, Quaternion.identity);
        
        // SpriteRenderer ����
        var spriteRenderer = currentPopup.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteIndex >= 0 && spriteIndex < missionDetailSprites.Length)
        {
            spriteRenderer.sprite = missionDetailSprites[spriteIndex];
        }        

        // ��ư ó��
        var popupComp = currentPopup.GetComponent<MissionDetailPopup>();
        if (popupComp != null)
        {
            Debug.Log("�˾� ������Ʈ ã��");

            if (popupComp.acceptButton == null) Debug.LogError("acceptButton ���� �� ��!");
            if (popupComp.rejectButton == null) Debug.LogError("rejectButton ���� �� ��!");

            popupComp.acceptButton.SetClickAction(() =>
            {
                print("����");
                PostedMissionPanel.Instance.PopupBtnAccept();
            });

            popupComp.rejectButton.SetClickAction(() =>
            {
                print("������!");
                PostedMissionPanel.Instance.PopupBtnReject();
            });
        }
    }

    public void PopupBtnAccept()
    {
        print("����");
        PauseManager.Instance.pauseButtonInstance.SetActive(true);
        GuildRoomManager.Instance.missionBoardPanel.SetActive(false);
        GuildRoomManager.Instance.DoorOutOn();
        GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);
        GuildRoomManager.Instance.SelectMission(mIdx);
        CardShowSet(false);
        Destroy(currentPopup);
        
    }

    public void PopupBtnReject()
    {
        print("�㳪 �����Ѵ�");
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
            print("ī���  " + onoff);
            card.SetActive(onoff);
        }
    }    

}