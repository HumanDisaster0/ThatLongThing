using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PortalEvent : MonoBehaviour
{
    public UnityEvent OnEnterPortal;

    public void SceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
