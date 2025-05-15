using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager instance;
    public string spawnPointName = "PlayerSpawnPoint";

    Transform m_spawnPoint = null;
    Transform m_player;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        m_spawnPoint = GameObject.Find(spawnPointName).transform;
        m_player = GameObject.Find("Player").transform;
    }

    public void Respawn()
    {
        if(m_spawnPoint)
            m_player.position = m_spawnPoint.position;
        else
        {
            m_spawnPoint = GameObject.Find(spawnPointName).transform;
            m_player.position = m_spawnPoint.position;
        }
        TrapTriggerCMD.ResetAllTrigger();
    }
}
