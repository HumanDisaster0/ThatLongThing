using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStep : MonoBehaviour
{
    [Header("�ݵ�ũ��")]
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
            PlayerController pc = collision.GetComponent<PlayerController>();

            //collision.GetComponent<PlayerController>().AddForceToRB(new Vector2(0, collision.GetComponent<Rigidbody2D>().velocity.y * -1.0f));
            pc.SetVelocity(new Vector2(collision.GetComponent<Rigidbody2D>().velocity.x, 0f));
            pc.AddForceToRB(Vector2.up * boundForce);
            move.SetStatus(MStatus.die);
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
