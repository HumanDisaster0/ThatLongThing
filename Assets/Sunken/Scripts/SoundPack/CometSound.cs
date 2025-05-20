using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CometSound : MonoBehaviour
{
    [SerializeField] GameObject comet;
    [SerializeField] AnimationCurve volumeCurve;

    AudioSource audsrc = null;
    float timer;
    [Range(0f, 1f)]
    float volume;

    private void Start()
    {
        comet = GameObject.Find("Comet");
        timer = 0f;
        volume = 0f;

        if (comet)
        {
            volumeCurve = new AnimationCurve();
            foreach(var key in comet.GetComponent<CometController>().TMaskAnimation.keys)
            {
                volumeCurve.AddKey(key);
                //audsrc.volume = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        if (!comet)
            Destroy(this);

        if (!audsrc)
            audsrc = SoundManager.instance.PlayLoopBackSound("Meteor_Run");

        timer += Time.deltaTime;

        volume = volumeCurve.Evaluate(timer);

        if(audsrc)
            audsrc.volume = volume;

        if(comet.GetComponent<CometController>().HitTime < timer)
        {
            SoundManager.instance.StopSound(audsrc);
            SoundManager.instance.PlayBackSound("Meteor_Explosion");
        }
    }

    public void ResetCometSound()
    {
        timer = 0f;
        if(audsrc)
            audsrc.volume = 0f;

        audsrc = null;
    }
}
