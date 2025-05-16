using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSelectCloseButton : MonoBehaviour
{
    public Panel panelA;
    public Panel panelB;
    public Panel panelC;

    public int missionNumber;

    // Start is called before the first frame update
    void Start()
    {        
        CustomClickable clickable = GetComponent<CustomClickable>();

        //clickable.gameObject.SetActive(false);  

        clickable.SetClickAction(() =>
        {
            if (panelA != null)
            {
                panelA.Close(); // 클릭 시 패널 비활성화
                
            }

            if (panelB != null)
            {
                panelB.Close(); // 클릭 시 패널 비활성화

            }

            if (panelC != null)
            {
                panelC.Close(); // 클릭 시 패널 비활성화

            }

            GuildRoomManager.Instance.SelectMission(missionNumber);
            GuildRoomManager.Instance.DoorOutOn();
            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);


        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
