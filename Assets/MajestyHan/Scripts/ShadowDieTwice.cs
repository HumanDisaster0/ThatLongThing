using UnityEngine;

/// <summary>
/// 캐릭터 아래에 반응형 그림자를 생성한다. 좌/우 바닥 유무에 따라 절반씩 보여준다.
/// </summary>
public class ShadowDieTwice : MonoBehaviour
{
    [Header("참조")]
    public Transform shadowTransform;              // 그림자 객체
    public SpriteRenderer shadowRenderer;          // 그림자 SpriteRenderer
    public BoxCollider2D playerCollider;           // 캐릭터 콜라이더

    [Header("설정")]
    public LayerMask groundMask;                   // Ground 레이어 마스크
    public float rayLength = 6f;                   // 레이 길이
    public float shadowYOffset = 0.01f;            // 지면에서의 Y 보정값
    public float shadowWidth = 1f;                 // 그림자 너비 (풀 사이즈)

    void Update()
    {
        Vector2 leftOrigin = new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.min.y);
        Vector2 rightOrigin = new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.min.y);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, rayLength, groundMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, rayLength, groundMask);

        // 둘 중 낮은 바닥을 기준으로 그림자 위치 결정
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

        // 그림자 위치는 캐릭터 중심 + 바닥 기준 Y
        shadowTransform.position = new Vector3(transform.position.x, minY + shadowYOffset, 0f);

        // 그림자 크기/오프셋 조정
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

        // 투명도: 캐릭터와 그림자 거리 기반
        float dist = transform.position.y - minY;
        float alpha = Mathf.Clamp01(1f - (dist / rayLength));
        shadowRenderer.color = new Color(0f, 0f, 0f, alpha * 0.8f);
    }
}
