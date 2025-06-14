using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

public class MapOnOffControl : MonoBehaviour
{
    public bool activeControl = true;

    public RectTransform miniMapUIRect;
    public RectTransform miniMapOpenRect;

    public UnityAction OnMiniMapShow;
    public UnityAction OnMiniMapHide;

    private HashSet<GameObject> m_uiStack = new HashSet<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
        {
            ShowMinimap();
            return;
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            HideMinimap();
            return;
        }
    }

    public void HideMinimap()
    {
        if (!activeControl || m_uiStack.Count != 1)
            return;
        m_uiStack.Clear();
        OnMiniMapHide?.Invoke();
        SoundManager.instance.PlayNewBackSound("Map_Button");
        miniMapUIRect.gameObject.SetActive(false);
        miniMapOpenRect.gameObject.SetActive(true);
    }

    public void ShowMinimap()
    {
        if (!activeControl || m_uiStack.Count != 0)
            return;
        m_uiStack.Add(gameObject);
        OnMiniMapShow?.Invoke();
        SoundManager.instance.PlayNewBackSound("Map_Button");
        miniMapUIRect.gameObject.SetActive(true);
        miniMapOpenRect.gameObject.SetActive(false);
    }

    public void AddUIStack(GameObject uiStack)
    {
        if (uiStack == null || m_uiStack.Contains(uiStack)) return;

        m_uiStack.Add(uiStack);
    }

    public void RemoveUIStack(GameObject uiStack)
    {
        if (uiStack == null || !m_uiStack.Contains(uiStack)) return;

        m_uiStack.Remove(uiStack);
    }
}
