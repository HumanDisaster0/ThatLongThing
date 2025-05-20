using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    public int gold = 0;

    public int rewardGold = 0;

    public int findTrapCount = 0;
    public int deadCount = 0;
    public int ejectionCount = 0;



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

    public void getRewardGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gold 증가: {gold}");

        rewardGold = 0;
    }

    public void MinusGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"Gold 감소: {gold}");
        }
        else
        {
            Debug.Log("Gold 부족!");
        }
    }
}
