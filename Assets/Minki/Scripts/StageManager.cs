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
        if(result.Length == offTraps)
        {
            result[Random.Range(0, result.Length)].SpecifiedSet(true);
        }

        //mappinSetter ����
        var mappinSetter = FindObjectsByType<MapPinSetter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (mappinSetter != null)
            Debug.LogWarning("StageManger - ���� ĵ������ ã�� �� ����!, ���� �������� �߰��ϼ���");

        foreach (var com in mappinSetter)
            com.maxPinCount = result.Length;

        //�̻�����
        switch (anomalyIdx)
        {
            //no.2 - ���� ����
            case 2:
                {
                    GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                    GameObject.Find("Anomaly").transform.Find("Trex").gameObject.SetActive(true);
                    break;
                }


            //no.3 - ���������� �ູ
            case 3:
                {
                    GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.Rabbit);
                    break;
                }

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
                {
                    GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.None);
                    foreach (var setter in result)
                    {
                        setter.SpecifiedSet(false);
                    }


                    break;
                }
                

            //no.9 - ���α� ��Ƽ
            //todo - �÷��̾� ����
            case 9:
                {
                    var pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                    pc.Invincibility = true;
                    pc.playerScale = 4.0f;
                    pc.ApplyScale();
                    var destroyer = pc.AddComponent<DestroyerForPlayer>();
                    destroyer.destructibleTilemaps = new UnityEngine.Tilemaps.Tilemap[1];
                    destroyer.destructibleTilemaps[0] = GameObject.Find("Ground").GetComponent<Tilemap>();
                    destroyer.tilemapYMin = -2;

                    var trapTrigger = pc.AddComponent<TrapTrigger>();
                    trapTrigger.OnResetTrigger = new UnityEngine.Events.UnityEvent();
                    trapTrigger.OnResetTrigger.AddListener(destroyer.RestoreDestroyedTiles);

                    var flyingTile = Resources.Load<GameObject>("FlyingThings");
                    destroyer.flyingTilePrefab = flyingTile;

                    destroyer.sideJitter = 10;

                    var minimapPlayerPos = FindObjectsByType<MinimapPlayerPos>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    if(minimapPlayerPos != null)
                    {
                        var rect = minimapPlayerPos[0].GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(100, 200);
                    }

                    break;

                }

            //no.11 - �ſ� �ӿ� ��ģ ��
            case 11:
                {
                    //ī�޶� ���� Ȱ��ȭ
                    var camCon = Camera.main.GetComponent<CameraController>();
                    camCon.isMirrored = true;

                    //�ݴ��� Ȱ��ȭ
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

                    //var mappinSetter = FindObjectsByType<MapPinSetter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    //foreach (var com in mappinSetter)
                    //    com.maxPinCount = 4;

                    break;
                }


            //no.12 - ���� �ϴÿ� ������ (�� ��������!!)
            case 12:
                {
                    GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                    GameObject.Find("Anomaly").transform.Find("BigStone").gameObject.SetActive(true);
                    break;
                }
            default:
                break;
        }
    }

}
