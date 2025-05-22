using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundBoolValue : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("isLowRes") == 0 ? false : true;
    }

    public void SetToggle()
    {
        PlayerPrefs.SetInt("isLowRes", GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.Save();

        SoundManager.instance.isLowResource = GetComponent<Toggle>().isOn;
    }
}
