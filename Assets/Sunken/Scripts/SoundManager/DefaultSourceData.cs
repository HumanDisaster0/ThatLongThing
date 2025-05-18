using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefaultSourceData : MonoBehaviour
{
    [Range(0f, 1f)]
    public float volume = 1f;

    public Coroutine myCoroutine;
    
    public bool isVolCon = true;
    public bool isVisible = true;

    private void Awake()
    {
        
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }
}
