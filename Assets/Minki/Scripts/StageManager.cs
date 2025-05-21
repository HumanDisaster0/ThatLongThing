using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.Windows;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    public int anomalyIdx = 0;
    public int deathCount = 0;
    public MapMatchData matchData;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += SetStage;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetAnomalyName(int anomalyIdx)
    {
        if (anomalyIdx >= m_anomalyName.Count
            || anomalyIdx < 1)
            return "No?. ???";

        return m_anomalyName[anomalyIdx - 1];
    }

    private List<string> m_anomalyName;

    private List<string> m_colsLinq = new List<string>();

    void SetStage(Scene scene, LoadSceneMode mode)
    {
        deathCount = 0;
        matchData = new MapMatchData();

        if (m_anomalyName == null)
        {
            m_colsLinq.Clear();
            TextAsset csv = Resources.Load<TextAsset>("missionCheckText");
            string[] lines = csv.text.Split('\n');

            for (int i = 1; i< lines.Length; i += 3)
            {
                string[] cols = lines[i].Split('/');
                m_colsLinq.Add(cols[0]);
            }

            m_anomalyName = m_colsLinq.OrderBy(s => 
            {
                var match = Regex.Match(s, @"\d+");
                return match.Success ? int.Parse(match.Value) : int.MaxValue;
            }).ToList();
        }

        //�÷��̾� ����


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

        //������ ��� ���� ��� ���� �ϳ� ����
        else if (offTraps == 0)
        {
            result[Random.Range(0, result.Length)].SpecifiedSet(false);
        }

        //mappinSetter ����
        var mappinSetter = FindObjectsByType<MapPinSetter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (mappinSetter == null)
            Debug.LogWarning("StageManger - ���� ĵ������ ã�� �� ����!, ���� �������� �߰��ϼ���");

        foreach (var com in mappinSetter)
            com.maxPinCount = result.Length;

        //�̻�����
        switch (anomalyIdx)
        {
            //no.1 - ���α� ��Ƽ
            case 1:
                {
                    MonsterManager.instance?.SetSpawnType(MSpawnType.Alter);
                    MonsterManager.instance?.ChangeAllScale(4.0f);
                    PlatformManager.instance?.SetPlatformType(PlatformType.Alter);
                    PlatformManager.instance?.StopSelectedMoveTiles();
                    break;
                }

            //no.2 - ���� ����
            case 2:
                {
                    PlatformManager.instance?.SetPlatformType(PlatformType.Alter);
                    GameObject.Find("Anomaly").transform.Find("Trex").gameObject.SetActive(true);
                    var fallPlatforms = GameObject.Find("TrapInfos").transform;
                    for(int i = 4; i <= 6; i++)
                    {
                        fallPlatforms.GetChild(i).GetComponent<TrapSetter>().SpecifiedSet(false);
                    }
                    break;
                }


            //no.3 - ���������� �ູ
            case 3:
                {
                    MonsterManager.instance?.SetType(MonsterType.Rabbit);
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


            //no.5 - ���� Ư��
            case 5:
                {
                    //GameObject.Find("ShadowMask").SetActive(false);
                    GameObject.Find("Anomaly").transform.Find("Comet").gameObject.SetActive(true);
                    //GameObject.Find("RedPortal").SetActive(false); 

                    break;
                }


            //no.6 - �뷮�߻�!! (4��)
            case 6:
                {
                    var X4 = GameObject.Find("Anomaly").transform.Find("X4").gameObject;
                    X4.SetActive(true);

                    var trapTrigger = X4.transform.Find("TrapTrigger").GetComponent<TrapTrigger>();

                    trapTrigger.OnResetTrigger = new UnityEngine.Events.UnityEvent();

                    var monsters = X4.transform.Find("Monsters");

                    for(int i = 0; i< monsters.childCount; i++)
                    {
                        trapTrigger.OnResetTrigger.AddListener(monsters.GetChild(i).GetComponentInChildren<MMove>(true).Respawn);
                    }

                    PlatformManager.instance?.SetPlatformType(PlatformType.Alter);
                    PlatformManager.instance?.StopSelectedMoveTiles();
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

            //no.8 - �� ���� Ȩ
            case 8:
                {
                    GameObject.Find("BluePortal").SetActive(false);

                    break;
                }


            //no.9 - ���α� ��Ƽ
            //todo - �÷��̾� ����
            case 9:
                {
                    var pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                    pc.Invincibility = true;
                    pc.playerScale = 4.0f;
                    pc.jumpForce = 8;
                    pc.gravityForce = 15;
                    pc.ApplyScale();

                    pc.transform.GetComponentInChildren<MagicAbility>().scale = 4.0f;

                    var destroyer = pc.gameObject.AddComponent<DestroyerForPlayer>();
                    destroyer.destructibleTilemaps = new UnityEngine.Tilemaps.Tilemap[6];
                    destroyer.destructibleTilemaps[0] = GameObject.Find("Ground")?.GetComponent<Tilemap>();
                    if(destroyer.destructibleTilemaps[0] == null)
                    {
                        Debug.LogWarning("GroundŸ���� ã�� �� �����ϴ�.");
                        destroyer.enabled = false;
                    }

                    if(destroyer.enabled)
                    {
                        var fallPlatforms = GameObject.Find("FallPlatforms").transform;
                        for (int i = 0; i <= 3; i++)
                        {
                            destroyer.destructibleTilemaps[i + 1] = fallPlatforms.GetChild(i).GetComponent<Tilemap>();
                        }

                        destroyer.destructibleTilemaps[destroyer.destructibleTilemaps.Length - 1] = GameObject.Find("DynamicPlatforms").transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Tilemap>();
                    }
                    destroyer.tilemapYMin = -2;

                    var trapTrigger = pc.gameObject.AddComponent<TrapTrigger>();
                    trapTrigger.OnResetTrigger = new UnityEngine.Events.UnityEvent();
                    trapTrigger.OnResetTrigger.AddListener(destroyer.RestoreDestroyedTiles);
                    trapTrigger.OnResetTrigger.AddListener(destroyer.RespawnDestoryedEnemy);

                    var flyingTile = Resources.Load<GameObject>("FlyingThings");
                    destroyer.flyingTilePrefab = flyingTile;

                    destroyer.sideJitter = 10;

                    destroyer.shakeForce = new Vector3(8f, 0.2f, 5f);

                    var minimapPlayerPos = FindObjectsByType<MinimapPlayerPos>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                    if(minimapPlayerPos != null)
                    {
                        var rect = minimapPlayerPos[0].GetComponent<RectTransform>();
                        rect.sizeDelta *= 4;
                    }

                    var monsters = GameObject.Find("Monsters").transform;

                    MonsterManager.instance.ChangeAllScale(1.01f);

                    break;

                }

            //no.10 - ���� �÷���
            case 10:
                {
                    GameObject.Find("MovePlatforms").SetActive(false);
                    GameObject.Find("TrapInfos").SetActive(false);

                    GameObject.Find("Anomaly").transform.Find("InvisiblePlatform").gameObject.SetActive(true);

                    //����
                    result = FindObjectsByType<TrapSetter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
                    if (result == null)
                    {
                        return;
                    }

                    offTraps = 0;
                    foreach (var setter in result)
                    {
                        setter.RandomSet();
                        if (!setter.GetResult)
                            offTraps++;
                    }

                    //������ ��� ���� ��� ���� �ϳ� �ѱ�
                    if (result.Length == offTraps)
                    {
                        result[Random.Range(0, result.Length)].SpecifiedSet(true);
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
                    offTraps = 0;
                    foreach (var setter in result)
                    {
                        setter.RandomSet();
                        if (!setter.GetResult)
                            offTraps++;
                    }

                    //������ ��� ���� ��� ���� �ϳ� �ѱ�
                    if (result.Length == offTraps)
                    {
                        result[Random.Range(0, result.Length)].SpecifiedSet(true);
                    }

                    //������ ��� ���� ��� ���� �ϳ� ����
                    else if (offTraps == 0)
                    {
                        result[Random.Range(0, result.Length)].SpecifiedSet(false);
                    }


                    break;
                }


            //no.12 - ���� �ϴÿ� ������ (�� ��������!!)
            case 12:
                {
                    GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                    GameObject.Find("Anomaly").transform.Find("BigStone").gameObject.SetActive(true);
                    var fallPlatforms = GameObject.Find("TrapInfos").transform;
                    for (int i = 4; i <= 6; i++)
                    {
                        fallPlatforms.GetChild(i).GetComponent<TrapSetter>().SpecifiedSet(false);
                    }
                    break;
                }

            //no.13 - �ϴÿ��� ��尡 �����´ٸ�
            case 13:
                {
                    GameObject.Find("Anomaly").transform.Find("GoldRain").gameObject.SetActive(true);
                    break;
                }

            //no.14 - ����..�� ������ �� ����?
            case 14:
                {
                    //��� ���� ON
                    foreach (var setter in result)
                    {
                        setter.SpecifiedSet(true);
                    }
                    break;
                }

            //no.15 - Ÿ�̹�������
            case 15:
                {
                    PlatformManager.instance.StopSelectedMoveTiles();
                    MonsterManager.instance.SetAllBehavior(MBehavior.Stop);
                    foreach (var setter in result)
                    {
                        setter.SpecifiedSet(false);
                    }
                    break;
                }
            default:
                break;
        }
    }

    public void LoadStage(int stageData)
    {
        if (stageData > 400)
            return;

        int stageNum = stageData / 100;
        int anomalyNum = stageData % 100;

        anomalyIdx = anomalyNum;

        switch (stageNum)
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
    }

    //static KeyValuePair<string,string> GetCurrentStageInfo()
    //{

    //}
}
