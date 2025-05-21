using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite EnterSprite;
    public Sprite ExitSprite;

    Image m_image;

    void Start()
    {
        m_image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_image.sprite = EnterSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_image.sprite = ExitSprite;
    }

    void OnEnable()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            Input.mousePosition,
            null)) // ¶Ç´Â Camera.main
        {
            OnPointerEnter(new PointerEventData(EventSystem.current));
        }
        else
        {
            OnPointerExit(new PointerEventData(EventSystem.current));
        }
    }
}
