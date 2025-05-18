using SPTr.DeveloperConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Minki.Scripts.CMD
{
    public static class CameraControlCMD
    {
        public const string COLOR_ERROR = "#F78181";

        public static CameraController camCon
        {
            get
            {
                return Camera.main.GetComponent<CameraController>();
            }
        }

        public static ConsoleCommand cmd_cam_shake = new ConsoleCommand(
            "cmd_cam_shake",
            (string arguments) =>
            {
                string[] splitText = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (splitText.Length > 3)
                {
                    Debug.Log($"<color={COLOR_ERROR}>오류 : 인자가 너무 많습니다.</color>");
                    return;
                }

                if (splitText.Length < 3)
                {
                    Debug.Log($"<color={COLOR_ERROR}>오류 : 인자가 적습니다.</color>");
                }
                else
                {
                    float speed = 0;
                    float amount = 0;
                    float recovery = 0;

                    if(float.TryParse(splitText[0], out speed)
                       && float.TryParse(splitText[1], out amount)
                       && float.TryParse(splitText[2], out recovery))
                    {
                        ShakeCamera(speed, amount, recovery);
                    }
                    else
                    {
                        Debug.Log($"<color={COLOR_ERROR}>오류 : 인식할 수 없는 인자입니다.</color>");
                    }
                }

            },
            "카메라를 흔듭니다. \n cmd_cam_shake <float : 속도>, <float : 충격량>, <float : 회복력>", ExecFlag.CHEAT);

        public static ConsoleCommand cmd_cam_trauma = new ConsoleCommand(
            "cmd_cam_trauma",
            (bool arguments) =>
            {
                SetShakeTrauma(arguments);
            },
            "카메라 흔들림의 지속여부입니다.", ExecFlag.CHEAT).SetTrackingValue(() => (!camCon.IsNonTrauma()).ToString());

        static bool m_lastTrauma = false;
        
        public static void ShakeCamera(float speed, float amount, float recovery)
        {
            camCon.ShakeCamera(speed, amount, recovery);
        }

        public static void SetShakeTrauma(bool trauma)
        {
            camCon.SetShakeTrauma(trauma);
        }
    }
}
