using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldHUD : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        if (GoldManager.Instance)
            text.text = GoldManager.Instance.totalGold.ToString();
        else
            text.text = "?";
    }
}
