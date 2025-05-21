using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryCanvas : MonoBehaviour
{
    public RectTransform layout;

    //Ư�� �߾� �ر�
    public void EnableMemory(int anomalyIdx)
    {
        if (anomalyIdx < 0 || anomalyIdx > 15)
            return;

        layout.GetChild(anomalyIdx - 1).GetComponent<Memory>().enabled = true;
    }

    //Ŭ������ �̻����� �߾� �ر� -> StageManager ������ ����
    public void UnlockMemories()
    {
        for(int i = 0; i < layout.childCount; i++)
        {
            if (StageManager.instance.IsClearedAnomaly(i + 1))
                layout.GetChild(i).GetComponent<Memory>().enabled = true;
        }
    }

    private void OnEnable()
    {
        BgmPlayer.instance.ChangeBgm("Guild_Album");
    }

    private void OnDisable()
    {
        BgmPlayer.instance.ChangeBgm("Guild_BGM");
    }
}
