using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [Header("Dead Setting")]

    public float slideDuration = 0.4f;

    private RectTransform rectTransform;

    private Vector2 initPos;

    private Vector2 endPos;

    public float rate = 0.5f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

       
        initPos = Vector2.zero;
        
    
        endPos = new Vector2(-3000f, 0f);

        rectTransform.anchoredPosition = endPos;
    }

    public void SlideIn()
    {
        StartCoroutine(SlideRoutine());
    }

    IEnumerator SlideRoutine()
    {
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            float t = Mathf.Clamp01(elapsed / slideDuration);

            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(endPos, initPos, easeT * rate);
            
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition = initPos;
    }
}
