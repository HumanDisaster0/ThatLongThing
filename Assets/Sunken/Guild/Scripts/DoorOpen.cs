using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SoundManager.instance?.PlayNewBackSound("Door_Open");
    }
}
