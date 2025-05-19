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
        //이름으로 피하기
        //if (!scene.name.Contains("Stage"))
        //    return;

        var result = FindObjectsByType<TrapSetter>(FindObjectsSortMode.None);
        if (result == null)
        {
            return;
        }

        //함정
        int offTraps = 0;
        foreach (var setter in result)
        {
            setter.RandomSet();
            if(!setter.GetResult)
                offTraps++;
        }

        //함정이 모두 꺼진 경우 랜덤 하나 켜기
        if(result.Length == offTraps)
        {
            result[Random.Range(0, result.Length)].SpecifiedSet(true);
        }

        //함정이 모두 켜진 경우 랜덤 하나 끄기
        else if (offTraps == 0)
        {
            result[Random.Range(0, result.Length)].SpecifiedSet(false);
        }

        //mappinSetter 설정
        var mappinSetter = FindObjectsByType<MapPinSetter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (mappinSetter == null)
            Debug.LogWarning("StageManger - 지도 캔버스를 찾을 수 없음!, 지도 프리펩을 추가하세요");

        foreach (var com in mappinSetter)
            com.maxPinCount = result.Length;

        //이상현상
        switch (anomalyIdx)
        {
            //no.1 - 소인국 네티
            case 1:
                {
                    MonsterManager.instance?.SetSpawnType(MSpawnType.Alter);
                    MonsterManager.instance?.ChangeAllScale(4.0f);
                    PlatformManager.instance?.SetPlatformType(PlatformType.Alter);
                    PlatformManager.instance?.StopMoveTiles();
                    break;
                }

            //no.2 - 쥬라식 던전
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


            //no.3 - 복슬복슬한 행복
            case 3:
                {
                    MonsterManager.instance?.SetType(MonsterType.Rabbit);
                    break;
                }

            //no.4 - 날아오르라 주작이여
            case 4:
                {
                    var anomaly = GameObject.Find("Level").transform.Find("Normal").Find("Anomaly");
                    anomaly.Find("Jujak").gameObject.SetActive(true);
                    anomaly.Find("JujakTrapGrid").gameObject.SetActive(true);
                    //anomaly.Find("JujakTrapInfo").gameObject.SetActive(true);

                    var trap = GameObject.Find("Level").transform.Find("Normal").Find("TrapInfo");

                    //돌 함정이 켜져있는 경우 돌 함정을 제외한 나머지 함정 중 랜덤하게 하나 켜기
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


            //no.5 - 혜성 특급
            case 5:
                {
                    //GameObject.Find("ShadowMask").SetActive(false);
                    GameObject.Find("Anomaly").transform.Find("Comet").gameObject.SetActive(true);
                    //GameObject.Find("RedPortal").SetActive(false); 

                    break;
                }


            //no.7 - 나 홀로 던전
            case 7:
                {
                    GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.None);
                    foreach (var setter in result)
                    {
                        setter.SpecifiedSet(false);
                    }


                    break;
                }

            //no.8 - 노 웨이 홈
            case 8:
                {
                    GameObject.Find("BluePortal").SetActive(false);

                    break;
                }


            //no.9 - 거인국 네티
            //todo - 플레이어 무적
            case 9:
                {
                    var pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                    pc.Invincibility = true;
                    pc.playerScale = 4.0f;
                    pc.ApplyScale();
                    var destroyer = pc.gameObject.AddComponent<DestroyerForPlayer>();
                    destroyer.destructibleTilemaps = new UnityEngine.Tilemaps.Tilemap[6];
                    destroyer.destructibleTilemaps[0] = GameObject.Find("Ground")?.GetComponent<Tilemap>();
                    if(destroyer.destructibleTilemaps[0] == null)
                    {
                        Debug.LogWarning("Ground타일을 찾을 수 없습니다.");
                        destroyer.enabled = false;
                    }

                    if(destroyer.enabled)
                    {
                        var fallPlatforms = GameObject.Find("FallPlatforms").transform;
                        for (int i = 0; i <= 3; i++)
                        {
                            destroyer.destructibleTilemaps[i + 1] = fallPlatforms.GetChild(i).GetComponent<Tilemap>();

                            print(destroyer.destructibleTilemaps[i + 1]);
                        }

                        destroyer.destructibleTilemaps[destroyer.destructibleTilemaps.Length - 1] = GameObject.Find("DynamicPlatforms").transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Tilemap>();
                    }
                    destroyer.tilemapYMin = -2;

                    var trapTrigger = pc.gameObject.AddComponent<TrapTrigger>();
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

            //no.10 - 투명 플랫폼
            case 10:
                {
                    GameObject.Find("MovePlatforms").SetActive(false);
                    GameObject.Find("TrapInfos").SetActive(false);

                    GameObject.Find("Anomaly").transform.Find("InvisiblePlatform").gameObject.SetActive(true);

                    //함정
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

                    //함정이 모두 꺼진 경우 랜덤 하나 켜기
                    if (result.Length == offTraps)
                    {
                        result[Random.Range(0, result.Length)].SpecifiedSet(true);
                    }

                    break;
                }


            //no.11 - 거울 속에 비친 나
            case 11:
                {
                    //카메라 연출 활성화
                    var camCon = Camera.main.GetComponent<CameraController>();
                    camCon.isMirrored = true;

                    //반대쪽 활성화
                    GameObject.Find("Level").transform.Find("Mirrored").gameObject.SetActive(true);
                    GameObject.Find("StartWall").SetActive(false);

                    //정방향 클리어존 끄기
                    GameObject.Find("NormalClearZone").SetActive(false);

                    //함정 상태 동기화
                    offTraps = 0;
                    foreach (var setter in result)
                    {
                        setter.RandomSet();
                        if (!setter.GetResult)
                            offTraps++;
                    }

                    //함정이 모두 꺼진 경우 랜덤 하나 켜기
                    if (result.Length == offTraps)
                    {
                        result[Random.Range(0, result.Length)].SpecifiedSet(true);
                    }

                    //함정이 모두 켜진 경우 랜덤 하나 끄기
                    else if (offTraps == 0)
                    {
                        result[Random.Range(0, result.Length)].SpecifiedSet(false);
                    }


                    break;
                }


            //no.12 - 마른 하늘에 날벼락 (돌 굴러가요!!)
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

            //no.14 - 정말..다 멀쩡한 거 맞지?
            case 14:
                {
                    //모든 함정 ON
                    foreach (var setter in result)
                    {
                        setter.SpecifiedSet(true);
                    }
                    break;
                }

            default:
                break;
        }
    }

}
