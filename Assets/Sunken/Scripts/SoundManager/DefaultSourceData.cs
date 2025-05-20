using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DefaultSourceData : MonoBehaviour
{
    public Coroutine myCoroutine;
    public float maxDistance = 20f;
    
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
