using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public enum TrapType
{
    Fine,
    Danger,
    Strange
}

public class TrapInfo : MonoBehaviour
{
    public TrapType type;
    public void SetType(TrapType trapType) {  type = trapType; } 
}
