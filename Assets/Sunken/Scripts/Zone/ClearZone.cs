using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearZone : MonoBehaviour
{
    [SerializeField] float scaleSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;

    GameObject target = null;
    bool playAnim = false;


    private void Update()
    {
        if(playAnim && target != null)
        {
            PlayerController pc = target.GetComponent<PlayerController>();
            pc.SetVelocity(Vector2.zero);

            if (pc.playerScale > 0f)
                pc.playerScale = pc.playerScale - scaleSpeed * Time.deltaTime;
            pc.ApplyScale();

            Vector3 dir = transform.position - target.transform.position;
            target.transform.position += dir * moveSpeed * Time.deltaTime;

            if(Vector2.Distance(target.transform.position, transform.position) < 0.1f)
            {
                // 씬 체인지
                Debug.Log("씬 체인지!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Debug.Log("플레이어 감지!");
            target = collision.gameObject;
            playAnim = true;
        }
    }
}
