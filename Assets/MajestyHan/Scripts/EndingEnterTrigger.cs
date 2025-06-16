using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class EndingEnterTrigger : MonoBehaviour
{
    public CameraController cam;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("NPC"))
        {
            if (cam != null)
                cam.ShakeCamera(16f, 0.5f, 2f); //사운드(효과음도 넣어주면 좋을듯) 용수철 튕기는 요상한 마법소리 - 벽에 막히는 소리

            var volume = FindObjectOfType<Volume>();
            volume.profile.TryGet(out ChromaticAberration chromatic);
            chromatic.intensity.value = 1.0f;

            GameObject.Find("Player").transform.Find("ShadowMask").gameObject.SetActive(false);

            var pl = collision.GetComponent<PlayerController>();
            pl.SkipInput = true;
        }
    }
}