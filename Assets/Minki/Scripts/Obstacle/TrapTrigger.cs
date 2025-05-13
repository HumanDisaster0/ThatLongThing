using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class TrapTrigger : MonoBehaviour
{
    [Header("��ȣ�ۿ��� ���̾�")]
    public LayerMask interactionLayer;

    [Header("Ʈ���� �ߵ� ����")]
    public bool isEnterTrigger = true;
    public bool isStayTrigger = true;
    public bool isExitTrigger = false;

    [Header("Ʈ���� �ߵ� �����ð� (��)")]
    public float activeTime = 0.5f;


    [Header("Ʈ���� �̺�Ʈ")]
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

    // ���� ������ ���� ���·� ��������� �Լ�
    public void ResetTrigger()
    {
        //���� �� �����ų �͵�
        m_isActiveObstacle = false;

        //��ϵ� ���� �Լ�
        OnResetTrigger?.Invoke();

        //�ڷ�ƾ�� �ִ� ��� ����
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
