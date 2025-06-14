using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Map_DataBtn : MonoBehaviour
{
    [Serializable]
    struct MissionData
    {
        public int code;
        public Sprite sprite;
    };

    [SerializeField] List<MissionData> missionDataList = new List<MissionData>();
    Dictionary<int, Sprite> spriteDictionary = new Dictionary<int, Sprite>();

    [SerializeField] int code = 0;

    private void Awake()
    {
        // List 데이터를 Dictionary로 변환
        foreach (var entry in missionDataList)
        {
            spriteDictionary[entry.code] = entry.sprite;
        }
    }

    public void ShowMissionPanel()
    {
        if (StageManager.instance != null)
            code = StageManager.instance.anomalyIdx;
        else
            Debug.LogError("StageManager instance is null");

        Sprite newSpr = null;
        spriteDictionary.TryGetValue(code, out newSpr);
        MissionCheckCanvas.instance.ShowMission(newSpr);
    }

    public void CloseMinimap()
    {
        MapOnOffControl mapOnOffControl = FindObjectOfType<MapOnOffControl>();
        if (mapOnOffControl != null)
        {
            mapOnOffControl.HideMinimap();
        }
        else
        {
            Debug.LogError("MapOnOffControl instance is null");
        }
    }
}
