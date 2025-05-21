using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathCountHUD : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    // Update is called once per frame
    void Update()
    {
        text.text = StageManager.instance.deathCount.ToString();
    }
}
