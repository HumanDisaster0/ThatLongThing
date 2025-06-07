using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public enum TrapType
{
    Fine = 0,
    Danger,
    Strange
}

public class TrapInfo : MonoBehaviour
{
    public TrapType type;
    public bool dontShowFX = false;
    public bool staticType = false;
    public void SetType(TrapType trapType) {  type = trapType; } 
    public void SetType(int trapType)
    {
        if (trapType < 0 || trapType >= (int)TrapType.Strange)
        {
            Debug.LogError("Invalid TrapType: " + trapType);
            return;
        }
        type = (TrapType)trapType;
    }
}
