using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TrexSond : MonoBehaviour
{
    [SerializeField] TrexMove trex;
    TrexMove.MonsterState prevState = TrexMove.MonsterState.Pause;

    private void Start()
    {
        if(!trex)
        {
            trex = GameObject.Find("SuperTrex").GetComponent<TrexMove>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trex)
        {
            if (trex.state != prevState)
            {
                //prevState = trex.state;
                //switch (trex.state)
                //{
                //    case TrexMove.MonsterState.Idle:
                //        SoundManager.instance.PlayLoopSound("", trex.gameObject);

                //}
            }
        }
        else
            Destroy(this.gameObject);
    }
}
