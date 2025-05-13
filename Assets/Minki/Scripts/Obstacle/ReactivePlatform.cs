using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ReactivePlatformType
{
    Hide,
    Show
}

public class ReactivePlatform : MonoBehaviour
{
    [Header("타입")]
    public ReactivePlatformType type = ReactivePlatformType.Hide;

    [Header("참조 컴포넌트")]
    public SpriteRenderer sprite;
    public TilemapRenderer tilemap;

    BoxCollider2D m_col;
    bool m_isActive = false;

    void Start()
    {
        m_col = GetComponent<BoxCollider2D>();
        m_col.isTrigger = type == ReactivePlatformType.Hide ? true : false;
        if(sprite)
            sprite.enabled = type == ReactivePlatformType.Hide ? true : false;
        if(tilemap)
            tilemap.enabled = type == ReactivePlatformType.Hide ? true : false;
    }

    public void ActivePlatform()
    {
        if (m_isActive)
            return;

        m_isActive = true;
        switch(type)
        {
            case ReactivePlatformType.Hide:
                if (sprite)
                    sprite.enabled = false;
                if (tilemap)
                    tilemap.enabled = false;
                break;
            case ReactivePlatformType.Show:
                if (sprite)
                    sprite.enabled = true;
                if (tilemap)
                    tilemap.enabled = true;
                break;
        }
    }

    public void ResetPlatform()
    {
        m_isActive = false;
        m_col.isTrigger = type == ReactivePlatformType.Hide ? true : false;
        if (sprite)
            sprite.enabled = type == ReactivePlatformType.Hide ? true : false;
        if (tilemap)
            tilemap.enabled = type == ReactivePlatformType.Hide ? true : false;
    }
}
