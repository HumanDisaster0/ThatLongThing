using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedPortalInteraction : MonoBehaviour
{
    public void EnterPortal()
    {
        GoldManager.Instance.ejectionCount++;
        StageManager.instance.EndStage();
    }
}
