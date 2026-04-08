using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadEyePostProcess : MonoBehaviour
{
    public Material effectMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, effectMaterial);
    }
}
