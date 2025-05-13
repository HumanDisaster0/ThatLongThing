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
                targetMonster.ActivateChase();
                break;

            case TriggerAction.Deactivate:
                targetMonster.DeactivateChase();
                break;
        }
    }
}
