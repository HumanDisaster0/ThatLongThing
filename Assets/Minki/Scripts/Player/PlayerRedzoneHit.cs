using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRedzoneHit : MonoBehaviour
{
    public string RedZoneName = "RedZone";

    PlayerController m_playerController;

    private void Awake()
    {
        m_playerController = GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(RedZoneName))
        {
            m_playerController.AnyState(PlayerState.Die);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(RedZoneName))
        {
            m_playerController.AnyState(PlayerState.Die);
        }
    }
}
