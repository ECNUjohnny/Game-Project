using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Western.Settings
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameSettings : MonoBehaviour
    {
        public const string PreferenceKey = "Western.Settings.v1";
        public static readonly int[] FrameLimits = { 30, 60, 90, 120, 144, -1 };
        public static readonly int[] AntiAliasingOptions = { 0, 2, 4, 8 };
        static GameSettings instance;
        public static GameSettings Instance
        {
            get
            {
                if (instance == null && Application.isPlaying)
                    new GameObject("Global Game Settings").AddComponent<GameSettings>();
                return instance;
            }
        }

        public GameSettingsData Data { get; private set; }
        public bool HasUnsavedChanges { get; private set; }
        public event Action Changed;
        public event Action Saved;

        GameSettingsData defaults;
        AudioMixer mixer;
        bool audioReady;
        float saveAt;
        float fpsTime;
        int fpsFrames;
        string fpsText = "";
        GUIStyle fpsStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { instance = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap() { _ = Instance; }

        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            var defaultAsset = Resources.Load<GameSettingsDefaults>("GameSettings/Defaults");
            defaults = defaultAsset != null ? defaultAsset.values.Copy() : GameSettingsData.CaptureDefaults();
            defaults.Sanitize(QualitySettings.names.Length);
            Data = GameSettingsData.Decode(PlayerPrefs.GetString(PreferenceKey, ""), defaults, QualitySettings.names.Length);
            mixer = Resources.Load<AudioMixer>("GameSettings/GlobalAudio");
            SceneManager.sceneLoaded += SceneLoaded;
            ApplyGraphics();
            ApplyDisplay();
        }

        void Start()
        {
            // AudioMixer.SetFloat must run after Awake / OnEnable.
            audioReady = true;
            ApplyAudio();
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // AudioSources in scenes and spawnable prefabs already reference the mixer.
            if (audioReady) ApplyAudio();
        }

        void Update()
        {
            if (HasUnsavedChanges && Time.unscaledTime >= saveAt) Save();
            if (!Data.showFps) return;
            fpsTime += Time.unscaledDeltaTime;
            fpsFrames++;
            if (fpsTime >= .5f)
            {
                fpsText = Mathf.RoundToInt(fpsFrames / fpsTime) + " FPS";
                fpsTime = 0f;
                fpsFrames = 0;
            }
        }

        void OnGUI()
        {
            if (Data == null || !Data.showFps) return;
            if (fpsStyle == null)
                fpsStyle = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(Screen.width - 126, 12, 114, 32), fpsText, fpsStyle);
        }

        public void SetValue(SettingKey key, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return;
            switch (key)
            {
                case SettingKey.Quality:
                    Data.quality = Mathf.Clamp(Mathf.RoundToInt(value), 0, QualitySettings.names.Length - 1);
                    QualitySettings.SetQualityLevel(Data.quality, true);
                    // A preset replaces only graphics overrides. Audio, input and display preferences stay independent.
                    Data.lodBias = QualitySettings.lodBias;
                    Data.shadowDistance = QualitySettings.shadowDistance;
                    Data.shadows = (int)QualitySettings.shadows;
                    Data.textureQuality = QualitySettings.globalTextureMipmapLimit;
                    Data.antiAliasing = QualitySettings.antiAliasing;
                    break;
                case SettingKey.LodBias: Data.lodBias = value; break;
                case SettingKey.ShadowDistance: Data.shadowDistance = value; break;
                case SettingKey.Shadows: Data.shadows = Mathf.RoundToInt(value); break;
                case SettingKey.TextureQuality: Data.textureQuality = Mathf.RoundToInt(value); break;
                case SettingKey.AntiAliasing: Data.antiAliasing = Mathf.RoundToInt(value); break;
                case SettingKey.VSync: Data.vSync = value > .5f; break;
                case SettingKey.FrameLimit: Data.frameLimit = Mathf.RoundToInt(value); break;
                case SettingKey.Fullscreen: Data.fullscreen = value > .5f; break;
                case SettingKey.MasterVolume: Data.masterVolume = value; break;
                case SettingKey.MusicVolume: Data.musicVolume = value; break;
                case SettingKey.EffectsVolume: Data.effectsVolume = value; break;
                case SettingKey.MouseSensitivity: Data.mouseSensitivity = value; break;
                case SettingKey.InvertY: Data.invertY = value > .5f; break;
                case SettingKey.FieldOfView: Data.fieldOfView = value; break;
                case SettingKey.ShowFps: Data.showFps = value > .5f; break;
                default: return;
            }
            Data.Sanitize(QualitySettings.names.Length);
            switch (key)
            {
                case SettingKey.Quality: ApplyGraphicsOverrides(); ApplyFrameTiming(); break;
                case SettingKey.LodBias: QualitySettings.lodBias = Data.lodBias; break;
                case SettingKey.ShadowDistance: QualitySettings.shadowDistance = Data.shadowDistance; break;
                case SettingKey.Shadows: QualitySettings.shadows = (ShadowQuality)Data.shadows; break;
                case SettingKey.TextureQuality: QualitySettings.globalTextureMipmapLimit = Data.textureQuality; break;
                case SettingKey.AntiAliasing: QualitySettings.antiAliasing = Data.antiAliasing; break;
                case SettingKey.VSync:
                case SettingKey.FrameLimit: ApplyFrameTiming(); break;
                case SettingKey.Fullscreen: ApplyFullscreen(); break;
                case SettingKey.MasterVolume:
                case SettingKey.MusicVolume:
                case SettingKey.EffectsVolume: if (audioReady) ApplyAudio(); break;
            }
            NotifyChanged();
        }

        public float GetValue(SettingKey key)
        {
            switch (key)
            {
                case SettingKey.Quality: return Data.quality;
                case SettingKey.LodBias: return Data.lodBias;
                case SettingKey.ShadowDistance: return Data.shadowDistance;
                case SettingKey.Shadows: return Data.shadows;
                case SettingKey.TextureQuality: return Data.textureQuality;
                case SettingKey.AntiAliasing: return Data.antiAliasing;
                case SettingKey.VSync: return Data.vSync ? 1 : 0;
                case SettingKey.FrameLimit: return Data.frameLimit;
                case SettingKey.Fullscreen: return Data.fullscreen ? 1 : 0;
                case SettingKey.MasterVolume: return Data.masterVolume;
                case SettingKey.MusicVolume: return Data.musicVolume;
                case SettingKey.EffectsVolume: return Data.effectsVolume;
                case SettingKey.MouseSensitivity: return Data.mouseSensitivity;
                case SettingKey.InvertY: return Data.invertY ? 1 : 0;
                case SettingKey.FieldOfView: return Data.fieldOfView;
                case SettingKey.ShowFps: return Data.showFps ? 1 : 0;
                default: return 0;
            }
        }

        public void RestoreDefaults()
        {
            Data = defaults.Copy();
            ApplyGraphics();
            ApplyDisplay();
            if (audioReady) ApplyAudio();
            NotifyChanged();
            Save();
        }

        void NotifyChanged()
        {
            HasUnsavedChanges = true;
            saveAt = Time.unscaledTime + .5f;
            Changed?.Invoke();
        }

        public void Save()
        {
            if (!HasUnsavedChanges || Data == null) return;
            PlayerPrefs.SetString(PreferenceKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            HasUnsavedChanges = false;
            Saved?.Invoke();
        }

        public static float LinearToDecibels(float value)
            => value <= .0001f ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(value));

        void ApplyGraphics()
        {
            QualitySettings.SetQualityLevel(Data.quality, true);
            ApplyGraphicsOverrides();
        }

        void ApplyGraphicsOverrides()
        {
            QualitySettings.lodBias = Data.lodBias;
            QualitySettings.shadowDistance = Data.shadowDistance;
            QualitySettings.shadows = (ShadowQuality)Data.shadows;
            QualitySettings.globalTextureMipmapLimit = Data.textureQuality;
            QualitySettings.antiAliasing = Data.antiAliasing;
        }

        void ApplyDisplay() { ApplyFrameTiming(); ApplyFullscreen(); }
        void ApplyFrameTiming()
        {
            QualitySettings.vSyncCount = Data.vSync ? 1 : 0;
            Application.targetFrameRate = Data.vSync ? -1 : Data.frameLimit;
        }

        void ApplyFullscreen()
        {
            if (!Application.isEditor)
                Screen.fullScreenMode = Data.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }

        void ApplyAudio()
        {
            if (mixer == null) return;
            mixer.SetFloat("MasterVolume", LinearToDecibels(Data.masterVolume));
            mixer.SetFloat("MusicVolume", LinearToDecibels(Data.musicVolume));
            mixer.SetFloat("EffectsVolume", LinearToDecibels(Data.effectsVolume));
        }

        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnApplicationQuit() { Save(); }
        void OnDestroy()
        {
            if (instance != this) return;
            Save();
            SceneManager.sceneLoaded -= SceneLoaded;
            instance = null;
        }
    }
}
