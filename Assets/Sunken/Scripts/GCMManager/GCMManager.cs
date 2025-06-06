using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCMManager : MonoBehaviour
{
    [SerializeField] CustomClickable gcm_MissionBoard;
    [SerializeField] CustomClickable gcm_Pokedex;
    [SerializeField] CustomClickable gcm_Talk;

    // Start is called before the first frame update
    void Start()
    {
        gcm_MissionBoard.onClick = () =>
        {
            // 미션보드
            GuildRoomManager.Instance.curVstate = GuildRoomManager.viewState.MISSIONBOARD;
        };
        gcm_Pokedex.onClick = () =>
        {
            // 앨범
            GuildRoomManager.Instance.curVstate = GuildRoomManager.viewState.POKEDEX;
        };
        gcm_Talk.onClick = () =>
        {
            // 접수원과의 대화
        };
    }
}
