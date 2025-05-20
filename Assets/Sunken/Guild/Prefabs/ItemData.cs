using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]public class ItemData
{
    public string itemName;      
    public string description;

    public ItemData(string name,  string desc)
    {
        //상점 판매 아이템(업그레이드능력) 멤버 예시
        itemName = name;        
        description = desc;
    }
}
