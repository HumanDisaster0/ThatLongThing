using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessEffect : MonoBehaviour
{
    public Material postProcessMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, postProcessMaterial);
    }
}
