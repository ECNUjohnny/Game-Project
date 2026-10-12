using UnityEngine;

namespace Western.Settings
{
    [CreateAssetMenu(menuName = "Western/Settings Defaults", fileName = "Defaults")]
    public sealed class GameSettingsDefaults : ScriptableObject
    {
        public GameSettingsData values = new GameSettingsData();
    }
}
