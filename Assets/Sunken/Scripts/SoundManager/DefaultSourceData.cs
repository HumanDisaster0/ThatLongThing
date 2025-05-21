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
    
    Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);
        isVisible = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1 && screenPoint.z > 0;
    }


}
