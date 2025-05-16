using UnityEngine;
using UnityEngine.Events;

public class TriggerRepeater : MonoBehaviour
{
    [Header("�۵��� ���̾�")]
    public LayerMask targetLayer;

    [Header("�ݺ� ȣ�� ���� (��)")]
    public float repeatInterval = 0.2f;

    [Header("Ʈ���� ���Խ�")]
    public UnityEvent<GameObject> OnTriggerEnterEvent;

    [Header("Ʈ���� ���� �ݺ�")]
    public UnityEvent<GameObject> OnTriggerRepeatEvent;

    [Header("Ʈ���� ��Ż��")]
    public UnityEvent<GameObject> OnTriggerExitEvent;

    private bool isInside = false;
    private GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            isInside = true;
            currentTarget = other.gameObject;

            OnTriggerEnterEvent?.Invoke(currentTarget); // ���� �̺�Ʈ
            InvokeRepeating(nameof(InvokeRepeatEvent), 0f, repeatInterval);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentTarget)
        {
            OnTriggerExitEvent?.Invoke(currentTarget); // ��Ż �̺�Ʈ

            isInside = false;
            currentTarget = null;
            CancelInvoke(nameof(InvokeRepeatEvent));
        }
    }

    private void InvokeRepeatEvent()
    {
        if (isInside && currentTarget != null)
        {
            OnTriggerRepeatEvent?.Invoke(currentTarget);
        }
    }
}
