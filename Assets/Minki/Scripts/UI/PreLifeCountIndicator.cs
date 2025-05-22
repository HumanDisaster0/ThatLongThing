using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PreLifeCountIndicator : MonoBehaviour
{
    [Header("Image")]
    public Image BG;
    public Image DeathIcon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI xText;
    public TextMeshProUGUI prev;
    public TextMeshProUGUI next;

    public RectTransform nextRect;
    public RectTransform prevRect;

    public float showTime = 1.5f;

    public float fadeTime = 0.8f;

    public AnimationCurve nextNumAnimation;


    GraphicRaycaster m_graphicRaycaster;
    CameraController m_camCon;
    MapKeyboardControl m_minimapControl;
    PlayerController m_playerController;

    // Start is called before the first frame update
    void Start()
    {
        m_camCon = Camera.main.GetComponent<CameraController>();
        m_graphicRaycaster = GetComponent<GraphicRaycaster>();
        m_minimapControl = GameObject.FindGameObjectWithTag("Minimap").GetComponent<MapKeyboardControl>();
        m_playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        title.text = StageManager.instance.GetAnomalyName(StageManager.instance.anomalyIdx);
        StartCoroutine(StartAnim());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayStartAnimation()
    {
        
        StartCoroutine(StartAnim());
    }

    public void PlayRespawnAnimation(int nextCount)
    {
        StartCoroutine(RespawnAnim(nextCount));
    }

    //시작시 애니메이션
    IEnumerator StartAnim()
    {
        SoundManager.instance.ForceSetMute(true);
        m_graphicRaycaster.enabled = true;
        Time.timeScale = 0.0f;
        SetAlphaColorForOwnRect(1.0f);
        yield return null;
        float timer = 0.0f;
        while (timer < showTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        m_graphicRaycaster.enabled = false;
        timer = 0.0f;
        //소리재생
        SoundManager.instance?.SetMute(false);
        Time.timeScale = 1.0f;
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            SetAlphaColorForOwnRect(1.0f - (timer / fadeTime));
            yield return null;
        }
    }

    IEnumerator RespawnAnim(int nextCount)
    {
        m_graphicRaycaster.enabled = true;
        m_minimapControl.HideMinimap();

        next.text = nextCount.ToString();
        prev.text = (nextCount - 1).ToString();

        //fade out
        float timer = 0.0f;
        while(timer < fadeTime)
        {
            timer += Time.deltaTime;
            SetAlphaColorForOwnRect(timer / fadeTime);
            yield return null;
        }

        m_playerController.AnyState(PlayerState.Fall);
        m_playerController.SetVelocity(Vector2.zero);
        Time.timeScale = 0.0f;
        SetAlphaColorForOwnRect(1.0f);
        yield return null;

        timer = 0.0f;

        var nextY = nextRect.anchoredPosition.y;
        var prevY = prevRect.anchoredPosition.y;

        //카운트 애니메션
        while (timer < nextNumAnimation[nextNumAnimation.length - 1].time)
        {
            timer += Time.unscaledDeltaTime;
            var y = nextNumAnimation.Evaluate(timer);

            nextRect.anchoredPosition = new Vector2(nextRect.anchoredPosition.x, nextY - y);
            prevRect.anchoredPosition = new Vector2(prevRect.anchoredPosition.x, prevY - y);

            yield return null;
        }

        //대기
        timer = 0.0f;
        while (timer < showTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        PlayerSpawnManager.instance.Respawn();
        m_playerController.Freeze = false;
        m_camCon.TrackPositionImediate();
        m_graphicRaycaster.enabled = false;
        StageManager.instance.deathCount++;
        timer = 0.0f;
        SoundManager.instance?.SetMute(false);

        Time.timeScale = 1.0f;

        //fade in
        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            SetAlphaColorForOwnRect(1.0f - (timer / fadeTime));
            yield return null;
        }

        nextRect.anchoredPosition = new Vector2(nextRect.anchoredPosition.x, nextY);
        prevRect.anchoredPosition = new Vector2(prevRect.anchoredPosition.x, prevY);
    }

    void SetAlphaColorForOwnRect(float alpha)
    {
        BG.color = new Color(BG.color.r,BG.color.g,BG.color.b,alpha);
        DeathIcon.color = new Color(DeathIcon.color.r,DeathIcon.color.g,DeathIcon.color.b,alpha);
        title.color = new Color(title.color.r,title.color.g,title.color.b,alpha);
        xText.color = new Color(xText.color.r, xText.color.g, xText.color.b, alpha);
        next.color = new Color(next.color.r,next.color.g,next.color.b,alpha);
        prev.color = new Color(prev.color.r,prev.color.g,prev.color.b,alpha);
    }
}
