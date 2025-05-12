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

    bool m_isActiveObstacle;
    bool m_moveStart;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<BoxCollider2D>();

        m_rb.bodyType = RigidbodyType2D.Static;
    }

    private void Update()
    {
        if(m_moveStart && footholdType == FootholdType.MoveUp)
        {
            if (acceleration)
                m_rb.velocity += Vector2.up * moveSpeed;
            else
                m_rb.velocity = Vector2.up * moveSpeed;
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
            m_rb.bodyType = RigidbodyType2D.Dynamic;
            m_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            m_rb.freezeRotation = true;
            m_moveStart = true;
            yield break;
        }

        var waitTime = 0.0f;
        while(waitTime < activeTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        m_rb.freezeRotation = true;
        m_rb.bodyType = RigidbodyType2D.Dynamic;
        m_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_moveStart = true;
    }
}
