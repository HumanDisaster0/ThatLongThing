using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEventController : MonoBehaviour
{
    [SerializeField] MDamageEvent mEvent;

    void Attack()
    {
        //TODO :: �÷��̾� ������ ���� �Լ� ����ֱ�
        if (mEvent.isTouchingPlayer)
            PlayerSpawnManager.instance.Respawn();
            //gameManager.SendMessage("Respawn");
    }
}
