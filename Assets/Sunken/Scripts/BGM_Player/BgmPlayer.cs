using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmPlayer : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField]int volume = 50;
    AudioSource defaultSrc;

    // Start is called before the first frame update
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Stage1"))
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Stage1_BGM");
        }
        else if (sceneName.StartsWith("Stage2"))
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Stage2_BGM");
        }
        else if (sceneName.StartsWith("Stage3"))
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Stage3_BGM");

        //defaultSrc.GetComponent<DefaultSourceData>().isVolCon = false;
        defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);
    }

    private void OnDestroy()
    {
        if (defaultSrc != null)
            SoundManager.instance.StopSound(defaultSrc);
    }
}
