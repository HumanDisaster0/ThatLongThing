using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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

    Collider2D m_col;
    bool m_isActive = false;
    bool m_trapOff = false;

    public bool TrapIsOff => m_trapOff;

    void Awake()
    {
        m_col = GetComponent<BoxCollider2D>();

        if (m_col == null)
            m_col = GetComponent<TilemapCollider2D>();

        SetPlatformOption();
    }

    public void ActivePlatform()
    {
        if (!gameObject.activeInHierarchy || m_trapOff || m_isActive)
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
        if (!gameObject.activeInHierarchy)
            return;

        m_isActive = false;
        SetPlatformOption();
    }

    public void TrapOn()
    {
        if (!gameObject.activeInHierarchy)
            return;
        m_trapOff = false;

        var rand = Random.Range(0, 2);

        type = rand == 0 ? ReactivePlatformType.Show : ReactivePlatformType.Hide;
        SetPlatformOption();
    }

    public void TrapOff()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (sprite)
            sprite.enabled = false;
        if (tilemap)
            tilemap.enabled = false;
        m_trapOff = true;
        m_col.isTrigger = true;
    }

    public void SetPlatformOption()
    {
        m_col.isTrigger = type == ReactivePlatformType.Hide ? true : false;
        if (sprite)
            sprite.enabled = type == ReactivePlatformType.Hide ? true : false;
        if (tilemap)
            tilemap.enabled = type == ReactivePlatformType.Hide ? true : false;
    }
}
