using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RedZone : MonoBehaviour
{
    [Header("���ӸŴ������� ���� �޽���")]
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
