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

            var map = FindFirstObjectByType<MapOnOffControl>();
            map.activeControl = false; // 맵 끄기

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

    IEnumerator DirectingEnterToEnding()
    {
        //====================[ 초기 설정 ]====================
        var maskObj = GameObject.Find("Player").transform.Find("ShadowMask");
        var sr = maskObj.GetComponent<SpriteRenderer>();
        var volume = FindObjectOfType<Volume>();

        volume.profile.TryGet(out ChromaticAberration chromatic);
        volume.profile.TryGet(out Bloom bloom);
        volume.profile.TryGet(out FilmGrain grain);

        // PostProcessing 기본 세팅
        chromatic.intensity.value = 0.0f;

        bloom.threshold.value = 0.5f;
        bloom.scatter.value = 0.5f;
        bloom.intensity.value = 0.0f;

        grain.intensity.value = 0.0f;
        grain.response.value = 0.7f;
        grain.type.value = FilmGrainLookup.Thin1;

        //====================[ Step 0: 전조 (1.0초) ]====================
        grain.intensity.value = 0.7f;
        chromatic.intensity.value = 0.6f;
        bloom.intensity.value = 0.5f;

        cam.ShakeCamera(60f, 1.0f, 4.605f); // 순간적인 흔들림
        yield return new WaitForSeconds(1.0f);

        //====================[ Step 1: 긴장감 고조 (0.5초) ]====================

        cam.ShakeCamera(30f, 2.0f, 4.605f);
        chromatic.intensity.value = 0.0f;
        bloom.intensity.value = 0.0f;
        grain.intensity.value = 0.0f;
        yield return StartCoroutine(ExpandMask(sr, 0.0076f, 0.5f));

        //====================[ Step 2: 응축 (1.0초) ]====================

        cam.ShakeCamera(8f, 0.15f, 2.3f);
        yield return StartCoroutine(ExpandMask(sr, 0.0f, 1.0f));
        Transform holySphere = GameObject.Find("Player").transform.Find("HolySphere");
        holySphere.gameObject.SetActive(true);

        //====================[ Step 3: 폭발 (1.5초) ]====================
        chromatic.intensity.value = 1.0f;
        bloom.intensity.value = 3.5f;
        cam.ShakeCamera(250f, 2.5f, 0.5f);
        yield return StartCoroutine(ExpandMask(sr, 1.0f, 2.0f));
        //yield return new WaitForSeconds(1.5f);
    }
}
