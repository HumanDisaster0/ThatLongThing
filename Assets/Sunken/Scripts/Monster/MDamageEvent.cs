using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MDamageEvent : MonoBehaviour
{
    public bool isTouchingPlayer = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name == "Player")
            isTouchingPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Player")
            isTouchingPlayer = false;
    }
}
