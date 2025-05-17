using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPortalInteract : MonoBehaviour
{
    public AnimationCurve portalEnterAnimation;

    Collider2D m_col;
    PlayerController m_pc;
    Vector3 m_portalCenter;

    bool m_isEntering;

    private void Awake()
    {
        m_col = GetComponent<Collider2D>();
        m_pc = GetComponent<PlayerController>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_isEntering)
            return;

        //Portal인지 조사
        if(collision.CompareTag("Portal"))
        {
            //상호작용 키
            if((m_pc.GetCurrentState() == PlayerState.Walk
                || m_pc.GetCurrentState() == PlayerState.Idle) && Input.GetKeyDown(KeyCode.UpArrow))
            {
                m_portalCenter = collision.transform.position;
                m_isEntering = true;
                //PortalEvent 컴포넌트 있는지 검사
                //있다면 OnPortalEnter 실행
                StartCoroutine(EnterPortal());
            }
        }
    }

    IEnumerator EnterPortal()
    {
        m_col.isTrigger = true;
        m_pc.SetVelocity(Vector2.zero);
        m_pc.AnyState(PlayerState.Fall, true);
        m_pc.Freeze = true;
        yield return null;

        Vector3 m_startPos = m_pc.transform.position;
        Vector3 m_startScale = m_pc.transform.localScale;

        float timer = 0.0f;
        while (timer < portalEnterAnimation[portalEnterAnimation.length - 1].time)
        {
            timer += Time.deltaTime;
            m_pc.transform.position = Vector3.Lerp(m_startPos, m_portalCenter, portalEnterAnimation.Evaluate(timer));
            m_pc.transform.localScale = Vector3.Lerp(m_startScale, Vector3.zero, portalEnterAnimation.Evaluate(timer));
            yield return null;
        }

        //씬 넘기기

        yield return null;
    }
}
