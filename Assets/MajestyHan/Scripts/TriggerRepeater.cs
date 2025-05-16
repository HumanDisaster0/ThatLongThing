using UnityEngine;
using UnityEngine.Events;

public class TriggerRepeater : MonoBehaviour
{
    [Header("작동할 레이어")]
    public LayerMask targetLayer;

    [Header("반복 호출 간격 (초)")]
    public float repeatInterval = 0.2f;

    [Header("트리거 진입시")]
    public UnityEvent<GameObject> OnTriggerEnterEvent;

    [Header("트리거 내부 반복")]
    public UnityEvent<GameObject> OnTriggerRepeatEvent;

    [Header("트리거 이탈시")]
    public UnityEvent<GameObject> OnTriggerExitEvent;

    private bool isInside = false;
    private GameObject currentTarget;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            isInside = true;
            currentTarget = other.gameObject;

            OnTriggerEnterEvent?.Invoke(currentTarget); // 진입 이벤트
            InvokeRepeating(nameof(InvokeRepeatEvent), 0f, repeatInterval);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentTarget)
        {
            OnTriggerExitEvent?.Invoke(currentTarget); // 이탈 이벤트

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
