using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmPlayer : MonoBehaviour
{
    [SerializeField] int volume = 50;
    AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Stage1"))
        {
            audio = SoundManager.instance?.PlayLoopSound("Stage1_BGM", Camera.main.gameObject);
        }
        else if (sceneName.StartsWith("Stage2"))
        {
            audio = SoundManager.instance?.PlayLoopSound("Stage2_BGM", Camera.main.gameObject);
        }

        audio.GetComponent<DefaultSourceData>().isVolConByManager = false;
        audio.volume = (float)volume / 100;
    }
}
