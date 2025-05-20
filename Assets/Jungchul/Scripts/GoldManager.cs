using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    public int totalGold = 0;

    public int rewardGold = 0;

    public int findTrapCount = 0;
    public int deadCount = 0;
    public int ejectionCount = 0;

    public int Tax = 0;



    private void Awake()
    {
        // �̱��� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �ߺ� ����
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı� ����
    }

    public int calRewardGold()
    {
        return findTrapCount * 100 - deadCount * 20 - ejectionCount * 200;
    }

    public void getRewardGold(int amount)
    {
        totalGold += amount;
        Debug.Log($"Gold ����: {amount}");

        //rewardGold = 0;
    }

    public void MinusGold(int amount)
    {
        totalGold -= amount;
        
        if (totalGold < 0)
        {
            Debug.Log("õ���Ļ��!");
        }
    }

    public void SetReward()
    {
        GoldManager.Instance.findTrapCount = 5;
        GoldManager.Instance.deadCount = 2;
        GoldManager.Instance.ejectionCount = 1;
        GoldManager.Instance.rewardGold = GoldManager.Instance.calRewardGold();

        //calRewardGold �ӽð� : findTrapCount * 100 + deadCount * 20 + ejectionCount * 200;
    }

    public void ClearReward()
    {
        GoldManager.Instance.findTrapCount = 0;
        GoldManager.Instance.deadCount = 0;
        GoldManager.Instance.ejectionCount = 0;
        GoldManager.Instance.rewardGold = 0;
    }
}
