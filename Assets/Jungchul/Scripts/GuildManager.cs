using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildManager : MonoBehaviour
{
    public static GuildManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 멤버변수 예시

    public List<string> guildMission = new List<string>();
        

    public void InitMission()
    {
        //예시 코드
        //guildMission.Add(name);
    }
}
