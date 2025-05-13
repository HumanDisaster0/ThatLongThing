using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public enum FootholdType
{
    None,
    MoveDown,
    MoveUp
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Foothold : MonoBehaviour
{
    public LayerMask interactionLayer;
    public FootholdType footholdType = FootholdType.None;
    public float moveSpeed = 2.0f;
    public bool acceleration = false;
    public float activeTime = 0.5f;

    Rigidbody2D m_rb;
    BoxCollider2D m_col;
    Vector3 m_defaultPos;
    Quaternion m_defaultRot;
    Vector3 m_defaultScale;

    bool m_isActiveObstacle;
    bool m_moveStart;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<BoxCollider2D>();

        m_rb.bodyType = RigidbodyType2D.Static;
        m_defaultPos = transform.position;
        m_defaultRot = transform.rotation;
        m_defaultScale = transform.localScale;
    }

    private void Update()
    {
        if(m_moveStart)
        {
            switch(footholdType)
            {
                case FootholdType.MoveUp:
                    if (acceleration)
                        m_rb.velocity += Vector2.up * moveSpeed;
                    else
                        m_rb.velocity = Vector2.up * moveSpeed;
                    break;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (footholdType != FootholdType.None
            && !m_isActiveObstacle
            && ((1 << collision.gameObject.layer) & interactionLayer.value) != 0)
        {
            m_isActiveObstacle = true;
            StartCoroutine(StartMove());
        }
    }

    IEnumerator StartMove()
    {
        if (activeTime == 0.0f)
        {
            ChangeBodyType();
            yield break;
        }

        var waitTime = 0.0f;
        while(waitTime < activeTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        ChangeBodyType();
    }

    public void ResetFoothold()
    {
        transform.position = m_defaultPos;
        transform.rotation = m_defaultRot;
        transform.localScale = m_defaultScale;
        m_moveStart = false;
        m_isActiveObstacle = false;
        m_rb.bodyType = RigidbodyType2D.Static;
        m_rb.interpolation = RigidbodyInterpolation2D.None;
        m_rb.freezeRotation = false;
    }

    public void ChangeBodyType()
    {
        switch(footholdType)
        {
            case FootholdType.MoveUp:
                m_rb.bodyType = RigidbodyType2D.Kinematic;
                break;
            case FootholdType.MoveDown:
                m_rb.bodyType = RigidbodyType2D.Dynamic;
                break;
        }
        m_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_rb.freezeRotation = true;
        m_moveStart = true;
    }
}
