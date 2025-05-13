using System.Collections;
using System.Collections.Generic;
using SPTr.DeveloperConsole;
using UnityEngine;

public static class FootholdCMD
{
    public static ConsoleCommand cmd_foothold_reset_all = new ConsoleCommand(
        "cmd_foothold_reset_all",
        () =>
        {
            foreach (var foothold in GameObject.FindObjectsByType<Foothold>(FindObjectsSortMode.None))
            {
                foothold.ResetFoothold();
            }
        },
        "/현재 맵의 발판의 상태를 모두 초기값으로 돌립니다.", ExecFlag.CHEAT);
}
