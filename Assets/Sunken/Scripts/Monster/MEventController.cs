using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEventController : MonoBehaviour
{
    [SerializeField] MDamageEvent mEvent;

    void Attack()
    {
        //TODO :: 플레이어 죽으면 쓰는 함수 집어넣기
        if (mEvent.isTouchingPlayer)
            PlayerSpawnManager.instance.Respawn();
            //gameManager.SendMessage("Respawn");
    }
}
