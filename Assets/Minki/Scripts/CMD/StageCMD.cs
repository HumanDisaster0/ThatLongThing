using SPTr.DeveloperConsole;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StageCMD 
{
    public const string COLOR_ERROR = "#F78181";

    public static ConsoleCommand cmd_check_anomalyname = new ConsoleCommand("cmd_check_anomalyname",
            (int number) =>
            {

                Debug.Log(StageManager.instance.GetAnomalyName(number));

            }, "이상현상의 이름을 출력합니다. \n- cmd_changestage <이상현상 번호 (1~15)>", ExecFlag.CHEAT);


    public static ConsoleCommand cmd_changestage = new ConsoleCommand("cmd_changestage",
            (string arguments) =>
            {
                string[] splitText = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (splitText.Length > 2)
                {
                    Debug.Log($"<color={COLOR_ERROR}>오류 : 인자가 너무 많습니다.</color>");
                    return;
                }

                int stageNum = 0;
                int anomalyNum = 0;

                if (splitText.Length < 2)
                {
                    if(!int.TryParse(splitText[0], out stageNum))
                    {
                        Debug.Log($"<color={COLOR_ERROR}>인자가 숫자가 아닙니다.</color>");
                        return;
                    }
                }
                else
                {
                    if (!int.TryParse(splitText[0], out stageNum)
                       || !int.TryParse(splitText[1], out anomalyNum))
                    {
                        Debug.Log($"<color={COLOR_ERROR}>인자가 숫자가 아닙니다.</color>");
                        return;
                    }
                }

                ChangeStage(stageNum, anomalyNum);


            }, "씬을 변경합니다. \n- cmd_changestage <스테이지 번호 (1~3)>, <이상현상 번호 (1~15)>", ExecFlag.CHEAT);

    public static void ChangeStage(int stage, int anomaly = 0)
    {
        StageManager.instance.anomalyIdx = anomaly;
        
        switch(stage)
        {
            case 1:
                SceneManager.LoadScene("Stage1");
                break;
            case 2:
                SceneManager.LoadScene("Stage2");
                break;
            case 3:
                SceneManager.LoadScene("Stage3");
                break;
        }

        StageManager.instance.DebugStageCountCorretion();
    }
}
