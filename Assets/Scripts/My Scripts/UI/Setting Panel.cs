using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAnimation : MonoBehaviour
{
    [Header("UI 引用")]

    public GameObject pausePanel;

    public Image backgroundImage;      // 拖入黑底背景

    public CanvasGroup uiContentGroup; // 拖入装有所有按钮的 UI_Content

    [Header("动画设置")]
    public float bgFadeDuration = 0.3f; // 背景淡入淡出时间

    public float uiFadeDuration = 0.3f; // UI淡入淡出时间

    public float pauseDuration = 0.2f;  // 两段动画之间的停顿时间

    public float targetBgAlpha = 1f;    // 背景目标透明度

    private Coroutine currentAnim;


    public void OpenSettings()
    {
        gameObject.SetActive(true); 

        if (pausePanel != null) pausePanel.SetActive(false);

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimationSequence(true));
    }

    public void CloseSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(true);

        if (currentAnim != null) StopCoroutine(currentAnim);

        currentAnim = StartCoroutine(AnimationSequence(false));
    }

    private IEnumerator AnimationSequence(bool isOpening)
    {
        uiContentGroup.interactable = false;

        if (isOpening)
        {
            SetBgAlpha(0f);
            uiContentGroup.alpha = 0f;

            yield return StartCoroutine(FadeBg(0f, targetBgAlpha, bgFadeDuration));
            
            yield return new WaitForSecondsRealtime(pauseDuration);
            
            yield return StartCoroutine(FadeUI(0f, 1f, uiFadeDuration));

            uiContentGroup.interactable = true;
        }
        else
        {
            
            yield return StartCoroutine(FadeUI(uiContentGroup.alpha, 0f, uiFadeDuration));
            
            yield return new WaitForSecondsRealtime(pauseDuration);
            
            yield return StartCoroutine(FadeBg(backgroundImage.color.a, 0f, bgFadeDuration));

            gameObject.SetActive(false);
        }
    }


    private IEnumerator FadeBg(float startA, float endA, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            SetBgAlpha(Mathf.Lerp(startA, endA, timer / duration));
            yield return null;
        }
        SetBgAlpha(endA);
    }

    private IEnumerator FadeUI(float startA, float endA, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            uiContentGroup.alpha = Mathf.Lerp(startA, endA, timer / duration);
            yield return null;
        }
        uiContentGroup.alpha = endA;
    }

    private void SetBgAlpha(float alpha)
    {
        Color c = backgroundImage.color;
        c.a = alpha;
        backgroundImage.color = c;
    }
}
