using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinimapAnomalyText : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Start()
    {
        text.text = StageManager.instance.GetAnomalyName(StageManager.instance.anomalyIdx);
    }
}
