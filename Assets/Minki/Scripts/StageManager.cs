using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    public int anomalyIdx = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += LoadStage;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadStage(Scene scene, LoadSceneMode mode)
    {
        //이름으로 피하기
        //if (scene.name.Contains("Stage"))
        //    return;

        var result = FindObjectsByType<TrapSetter>(FindObjectsSortMode.None);
        if (result == null)
        {
            return;
        }

        //함정
        foreach (var setter in result)
        {
            setter.RandomSet();
        }

        //이상현상
        switch (anomalyIdx)
        {

            //no.4 - 날아오르라 주작이여
            case 4:
                var anomaly = GameObject.Find("Level").transform.Find("Normal").Find("Anomaly");
                anomaly.Find("Jujak").gameObject.SetActive(true);
                anomaly.Find("JujakTrap").gameObject.SetActive(true);
                anomaly.Find("JujakTrapInfo").gameObject.SetActive(true);

                var trap = GameObject.Find("Level").transform.Find("Normal").Find("Trap");
                trap.Find("StoneTrapInfo").GetComponent<TrapSetter>().SpecifiedSet(false);
                break;

            //no.11 - 거울 속에 비친 나
            case 11:
                var camCon = Camera.main.GetComponent<CameraController>();
                camCon.isMirrored = true;

                GameObject.Find("Level").transform.Find("Mirrored").gameObject.SetActive(true);
                GameObject.Find("StartWall").SetActive(false);
                break;

            default:
                break;
        }
    }

}
