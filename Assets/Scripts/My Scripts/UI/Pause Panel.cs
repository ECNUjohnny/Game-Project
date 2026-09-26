using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class PausePanel : MonoBehaviour
{

    [Header("Pause Panel Setting")]

    public float slideDuration = 0.2f;

    private RectTransform rect;

    private Vector2 startPos;

    private Vector2 endPos;

    private Coroutine slide;

    [Header("Setting Panel")]

    public GameObject setting;


    void Awake()
    {
        rect = GetComponent<RectTransform>();

        startPos = new(-610, 1080);

        endPos = new(-610, 0);

        rect.anchoredPosition = startPos;
    
        if (setting != null) setting.SetActive(false);
    }

    public void SlideIn()
    {
        if (slide != null)
        {
            StopCoroutine(slide);
        }
        
        slide = StartCoroutine(Slide(startPos, endPos));
    } 

    public void SlideOut()
    {
        if (slide != null)
        {
            StopCoroutine(slide);
        }         

        slide = StartCoroutine(Slide(endPos, startPos));
    }

    IEnumerator Slide(Vector2 startPos, Vector2 endPos)
    {
        float elapsed = 0;

        while (elapsed < slideDuration)
        {
            float r = elapsed / slideDuration;

            elapsed += Time.unscaledDeltaTime;

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, r);

            yield return null;
        }
    
        rect.anchoredPosition = endPos;
    }

     
}
