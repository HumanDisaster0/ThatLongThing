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
        // 싱글톤 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 제거
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
    }

    public int calRewardGold()
    {
        return findTrapCount * 100 - deadCount * 20 - ejectionCount * 200;
    }

    public void getRewardGold(int amount)
    {
        totalGold += amount;
        Debug.Log($"Gold 증가: {amount}");

        //rewardGold = 0;
    }

    public void MinusGold(int amount)
    {
        totalGold -= amount;
        
        if (totalGold < 0)
        {
            Debug.Log("천강파산뢰!");
        }
    }

    public void SetReward(int ftc, int dc, int ec)
    {
        GoldManager.Instance.findTrapCount = ftc;
        GoldManager.Instance.deadCount = dc;
        GoldManager.Instance.ejectionCount = ec;
        GoldManager.Instance.rewardGold = GoldManager.Instance.calRewardGold();

        //calRewardGold 임시값 : findTrapCount * 100 + deadCount * 20 + ejectionCount * 200;
    }

    public void ClearReward()
    {
        GoldManager.Instance.findTrapCount = 0;
        GoldManager.Instance.deadCount = 0;
        GoldManager.Instance.ejectionCount = 0;
        GoldManager.Instance.rewardGold = 0;
    }
}
