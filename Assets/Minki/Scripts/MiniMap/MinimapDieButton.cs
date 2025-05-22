using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapDieButton : MonoBehaviour
{
    public void PlayerDie()
    {
        PlayerCMD.ForceDie();
    }
}
