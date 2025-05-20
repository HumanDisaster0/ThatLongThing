using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrexSond : MonoBehaviour
{
    [SerializeField] TrexMove trex;
    TrexMove.MonsterState prevState = TrexMove.MonsterState.Chase;

    private void Start()
    {
        if(trex == null)
        {
            trex = GameObject.Find("SuperTrex")?.GetComponent<TrexMove>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trex != null)
        {
            if (trex.state != prevState)
            {
                prevState = trex.state;
                switch (trex.state)
                {
                    case TrexMove.MonsterState.Idle:
                        SoundManager.instance?.PlayLoopSound("Trex_Idle", trex.gameObject, 2f);
                        break;
                    case TrexMove.MonsterState.Chase:
                        SoundManager.instance?.PlayLoopSound("Trex_Footstep", trex.gameObject, 2f);
                        break;
                    case TrexMove.MonsterState.Patrol:
                        SoundManager.instance?.PlayLoopSound("Trex_Footstep", trex.gameObject, 2f);
                        break;
                    case TrexMove.MonsterState.Jumping:
                        SoundManager.instance?.PlayNewSound("Trex_Attack2", trex.gameObject, 2f);
                        break;
                }

                if (trex.state != TrexMove.MonsterState.Idle)
                    SoundManager.instance?.StopSound("Trex_Idle", trex.gameObject);
                if (trex.state != TrexMove.MonsterState.Chase && trex.state != TrexMove.MonsterState.Patrol)
                    SoundManager.instance?.StopSound("Trex_Footstep", trex.gameObject);
            }
        }
        else
            Destroy(this.gameObject);
    }
}
