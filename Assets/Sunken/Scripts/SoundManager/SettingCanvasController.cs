using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingCanvasController : MonoBehaviour
{
    [SerializeField] GameObject settingCanvas;

    void Start()
    {
        if (settingCanvas == null)
        {
            settingCanvas = transform.Find("SettingCanvas").gameObject;
        }
    }

    public void SetSettingCanvas(bool _value)
    {
        settingCanvas?.SetActive(_value);
    }
}
