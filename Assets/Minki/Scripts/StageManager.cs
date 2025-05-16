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
        foreach (var setter in result)
        {
            setter.RandomSet();
        }

        //이상현상
        switch (anomalyIdx)
        {
            //no.2 - 쥬라식 던전
            //todo - 미완성, 티라노 추가해야함
            case 2:
                GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                break;

            //no.3 - 복슬복슬한 행복
            case 3:
                GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.Rabbit);
                break;

            //no.4 - 날아오르라 주작이여
            case 4:
                var anomaly = GameObject.Find("Level").transform.Find("Normal").Find("Anomaly");
                anomaly.Find("Jujak").gameObject.SetActive(true);
                anomaly.Find("JujakTrapGrid").Find("JujakTrap").gameObject.SetActive(true);
                anomaly.Find("JujakTrapInfo").gameObject.SetActive(true);

                var trap = GameObject.Find("Level").transform.Find("Normal").Find("TrapInfo");
                trap.Find("RockTrapInfo").GetComponent<TrapSetter>().SpecifiedSet(false);
                break;

            //no.7 - 나 홀로 던전
            case 7:
                GameObject.Find("MonsterManager").GetComponent<MonsterManager>().SetType(MonsterType.None);
                foreach (var setter in result)
                {
                    setter.SpecifiedSet(false);
                }
                break;

            //no.9 - 거인국 네티
            case 9:
                //일단 3배로 키우기
                var pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                pc.playerScale = 3.0f;
                pc.ApplyScale();
                var destroyer = pc.AddComponent<Destroyer>();
                destroyer.detectionRadius = 4.0f;
                destroyer.destructibleTilemaps = new UnityEngine.Tilemaps.Tilemap[1];
                destroyer.destructibleTilemaps[0] = GameObject.Find("Ground").GetComponent<Tilemap>();

                var flyingTile = Resources.Load<GameObject>("FlyingThings");
                destroyer.flyingTilePrefab = flyingTile;

                destroyer.sideJitter = 10;
                break;

            //no.11 - 거울 속에 비친 나
            //todo - 나가는 포탈 거꾸로 배치
            case 11:
                var camCon = Camera.main.GetComponent<CameraController>();
                camCon.isMirrored = true;

                //거울 해제
                GameObject.Find("Level").transform.Find("Mirrored").gameObject.SetActive(true);
                GameObject.Find("StartWall").SetActive(false);

                //함정 상태 동기화
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

                break;

            //no.12 - 마른 하늘에 날벼락 (돌 굴러가요!!)
            //todo - 돌넣어야 함
            case 12:
                GameObject.Find("PlatformManager").GetComponent<PlatformManager>().SetPlatformType(PlatformType.Alter);
                break;

            default:
                break;
        }
    }

}
