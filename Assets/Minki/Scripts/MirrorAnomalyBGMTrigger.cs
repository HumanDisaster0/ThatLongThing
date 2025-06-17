using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEditor.Rendering;
using Unity.VisualScripting;

public class MirrorAnomalyBGMTrigger : MonoBehaviour
{
    public float PostProcessSpeed = 6.0f;

    private bool m_isTriggered;
    private Rigidbody2D m_rb;
    private Volume m_volume;
    private ChromaticAberration m_chromaticAberration;
    private LiftGammaGain m_liftGammaGain;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_volume = FindFirstObjectByType<Volume>();

        if(m_volume == null)
        {
            Debug.LogWarning("거울 이상현상 트리거 : 글로벌 볼륨을 찾을 수 없습니다.");
            return;
        }

        if (!m_volume.profile.TryGet(out m_chromaticAberration)
            || !m_volume.profile.TryGet(out m_liftGammaGain))
        {
            Debug.LogWarning("거울 이상현상 트리거 : 후처리 효과를 가져오지 못했습니다.");
        }
    }

    public void Update()
    {
        if(m_isTriggered)
        {
            m_chromaticAberration.intensity.value = Mathf.Lerp(m_chromaticAberration.intensity.value, 0.5f,Time.deltaTime * PostProcessSpeed);
            m_liftGammaGain.lift.value = Vector4.Lerp(m_liftGammaGain.lift.value, new Vector4(0.7597156167030335f, 0.7972372174263f, 1.0f, -0.07411328703165055f), Time.deltaTime * PostProcessSpeed);
            m_liftGammaGain.gamma.value = Vector4.Lerp(m_liftGammaGain.gamma.value, new Vector4(1.0f,1.0f, 1.0f, 0.42350447177886965f), Time.deltaTime * PostProcessSpeed);
            m_liftGammaGain.gain.value = Vector4.Lerp(m_liftGammaGain.gain.value, new Vector4(1.0f,1.0f, 1.0f, 0.052938055247068408f), Time.deltaTime * PostProcessSpeed);

        }
    }

    public void StartTrigger()
    {
        if (!m_isTriggered)
        {
            //중복 방지
            m_isTriggered = true;

            //BGM변경 시작!!
            StartCoroutine(ChangeBGM());
        }
    }

    public void ResetTrigger()
    {
        m_isTriggered = false;
        StopAllCoroutines();
        m_chromaticAberration.intensity.value = 0.0f;
        m_liftGammaGain.lift.value = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
        m_liftGammaGain.gamma.value = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
        m_liftGammaGain.gain.value = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
    }


    //BGM변경용 코루틴
    IEnumerator ChangeBGM()
    {
        BgmPlayer.instance.ChangeBgm("");

        var timer = 0.0f;
        while (timer < 1.2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        BgmPlayer.instance.ChangeBgm("reflection_world");

    }
}
