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
                    if(m_matcher != null)
                    {
                        m_matcher = matcherCom;
                        return m_matcher;
                    }
                }
                return null;
            }
        }

        static MapPinMatchChecker m_matcher;


        public static ConsoleCommand cmd_matcher_checkmappin = new ConsoleCommand(
            "cmd_matcher_checkmappin",
            () =>
            {
                var data = matcher.CreateMatchData();
                Debug.Log($"{data.correct} : {data.wrong}");
            },
            "/지도 핀의 판정을 확인합니다.", ExecFlag.CHEAT);
    }
}
