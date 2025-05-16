using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrapSetter : MonoBehaviour
{
    public TrapInfo trapInfo;
    public UnityEvent turnOnSet;
    public UnityEvent turnOffSet;

    private void Awake()
    {
        if (!trapInfo)
            trapInfo = GetComponent<TrapInfo>();
    }

    public void RandomSet()
    {
        var rand = Random.Range(0, 2);
        
        if(rand == 0)
        {
            trapInfo.type = TrapType.Fine;
            turnOffSet?.Invoke();
        }
        else
        {
            trapInfo.type = TrapType.Danger;
            turnOnSet?.Invoke();
        }
    }

    public void SpecifiedSet(bool on)
    {
        if(on)
        {
            trapInfo.type = TrapType.Danger;
            turnOnSet?.Invoke();
        }
        else
        {
            trapInfo.type = TrapType.Fine;
            turnOffSet?.Invoke();
        }
    }
}
