using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsAnimation : MonoBehaviour
{
    public static SettingsAnimation Active { get; private set; }
    public static int LastEscapeFrame { get; private set; } = -1;

    [Header("UI References")]
    public GameObject pausePanel;
    public Image backgroundImage;
    public CanvasGroup uiContentGroup;

    [Header("Animation")]
    public float bgFadeDuration = .2f;
    public float uiFadeDuration = .2f;
    public float pauseDuration = .05f;
    public float targetBgAlpha = 1f;

    Coroutine currentAnim;
    bool closing;
    bool restorePause;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { Active = null; LastEscapeFrame = -1; }

    public void OpenSettings()
    {
        if (Active == this && !closing) return;
        gameObject.SetActive(true);
        Active = this;
        closing = false;
        restorePause = pausePanel != null && pausePanel.activeSelf;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimationSequence(true));
    }

    public void CloseSettings()
    {
        if (closing || !gameObject.activeSelf) return;
        closing = true;
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(AnimationSequence(false));
    }

    public void HandleEscape()
    {
        if (LastEscapeFrame == Time.frameCount) return;
        LastEscapeFrame = Time.frameCount;
        foreach (var dropdown in GetComponentsInChildren<TMP_Dropdown>())
        {
            if (!dropdown.IsExpanded) continue;
            dropdown.Hide();
            return;
        }
        CloseSettings();
    }

    public void CloseImmediately()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        FinishClose();
    }

    void Update()
    {
        // The gameplay UI manager owns Escape in Game; the start menu has no UI manager.
        if (Active == this && pausePanel == null && Input.GetKeyDown(KeyCode.Escape))
            HandleEscape();
    }

    IEnumerator AnimationSequence(bool opening)
    {
        uiContentGroup.interactable = false;
        uiContentGroup.blocksRaycasts = true;
        if (opening)
        {
            SetBgAlpha(0f);
            uiContentGroup.alpha = 0f;
            yield return FadeBg(0f, targetBgAlpha, bgFadeDuration);
            yield return new WaitForSecondsRealtime(pauseDuration);
            yield return FadeUI(0f, 1f, uiFadeDuration);
            uiContentGroup.interactable = true;
            var view = GetComponentInChildren<Western.Settings.SettingsPanelView>();
            if (view != null && view.backButton != null) view.backButton.Select();
            currentAnim = null;
        }
        else
        {
            yield return FadeUI(uiContentGroup.alpha, 0f, uiFadeDuration);
            yield return FadeBg(backgroundImage.color.a, 0f, bgFadeDuration);
            FinishClose();
        }
    }

    void FinishClose()
    {
        currentAnim = null;
        closing = false;
        if (restorePause && pausePanel != null) pausePanel.SetActive(true);
        if (Active == this) Active = null;
        gameObject.SetActive(false);
    }

    IEnumerator FadeBg(float from, float to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            SetBgAlpha(Mathf.Lerp(from, to, time / duration));
            yield return null;
        }
        SetBgAlpha(to);
    }

    IEnumerator FadeUI(float from, float to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            uiContentGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        uiContentGroup.alpha = to;
    }

    void SetBgAlpha(float alpha)
    {
        Color color = backgroundImage.color;
        color.a = alpha;
        backgroundImage.color = color;
    }

    void OnDisable()
    {
        if (Active == this) Active = null;
        currentAnim = null;
        closing = false;
    }
}
