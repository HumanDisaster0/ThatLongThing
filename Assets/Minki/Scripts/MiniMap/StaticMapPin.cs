using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticMapPin : MonoBehaviour
{
    public MapPinState MapPinState
    {
        get => m_mapPinState;
        set
        {
            m_mapPinState = value;
            UpdateSpriteAndColor();
        }
    }

    public Sprite DangerSprite;
    public Sprite FineSprite;
    public Sprite StrangeSprite;

    Image m_image;

    MapPinState m_mapPinState = MapPinState.Danger;

    private void Start()
    {
        m_image = GetComponent<Image>();
        UpdateSpriteAndColor();
    }

    private void ImageAssign()
    {
        if (m_image == null)
        {
            m_image = GetComponent<Image>();
        }
    }

    private void UpdateSpriteAndColor()
    {
        ImageAssign();

        switch (m_mapPinState)
        {
            case MapPinState.Danger:
                m_image.sprite = DangerSprite;
                break;
            case MapPinState.Fine:
                m_image.sprite = FineSprite;
                break;
            case MapPinState.Strange:
                m_image.sprite = StrangeSprite;
                break;
        }
        
        m_image.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Set a default color, can be customized
    }
}
