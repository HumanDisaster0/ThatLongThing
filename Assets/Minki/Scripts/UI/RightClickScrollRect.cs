using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RightClickScrollRect : ScrollRect
{
    // ���콺 ��Ŭ��(��ư 1)�� ���� �巡�� ����
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnInitializePotentialDrag(eventData);
            eventData.button = PointerEventData.InputButton.Middle;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnInitializePotentialDrag(eventData);
            eventData.button = PointerEventData.InputButton.Right;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnBeginDrag(eventData);
            eventData.button = PointerEventData.InputButton.Middle;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnBeginDrag(eventData);
            eventData.button = PointerEventData.InputButton.Right;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnDrag(eventData);
            eventData.button = PointerEventData.InputButton.Middle;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnDrag(eventData);
            eventData.button = PointerEventData.InputButton.Right;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnEndDrag(eventData);
            eventData.button = PointerEventData.InputButton.Middle;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnEndDrag(eventData);
            eventData.button = PointerEventData.InputButton.Right;
        }
    }
}