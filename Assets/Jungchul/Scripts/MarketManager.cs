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

    //�Ǹ����� ������ ����Ʈ �� ������� ����
    //
    public List<ItemData> ItemsOnSale = new List<ItemData>();

    public int marketLevel = 1;

    public void addItem(ItemData item)
    {
        ItemsOnSale.Add(item);        
    }

    public void buyItem(ItemData item)
    {
        ItemsOnSale.Add(item);
    }

}
