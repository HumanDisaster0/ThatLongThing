using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public enum ReactivePlatformType
{
    Hide,
    Show
}

[RequireComponent(typeof(BoxCollider2D))]
public class ReactivePlatform : MonoBehaviour
{
    public ReactivePlatformType type = ReactivePlatformType.Hide;
    public SpriteRenderer sprite;

    BoxCollider2D m_col;

    bool m_isActive = false;

    bool m_defaultTriggerOption;
    bool m_defaultSpriteEnable;

    void Start()
    {
        m_col = GetComponent<BoxCollider2D>();
        m_defaultTriggerOption = m_col.isTrigger;
        m_defaultSpriteEnable = sprite.enabled;
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
                m_col.isTrigger = true;
                break;
            case ReactivePlatformType.Show:
                sprite.enabled = true;
                break;
        }
    }

    public void ResetPlatform()
    {
        m_isActive = false;
        m_col.isTrigger = m_defaultTriggerOption;
        sprite.enabled = m_defaultSpriteEnable;
    }
}
