using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZoneScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered spawn zone");
            Transform playerSprite = collision.gameObject.transform.Find("PlayerSprite");

            if(playerSprite.localScale != new Vector3(1f, 1f, 1f))
            {
                Destroy(this.gameObject);
            }
        }
    }
}
