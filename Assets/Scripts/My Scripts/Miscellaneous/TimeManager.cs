using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("Time Setting")]

    [Range(0, 24f)]
    public float timeOfDay = 0f;

    public float timeMultipler = 1f; // 1 sec in Game = 1 hour in Reality

    [Header("Sun Setting")]

    public Light sunLight;

    public AnimationCurve sunIntensityCurve;

    [Header("Environment Setting")]

    public Gradient ambientLightColor;

    void Update()
    {
        UpdateClock();
        
        UpdateSun();
    }

    public void UpdateClock()
    {
        timeOfDay += Time.deltaTime * timeMultipler;

        if (timeOfDay >= 24f)
        {
            timeOfDay %= 24f;
        }
    }

    public void UpdateSun()
    {
        if (sunLight == null) return;

        float sunRotation = timeOfDay / 24f * 360f - 90f;

        sunLight.transform.localRotation = Quaternion.Euler(sunRotation, -30f, 0);

        float timeRatio = timeOfDay / 24f;

        // Debug.Log(timeRatio);

        sunLight.enabled = sunLight.intensity > 0.05f;

        sunLight.intensity = sunIntensityCurve.Evaluate(timeRatio);

        RenderSettings.ambientLight = ambientLightColor.Evaluate(timeRatio);
    }
}
