using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RedZone : MonoBehaviour
{
    [Header("게임매니저한테 보낼 메시지")]
    [SerializeField] List<string> messages;
    public UnityEvent<Collider2D> OnTriggerEnterEvents;

    GameManager gameManager;

    private void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnTriggerEnterEvents?.Invoke(collision);
    }
}
