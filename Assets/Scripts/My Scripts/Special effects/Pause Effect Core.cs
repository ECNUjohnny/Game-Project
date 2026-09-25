using UnityEngine;

public class PauseEffectCore : MonoBehaviour
{
    [Header("Material")]

    public Material material;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            Graphics.Blit(source, destination, material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void Pause(bool isPause)
    {
        if (isPause) material.SetFloat("_Blend", 1f);
        else material.SetFloat("_Blend", 0f);
    }

    public void OnDestroy()
    {
        material.SetFloat("_Blend", 0f);   
    }
}
