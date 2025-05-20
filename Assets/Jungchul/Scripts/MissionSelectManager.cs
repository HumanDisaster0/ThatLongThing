using System.Collections.Generic;
using UnityEngine;

public class MissionSelectManager : MonoBehaviour
{
    public static MissionSelectManager Instance;

    private List<int> allMissionCodes = new List<int>()
    {
        104, 111, 203,
        212, 207, 314, 
        209, 202, 301, 
        306, 313, 315, 
        305, 308, 310,    
    };

    private Dictionary<int, bool> missionClearDict = new Dictionary<int, bool>();

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

    /// <summary>
    /// �̼� Ŭ���� ó��
    /// </summary>
    public void SetMissionCleared(int code)
    {
        if (missionClearDict.ContainsKey(code))
            missionClearDict[code] = true;
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