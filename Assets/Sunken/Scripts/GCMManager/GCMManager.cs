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
            // �̼Ǻ���
            GuildRoomManager.Instance.curVstate = GuildRoomManager.viewState.MISSIONBOARD;
        };
        gcm_Pokedex.onClick = () =>
        {
            // �ٹ�
            GuildRoomManager.Instance.curVstate = GuildRoomManager.viewState.POKEDEX;
        };
        gcm_Talk.onClick = () =>
        {
            // ���������� ��ȭ
        };
    }
}
