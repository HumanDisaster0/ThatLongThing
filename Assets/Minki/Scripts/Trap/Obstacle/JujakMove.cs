using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JujakMove : MonoBehaviour
{
    public float speed;

    Rigidbody2D m_rb;
    BoxCollider2D m_col;

    SpriteRenderer m_sprite;
    bool m_active;

    //±âº»°ª
    Vector3 m_defaultPos;

    // Start is called before the first frame update
    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<BoxCollider2D>();
        m_sprite = GetComponent<SpriteRenderer>();
        m_sprite.enabled = false;
        m_defaultPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_active)
        {
            m_rb.velocity = Vector2.up * speed;
        }
    }

    public void StartMove()
    {
        if (!gameObject.activeInHierarchy || m_active)
            return;

        SoundManager.instance?.PlayNewBackSound("Phoenix_Appear");
        m_active = true;
        m_sprite.enabled = true;
    }

    public void ResetJujak()
    {
        if (!gameObject.activeInHierarchy)
            return;

        m_active = false;
        m_sprite.enabled = false;
        m_rb.velocity = Vector2.zero;
        transform.position = m_defaultPos;
    }
}
