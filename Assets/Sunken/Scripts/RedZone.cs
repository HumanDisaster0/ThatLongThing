using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedZone : MonoBehaviour
{
    [Header("게임매니저한테 보낼 메시지")]
    [SerializeField] List<string> messages;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            foreach (var message in messages)
                gameManager.SendMessage(message);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<MMove>().SetStatus(MStatus.die);
        }
    }
}
