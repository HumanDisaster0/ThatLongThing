using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCheckRedZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.tag == "RedZone")
        {
            GetComponent<MMove>().SetStatus(MStatus.die);
        }
    }
}
