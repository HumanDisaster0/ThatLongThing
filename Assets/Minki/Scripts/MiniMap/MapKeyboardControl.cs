using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MapKeyboardControl : MonoBehaviour
{
    public RectTransform miniMapUIRect;
    public RectTransform miniMapOpenRect;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            miniMapUIRect.gameObject.SetActive(false);
            miniMapOpenRect.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            miniMapUIRect.gameObject.SetActive(true);
            miniMapOpenRect.gameObject.SetActive(false);
        }
    }
}
