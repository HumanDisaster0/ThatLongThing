using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultSourceData : MonoBehaviour
{
    public Coroutine myCoroutine;
    public float volume = 0f;
    public bool isVolConByManager = true;
    float prevVolume = 0f;

    private void Update()
    {
        if (prevVolume != volume)
        {
            GetComponent<AudioSource>().volume = volume;
            prevVolume = volume;
        }
    }
}
