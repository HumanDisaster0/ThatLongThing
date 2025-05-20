using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingStoneSound : MonoBehaviour
{
    AudioSource m_AudioSource;
    public void PlayRollSound()
    {
        m_AudioSource = SoundManager.instance.PlayLoopBackSound("Meteor_Run");
        m_AudioSource.volume = 0.5f;
    }

    public void StopRollSound()
    {
        SoundManager.instance.StopSound(m_AudioSource);
    }
}
