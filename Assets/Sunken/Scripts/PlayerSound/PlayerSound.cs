  using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
   GameObject player;

   PlayerState currPs = PlayerState.Idle;
   PlayerState prevPs = PlayerState.Idle;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError(this.gameObject + "Player object not found!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //currPs = player.GetComponent<PlayerController>().GetCurrentState();

        //if(prevPs != currPs)
        //{
        //    switch(currPs)
        //    {
        //        case PlayerState.Idle:
        //            // Play idle sound
        //            break;
        //        case PlayerState.Walk:
        //            // Play walking sound
        //            SoundManager.instance.PlayLoopSound("Stone_Step", player);
        //            break;
        //        case PlayerState.Jump:
        //            // Play jumping sound
        //            SoundManager.instance.PlayNewSound("Jump", player);
        //            break;
        //        case PlayerState.Fall:
        //            // Play attacking sound
        //            break;
        //        case PlayerState.Die:
        //            // Play dashing sound
        //            SoundManager.instance.PlaySound("Die1", player);
        //            break;
        //        default:
        //            break;
        //    }
        //    prevPs = currPs;
        //}

        //if (currPs != PlayerState.Walk)
        //    SoundManager.instance.StopSound("Stone_Step", player);

        currPs = player.GetComponent<PlayerController>().GetCurrentState();

        if(player.transform.localScale.y > 2.0f)
        {
            switch(currPs)
            {
                case PlayerState.Walk:
                    StartCoroutine(LargeStep());
                    break;
            }

            if(currPs != PlayerState.Walk)
                StopCoroutine(LargeStep());
        }
    }

    IEnumerator LargeStep()
    {
        while(true)
        {
            SoundManager.instance.PlayNewSound("Trex_Land", player);
            yield return new WaitForSeconds(0.5f);
        }
    }

}
