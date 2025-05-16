using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButtonConnector : MonoBehaviour
{
    public CustomClickable pauseButton;


    void Start()
    {   
        
        if (pauseButton != null)
            pauseButton.onClick += PauseManager.Instance.Pause;
    }

    void OnDestroy()
    {
        if (pauseButton != null)
            pauseButton.onClick -= PauseManager.Instance.Pause;
    }

   
}
