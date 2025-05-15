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
        //�̸����� ���ϱ�
        //if (scene.name.Contains("Stage"))
        //    return;

        var result = FindObjectsByType<TrapSetter>(FindObjectsSortMode.None);
        if (result == null)
        {
            return;
        }

        //����
        foreach (var setter in result)
        {
            setter.RandomSet();
        }

        //�̻�����
        switch (anomalyIdx)
        {

            //no.4 - ���ƿ����� �����̿�
            case 4:
                var anomaly = GameObject.Find("Level").transform.Find("Normal").Find("Anomaly");
                anomaly.Find("Jujak").gameObject.SetActive(true);
                anomaly.Find("JujakTrap").gameObject.SetActive(true);
                anomaly.Find("JujakTrapInfo").gameObject.SetActive(true);

                var trap = GameObject.Find("Level").transform.Find("Normal").Find("Trap");
                trap.Find("StoneTrapInfo").GetComponent<TrapSetter>().SpecifiedSet(false);
                break;

            //no.11 - �ſ� �ӿ� ��ģ ��
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
