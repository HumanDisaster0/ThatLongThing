using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum FootholdType
{
    MoveDown,
    MoveUp
}

[RequireComponent(typeof(Rigidbody2D))]
public class FootholdMover : MonoBehaviour
{
    [Header("타입")]
    public FootholdType footholdType = FootholdType.MoveDown;

    [Header("발판 속성")]
    public float moveSpeed = 2.0f;
    public bool acceleration = false;

    Rigidbody2D m_rb;
    Collider2D m_col;
    Vector3 m_defaultPos;
    Quaternion m_defaultRot;
    Vector3 m_defaultScale;

    bool m_moveStart;
    bool m_trapOff;

    // Start is called before the first frame update
    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<BoxCollider2D>();

        if (m_col == null)
            m_col = GetComponent<CompositeCollider2D>();
        if (m_col == null)
            m_col = GetComponent<TilemapCollider2D>();


        m_rb.bodyType = RigidbodyType2D.Static;
        m_defaultPos = transform.position;
        m_defaultRot = transform.rotation;
        m_defaultScale = transform.localScale;
    }

    void FixedUpdate()
    {
        //움직임 트리거 발동 이후 행동
        if(m_moveStart)
        {
            switch(footholdType)
            {
                case FootholdType.MoveUp:
                    if (acceleration)
                        m_rb.velocity += Vector2.up * moveSpeed * Time.deltaTime;
                    else
                        m_rb.velocity = Vector2.right * moveSpeed;
                    break;
            }
        }
    }

    /// <summary>
    /// 움직임 트리거를 발동합니다.
    /// </summary>
    public void StartMove()
    {
        if (!gameObject.activeInHierarchy || m_trapOff)
            return;

        switch (footholdType)
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

    public void ResetFoothold()
    {
        if (!gameObject.activeInHierarchy)
            return;

        //초기상태로 되돌림
        transform.position = m_defaultPos;
        transform.rotation = m_defaultRot;
        transform.localScale = m_defaultScale;
        m_moveStart = false;
        m_rb.bodyType = RigidbodyType2D.Static;
        m_rb.interpolation = RigidbodyInterpolation2D.None;
        m_rb.freezeRotation = false;
    }

    public void TrapOn()
    {
        if (!gameObject.activeInHierarchy)
            return;

        m_trapOff = false;
    }

    public void TrapOff()
    {
        if (!gameObject.activeInHierarchy)
            return;

        m_trapOff = true;
    }
}
