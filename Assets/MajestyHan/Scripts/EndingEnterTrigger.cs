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
        if (cam != null)
            cam.ShakeCamera(16f, 0.5f, 2f); //사운드(효과음도 넣어주면 좋을듯) 용수철 튕기는 요상한 마법소리 - 벽에 막히는 소리

        var volume = FindObjectOfType<Volume>();
        volume.profile.TryGet(out ChromaticAberration chromatic);
        chromatic.intensity.value = 1.0f; // 색수차

        var maskObj = GameObject.Find("Player").transform.Find("ShadowMask");
        var sr = maskObj.GetComponent<SpriteRenderer>();

        SetMaskColor(sr, Color.magenta); // 마스크 기본색 변경
        StartCoroutine(ExpandMask(sr, 0.3f, 1.5f)); // 마스크 크기 변경
        //maskObj.gameObject.SetActive(false); // 이건 확장 끝난 후에 하고 싶으면 코루틴 마지막에 옮겨도 됨

        yield return new WaitForSeconds(1.5f); //연출시간
    }
}