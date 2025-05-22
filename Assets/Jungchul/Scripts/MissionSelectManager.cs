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

        // �ʱ�ȭ: ��� �̼��� Ŭ������� ���� ���·� ����
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
    /// �̼� Ŭ���� ó��
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
    /// �̼� �ʱ�ȭ (��� ��Ŭ���� ���·�)
    /// </summary>
    public void ResetAllMissions()
    {
        foreach (var code in allMissionCodes)
        {
            missionClearDict[code] = false;
        }
    }

    /// <summary>
    /// �տ������� Ŭ������� ���� �̼� 3�� ����, ���� �� ������ 0���� ä��
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

        // ������ ����ŭ 0���� ä��
        while (result.Count < 3)
        {
            result.Add(0);
        }

        return result;
    }
}