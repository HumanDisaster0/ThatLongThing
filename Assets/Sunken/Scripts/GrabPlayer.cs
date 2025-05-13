using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPlayer : MonoBehaviour
{   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("�÷��̾� �浹");

        if (collision.gameObject.CompareTag("Player"))
            collision.gameObject.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("�÷��̾� ����");

        if (collision.gameObject.CompareTag("Player"))
            collision.gameObject.transform.SetParent(null);
    }
}
