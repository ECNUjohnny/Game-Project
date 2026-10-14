#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Western.Settings;

public static class SettingsUISetup
{
    public const string PrefabPath = "Assets/UI/Settings/GlobalSettingsPanel.prefab";
    const string MixerPath = "Assets/Scripts/My Scripts/SettingsSystem/Resources/GameSettings/GlobalAudio.mixer";
    static readonly Color Ink = new Color(.065f, .077f, .076f);
    static readonly Color Card = new Color(.105f, .119f, .113f);
    static readonly Color Row = new Color(.135f, .149f, .140f);
    static readonly Color Gold = new Color(.76f, .55f, .32f);
    static readonly Color Cream = new Color(.92f, .89f, .81f);
    static readonly Color Muted = new Color(.62f, .67f, .62f);
    static TMP_FontAsset bodyFont, titleFont;
    static Sprite knob, arrow, check;
    static List<SettingsWidget> widgets;
    static AudioClip buttonClip;

    [MenuItem("Tools/Western/Settings/Rebuild settings UI in Begin and Game")]
    public static void Install()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save open scene changes before rebuilding settings.");

        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();
        var dependencies = AssetDatabase.GetDependencies(new[] { "Assets/Scenes/Begin.unity", "Assets/Scenes/Game.unity" }, true);
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        if (mixer == null) throw new InvalidOperationException("GlobalAudio mixer is missing.");
        var music = mixer.FindMatchingGroups("Music").Single(g => g.name == "Music");
        var effects = mixer.FindMatchingGroups("SFX").Single(g => g.name == "SFX");

        bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Font/chinese rocks rg SDF.asset");
        titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Font/chinese rocks rg SDF.asset");
        knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        arrow = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        check = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        var sample = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(a => a.clip != null && a.clip.name == "button");
        buttonClip = sample != null ? sample.clip : AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath("d8b8ab4b1ab1fc349bcb5d9b19845130"));
        const string defaultsPath = "Assets/Scripts/My Scripts/SettingsSystem/Resources/GameSettings/Defaults.asset";
        if (AssetDatabase.LoadAssetAtPath<GameSettingsDefaults>(defaultsPath) == null)
        {
            var defaultsAsset = ScriptableObject.CreateInstance<GameSettingsDefaults>();
            defaultsAsset.values = GameSettingsData.CaptureDefaults();
            AssetDatabase.CreateAsset(defaultsAsset, defaultsPath);
        }
        BuildPrefab(effects);
        int prefabAudioCount = 0;
        try
        {
            foreach (var path in dependencies.Where(p => p.EndsWith(".prefab")))
            {
                var original = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (original == null || !original.GetComponentsInChildren<AudioSource>(true).Any(a => a.outputAudioMixerGroup == null)) continue;
                Backup(path);
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var source in contents.GetComponentsInChildren<AudioSource>(true))
                    {
                        if (source.outputAudioMixerGroup != null) continue;
                        source.outputAudioMixerGroup = source.GetComponent<RandomMusicPlayer>() != null ? music : effects;
                        prefabAudioCount++;
                    }
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }

            foreach (var path in new[] { "Assets/Scenes/Begin.unity", "Assets/Scenes/Game.unity" })
            {
                Backup(path);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var panel = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SettingsAnimation>(true)).Single();
                var oldContent = panel.transform.Find("UI");
                if (oldContent != null) UnityEngine.Object.DestroyImmediate(oldContent.gameObject);
                var content = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), panel.transform);
                content.name = "UI";
                Stretch((RectTransform)content.transform);
                var view = content.GetComponent<SettingsPanelView>();
                panel.backgroundImage = panel.GetComponent<Image>();
                panel.backgroundImage.color = new Color(.027f, .035f, .031f, 1f);
                panel.backgroundImage.raycastTarget = true;
                panel.uiContentGroup = content.GetComponent<CanvasGroup>();
                panel.pausePanel = scene.name == "Game" ? panel.transform.parent.Find("Pause Panel").gameObject : null;
                panel.bgFadeDuration = .18f;
                panel.uiFadeDuration = .18f;
                panel.pauseDuration = .04f;
                panel.targetBgAlpha = 1f;
                Stretch((RectTransform)panel.transform);
                panel.gameObject.SetActive(false);
                panel.transform.SetAsLastSibling();
                UnityEventTools.AddPersistentListener(view.backButton.onClick, panel.CloseSettings);
                PrefabUtility.RecordPrefabInstancePropertyModifications(view.backButton);

                foreach (var source in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<AudioSource>(true)))
                    if (source.outputAudioMixerGroup == null)
                    {
                        source.outputAudioMixerGroup = source.GetComponent<RandomMusicPlayer>() != null ? music : effects;
                        if (PrefabUtility.IsPartOfPrefabInstance(source))
                            PrefabUtility.RecordPrefabInstancePropertyModifications(source);
                    }

                if (scene.name == "Game")
                {
                    var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<CameraMovement>(true)).Single();
                    if (camera.GetComponent<SettingsCamera>() == null) camera.gameObject.AddComponent<SettingsCamera>();
                    var pause = panel.pausePanel.GetComponent<PausePanel>();
                    if (pause != null) pause.setting = panel.gameObject;
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(sceneSetup); }
        AssetDatabase.SaveAssets();
        Debug.Log("Settings UI installed in Begin and Game. Routed prefab audio sources: " + prefabAudioCount);
    }

    static void Backup(string path)
    {
        string destination = ".utmp/settings-system/before/" + path;
        if (File.Exists(destination)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.Copy(path, destination);
    }

    static void BuildPrefab(AudioMixerGroup effects)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
        widgets = new List<SettingsWidget>();
        var root = new GameObject("Global Settings Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(SettingsPanelView));
        try
        {
            Stretch((RectTransform)root.transform);
            var view = root.GetComponent<SettingsPanelView>();
            var frame = Box(root.transform, "Frame", Ink, 0, 0, 1440, 870);
            var f = (RectTransform)frame.transform;
            f.anchorMin = f.anchorMax = f.pivot = new Vector2(.5f, .5f);
            f.anchoredPosition = Vector2.zero;
            Box(frame.transform, "Top Accent", Gold, 0, 0, 1440, 3);
            Label(frame.transform, "Heading", "SETTINGS", 46, 22, 780, 72, 58, Cream, true);
            Label(frame.transform, "Subtitle", "VIDEO, SOUND & CONTROLS", 49, 94, 850, 30, 19, Muted);
            Label(frame.transform, "Menu Hint", "ESC  /  BACK", 1130, 61, 262, 28, 18, Muted, false, TextAlignmentOptions.Right);
            Box(frame.transform, "Header Line", Row, 46, 138, 1348, 1);
            Box(frame.transform, "Sidebar Line", Row, 326, 166, 1, 573);

            view.tabButtons = new Button[3];
            string[] tabs = { "01   GRAPHICS", "02   AUDIO", "03   CONTROLS" };
            for (int i = 0; i < tabs.Length; i++)
                view.tabButtons[i] = MakeButton(frame.transform, "Tab " + i, tabs[i], 46, 174 + 84 * i, 254, 65, effects, false);
            Label(frame.transform, "Scope Hint", "Changes are saved\nfor your next session.", 55, 597, 235, 106, 20, Muted);

            var body = Box(frame.transform, "Body", Card, 355, 168, 1039, 570);
            view.pageTitle = Label(body.transform, "Page Title", "DISPLAY & GRAPHICS", 25, 18, 980, 36, 27, Cream);
            view.pageDescription = Label(body.transform, "Page Description", "Balance distant detail, image quality and performance.", 25, 60, 980, 26, 18, Muted);

            var scrollObject = new GameObject("Settings Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(body.transform, false);
            Place((RectTransform)scrollObject.transform, 14, 103, 1010, 452);
            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 38;
            var viewport = Box(scrollObject.transform, "Viewport", Color.clear, 0, 0, 990, 452);
            viewport.GetComponent<Image>().raycastTarget = true;
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = (RectTransform)viewport.transform;

            var scrollbarObject = DefaultControls.CreateScrollbar(new DefaultControls.Resources { standard = knob });
            scrollbarObject.name = "Scrollbar";
            scrollbarObject.transform.SetParent(scrollObject.transform, false);
            Place((RectTransform)scrollbarObject.transform, 998, 0, 7, 452);
            var bar = scrollbarObject.GetComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;
            bar.GetComponent<Image>().color = Ink;
            bar.handleRect.GetComponent<Image>().color = Gold;
            bar.handleRect.GetComponent<Image>().sprite = null;
            bar.GetComponent<Image>().sprite = null;
            bar.size = .5f;
            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            view.scroll = scroll;
            view.pages = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var page = new GameObject(new[] { "Graphics Page", "Audio Page", "Controls Page" }[i], typeof(RectTransform));
                page.transform.SetParent(viewport.transform, false);
                Place((RectTransform)page.transform, 0, 0, 990, i == 0 ? 9 * 88 : i == 1 ? 3 * 88 : 4 * 88);
                view.pages[i] = page;
            }
            Transform graphics = view.pages[0].transform;
            Drop(graphics, 0, SettingKey.Quality, "Quality preset", "Loads a preset; fine-tune individual settings below.", QualitySettings.names);
            Slide(graphics, 1, SettingKey.LodBias, "LOD detail", "Higher values keep detailed models visible farther away.", .25f, 4f);
            Drop(graphics, 2, SettingKey.TextureQuality, "Texture detail", "Lower detail uses less texture memory.", new[] { "Full resolution", "Half resolution", "Quarter resolution" });
            Drop(graphics, 3, SettingKey.Shadows, "Shadows", "Soft shadows look smoother but cost more to render.", new[] { "Off", "Hard shadows", "Soft shadows" });
            Slide(graphics, 4, SettingKey.ShadowDistance, "Shadow distance", "How far from the camera shadows remain visible.", 10, 150, true);
            Drop(graphics, 5, SettingKey.AntiAliasing, "Anti-aliasing", "Smooths jagged edges. Higher levels use more GPU power.", new[] { "Off", "2x MSAA", "4x MSAA", "8x MSAA" }, GameSettings.AntiAliasingOptions);
            Switch(graphics, 6, SettingKey.VSync, "VSync", "Matches display refresh rate to reduce screen tearing.");
            Drop(graphics, 7, SettingKey.FrameLimit, "Frame rate limit", "Controlled by VSync. Turn VSync off to set a limit.", new[] { "30 FPS", "60 FPS", "90 FPS", "120 FPS", "144 FPS", "Unlimited" }, GameSettings.FrameLimits);
            Switch(graphics, 8, SettingKey.Fullscreen, "Borderless fullscreen", "Switch between fullscreen and windowed mode in the build.");

            Transform audio = view.pages[1].transform;
            Slide(audio, 0, SettingKey.MasterVolume, "Master volume", "Overall volume for music, gameplay and interface sounds.", 0, 1);
            Slide(audio, 1, SettingKey.MusicVolume, "Music volume", "Background music in the main menu and music sources.", 0, 1);
            Slide(audio, 2, SettingKey.EffectsVolume, "Effects volume", "Footsteps, gunshots, Dead Eye and interface sounds.", 0, 1);

            Transform controls = view.pages[2].transform;
            Slide(controls, 0, SettingKey.MouseSensitivity, "Mouse sensitivity", "Multiplies your existing horizontal and vertical look speed.", .25f, 3f);
            Switch(controls, 1, SettingKey.InvertY, "Invert vertical look", "Reverses the mouse direction for looking up and down.");
            Slide(controls, 2, SettingKey.FieldOfView, "Field of view", "A wider camera angle shows more of your surroundings.", 45, 90, true);
            Switch(controls, 3, SettingKey.ShowFps, "Show frame rate", "Displays a small FPS counter in the top-right corner.");

            Box(frame.transform, "Footer Line", Row, 46, 763, 1348, 1);
            view.saveStatus = Label(frame.transform, "Save Status", "All changes saved automatically", 47, 796, 635, 29, 19, Muted);
            view.resetButton = MakeButton(frame.transform, "Restore Defaults", "RESTORE DEFAULTS", 788, 788, 293, 51, effects, false);
            view.resetLabel = view.resetButton.GetComponentInChildren<TMP_Text>();
            view.backButton = MakeButton(frame.transform, "Back", "BACK", 1110, 788, 284, 51, effects, true);
            view.widgets = widgets.ToArray();
            view.SelectTab(0);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    static GameObject Box(Transform parent, string name, Color color, float x, float y, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, x, y, width, height);
        go.GetComponent<Image>().color = color;
        go.GetComponent<Image>().raycastTarget = false;
        return go;
    }

    static TMP_Text Label(Transform parent, string name, string text, float x, float y, float width, float height, float size, Color color, bool title = false, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        Place((RectTransform)go.transform, x, y, width, height);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = title ? titleFont : bodyFont;
        label.fontSize = size;
        label.text = text;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.enableWordWrapping = true;
        return label;
    }

    static Button MakeButton(Transform parent, string name, string text, float x, float y, float width, float height, AudioMixerGroup effects, bool primary)
    {
        var go = Box(parent, name, primary ? Gold : Row, x, y, width, height);
        go.GetComponent<Image>().raycastTarget = true;
        var b = go.AddComponent<Button>();
        b.targetGraphic = go.GetComponent<Image>();
        var colors = b.colors;
        colors.highlightedColor = new Color(1.25f, 1.2f, 1.12f);
        colors.pressedColor = new Color(.75f, .75f, .75f);
        colors.selectedColor = new Color(1.15f, 1.1f, 1.05f);
        b.colors = colors;
        Label(go.transform, "Label", text, 14, 0, width - 28, height, 22, primary ? Ink : Cream, false, TextAlignmentOptions.Center);
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = buttonClip;
        source.outputAudioMixerGroup = effects;
        UnityEventTools.AddPersistentListener(b.onClick, source.Play);
        return b;
    }

    static SettingsWidget BaseRow(Transform page, int index, SettingKey key, string title, string hint, out Transform control)
    {
        var row = Box(page, key.ToString(), Row, 0, index * 88, 990, 80);
        row.GetComponent<Image>().raycastTarget = true;
        Label(row.transform, "Label", title, 20, 11, 553, 31, 24, Cream);
        var hintText = Label(row.transform, "Hint", hint, 20, 45, 563, 26, 16, Muted);
        var area = new GameObject("Control", typeof(RectTransform), typeof(CanvasGroup));
        area.transform.SetParent(row.transform, false);
        Place((RectTransform)area.transform, 618, 20, 349, 40);
        control = area.transform;
        var widget = new SettingsWidget { key = key, hintText = hintText, controlGroup = area.GetComponent<CanvasGroup>() };
        widgets.Add(widget);
        return widget;
    }

    static void Slide(Transform page, int index, SettingKey key, string title, string hint, float min, float max, bool whole = false)
    {
        var widget = BaseRow(page, index, key, title, hint, out var control);
        var go = DefaultControls.CreateSlider(new DefaultControls.Resources { knob = knob });
        go.name = "Slider";
        go.transform.SetParent(control, false);
        Place((RectTransform)go.transform, 0, 0, 245, 40);
        var slider = go.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = whole;
        slider.fillRect.GetComponent<Image>().color = Gold;
        slider.handleRect.GetComponent<Image>().color = Cream;
        var track = (RectTransform)go.transform.Find("Background");
        track.GetComponent<Image>().color = Ink;
        track.anchorMin = new Vector2(0, .5f);
        track.anchorMax = new Vector2(1, .5f);
        track.sizeDelta = new Vector2(0, 6);
        var fillArea = (RectTransform)slider.fillRect.parent;
        fillArea.anchorMin = new Vector2(0, .5f);
        fillArea.anchorMax = new Vector2(1, .5f);
        fillArea.sizeDelta = new Vector2(-20, 6);
        slider.fillRect.offsetMin = slider.fillRect.offsetMax = Vector2.zero;
        var handleArea = (RectTransform)slider.handleRect.parent;
        handleArea.anchorMin = new Vector2(0, .5f);
        handleArea.anchorMax = new Vector2(1, .5f);
        handleArea.sizeDelta = new Vector2(-20, 0);
        slider.handleRect.sizeDelta = new Vector2(18, 24);
        widget.slider = slider;
        widget.valueText = Label(control, "Value", "", 253, 0, 96, 40, 22, Cream, false, TextAlignmentOptions.Right);
    }

    static void Switch(Transform page, int index, SettingKey key, string title, string hint)
    {
        var widget = BaseRow(page, index, key, title, hint, out var control);
        var hit = control.gameObject.AddComponent<Image>();
        hit.color = Color.clear;
        var toggle = control.gameObject.AddComponent<Toggle>();
        var box = Box(control, "Checkbox", Ink, 234, 4, 32, 32);
        box.GetComponent<Image>().raycastTarget = true;
        var mark = Box(box.transform, "Check", Gold, 4, 4, 24, 24);
        mark.GetComponent<Image>().sprite = check;
        toggle.targetGraphic = box.GetComponent<Image>();
        toggle.graphic = mark.GetComponent<Image>();
        widget.toggle = toggle;
        widget.valueText = Label(control, "Value", "OFF", 277, 0, 72, 40, 22, Cream, false, TextAlignmentOptions.Right);
    }

    static void Drop(Transform page, int index, SettingKey key, string title, string hint, string[] options, int[] values = null)
    {
        var widget = BaseRow(page, index, key, title, hint, out var control);
        var go = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources { dropdown = arrow, checkmark = check });
        go.name = "Dropdown";
        go.transform.SetParent(control, false);
        Place((RectTransform)go.transform, 0, 0, 349, 40);
        var dropdown = go.GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        foreach (var label in go.GetComponentsInChildren<TMP_Text>(true))
        {
            label.font = bodyFont;
            label.fontSize = 20;
            label.color = Cream;
            label.enableWordWrapping = false;
        }
        go.GetComponent<Image>().color = Ink;
        dropdown.template.GetComponent<Image>().color = Ink;
        dropdown.template.sizeDelta = new Vector2(0, 200);
        var item = dropdown.template.GetComponentInChildren<Toggle>(true);
        ((RectTransform)item.transform).sizeDelta = new Vector2(0, 38);
        ((RectTransform)item.transform.parent).sizeDelta = new Vector2(0, 40);
        item.targetGraphic.color = Row;
        item.graphic.color = Gold;
        foreach (var scrollbar in dropdown.template.GetComponentsInChildren<Scrollbar>(true))
        {
            scrollbar.GetComponent<Image>().color = Ink;
            scrollbar.GetComponent<Image>().sprite = null;
            scrollbar.handleRect.GetComponent<Image>().color = Gold;
            scrollbar.handleRect.GetComponent<Image>().sprite = null;
            ((RectTransform)scrollbar.transform).sizeDelta = new Vector2(7, 0);
        }
        dropdown.template.gameObject.SetActive(false);
        widget.dropdown = dropdown;
        widget.optionValues = values;
    }

    static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
#endif
