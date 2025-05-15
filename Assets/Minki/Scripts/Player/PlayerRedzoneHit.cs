using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRedzoneHit : MonoBehaviour
{
    public string RedZoneName = "RedZone";

    PlayerController m_playerController;

    private void Start()
    {
        m_playerController = GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(RedZoneName))
        {
            PlayerSpawnManager.instance.Respawn();
            m_playerController.SetVelocity(Vector2.zero);
        }
    }
}
