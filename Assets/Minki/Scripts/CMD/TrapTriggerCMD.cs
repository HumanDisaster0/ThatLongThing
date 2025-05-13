using System.Collections;
using System.Collections.Generic;
using SPTr.DeveloperConsole;
using UnityEngine;

public static class TrapTriggerCMD
{
    public static ConsoleCommand cmd_traptrigger_reset_all = new ConsoleCommand(
        "cmd_traptrigger_reset_all",
        () =>
        {
            ResetAllTrigger();
        },
        "/���� �ʿ� �ִ� ���� Ʈ������ ���¸� ��� �ʱⰪ���� �����ϴ�.", ExecFlag.CHEAT);

    public static void ResetAllTrigger()
    {
        foreach (var trap in GameObject.FindObjectsByType<TrapTrigger>(FindObjectsSortMode.None))
        {
            trap.ResetTrigger();
        }
    }
}
