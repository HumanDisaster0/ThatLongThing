using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicFX : MonoBehaviour
{
    [Header("FX ������Ʈ")]
    public Transform FX1;
    public Transform FX2;
    public SpriteRenderer SPR1;
    public SpriteRenderer SPR2;
    public Sprite LowLevel;
    public Sprite MidLevel;
    public Sprite HighLevel;

    [Header("�ι�° �ĵ��� ���۵Ǵ� �ð� ����")]
    public float fxTimerDiff = 0.5f;

    [Header("������� �����ϴ� Ű������")]
    public int fadeKeyframeIdx = 0;

    [Header("�ִϸ��̼�")]
    public AnimationCurve FXCurve;
    public AnimationCurve FadeCurve;

    float m_fx1Timer = 0;
    float m_fx2Timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        //�ĵ� ���ݸ�ŭ �ð� ��������
        m_fx2Timer = -fxTimerDiff;
    }

    // Update is called once per frame
    void Update()
    {
        var deltaTime = Time.deltaTime;
        m_fx1Timer += deltaTime;
        m_fx2Timer += deltaTime;
        CalcAnimation();
    }

    public void RestartAnimation()
    {
        m_fx1Timer = 0.0f;
        m_fx2Timer = -fxTimerDiff;
        CalcAnimation();
    }

    public void SetLevel(int level)
    {
        switch(level)
        {
            case 1:
                SPR1.sprite = LowLevel;
                SPR2.sprite = LowLevel;
                break;
            case 2:
                SPR1.sprite = MidLevel;
                SPR2.sprite = MidLevel;
                break;
            case 3:
                SPR1.sprite = HighLevel;
                SPR2.sprite = HighLevel;
                break;
        }
    }

    void CalcAnimation()
    {
        var maximumTime = FXCurve[FXCurve.length - 1].time;

        if (m_fx1Timer > maximumTime)
            m_fx1Timer -= maximumTime;

        if (m_fx2Timer > maximumTime)
            m_fx2Timer -= maximumTime;

        FX1.transform.localScale = Vector3.one * FXCurve.Evaluate(m_fx1Timer);
        FX2.transform.localScale = Vector3.one * FXCurve.Evaluate(m_fx2Timer);

        var fadeTime = maximumTime - FXCurve[fadeKeyframeIdx].time;
        var currentFade1Timer = maximumTime - m_fx1Timer;
        var currentFade2Timer = maximumTime - m_fx2Timer;

        SPR1.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), FadeCurve.Evaluate(1 - Mathf.Clamp01(currentFade1Timer / fadeTime)));
        SPR2.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), FadeCurve.Evaluate(1 - Mathf.Clamp01(currentFade2Timer / fadeTime)));
    }
}
