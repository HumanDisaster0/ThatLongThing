using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    public int gold = 0;

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

    public void PlusGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gold ����: {gold}");
    }

    public void MinusGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"Gold ����: {gold}");
        }
        else
        {
            Debug.Log("Gold ����!");
        }
    }
}
