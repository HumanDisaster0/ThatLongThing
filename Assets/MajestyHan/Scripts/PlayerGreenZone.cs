using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGreenZone : MonoBehaviour
{
    public string spawnPointName = "PlayerSpawnPoint";

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var spawn = collision.GetComponent<PlayerSpawnManager>();
            if (spawn != null)
            {
                collision.transform.position = spawn.spawnPoint.position;
            }
            else
            {
                var point = GameObject.Find(spawnPointName);
                collision.transform.position = point.transform.position;

            }
        }
    }
}