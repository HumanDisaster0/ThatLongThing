using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    BoxCollider2D m_col;
    bool m_isActive = false;

    void Start()
    {
        m_col = GetComponent<BoxCollider2D>();
        m_col.isTrigger = type == ReactivePlatformType.Hide ? true : false;
        sprite.enabled = type == ReactivePlatformType.Hide ? true : false;
    }

    public void ActivePlatform()
    {
        if (m_isActive)
            return;

        m_isActive = true;
        switch(type)
        {
            case ReactivePlatformType.Hide:
                sprite.enabled = false;
                break;
            case ReactivePlatformType.Show:
                sprite.enabled = true;
                break;
        }
    }

    public void ResetPlatform()
    {
        m_isActive = false;
        m_col.isTrigger = type == ReactivePlatformType.Hide ? true : false;
        sprite.enabled = type == ReactivePlatformType.Hide ? true : false;
    }
}
