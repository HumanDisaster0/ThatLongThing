using UnityEngine;

public class RainsSelfReturn : MonoBehaviour
{
    [Tooltip("플레이어가 있는 레이어만 작동")]
    public LayerMask playerLayer;

    [Tooltip("풀 매니저")]
    public RainObjectPool rainPool;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            SoundManager.instance.PlayNewSound("Gold_Collect", GameObject.FindWithTag("Player"));
            rainPool?.Return(gameObject);
        }
    }
}
