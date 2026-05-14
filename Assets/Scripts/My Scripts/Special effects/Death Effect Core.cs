using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class DeathEffectCore : MonoBehaviour
{
    [Header("Material")]

    public Material DeathMat;

    [Header("Effect Setting")]

    public float effectDuration = 0.05f;

    public float brightness = 0.2f;

    public float timeScale = 0.3f;

    public bool isDead = false;

    public KeyCode key = KeyCode.O;
    
    public PlayerHealthSystem healthSystem;

    void Start()
    {
        healthSystem.Dead += TriggerDeath;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (DeathMat != null)
        {
            Graphics.Blit(src, dest, DeathMat);
        }

        else
        {
            Graphics.Blit(src, dest);
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        float timeElapsed = 0;

        float startFixedDelta = Time.fixedDeltaTime;

        while (timeElapsed < effectDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timeElapsed / effectDuration);

            if (DeathMat != null)
            {
                DeathMat.SetFloat("_Blend", 1);

                // float p = Mathf.Lerp(0f, 1f, Mathf.Pow(t, 5));

                DeathMat.SetFloat("_Brightness", 1);
            }

            Time.timeScale = Mathf.Lerp(1f, timeScale, t);

            Time.fixedDeltaTime = startFixedDelta * Time.timeScale; 

            yield return null;
        }
    }

    
    void Update()
    {
        if (Input.GetKey(key))
        {
            TriggerDeath();
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1.0f;

        DeathMat.SetFloat("_Blend", 0);

        DeathMat.SetFloat("_Brightness", 0);        
    }
}
