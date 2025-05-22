using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GUIHoverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isInteractable = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isInteractable)
        {
            UiSoundManager.instance?.PlaySound("Default", UISFX.Hover);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInteractable)
        {
            UiSoundManager.instance?.PlaySound("Default", UISFX.Exit);
        }
    }
}
