using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingRock : MonoBehaviour
{
    [Header("참조 컴포넌트")]
    public SpriteRenderer sprite;
    public LayerMask groundMask;

    //내부 컴포넌트
    Rigidbody2D m_rb;
    CircleCollider2D m_col;

    //기본값
    Vector3 m_defaultPos;
    Quaternion m_defaultRot;

    //활성 트리거
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
        m_col.isTrigger = true;
        if (((1 <<collision.gameObject.layer) & groundMask.value) != 0)
        {
            tag = "Untagged";
        }
        else
        {
            var randDir = System.Math.Sign(Random.Range(-1, 2));
            m_rb.velocity = Vector2.zero;
            m_rb.AddForce(new Vector2(randDir * Random.Range(18f, 32f), 55f), ForceMode2D.Impulse);
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
        tag = "RedZone";
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
