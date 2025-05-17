using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.instance.PlayLoopSound("Grass_Step", this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //SoundManager.instance.PlaySound("Grass_Step", this.gameObject);
    }
}
