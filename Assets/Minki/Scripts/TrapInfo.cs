using System.Collections;
using System.Collections.Generic;
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
}
