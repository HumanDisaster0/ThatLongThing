using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmPlayer : MonoBehaviour
{
    public static BgmPlayer instance;

    [Range(0, 100)]
    [SerializeField]int volume = 50;
    AudioSource defaultSrc;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartBgmPlayer();
    }

    public void StartBgmPlayer()
    {
        if (defaultSrc != null)
            SoundManager.instance?.StopSound(defaultSrc);

        string sceneName = SceneManager.GetActiveScene().name;

        if (StageManager.instance?.anomalyIdx == 3)
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Rabbit_BGM");
        }
        else if (StageManager.instance?.anomalyIdx == 5)
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Danger_BGM");
        }
        else if (sceneName.StartsWith("Stage1"))
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

    public void ChangeBgmDanger()
    {
        SoundManager.instance.StopSound(defaultSrc);
        defaultSrc = SoundManager.instance?.PlayLoopBackSound("Danger_BGM");
        defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);
    }

    private void OnDestroy()
    {
        if (defaultSrc != null)
            SoundManager.instance?.StopSound(defaultSrc);
    }
}
