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
        //TODO :: �÷��̾� ������ ���� �Լ� ����ֱ�
        if (mEvent.isTouchingPlayer)
            pc.AnyState(PlayerState.Die);
            //gameManager.SendMessage("Respawn");
    }
}
