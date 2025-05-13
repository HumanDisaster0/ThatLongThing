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
        "/현재 맵에 있는 함정 트리거의 상태를 모두 초기값으로 돌립니다.", ExecFlag.CHEAT);

    public static void ResetAllTrigger()
    {
        foreach (var trap in GameObject.FindObjectsByType<TrapTrigger>(FindObjectsSortMode.None))
        {
            trap.ResetTrigger();
        }
    }
}
