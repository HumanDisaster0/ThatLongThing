using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStep : MonoBehaviour
{
    [Header("반동크기")]
    [SerializeField] float boundForce = 10.0f;
    [SerializeField] MMove move;

    private void Start()
    {
        if(move == null)
            move = GetComponentInParent<MMove>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            collision.GetComponent<PlayerController>().AddForceToRB(new Vector2(0, collision.GetComponent<Rigidbody2D>().velocity.y * -1.0f));
            collision.GetComponent<PlayerController>().AddForceToRB(Vector2.up * boundForce);
            GetComponent<BoxCollider2D>().enabled = false;
            move.SetStatus(MStatus.die);
        }
    }
}
