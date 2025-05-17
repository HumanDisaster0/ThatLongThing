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

        //mappinSetter 설정
        var mappinSetter = FindObjectsByType<MapPinSetter>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (mappinSetter != null)
            Debug.LogWarning("StageManger - 지도 캔버스를 찾을 수 없음!, 지도 프리펩을 추가하세요");

        foreach (var com in mappinSetter)
            com.maxPinCount = result.Length;

        //이상현상
        switch (anomalyIdx)
        {
            //no.2 - 쥬라식 던전
            case 2:
                {
                    GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                    GameObject.Find("Anomaly").transform.Find("Trex").gameObject.SetActive(true);
                    break;
                }


            //no.3 - 복슬복슬한 행복
            case 3:
                {
                    GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.Rabbit);
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
                

            //no.9 - 거인국 네티
            //todo - 플레이어 무적
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


            //no.12 - 마른 하늘에 날벼락 (돌 굴러가요!!)
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
