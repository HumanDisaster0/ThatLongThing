using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public enum FootholdType
{
    MoveDown,
    MoveUp
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class FootholdMover : MonoBehaviour
{
    [Header("Ÿ��")]
    public FootholdType footholdType = FootholdType.MoveDown;

    [Header("���� �Ӽ�")]
    public float moveSpeed = 2.0f;
    public bool acceleration = false;

    Rigidbody2D m_rb;
    BoxCollider2D m_col;
    Vector3 m_defaultPos;
    Quaternion m_defaultRot;
    Vector3 m_defaultScale;

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

    void Update()
    {
        //������ Ʈ���� �ߵ� ���� �ൿ
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

    /// <summary>
    /// ������ Ʈ���Ÿ� �ߵ��մϴ�.
    /// </summary>
    public void StartMove()
    {
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
        //�ʱ���·� �ǵ���
        transform.position = m_defaultPos;
        transform.rotation = m_defaultRot;
        transform.localScale = m_defaultScale;
        m_moveStart = false;
        m_rb.bodyType = RigidbodyType2D.Static;
        m_rb.interpolation = RigidbodyInterpolation2D.None;
        m_rb.freezeRotation = false;
    }
}
