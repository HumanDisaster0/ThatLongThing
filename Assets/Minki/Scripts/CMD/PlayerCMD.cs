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
        "/�÷��̾ ���콺 ������ ��ġ�� �ű�ϴ�.", ExecFlag.CHEAT);

    public static ConsoleCommand cmd_player_invicible = new ConsoleCommand(
        "cmd_player_invicible",
        (bool mode) =>
        {
            pc.Invincibility = mode;
        },
        "�÷��̾ �������·� ����ϴ�.\n cmd_player_invicible <bool>", ExecFlag.CHEAT).SetTrackingValue(() => pc.Invincibility.ToString());

    public static ConsoleCommand cmd_player_maxjumpcount = new ConsoleCommand(
       "cmd_player_maxjumpcount",
       (int count) =>
       {
           pc.maxJumpCount = count;
       },
       "�÷��̾��� �ִ� ���� Ƚ���� �����մϴ�. \n cmd_player_maxjumpcount <int>", ExecFlag.CHEAT);

    public static ConsoleCommand cmd_player_magiclevel = new ConsoleCommand(
       "cmd_player_magiclevel",
       (int level) =>
       {
           pc.GetComponentInChildren<MagicAbility>().magicLevel = level;
       },
       "�÷��̾��� ���������� �����մϴ�. \n cmd_player_magiclevel <1~3>", ExecFlag.CHEAT);
}
