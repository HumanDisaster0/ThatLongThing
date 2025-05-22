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
    bool m_isPlaying = false;
    float timer;
    [Range(0f, 1f)]
    float volume;

    private void Start()
    {
        comet = GameObject.Find("Comet");
        timer = 0f;
        volume = 0f;

        if (comet != null)
        {
            volumeCurve = new AnimationCurve();
            foreach (var key in comet.GetComponent<CometController>().TMaskAnimation.keys)
            {
                volumeCurve.AddKey(key);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (comet == null)
            Destroy(gameObject);

        if (audsrc == null)
        {
            print("오디오 소스 null");
            audsrc = SoundManager.instance.PlayLoopBackSound("Meteor_Run");
            audsrc.GetComponent<DefaultSourceData>().soundType = SoundType.Se;
            //m_isPlaying = true;
        }    
            

        timer += Time.deltaTime;

        volume = volumeCurve.Evaluate(timer) * SoundManager.instance.seVol;

        if(audsrc != null)
            audsrc.volume = volume;

        if(!m_isPlaying && comet?.GetComponent<CometController>().HitTime < timer)
        {
            m_isPlaying = true;
            SoundManager.instance.StopSound(audsrc);
            AudioSource exp = SoundManager.instance.PlayBackSound("Meteor_Explosion");
            exp.GetComponent<DefaultSourceData>().soundType = SoundType.Se;
        }
    }

    public void ResetCometSound()
    {
        m_isPlaying = false;
        timer = 0f;
        if(audsrc != null)
        {
            audsrc.volume = 0f;
            SoundManager.instance.StopSound(audsrc);
        }

        audsrc = null;
    }
}
