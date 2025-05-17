using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BgmPlayer : MonoBehaviour
{
    [SerializeField]int volume = 50;
    AudioSource audioSrc;

    // Start is called before the first frame update
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Stage1"))
        {
            audioSrc = SoundManager.instance?.PlayLoopSound("Stage1_BGM", Camera.main.gameObject);
        }
        else if (sceneName.StartsWith("Stage2"))
        {
            audioSrc = SoundManager.instance?.PlayLoopSound("Stage2_BGM", Camera.main.gameObject);
        }

        audioSrc.GetComponent<DefaultSourceData>().isVolConByManager = false;
        audioSrc.volume = (float)volume / 100;
    }
}
