using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using static Unity.Collections.AllocatorManager;
using UnityEditor.Rendering;

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

    public AnimationCurve TCometAnimation; //�����(0~100%) �ִϸ��̼�
    public AnimationCurve TScreenAnimation; //�����(0~100%) �ִϸ��̼�

    public UnityEvent OnCometHit;

    public Transform stone;

    private Volume m_volume;
    private Bloom m_bloom;
    private Vignette m_vignette;
    private ChromaticAberration m_chromaticAberration;
    private float m_timer;

    private Camera m_cam;
    private CameraController m_camCon;

    // Start is called before the first frame update
    void Start()
    {
        m_timer = 0.0f;
        m_cam = Camera.main;
        m_camCon = m_cam.GetComponent<CameraController>();  
        m_volume = FindObjectOfType<Volume>();
        if (m_volume == null)
        {
            Debug.LogError("Volume ������Ʈ�� ã�� �� �����ϴ�!");
            return;
        }

        // VolumeProfile���� Bloom ��������
        if (m_volume.profile.TryGet(out m_bloom)
            && m_volume.profile.TryGet(out m_vignette)
            && m_volume.profile.TryGet(out m_chromaticAberration))
        {
        }
        else
        {
            Debug.LogWarning("Bloom ������Ʈ�� Volume Profile�� �������� �ʽ��ϴ�.");
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        m_timer += Time.deltaTime;
        stone.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        float tc = TCometAnimation.Evaluate(m_timer);
        float ts = TScreenAnimation.Evaluate(m_timer);

        transform.position = Vector3.Lerp(startPos + m_cam.transform.position,endPos + m_cam.transform.position,tc);
        transform.localScale = Vector3.Lerp(startScale,endScale, tc);

        m_camCon.ShakeCamera(shakeSpeed, shakeAmount * ts, 0.0f);
        m_bloom.intensity.value = Mathf.Lerp(startBloom,endBloom, ts);
        m_vignette.intensity.value = Mathf.Lerp(startVignette,endVignette, ts);
        m_chromaticAberration.intensity.value = Mathf.Lerp(startChromaticity,endChromaticity, ts);
    }
}
