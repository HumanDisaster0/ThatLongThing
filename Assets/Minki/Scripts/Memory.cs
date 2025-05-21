using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Memory : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static Memory selected = null;

    public Sprite cursorExitSpr;
    public Sprite cursorOnSpr;

    Image m_image;
    RectTransform m_rectTransform;
    //GridLayoutGroup m_layoutGroup;

    bool m_isSelected;

    int m_savedIdx;
    Vector2 m_savedSize;
    Vector2 m_savedPos;

    public float torqueStrength = 50f;    // 스프링 강도
    public float damping = 10f;           // 감쇠
    public float inertia = 1f;            // 관성 (질량 역할)

    private float m_angularVelocity = 0f;   // ω: 각속도
    private float m_angle = 0f;

    bool m_animated;

    private void Start()
    {
        m_image = GetComponent<Image>();
        m_rectTransform = GetComponent<RectTransform>();
        //m_layoutGroup = GetComponentInParent<GridLayoutGroup>(); 
        m_savedIdx = m_rectTransform.GetSiblingIndex();
        m_savedSize = m_rectTransform.sizeDelta; //m_layoutGroup.cellSize;
        m_image.sprite = cursorExitSpr;
    }


    // Update is called once per frame
    void Update()
    {
        if (!m_animated
            && m_isSelected
            && Input.GetMouseButton(0))
        {
            StartCoroutine(AnimateToOrigin());
        }

        float dt = Time.deltaTime;

        // 스프링 힘 계산 (후크 법칙)
        float torque = -torqueStrength * m_angle - damping * m_angularVelocity;

        // 각속도와 각도 업데이트
        m_angularVelocity += torque / inertia * dt;
        m_angle += m_angularVelocity * dt;

        // 실제 회전에 적용
        Vector3 euler = m_rectTransform.localEulerAngles;
        euler.z = m_angle;
        m_rectTransform.localEulerAngles = euler;
    }

    private void OnDestroy()
    {
        selected = null;
    }

    private void OnDisable()
    {
        selected = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.instance.PlayNewBackSound("Album_Onmouse");
        ApplyTorque(35f);
        m_image.sprite = cursorOnSpr;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_image.sprite = cursorExitSpr;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.instance.PlayNewBackSound("Album_Click");

        if (selected == null 
            && eventData.pointerCurrentRaycast.gameObject == gameObject
            && eventData.button == PointerEventData.InputButton.Left)
        {
            ApplyTorque(-50f);
            selected = this;
            m_isSelected = true;
            StartCoroutine(AnimateToCenter());
        }
    }

    public void ApplyTorque(float torque)
    {
        m_angularVelocity += torque / inertia;
    }

    IEnumerator AnimateToCenter()
    {
        m_animated = true;

        Vector2 targetSize = m_savedSize * 2f;
        Vector3 targetPos = Vector3.zero;

        m_rectTransform.SetAsLastSibling();

        float t = 0;
        float duration = 0.3f;

        Vector3 startPos = m_rectTransform.localPosition;
        Vector2 startSize = m_savedSize;

        m_savedPos = m_rectTransform.localPosition;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            m_rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, easedT);
            m_rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, easedT);
            yield return null;
        }

        m_rectTransform.localPosition = targetPos;
        m_rectTransform.sizeDelta = targetSize;

        m_animated = false;
    }

    IEnumerator AnimateToOrigin()
    {
        Vector2 targetSize = m_savedSize;
        Vector3 targetPos = m_savedPos;

        float t = 0;
        float duration = 0.3f;

        Vector3 startPos = Vector3.zero;
        Vector2 startSize = m_savedSize * 2f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            m_rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, easedT);
            m_rectTransform.sizeDelta = Vector2.Lerp(startSize, targetSize, easedT);
            yield return null;
        }

        m_rectTransform.SetSiblingIndex(m_savedIdx);

        selected = null;
        m_isSelected = false;
    }
}
