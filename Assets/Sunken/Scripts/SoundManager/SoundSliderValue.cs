using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSliderValue : MonoBehaviour
{
    [SerializeField] string key;

    private void Start()
    {
        GetComponent<Slider>().value = PlayerPrefs.GetFloat(key, 1.0f);
    }
}
