using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEventController : MonoBehaviour
{
    [SerializeField] MDamageEvent mEvent;
    PlayerController pc;

    private void Start()
    {
        pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void Attack()
    {
        //TODO :: 플레이어 죽으면 쓰는 함수 집어넣기
        if (mEvent.isTouchingPlayer)
            pc.AnyState(PlayerState.Die);
            //gameManager.SendMessage("Respawn");
    }
}
