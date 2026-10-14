using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Western.Settings
{
    [Serializable]
    public sealed class SettingsWidget // can't be inherited by other classes
    {
        public SettingKey key;
        public Slider slider;
        public TMP_Dropdown dropdown;
        public Toggle toggle;
        public TMP_Text valueText;
        public TMP_Text hintText;
        public CanvasGroup controlGroup;
        public int[] optionValues;
    }

    public sealed class SettingsPanelView : MonoBehaviour
    {
        public SettingsWidget[] widgets;
        public Button[] tabButtons;
        public GameObject[] pages;
        public ScrollRect scroll;
        public TMP_Text pageTitle;
        public TMP_Text pageDescription;
        public TMP_Text saveStatus;
        public Button resetButton;
        public TMP_Text resetLabel;
        public Button backButton;
        public Color activeTabColor = new Color(.29f, .23f, .17f);
        public Color inactiveTabColor = new Color(.09f, .10f, .10f);

        GameSettings settings;
        bool bound;
        bool confirmReset;
        float resetDeadline;
        int activeTab;

        void Awake() { Bind(); }

        void OnEnable()
        {
            if (!Application.isPlaying) return;
            Bind();
            settings = GameSettings.Instance;
            settings.Changed += Refresh;
            settings.Saved += Refresh;
            confirmReset = false;
            SelectTab(activeTab);
            Refresh();
        }

        void Bind()
        {
            if (bound) return;
            bound = true;
            foreach (var widget in widgets)
            {
                var w = widget;
                if (w.slider != null) w.slider.onValueChanged.AddListener(v => Change(w.key, v));
                if (w.toggle != null) w.toggle.onValueChanged.AddListener(v => Change(w.key, v ? 1 : 0));
                if (w.dropdown != null)
                {
                    if (w.key == SettingKey.Quality)
                    {
                        w.dropdown.ClearOptions();
                        w.dropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                    }
                    w.dropdown.onValueChanged.AddListener(i =>
                        Change(w.key, w.optionValues != null && w.optionValues.Length > i ? w.optionValues[i] : i));
                }
            }
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int tab = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(tab));
            }
            resetButton.onClick.AddListener(ResetClicked);
        }

        void Change(SettingKey key, float value)
        {
            if (settings != null) settings.SetValue(key, value);
        }

        public void SelectTab(int index)
        {
            activeTab = Mathf.Clamp(index, 0, pages.Length - 1);
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetActive(i == activeTab);
                tabButtons[i].targetGraphic.color = i == activeTab ? activeTabColor : inactiveTabColor;
            }
            scroll.content = (RectTransform)pages[activeTab].transform;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
            pageTitle.text = new[] { "DISPLAY & GRAPHICS", "AUDIO", "CONTROLS & CAMERA" }[activeTab];
            pageDescription.text = new[] {
                "Balance distant detail, image quality and performance.",
                "Set the balance between music and the sounds around you.",
                "Make the camera and mouse feel right for you."
            }[activeTab];
        }

        void Refresh()
        {
            if (settings == null) return;
            foreach (var w in widgets)
            {
                float value = settings.GetValue(w.key);
                if (w.slider != null) w.slider.SetValueWithoutNotify(value);
                if (w.toggle != null) w.toggle.SetIsOnWithoutNotify(value > .5f);
                if (w.dropdown != null)
                {
                    int index = w.optionValues != null && w.optionValues.Length > 0
                        ? Array.IndexOf(w.optionValues, Mathf.RoundToInt(value)) : Mathf.RoundToInt(value);
                    w.dropdown.SetValueWithoutNotify(Mathf.Max(0, index));
                    w.dropdown.RefreshShownValue();
                }
                if (w.valueText != null) w.valueText.text = Format(w.key, value);
                bool interactable = !(w.key == SettingKey.FrameLimit && settings.Data.vSync)
                    && !(w.key == SettingKey.ShadowDistance && settings.Data.shadows == 0);
                if (w.controlGroup != null)
                {
                    w.controlGroup.interactable = interactable;
                    w.controlGroup.alpha = interactable ? 1f : .4f;
                }
                if (w.key == SettingKey.FrameLimit && w.hintText != null)
                    w.hintText.text = settings.Data.vSync
                        ? "Controlled by VSync. Turn VSync off to set a limit."
                        : "A lower limit can reduce heat and power use.";
            }
            saveStatus.text = settings.HasUnsavedChanges ? "Saving changes..." : "All changes saved automatically";
            resetLabel.text = confirmReset ? "CONFIRM RESET" : "RESTORE DEFAULTS";
        }

        public static string Format(SettingKey key, float value)
        {
            switch (key)
            {
                case SettingKey.MasterVolume:
                case SettingKey.MusicVolume:
                case SettingKey.EffectsVolume: return Mathf.RoundToInt(value * 100f) + "%";
                case SettingKey.LodBias:
                case SettingKey.MouseSensitivity: return value.ToString("0.00") + "x";
                case SettingKey.FieldOfView: return Mathf.RoundToInt(value) + " deg";
                case SettingKey.ShadowDistance: return Mathf.RoundToInt(value) + " m";
                default: return value > .5f ? "ON" : "OFF";
            }
        }

        void ResetClicked()
        {
            if (!confirmReset)
            {
                confirmReset = true;
                resetDeadline = Time.unscaledTime + 4f;
                Refresh();
                return;
            }
            confirmReset = false;
            settings.RestoreDefaults();
        }

        void Update()
        {
            if (confirmReset && Time.unscaledTime > resetDeadline)
            {
                confirmReset = false;
                Refresh();
            }
        }

        void OnDisable()
        {
            if (settings == null) return;
            settings.Changed -= Refresh;
            settings.Saved -= Refresh;
            settings.Save();
            foreach (var w in widgets)
                if (w.dropdown != null) w.dropdown.Hide();
        }
    }
}
