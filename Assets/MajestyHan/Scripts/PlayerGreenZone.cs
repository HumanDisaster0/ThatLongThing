using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerGreenZone : MonoBehaviour
{
    public TrapTrigger TT; // T_T

    public string spawnPointName = "PlayerSpawnPoint";
    private MovingWall movingWall;

    private void Start()
    {
        movingWall = FindObjectOfType<MovingWall>();
        if (movingWall == null)
        {
            Debug.Log("PlayerGreenZone : 투명벽을 찾을 수 없습니다.");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("NPC"))
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

            if (movingWall != null)
                movingWall.WallActiveFalse();

            if (TT != null)
                TT.ResetTrigger();

        }
    }
}