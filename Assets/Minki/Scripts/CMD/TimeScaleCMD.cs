#define ENABLE_TIMESCALE_CMD
#if ENABLE_TIMESCALE_CMD
using SPTr.DeveloperConsole;
using UnityEngine;
using System.Collections.Generic;

namespace SPTr.CMD
{
    public static class TimeScaleCMD
    {
        public static ConsoleCommand sys_modechange_timescale = new("sys_modechange_timescale",
            () =>
            {
                _timescaleMode++;
                _timescaleMode %= MAX_MODE_COUNT;

                switch(_timescaleMode)
                {
                    case 0:
                        Time.timeScale = 1.0f;
                        break;
                    case 1:
                        Time.timeScale = 0.5f;
                        break;
                    case 2:
                        Time.timeScale = 0.25f;
                        break;
                    case 3:
                        Time.timeScale = 0.05f;
                        break;
                }

                if(_printDebugMSG)
                    Debug.Log($"[m : {_timescaleMode}] 현재 타임스케일 {Time.timeScale}");

                return;
            },"/타임스케일을 4번의 모드로 나누어 변경합니다.");

        public static ConsoleCommand sys_timescale = new ConsoleCommand("sys_timescale", (float value) => { Time.timeScale = value; }, "타임스케일을 변경합니다.").SetTrackingValue(() => Time.timeScale.ToString());
        public static ConsoleCommand sys_timescale_printmsg = new ConsoleCommand("sys_timescale_printmsg", (bool value) => { _printDebugMSG = value; }, "타임스케일을 변경할 때 로그메시지의 출력 여부를 설정합니다.").SetTrackingValue(() => _printDebugMSG.ToString());

        private static int _timescaleMode = 0;
        private static bool _printDebugMSG = true;

        const int MAX_MODE_COUNT = 4;
    }
}
#endif