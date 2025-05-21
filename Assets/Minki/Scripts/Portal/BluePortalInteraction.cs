using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePortalInteraction : MonoBehaviour
{
    public void EnterPortal()
    {
        StageManager.instance.EndStage();
    }
}
