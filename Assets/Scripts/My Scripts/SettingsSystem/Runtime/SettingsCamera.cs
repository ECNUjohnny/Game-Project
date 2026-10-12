using UnityEngine;

namespace Western.Settings
{
    [RequireComponent(typeof(Camera))]
    public sealed class SettingsCamera : MonoBehaviour
    {
        Camera target;
        GameSettings settings;

        void OnEnable()
        {
            if (!Application.isPlaying) return;
            target = GetComponent<Camera>();
            settings = GameSettings.Instance;
            settings.Changed += Apply;
            Apply();
        }

        void OnDisable()
        {
            if (settings != null) settings.Changed -= Apply;
        }

        void Apply() { target.fieldOfView = settings.Data.fieldOfView; }
    }
}
