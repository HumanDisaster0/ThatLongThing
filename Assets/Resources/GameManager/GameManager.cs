using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Member
    [Header("오브젝트")]
    public GameObject player;
    #endregion

    #region Private Member
    Vector3 playerInitPos;
    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        if(player == null)
            player = GameObject.Find("Player");

        playerInitPos = player.transform.position;
    }

    void Respawn(string _name)
    {
        if(_name == "Player")
        {
            player.GetComponent<PlayerController>().SetVelocity(Vector2.zero);
            player.transform.position = playerInitPos;
        }
    }
}
