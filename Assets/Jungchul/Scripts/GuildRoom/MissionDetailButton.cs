using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public class MissionDetailButton : MonoBehaviour
{
    public Panel missionThumbnail;

    
    void Start()
    {
        CustomClickable clickable = GetComponent<CustomClickable>();

        //clickable.gameObject.SetActive(false);  

        clickable.SetClickAction(() =>
        {
            if (missionThumbnail != null)
            {
                missionThumbnail.gameObject.SetActive(true); 

            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
