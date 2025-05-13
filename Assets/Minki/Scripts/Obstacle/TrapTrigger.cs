using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class TrapTrigger : MonoBehaviour
{
    [Header("상호작용할 레이어")]
    public LayerMask interactionLayer;

    [Header("트리거 발동 조건")]
    public bool isEnterTrigger = true;
    public bool isStayTrigger = true;
    public bool isExitTrigger = false;

    [Header("트리거 발동 지연시간 (초)")]
    public float activeTime = 0.5f;


    [Header("트리거 이벤트")]
    public UnityEvent<GameObject> OnStartTrigger;
    public UnityEvent OnResetTrigger;

    BoxCollider2D m_col;
    bool m_isActiveObstacle;

    void Start()
    {
        m_col = GetComponent<BoxCollider2D>();   
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isEnterTrigger
            && !m_isActiveObstacle
            && ((1 << collision.gameObject.layer) & interactionLayer.value) != 0)
        {
            m_isActiveObstacle = true;
            StartCoroutine(ActiveTrap(collision.gameObject));
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {

        if (isStayTrigger
            && !m_isActiveObstacle
            && ((1 << collision.gameObject.layer) & interactionLayer.value) != 0)
        {
            m_isActiveObstacle = true;
            StartCoroutine(ActiveTrap(collision.gameObject));
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isExitTrigger
            && !m_isActiveObstacle
            && ((1 << collision.gameObject.layer) & interactionLayer.value) != 0)
        {
            m_isActiveObstacle = true;
            StartCoroutine(ActiveTrap(collision.gameObject));
        }
    }

    // 함정 켜지기 이전 상태로 만들기위한 함수
    public void ResetTrigger()
    {
        //리셋 시 적용시킬 것들
        m_isActiveObstacle = false;

        //등록된 리셋 함수
        OnResetTrigger?.Invoke();

        //코루틴이 있는 경우 정지
        StopAllCoroutines();
        return;
    }

    IEnumerator ActiveTrap(GameObject other)
    {
        if (activeTime == 0.0f)
        {
            OnStartTrigger?.Invoke(other);
            yield break;
        }

        var waitTime = 0.0f;
        while (waitTime < activeTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        OnStartTrigger?.Invoke(other);
    }
}
