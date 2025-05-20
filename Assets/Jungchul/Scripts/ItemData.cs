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
        //���� �Ǹ� ������(���׷��̵�ɷ�) ��� ����
        itemName = name;        
        description = desc;
    }
}
