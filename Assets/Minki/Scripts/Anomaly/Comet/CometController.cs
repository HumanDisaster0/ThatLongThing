using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using static Unity.Collections.AllocatorManager;
using UnityEditor.Rendering;
using UnityEngine.UI;

public class CometController : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;

    public Vector3 startScale;
    public Vector3 endScale;

    public float shakeSpeed;
    public float shakeAmount;

    public float startBloom;
    public float endBloom;

    public float startVignette;
    public float endVignette;

    public float startChromaticity;
    public float endChromaticity;

    public float rotateSpeed = 20.0f;

    public float HitTime = 50.0f;

    public RawImage FlashFX;

    public AnimationCurve TCometAnimation; //백분율(0~100%) 애니메이션
    public AnimationCurve TScreenAnimation; //백분율(0~100%) 애니메이션
    public AnimationCurve TMaskAnimation; //백분율(0~100%) 애니메이션

    public Transform stone;

    private Volume m_volume;
    private Bloom m_bloom;
    private Vignette m_vignette;
    private ChromaticAberration m_chromaticAberration;
    private float m_timer;

    private Camera m_cam;
    private CameraController m_camCon;

    private bool m_eventTrigger;
    private PlayerPortalInteract m_portalInteract;
    private PlayerController m_pc;
    private Material m_maskMat;

    // Start is called before the first frame update
    void Start()
    {
        m_timer = 0.0f;
        m_cam = Camera.main;
        m_camCon = m_cam.GetComponent<CameraController>();  
        m_volume = FindObjectOfType<Volume>();
        if (m_volume == null)
        {
            Debug.LogError("Volume 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // VolumeProfile에서 Bloom 가져오기
        if (m_volume.profile.TryGet(out m_bloom)
            && m_volume.profile.TryGet(out m_vignette)
            && m_volume.profile.TryGet(out m_chromaticAberration))
        {
        }
        else
        {
            Debug.LogWarning("Bloom 컴포넌트가 Volume Profile에 존재하지 않습니다.");
        }

        m_portalInteract = FindObjectOfType<PlayerPortalInteract>();
        m_pc = FindObjectOfType<PlayerController>();

        m_maskMat = m_pc.transform.Find("ShadowMask").GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        m_timer += Time.deltaTime;

        if(!m_eventTrigger 
            && m_timer > HitTime
            && !m_portalInteract.IsEntering
            && m_pc.GetCurrentState() != PlayerState.Die)
        {
            m_eventTrigger = true;
            m_portalInteract.enabled = false;
            StartCoroutine(Flash());
        }

        //if(!m_eventTrigger)
        //{
        //    FlashFX.color = Color.Lerp(FlashFX.color, new Color(1, 1, 1, 0.0f), 12f * Time.deltaTime);
        //}

        stone.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        float tc = TCometAnimation.Evaluate(m_timer);
        float ts = TScreenAnimation.Evaluate(m_timer);
        float tm = TMaskAnimation.Evaluate(m_timer);

        transform.position = Vector3.Lerp(startPos + m_cam.transform.position,endPos + m_cam.transform.position,tc);
        transform.localScale = Vector3.Lerp(startScale,endScale, tc);

        m_camCon.ShakeCamera(shakeSpeed, shakeAmount * ts, 0.0f);
        m_bloom.intensity.value = Mathf.Lerp(startBloom,endBloom, ts);
        m_vignette.intensity.value = Mathf.Lerp(startVignette,endVignette, ts);
        m_chromaticAberration.intensity.value = Mathf.Lerp(startChromaticity,endChromaticity, ts);
        m_maskMat.SetFloat("_AlphaFactor", 1 - tm);
    }

    IEnumerator Flash()
    {
        yield return null;

        var timer = 0.0f;

        while (timer < 1.2f)
        {
            timer += Time.deltaTime;
            FlashFX.color = new Color(1, 1, 1, timer / 1.2f);
            yield return null;
        }

        m_pc.AnyState(PlayerState.Die, true);
    }

    public void ResetComet()
    {
        FlashFX.color = new Color(1, 1, 1, 0.0f);
        m_portalInteract.enabled = true;
        m_eventTrigger = false;
        m_timer = 0.0f;
    }
}
