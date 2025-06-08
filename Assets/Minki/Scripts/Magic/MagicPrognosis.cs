using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class MagicPrognosis : MonoBehaviour
{
    public UnityEvent OnShowPrognosis;
    public UnityEvent OnResetPrognosis;

    public SpriteRenderer PrognosisSpriteRender;
    public Tilemap PrognosisTilemap;

    public float showTime = 5.0f;

    public float hideColorTime = 0.4f;

    public bool Active 
    { 
        get => m_isActive; 
        set 
        {  
            m_isActive = value;
            if (m_isActive)
            {
                m_isShowed = false;
                OnResetPrognosis?.Invoke();
            }
        } 
    }

    bool m_isActive = false;

    bool m_isShowed = false;
    float m_showTimer = 0.0f;

    float m_hideColorTimer = 0.0f;

    private void Update()
    {
        if(m_isActive)
        {
            m_hideColorTimer += Time.deltaTime;
        }
        else
        {
            m_hideColorTimer -= Time.deltaTime;

            if(m_hideColorTimer <= 0.0f && m_isShowed)
            {
                m_isShowed = false;
                OnResetPrognosis?.Invoke();
            }
        }

        m_hideColorTimer = Mathf.Clamp(m_hideColorTimer, 0.0f, hideColorTime);

        if (PrognosisSpriteRender != null)
            PrognosisSpriteRender.color = new Color(1.0f, 1.0f, 1.0f, m_hideColorTimer / hideColorTime);

        if(PrognosisTilemap != null)
            PrognosisTilemap.color = new Color(1.0f, 1.0f, 1.0f, m_hideColorTimer / hideColorTime);

        if (m_isActive && !m_isShowed)
        {
            m_isShowed = true;
            OnShowPrognosis?.Invoke();
            m_showTimer = showTime;
        }
        else if(m_isActive)
        {
            m_showTimer -= Time.deltaTime;
            if (m_showTimer <= 0.0f)
            {
                m_isShowed = false;
                OnResetPrognosis?.Invoke();
            }
        }
    }
}
