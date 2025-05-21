using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MapPinState
{
    Danger = 0,
    Fine,
    Strange
}

public class MapPin : MonoBehaviour, IPointerClickHandler
{
    public MapPinSetter pinSetter;

    public MapPinState GetMapPinState => m_mapPinState;

    public Sprite DangerSprite;
    public Sprite FineSprite;
    public Sprite StrangeSprite;

    Image m_image;

    MapPinState m_mapPinState = MapPinState.Danger;

    Color m_fineColor = Color.green;
    Color m_dangerColor = Color.red;
    Color m_strangeColor = Color.yellow;

    private void Start()
    {
        m_image = GetComponent<Image>();
        m_image.sprite = DangerSprite;
        //m_image.color = m_fineColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == gameObject
            && eventData.button == PointerEventData.InputButton.Left)
        {

            m_mapPinState = (MapPinState)((int)m_mapPinState + 1);

            SoundManager.instance?.PlayNewBackSound("Map_Check2");
            if ((int)m_mapPinState > Enum.GetValues(typeof(MapPinState)).Length - 2)
            {
                pinSetter.DeletePin(gameObject.GetHashCode());
                return;
            }


          

            switch(m_mapPinState)
            {
                case MapPinState.Danger:
                    m_image.sprite = DangerSprite;
                    //m_image.color = m_dangerColor;
                    break;
                case MapPinState.Fine:
                    m_image.sprite = FineSprite;
                    //m_image.color = m_fineColor;
                    break;
                case MapPinState.Strange:
                    m_image.sprite = StrangeSprite;
                    //m_image.color = m_strangeColor;
                    break;
            }
        }
    }
}
