using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;





public class MovingGreenZone : MonoBehaviour
{
    public string spawnPointName = "PlayerSpawnPoint";

    public CameraController cam;

    private int count;
    private void Start()
    {
        count = 0;
        transform.position = new Vector3(27f, transform.position.y, transform.position.z); //x 27로 고정        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("NPC"))
        {
            if (cam != null)
                cam.ShakeCamera(16f, 0.5f, 2f); //사운드(효과음도 넣어주면 좋을듯) 용수철 튕기는 요상한 마법소리 - 벽에 막히는 소리

            var spawn = collision.GetComponent<PlayerSpawnManager>();
            if (spawn != null)
            {
                collision.transform.position = new Vector2(spawn.spawnPoint.position.x, -1.625f);
            }
            else
            {
                var point = GameObject.Find(spawnPointName);
                collision.transform.position = new Vector2(point.transform.position.x, -1.625f);

            }

            switch (count)
            {
                case 0:
                    transform.position += Vector3.left * 15f; //27 > 12 
                    count++;
                    break;

                case 1:
                    transform.position += Vector3.left * 8f; // 12 > 4
                    count++;
                    break;
                default: break;

            }

        }
    }
}