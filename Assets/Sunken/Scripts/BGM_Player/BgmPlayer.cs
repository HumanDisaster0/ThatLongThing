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
            Destroy(this.gameObject);
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

        if (sceneName.StartsWith("Title"))
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Title_BGM");
        }
        else if (sceneName.StartsWith("GuildMain"))
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Guild_BGM");
        }
        else if (StageManager.instance?.anomalyIdx == 3)
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
        {
            defaultSrc = SoundManager.instance?.PlayLoopBackSound("Stage3_BGM");
        }

        //defaultSrc.GetComponent<DefaultSourceData>().isVolCon = false;
        if (defaultSrc)
        {
            defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);
        }
    }

    public void ChangeBgmDanger()
    {
        SoundManager.instance?.StopSound(defaultSrc);
        defaultSrc = SoundManager.instance?.PlayLoopBackSound("Danger_BGM");
        defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);
    }

    public AudioSource ChangeBgm(string _soundName)
    {
        SoundManager.instance?.StopSound(defaultSrc);
        defaultSrc = SoundManager.instance?.PlayLoopBackSound(_soundName);
        defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);

        return defaultSrc;
    }

    public void UpdateVolume()
    {
        // º¼·ý ÀúÀå
        PlayerPrefs.SetFloat("bgVol", SoundManager.instance.bgVol);
        PlayerPrefs.SetFloat("seVol", SoundManager.instance.seVol);
        PlayerPrefs.Save();

        if (defaultSrc)
            defaultSrc.GetComponent<AudioSource>().volume = SoundManager.instance.bgVol * ((float)volume / 100);
    }

    private void OnDestroy()
    {
        if (defaultSrc != null)
            SoundManager.instance?.StopSound(defaultSrc);
    }
}
