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
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
        {
            if (miniMapUIRect.gameObject.activeSelf)
            {
                HideMinimap();
            }
            else
            {
                ShowMinimap();
            }
            return;
        }
    }

    public void HideMinimap()
    {
        SoundManager.instance.PlayNewBackSound("Map_Button");
        miniMapUIRect.gameObject.SetActive(false);
        miniMapOpenRect.gameObject.SetActive(true);
    }

    public void ShowMinimap()
    {
        SoundManager.instance.PlayNewBackSound("Map_Button");
        miniMapUIRect.gameObject.SetActive(true);
        miniMapOpenRect.gameObject.SetActive(false);
    }
}
