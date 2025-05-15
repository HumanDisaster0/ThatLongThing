using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRedzoneHit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Redzone"))
        {
            PlayerSpawnManager.instance.Respawn();
        }
    }
}
