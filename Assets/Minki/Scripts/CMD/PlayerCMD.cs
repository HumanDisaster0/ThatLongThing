using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPTr.DeveloperConsole;

public static class PlayerCMD
{
    public static PlayerController pc
    {
        get
        {
            m_pc = GameObject.Find("Player").GetComponent<PlayerController>();

            return m_pc;
        }
    }

    public static Camera mainCam
    {
        get
        {
            m_mainCam = Camera.main;

            return m_mainCam;
        }
    }

    static Camera m_mainCam;
    static PlayerController m_pc;


    public static ConsoleCommand cmd_player_gotomouse = new ConsoleCommand(
        "cmd_player_gotomouse",
        () =>
        {
            var worldPoint = mainCam.ScreenToWorldPoint(Input.mousePosition);
            pc.transform.position = new Vector3(worldPoint.x, worldPoint.y, pc.transform.position.z);
            pc.SetVelocity(Vector2.zero);
        },
        "/플레이어를 마우스 포인터 위치로 옮깁니다.", ExecFlag.CHEAT);
}
