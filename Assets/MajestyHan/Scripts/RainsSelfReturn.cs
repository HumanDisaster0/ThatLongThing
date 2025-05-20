using UnityEngine;

public class RainsSelfReturn : MonoBehaviour
{
    [Tooltip("�÷��̾ �ִ� ���̾ �۵�")]
    public LayerMask playerLayer;

    [Tooltip("Ǯ �Ŵ���")]
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
