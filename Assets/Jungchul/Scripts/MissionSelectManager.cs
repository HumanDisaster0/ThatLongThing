using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSelectManager : MonoBehaviour
{
    public static MissionSelectManager Instance;

    public List<int> allMissionCodes = new List<int>()
    {
        104, 1111, 2202, 
        3203, 4209, 5212, 
        6207, 7301, 8305, 
        9314, 10306, 11308, 
        12310, 13313, 14315,        
    };
        

    public Dictionary<int, bool> missionClearDict = new Dictionary<int, bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기화: 모든 미션은 클리어되지 않은 상태로 시작
        foreach (int code in allMissionCodes)
        {
            missionClearDict[code] = false;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
        {
            ResetAllMissions();
        }
    }

   

    /// <summary>
    /// 미션 클리어 처리
    /// </summary>
    public void SetMissionCleared(int code)
    {
        if (missionClearDict.ContainsKey(code))
        {
            missionClearDict[code] = true;
            Debug.Log($"missionClearDict[code]: { missionClearDict[code]}");
        }

    }

    /// <summary>
    /// 미션 초기화 (모두 미클리어 상태로)
    /// </summary>
    public void ResetAllMissions()
    {
        foreach (var code in allMissionCodes)
        {
            missionClearDict[code] = false;
        }
    }

    /// <summary>
    /// 앞에서부터 클리어되지 않은 미션 3개 선택, 남은 게 없으면 0으로 채움
    /// </summary>
    public List<int> Generate3MissionCodes()
    {
        List<int> result = new List<int>();
        int count = 0;

        foreach (int code in allMissionCodes)
        {
            if (!missionClearDict[code])
            {
                result.Add(code);
                count++;
                if (count >= 3) break;
            }
        }

        // 부족한 수만큼 0으로 채움
        while (result.Count < 3)
        {
            result.Add(0);
        }

        return result;
    }
}