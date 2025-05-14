using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RightClickScrollRect : ScrollRect
{
    // 마우스 우클릭(버튼 1)일 때만 드래그 시작
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnInitializePotentialDrag(eventData);
            eventData.button = PointerEventData.InputButton.Middle;
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
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            eventData.button = PointerEventData.InputButton.Left;
            base.OnDrag(eventData);
            eventData.button = PointerEventData.InputButton.Middle;
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
    }
}