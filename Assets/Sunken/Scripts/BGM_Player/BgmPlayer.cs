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
            defaultSrc = SoundManager.instance?.PlayLoopSound("Stage1_BGM", Camera.main.gameObject);
        }
        else if (sceneName.StartsWith("Stage2"))
        {
            defaultSrc = SoundManager.instance?.PlayLoopSound("Stage2_BGM", Camera.main.gameObject);
        }
        else if (sceneName.StartsWith("Stage3"))
            defaultSrc = SoundManager.instance?.PlayLoopSound("Stage3_BGM", Camera.main.gameObject);

        defaultSrc.GetComponent<DefaultSourceData>().isVolCon = false;
        defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);
    }
}
