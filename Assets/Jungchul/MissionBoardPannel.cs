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
            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);
        });
    }

    void Start()
    {

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
        }

    }


}
