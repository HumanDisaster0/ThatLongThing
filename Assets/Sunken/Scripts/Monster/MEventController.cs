using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEventController : MonoBehaviour
{
    [SerializeField] MDamageEvent mEvent;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Attack()
    {
        if (mEvent.isTouchingPlayer)
            gameManager.SendMessage("Respawn");
    }
}
