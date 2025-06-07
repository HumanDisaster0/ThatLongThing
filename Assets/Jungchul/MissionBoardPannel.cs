using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionBoardPannel : MonoBehaviour
{
    public CustomClickable CloseBtn;
    // Start is called before the first frame update

    private void Awake()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        CloseBtn.SetClickAction(() =>
        {
            //PostedMissionPanel.Instance.CardShowSet(false);
            //PauseManager.Instance.pauseButtonInstance.SetActive(true);
            GuildRoomManager.Instance.missionBoardPanel.SetActive(false);
            PostedMissionPanel.Instance.CardShowSet(false);
            
            if(GuildRoomManager.Instance.preMissionBoardVstate == GuildRoomManager.viewState.COUNTER)
                GuildRoomManager.Instance.cState = GuildRoomManager.counterState.C_IDLE;
            
            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.Instance.preMissionBoardVstate);

        });
    }

    void Start()
    {

    }
    private void OnEnable()
    {
        CloseBtn.isInteractable = true;
        CloseBtn.inputAllowed = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (PostedMissionPanel.Instance.currentPopup)
        {
            CloseBtn.isInteractable = false;
        }
        else
        { 
            CloseBtn.isInteractable = true;
            CloseBtn.inputAllowed = true;
        }

    }


}
