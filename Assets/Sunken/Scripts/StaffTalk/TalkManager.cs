using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;

public class TalkManager : MonoBehaviour
{
    public static TalkManager instance;

    enum StaffEmotes
    {
        Smile1 = 0,
        Smile2,
        Angry,
        End
    }

    public GuildCounter gc;
    public GameObject defaultStaff;
    public GameObject talkPanel;

    [SerializeField] GameObject emote1;
    [SerializeField] GameObject emote2;
    [SerializeField] GameObject emote3;
    [SerializeField] GameObject angryBG;

    [SerializeField] TextMeshPro talkText;

    [SerializeField] List<TalkData> talkDatas;

    private bool isActive = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        talkDatas = TalkCSVLoader.LoadTalk();
        
        //OffEmote();
        talkPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if(isActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                SoundManager.instance.PlayNewBackSound("Map_Check2", SoundType.Se);

                CloseTalk();
            }
        }
    }

    // 대화패널 활성화
    void DisplayTalk()
    {
        talkPanel.SetActive(true);
        isActive = true;
        gc.btnOnOff(false);
        OffEmote();
        defaultStaff.SetActive(false);

        int index = Random.Range(0, talkDatas.Count);
        ChangeEmotion((StaffEmotes)(talkDatas[index].staffEmote - 1));
        talkText.text = talkDatas[index].staffComment;
    }

    // 대화패널 비활성화
    void CloseTalk()
    {
        isActive = false;
        gc.btnOnOff(true);
        OffEmote();
        defaultStaff.SetActive(true);

        talkPanel.SetActive(false);
    }

    void ChangeEmotion(StaffEmotes stemo)
    {
        switch (stemo)
        {
            case StaffEmotes.Smile1:
                OffEmote();
                emote1.SetActive(true);
                break;
            case StaffEmotes.Smile2:
                OffEmote();
                emote2.SetActive(true);
                break;
            case StaffEmotes.Angry:
                OffEmote();
                angryBG.SetActive(true);
                emote3.SetActive(true);
                break;
            default:
                break;
        }
    }

    void OffEmote()
    {
        angryBG.SetActive(false);
        emote1.SetActive(false);
        emote2.SetActive(false);
        emote3.SetActive(false);
    }
}
