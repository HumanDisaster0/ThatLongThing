using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultSourceData : MonoBehaviour
{
    public Coroutine myCoroutine;
    public float volume = 0f;
    public bool isVolConByManager = true;
    bool flag = true;

    private void Update()
    {
        if(flag != isVolConByManager)
        {
            if (!isVolConByManager)
            {
                GetComponent<AudioSource>().volume = volume;
            }
            flag = isVolConByManager;
        }
    }
}
