using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingRock : MonoBehaviour
{
    [Header("���� ������Ʈ")]
    public SpriteRenderer sprite;
    public LayerMask groundMask;

    //���� ������Ʈ
    Rigidbody2D m_rb;
    CircleCollider2D m_col;

    //�⺻��
    Vector3 m_defaultPos;
    Quaternion m_defaultRot;

    //Ȱ�� Ʈ����
    bool m_isActive = false;
    bool m_trapOff = false;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_col = GetComponent<CircleCollider2D>();
        m_defaultPos = transform.position;
        m_defaultRot = transform.rotation;
        m_rb.bodyType = RigidbodyType2D.Static;
        m_col.isTrigger = true;
        sprite.enabled = false;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 <<collision.gameObject.layer) & groundMask.value) != 0)
        {
            m_col.isTrigger = true;
        }
    }

    public void StartMove()
    {
        if (!gameObject.activeInHierarchy || m_trapOff || m_isActive)
            return;

        m_col.isTrigger = false;
        m_rb.bodyType = RigidbodyType2D.Dynamic;
        sprite.enabled = true;
        m_isActive = true;
    }

    public void ResetRock()
    {
        if (!gameObject.activeInHierarchy)
            return;

        m_isActive = false;
        transform.SetPositionAndRotation(m_defaultPos, m_defaultRot);
        sprite.enabled = false;
        m_rb.bodyType = RigidbodyType2D.Static;
    }

    public void TrapOff()
    {
        m_trapOff = true;
    }
}
