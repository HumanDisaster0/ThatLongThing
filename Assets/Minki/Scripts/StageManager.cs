using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
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
        //if (!scene.name.Contains("Stage"))
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
                trap.Find("RockTrapInfo").GetComponent<TrapSetter>().SpecifiedSet(false);
                break;

            //no.11 - �ſ� �ӿ� ��ģ ��
            case 11:
                var camCon = Camera.main.GetComponent<CameraController>();
                camCon.isMirrored = true;

                //�ſ� ����
                GameObject.Find("Level").transform.Find("Mirrored").gameObject.SetActive(true);
                GameObject.Find("StartWall").SetActive(false);

                //���� ���� ����ȭ
                var trapInfo = GameObject.Find("Level").transform.Find("Normal").Find("TrapInfo");

                for (int i = 0; i < trapInfo.childCount; i++)
                {
                    trapInfo.GetChild(i).GetComponent<TrapSetter>().RandomSet();
                }

                var mirroredApearTrap = GameObject.Find("Level").transform.Find("Mirrored").Find("Trap").Find("ApearTrap_M").GetComponent<ReactivePlatform>();
                var normalApearTrap = GameObject.Find("Level").transform.Find("Normal").Find("Trap").Find("ApearTrap").GetComponent<ReactivePlatform>();

                //var mappinSetter = FindObjectOfType<MapPinSetter>();
                //mappinSetter.maxPinCount = 4;

                if (!normalApearTrap.TrapIsOff)
                {
                    mirroredApearTrap.type = normalApearTrap.type;
                    mirroredApearTrap.SetPlatformOption();
                }

                break;

            default:
                break;
        }
    }

}
