using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeButtonConnector : MonoBehaviour
{
    public CustomClickable resumeButton;

    void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick += PauseManager.Instance.Resume;
    }

    void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick -= PauseManager.Instance.Resume;
    }
}
