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
    [Header("Ÿ��")]
    public FootholdType footholdType = FootholdType.MoveDown;

    [Header("���� �Ӽ�")]
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
        //������ Ʈ���� �ߵ� ���� �ൿ
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
    /// ������ Ʈ���Ÿ� �ߵ��մϴ�.
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

        //�ʱ���·� �ǵ���
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
