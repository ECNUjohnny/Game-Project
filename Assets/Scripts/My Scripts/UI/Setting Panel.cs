using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [Header("UI")]


    public CanvasGroup group;

    public Image background;

    public float uiFadeDuration = 0.5f;

    public float bgFadeDuration = 0.5f;

    void Awake()
    {
        Color backgroundColor = background.color;

        backgroundColor.a = 0;

        background.color = backgroundColor;        
    } 
 

    public void OnBackClick()
    {
        StartCoroutine(UIFadeInOut(0));
    }

    private IEnumerator UIFadeInOut(float tgAlpha)
    {
        float elpased = 0f;
        Color bgColor = background.color;


        while (elpased < bgFadeDuration)
        {
            elpased += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(bgColor.a, tgAlpha, elpased / bgFadeDuration);

            bgColor.a = alpha;

            background.color = bgColor;

            yield return null;
        }

        bgColor.a = tgAlpha;
        background.color = bgColor;

        elpased = 0;

        while (elpased < uiFadeDuration)
        {
            elpased += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(group.alpha, tgAlpha, elpased / bgFadeDuration);

            group.alpha = alpha;

            yield return null;
        }
        
        group.alpha = tgAlpha;

        if (tgAlpha == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
