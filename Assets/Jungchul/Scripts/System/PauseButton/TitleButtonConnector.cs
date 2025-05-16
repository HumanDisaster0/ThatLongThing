using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleButtonConnector : MonoBehaviour
{
    public CustomClickable titleButton;

    void Start()
    {
        if (titleButton != null)
            titleButton.onClick += PauseManager.Instance.ReturnToTitle;
    }

    void OnDestroy()
    {
        if (titleButton != null)
            titleButton.onClick -= PauseManager.Instance.ReturnToTitle;
    }
}
