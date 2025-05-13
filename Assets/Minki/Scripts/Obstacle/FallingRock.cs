using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingRock : MonoBehaviour
{
    public SpriteRenderer sprite;

    Rigidbody2D m_rb;

    Vector3 m_defaultPos;
    Quaternion m_defaultRot;

    bool m_isActive = false;

    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_defaultPos = transform.position;
        m_defaultRot = transform.rotation;
        m_rb.bodyType = RigidbodyType2D.Static;
        sprite.enabled = false;
    }

    public void StartMove()
    {
        if (m_isActive)
            return;

        m_rb.bodyType = RigidbodyType2D.Dynamic;
        sprite.enabled = true;
        m_isActive = true;
    }

    public void ResetRock()
    {
        m_isActive = false;
        transform.SetPositionAndRotation(m_defaultPos, m_defaultRot);
        sprite.enabled = false;
        m_rb.bodyType = RigidbodyType2D.Static;
    }
}
