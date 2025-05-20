using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //판매중인 아이템 리스트 등 멤버변수 예시
    public List<ItemData> ItemsOnSale = new List<ItemData>();

    private void Start()
    {
        //ItemsOnSale.Add();
    }

    public void addItem(ItemData item)
    {
        ItemsOnSale.Add(item);
    }

    public void buyItem(ItemData item)
    {
        ItemsOnSale.Add(item);
    }

}
