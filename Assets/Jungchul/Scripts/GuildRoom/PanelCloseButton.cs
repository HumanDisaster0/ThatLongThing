using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelCloseButton : MonoBehaviour
{
    public Panel panel;
    // Start is called before the first frame update
    void Start()
    {        
        CustomClickable clickable = GetComponent<CustomClickable>();

        //clickable.gameObject.SetActive(false);  

        clickable.SetClickAction(() =>
        {
            if (panel != null)
            {
                panel.Close(); // 클릭 시 패널 비활성화
                GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
