using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButtonConnector : MonoBehaviour
{
    public CustomClickable exitButton;

    void Start()
    {
        if (exitButton == null)
            print("À¯°¨");

        if (exitButton != null)
            exitButton.onClick += PauseManager.Instance.ExitGame;
    }

    void OnDestroy()
    {
        if (exitButton != null)
            exitButton.onClick -= PauseManager.Instance.ExitGame;
    }
}
