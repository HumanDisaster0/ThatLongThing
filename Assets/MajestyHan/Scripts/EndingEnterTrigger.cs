using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class EndingEnterTrigger : MonoBehaviour
{
    public CameraController cam;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("NPC"))
        {
            var pl = collision.GetComponent<PlayerController>();
            pl.SkipInput = true; // 움직임 잠금 + 맵 잠금 필요함!@#$!@#$

            StartCoroutine(EnterToEndingSequence());
        }
    }

    //==============================================================================
    IEnumerator ExpandMask(SpriteRenderer sr, float targetRadius, float duration)
    {
        MaterialPropertyBlock mpb = new();
        sr.GetPropertyBlock(mpb);
        float start = mpb.GetFloat("_MaskRadius");
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            mpb.SetFloat("_MaskRadius", Mathf.Lerp(start, targetRadius, t / duration));
            sr.SetPropertyBlock(mpb);
            yield return null;
        }
    }

    //==============================================================================
    IEnumerator EnterToEndingSequence()
    {
        yield return StartCoroutine(DirectingEnterToEnding());
        SceneManager.LoadScene("EndingScene");
    }

    void SetMaskColor(SpriteRenderer sr, Color color)
    {
        var mpb = new MaterialPropertyBlock();
        sr.GetPropertyBlock(mpb);
        mpb.SetColor("_MaskColor", color);
        sr.SetPropertyBlock(mpb);
    }

    //==============================================================================

    IEnumerator DirectingEnterToEnding() //연출
    {
        var maskObj = GameObject.Find("Player").transform.Find("ShadowMask");
        var sr = maskObj.GetComponent<SpriteRenderer>();
        var volume = FindObjectOfType<Volume>();
        volume.profile.TryGet(out ChromaticAberration chromatic);

        ///////////////////////////////////////////////////////////           
        chromatic.intensity.value = 0.5f; // 색수차
        StartCoroutine(ExpandMask(sr, 0.012f, 0.05f)); // 마스크 크기 변경
        yield return new WaitForSeconds(0.05f); //연출시간 보장

        ///////////////////////////////////////////////////////////   
        //SetMaskColor(sr, Color.magenta); // 마스크 기본색 변경                
        chromatic.intensity.value = 0.0f; // 색수차
        cam.ShakeCamera(16f, 0.5f, 2f);
        StartCoroutine(ExpandMask(sr, 0.0055f, 0.6f)); // 마스크 크기 변경
        yield return new WaitForSeconds(0.9f); //연출시간 보장

        ///////////////////////////////////////////////////////////   
        chromatic.intensity.value = 1.0f; // 색수차
        cam.ShakeCamera(52f, 1.0f, 2f);
        StartCoroutine(ExpandMask(sr, 0.2f, 1.0f)); // 마스크 크기 변경        
        yield return new WaitForSeconds(1.0f); //연출시간 보장

        ///////////////////////////////////////////////////////////   
        cam.ShakeCamera(100f, 2.0f, 1f);        
        yield return new WaitForSeconds(1.0f); //마지막

        ///////////////////////////////////////////////////////////   
        //StartCoroutine(ExpandMask(sr, 0.0f, 0.05f)); // 마스크 크기 변경              
        yield return new WaitForSeconds(0.2f); //마지막
    }
}