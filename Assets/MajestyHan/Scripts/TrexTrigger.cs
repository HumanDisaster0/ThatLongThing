using UnityEngine;

public class TrexTrigger : MonoBehaviour
{
    public enum TriggerAction { Activate, Deactivate }
    public TriggerAction action;

    public TrexMove targetMonster;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (action)
        {
            case TriggerAction.Activate:
                if (!targetMonster.gameObject.activeSelf)
                {
                    targetMonster.ActivateChase();
                }
                break;

            case TriggerAction.Deactivate:
                if (targetMonster != null && targetMonster.gameObject.activeSelf)
                {
                    targetMonster.DeactivateChase();
                }
                break;
        }
    }
}
