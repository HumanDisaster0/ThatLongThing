using SPTr.DeveloperConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Minki.Scripts.CMD
{
    public static class MapPinMatchCMD
    {
        public static MapPinMatchChecker matcher
        {
            get
            {
                foreach (var matcherCom in GameObject.FindObjectsByType<MapPinMatchChecker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if(matcherCom != null)
                    {
                        return matcherCom;
                    }
                }
                return null;
            }
        }


        public static ConsoleCommand cmd_matcher_checkmappin = new ConsoleCommand(
            "cmd_matcher_checkmappin",
            () =>
            {
                var data = (matcher?.CreateMatchData()) ?? null;
                foreach (var trapMatch in data)
                {
                    Debug.Log($"TrapPos: {trapMatch.position}, State: {trapMatch.state}");
                }
            },
            "/지도 핀의 판정을 확인합니다.", ExecFlag.CHEAT);
    }
}
