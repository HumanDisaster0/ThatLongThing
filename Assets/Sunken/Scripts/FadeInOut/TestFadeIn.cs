using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFadeIn : MonoBehaviour
{
    [SerializeField] FadeInOut fade;

    // Start is called before the first frame update
    private void OnEnable()
    {
        fade.ExeFadeIn();
    }
}
