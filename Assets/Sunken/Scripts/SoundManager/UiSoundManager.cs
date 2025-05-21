using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UISFX
{
    Hover = 0,
    Exit,
    Click
}

[Serializable]
public class UiSoundData
{
    public string soundTag;
    public UISFX uiSfx;
    public string soundName;
}
public class UiSoundManager : MonoBehaviour
{
    public static UiSoundManager instance;

    [SerializeField] List<UiSoundData> soundDatas = new List<UiSoundData>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySound(string _tag, UISFX _status)
    {
        foreach (UiSoundData data in soundDatas) {
            if (data.soundTag == _tag && data.uiSfx == _status)
            {
                AudioSource audSrc = SoundManager.instance?.PlayNewBackSound(data.soundName);
                
                if(audSrc != null)
                    audSrc.volume = SoundManager.instance.seVol;
            }
            else
                continue;
        }
    }
}
