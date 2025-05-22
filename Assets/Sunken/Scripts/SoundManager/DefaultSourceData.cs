using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum SoundType
{
    No = 0,
    Se,
    Bg
}

public class DefaultSourceData : MonoBehaviour
{
    public Coroutine myCoroutine;
    public float maxDistance = 15f;
    
    public bool isVolCon = true;
    public bool isVisible = true;

    public SoundType soundType = SoundType.No;
    [Range(0f, 1f)]
    public float volOverride = 1f;
    
    Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (SoundManager.instance.isLowResource)
        {
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
            isVisible = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1 && screenPoint.z > 0;
        }
    }
}
