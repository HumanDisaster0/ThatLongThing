using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MAttack : MonoBehaviour
{
    [SerializeField] MMove mmove;
    [SerializeField] BoxCollider2D dmgCol;

    private void Start()
    {
        if(mmove != null)
            mmove = GetComponentInParent<MMove>();
        
        if(dmgCol != null)
            dmgCol.enabled = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            dmgCol.enabled=true;
            mmove.SetStatus(MStatus.attack);
        }
            
    }
}
