using System.Collections;
using System.Collections.Generic;
using SPTr.DeveloperConsole;
using UnityEngine;

public static class FootholdCMD
{
    public static ConsoleCommand cmd_reset_foothold_all = new ConsoleCommand(
        "cmd_reset_foothold_all",
        () =>
        {
            foreach (var foothold in GameObject.FindObjectsByType<Foothold>(FindObjectsSortMode.None))
            {
                foothold.ResetFoothold();
            }
        },
        "/���� ���� ������ ���¸� ��� �ʱⰪ���� �����ϴ�.", ExecFlag.CHEAT);
}
