using System;
using UnityEngine;

namespace Western.Settings
{
    public enum SettingKey
    {
        Quality, LodBias, ShadowDistance, Shadows, TextureQuality, AntiAliasing,
        VSync, FrameLimit, Fullscreen, MasterVolume, MusicVolume, EffectsVolume,
        MouseSensitivity, InvertY, FieldOfView, ShowFps
    }

    [Serializable]
    public sealed class GameSettingsData
    {
        public int version = 1;
        public int quality;
        public float lodBias = 1f;
        public float shadowDistance = 40f;
        public int shadows = 2;
        public int textureQuality;
        public int antiAliasing;
        public bool vSync = true;
        public int frameLimit = 60;
        public bool fullscreen = true;
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float effectsVolume = 1f;
        public float mouseSensitivity = 1f;
        public bool invertY;
        public float fieldOfView = 60f;
        public bool showFps;

        public GameSettingsData Copy() => (GameSettingsData)MemberwiseClone();

        public static GameSettingsData CaptureDefaults()
        {
            return new GameSettingsData
            {
                quality = QualitySettings.GetQualityLevel(),
                lodBias = QualitySettings.lodBias,
                shadowDistance = QualitySettings.shadowDistance,
                shadows = (int)QualitySettings.shadows,
                textureQuality = QualitySettings.globalTextureMipmapLimit,
                antiAliasing = QualitySettings.antiAliasing,
                vSync = QualitySettings.vSyncCount > 0,
                fullscreen = Application.isEditor || Screen.fullScreen
            };
        }

        public void Sanitize(int qualityCount)
        {
            version = 1;
            quality = Mathf.Clamp(quality, 0, Mathf.Max(0, qualityCount - 1));
            lodBias = FiniteClamp(lodBias, .25f, 4f, 1f);
            shadowDistance = FiniteClamp(shadowDistance, 10f, 150f, 40f);
            shadows = Mathf.Clamp(shadows, 0, 2);
            textureQuality = Mathf.Clamp(textureQuality, 0, 2);
            if (antiAliasing != 0 && antiAliasing != 2 && antiAliasing != 4 && antiAliasing != 8)
                antiAliasing = 0;
            if (Array.IndexOf(GameSettings.FrameLimits, frameLimit) < 0) frameLimit = 60;
            masterVolume = FiniteClamp(masterVolume, 0f, 1f, 1f);
            musicVolume = FiniteClamp(musicVolume, 0f, 1f, 1f);
            effectsVolume = FiniteClamp(effectsVolume, 0f, 1f, 1f);
            mouseSensitivity = FiniteClamp(mouseSensitivity, .25f, 3f, 1f);
            fieldOfView = FiniteClamp(fieldOfView, 45f, 90f, 60f);
        }

        public static GameSettingsData Decode(string json, GameSettingsData defaults, int qualityCount)
        {
            var result = defaults.Copy();
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(json, result);
                    if (result.version != 1) result = defaults.Copy();
                }
                catch (ArgumentException) { result = defaults.Copy(); }
            }
            result.Sanitize(qualityCount);
            return result;
        }

        static float FiniteClamp(float value, float min, float max, float fallback)
            => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);
    }
}
