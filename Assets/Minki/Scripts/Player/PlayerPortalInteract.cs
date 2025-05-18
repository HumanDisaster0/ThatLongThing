using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPortalInteract : MonoBehaviour
{
    public AnimationCurve portalEnterAnimation;

    Collider2D m_col;
    PlayerController m_pc;
    Transform m_portal;

    bool m_isEntering;

    public bool IsEntering => m_isEntering;

    private void Awake()
    {
        m_col = GetComponent<Collider2D>();
        m_pc = GetComponent<PlayerController>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_isEntering)
            return;



        //Portal���� ����
        if(collision.CompareTag("Portal"))
        {
            //��ȣ�ۿ� Ű
            if(m_pc.IsGrounded && Input.GetKey(KeyCode.UpArrow))
            {
                m_portal = collision.transform;
                m_isEntering = true;
                
                StartCoroutine(EnterPortal());
            }
        }
    }

    IEnumerator EnterPortal()
    {
        //�÷��̾� ������
        var shadowMask = transform.Find("ShadowMask");

        if(shadowMask != null)
        {
            shadowMask.parent = null;
        }

        m_col.isTrigger = true;
        m_pc.SetVelocity(Vector2.zero);
        m_pc.AnyState(PlayerState.Fall, true);
        m_pc.Freeze = true;
        yield return null;

        Vector3 startPos = m_pc.transform.position;
        Vector3 startScale = m_pc.transform.localScale;
        Vector3 portalCenter = m_portal.transform.position;

        //�������� �ִϸ��̼� ���
        float timer = 0.0f;
        while (timer < portalEnterAnimation[portalEnterAnimation.length - 1].time)
        {
            timer += Time.deltaTime;
            m_pc.transform.position = Vector3.Lerp(startPos, portalCenter, portalEnterAnimation.Evaluate(timer));
            m_pc.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, portalEnterAnimation.Evaluate(timer));
            yield return null;
        }


        //PortalEvent ������Ʈ �ִ��� �˻�
        //�ִٸ� OnEnter ����
        if(m_portal.TryGetComponent(out PortalEvent portalEvent) 
            && portalEvent.OnEnterPortal != null)
        {
            portalEvent.OnEnterPortal.Invoke();
        }

        //���ٸ� �� �ѱ��
        else
        {
            SceneManager.LoadScene(0);
        }


        yield return null;
    }
}
