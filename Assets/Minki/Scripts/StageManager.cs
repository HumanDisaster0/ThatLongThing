using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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
        int offTraps = 0;
        foreach (var setter in result)
        {
            setter.RandomSet();
            if(!setter.GetResult)
                offTraps++;
        }

        //������ ��� ���� ��� ���� �ϳ� �ѱ�
        if(result.Length - 1 == offTraps)
        {
            result[Random.Range(0, result.Length)].SpecifiedSet(true);
        }

        //�̻�����
        switch (anomalyIdx)
        {
            //no.2 - ���� ����
            //todo - �̿ϼ�, Ƽ��� �߰��ؾ���
            case 2:
                GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                break;

            //no.3 - ���������� �ູ
            case 3:
                GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.Rabbit);
                break;

            //no.4 - ���ƿ����� �����̿�
            case 4:
                {
                    var anomaly = GameObject.Find("Level").transform.Find("Normal").Find("Anomaly");
                    anomaly.Find("Jujak").gameObject.SetActive(true);
                    anomaly.Find("JujakTrapGrid").gameObject.SetActive(true);
                    //anomaly.Find("JujakTrapInfo").gameObject.SetActive(true);

                    var trap = GameObject.Find("Level").transform.Find("Normal").Find("TrapInfo");

                    //�� ������ �����ִ� ��� �� ������ ������ ������ ���� �� �����ϰ� �ϳ� �ѱ�
                    if (trap.Find("RockTrapInfo").GetComponent<TrapSetter>().trapInfo.type == TrapType.Danger)
                    {
                        trap.Find("RockTrapInfo").GetComponent<TrapSetter>().SpecifiedSet(false);

                        int num = Random.Range(0, 3);

                        for(int i = 0; i < trap.childCount; i++)
                        {
                            if (trap.GetChild(i).name == "RockTrapInfo")
                                continue;

                            if (num == 0)
                            {
                                trap.GetChild(i).GetComponent<TrapSetter>().SpecifiedSet(true);
                                break;
                            }
                            num--;
                        }
                    }
                    break;
                }
                

            //no.7 - �� Ȧ�� ����
            case 7:
                GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.None);
                foreach (var setter in result)
                {
                    setter.SpecifiedSet(false);
                }
                break;

            //no.9 - ���α� ��Ƽ
            //todo - �μ��°� ����, �÷��̾� ����
            case 9:
                var pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                pc.playerScale = 4.0f;
                pc.ApplyScale();
                var destroyer = pc.AddComponent<Destroyer>();
                destroyer.detectionRadius = 2.5f;
                destroyer.destructibleTilemaps = new UnityEngine.Tilemaps.Tilemap[1];
                destroyer.destructibleTilemaps[0] = GameObject.Find("Ground").GetComponent<Tilemap>();
                destroyer.tilemapYMin = -2;

                var trapTrigger = pc.AddComponent<TrapTrigger>();
                trapTrigger.OnResetTrigger = new UnityEngine.Events.UnityEvent();
                trapTrigger.OnResetTrigger.AddListener(destroyer.RestoreDestroyedTiles);

                var flyingTile = Resources.Load<GameObject>("FlyingThings");
                destroyer.flyingTilePrefab = flyingTile;

                destroyer.sideJitter = 10;
                break;

            //no.11 - �ſ� �ӿ� ��ģ ��
            case 11:
                var camCon = Camera.main.GetComponent<CameraController>();
                camCon.isMirrored = true;

                //�ſ� ����
                GameObject.Find("Level").transform.Find("Mirrored").gameObject.SetActive(true);
                GameObject.Find("StartWall").SetActive(false);

                //������ Ŭ������ ����
                GameObject.Find("NormalClearZone").SetActive(false);

                //���� ���� ����ȭ
                var trapInfo = GameObject.Find("Level").transform.Find("Normal").Find("TrapInfo");

                for (int i = 0; i < trapInfo.childCount; i++)
                {
                    trapInfo.GetChild(i).GetComponent<TrapSetter>().RandomSet();
                }

                var mirroredApearTrap = GameObject.Find("Level").transform.Find("Mirrored").Find("Trap").Find("ApearTrapGrid M").Find("ApearTrap M").GetComponent<ReactivePlatform>();
                var normalApearTrap = GameObject.Find("Level").transform.Find("Normal").Find("Trap").Find("ApearTrapGrid").Find("ApearTrap").GetComponent<ReactivePlatform>();

                var mappinSetter = FindObjectsByType<MapPinSetter>(FindObjectsInactive.Include,FindObjectsSortMode.None);

                foreach (var com in mappinSetter)
                    com.maxPinCount = 4;

                //var mappinSetter = FindObjectOfType<MapPinSetter>();
                //mappinSetter.maxPinCount = 4;

                if (!normalApearTrap.TrapIsOff)
                {
                    mirroredApearTrap.type = normalApearTrap.type;
                    mirroredApearTrap.SetPlatformOption();
                }
                else
                {
                    mirroredApearTrap.TrapOff();
                }

                break;

            //no.12 - ���� �ϴÿ� ������ (�� ��������!!)
            //todo - ���־�� ��
            case 12:
                GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                break;

            default:
                break;
        }
    }

}
