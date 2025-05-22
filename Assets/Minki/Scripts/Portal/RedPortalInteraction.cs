using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedPortalInteraction : MonoBehaviour
{
    public Transform popup;

    BoxCollider2D m_col;

    private void Start()
    {
        m_col = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            popup.gameObject.SetActive(true);
        }    
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            popup.gameObject.SetActive(false);
        }
    }

    public void EnterPortal()
    {
        GoldManager.Instance.ejectionCount++;
        StageManager.instance.EndStage();
    }
}
