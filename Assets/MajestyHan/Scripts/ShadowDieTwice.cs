using UnityEngine;

/// <summary>
/// ĳ���� �Ʒ��� ������ �׸��ڸ� �����Ѵ�. ��/�� �ٴ� ������ ���� ���ݾ� �����ش�.
/// </summary>
public class ShadowDieTwice : MonoBehaviour
{
    [Header("����")]
    public Transform shadowTransform;              // �׸��� ��ü
    public SpriteRenderer shadowRenderer;          // �׸��� SpriteRenderer
    public BoxCollider2D playerCollider;           // ĳ���� �ݶ��̴�

    [Header("����")]
    public LayerMask groundMask;                   // Ground ���̾� ����ũ
    public float rayLength = 6f;                   // ���� ����
    public float shadowYOffset = 0.01f;            // ���鿡���� Y ������
    public float shadowWidth = 1f;                 // �׸��� �ʺ� (Ǯ ������)

    void Update()
    {
        Vector2 leftOrigin = new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.min.y);
        Vector2 rightOrigin = new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.min.y);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, rayLength, groundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, rayLength, groundMask);

        // �� �� ���� �ٴ��� �������� �׸��� ��ġ ����
        bool hasLeft = hitLeft.collider != null;
        bool hasRight = hitRight.collider != null;

        if (!hasLeft && !hasRight)
        {
            shadowRenderer.enabled = false;
            return;
        }

        shadowRenderer.enabled = true;

        float minY = Mathf.Infinity;
        if (hasLeft) minY = Mathf.Min(minY, hitLeft.point.y);
        if (hasRight) minY = Mathf.Min(minY, hitRight.point.y);

        // �׸��� ��ġ�� ĳ���� �߽� + �ٴ� ���� Y
        shadowTransform.position = new Vector3(transform.position.x, minY + shadowYOffset, 0f);

        // �׸��� ũ��/������ ����
        Vector3 scale = Vector3.one;
        Vector3 offset = Vector3.zero;

        if (hasLeft && hasRight)
        {
            scale.x = shadowWidth;
        }
        else if (hasLeft)
        {
            scale.x = shadowWidth * 0.5f;
            offset.x = -shadowWidth * 0.25f;
        }
        else if (hasRight)
        {
            scale.x = shadowWidth * 0.5f;
            offset.x = shadowWidth * 0.25f;
        }

        shadowTransform.localScale = scale;
        shadowTransform.localPosition += offset;

        // ����: ĳ���Ϳ� �׸��� �Ÿ� ���
        float dist = transform.position.y - minY;
        float alpha = Mathf.Clamp01(1f - (dist / rayLength));
        shadowRenderer.color = new Color(0f, 0f, 0f, alpha * 0.8f);
    }
}
