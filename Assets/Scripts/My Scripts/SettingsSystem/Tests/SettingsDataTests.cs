using NUnit.Framework;
using UnityEngine;

namespace Western.Settings.Tests
{
    public class SettingsDataTests
    {
        [Test]
        public void PartialSaveKeepsDefaultsForNewFields()
        {
            var defaults = new GameSettingsData { quality = 3, fieldOfView = 67f, mouseSensitivity = 1.25f };
            var loaded = GameSettingsData.Decode("{\"masterVolume\":0.3}", defaults, 6);
            Assert.That(loaded.masterVolume, Is.EqualTo(.3f).Within(.001));
            Assert.That(loaded.fieldOfView, Is.EqualTo(67f));
            Assert.That(loaded.mouseSensitivity, Is.EqualTo(1.25f));
            Assert.That(defaults.masterVolume, Is.EqualTo(1f));
        }

        [Test]
        public void CorruptSaveFallsBackWithoutChangingDefaults()
        {
            var defaults = new GameSettingsData { quality = 2, musicVolume = .7f };
            var loaded = GameSettingsData.Decode("not valid json", defaults, 6);
            Assert.That(loaded.quality, Is.EqualTo(2));
            Assert.That(loaded.musicVolume, Is.EqualTo(.7f));
        }

        [Test]
        public void UnknownSaveVersionDoesNotApplyItsValues()
        {
            var loaded = GameSettingsData.Decode("{\"version\":99,\"lodBias\":4}", new GameSettingsData(), 6);
            Assert.That(loaded.version, Is.EqualTo(1));
            Assert.That(loaded.lodBias, Is.EqualTo(1));
        }

        [Test]
        public void LoadedValuesAreClampedAndUnsupportedOptionsRejected()
        {
            var data = new GameSettingsData {
                quality = 99, lodBias = float.NaN, shadowDistance = -1, masterVolume = 5,
                musicVolume = float.PositiveInfinity, effectsVolume = -1,
                mouseSensitivity = -4, fieldOfView = 999, textureQuality = 8, shadows = -1,
                antiAliasing = 3, frameLimit = 17
            };
            data.Sanitize(6);
            Assert.That(data.quality, Is.EqualTo(5));
            Assert.That(data.lodBias, Is.EqualTo(1));
            Assert.That(data.shadowDistance, Is.EqualTo(10));
            Assert.That(data.masterVolume, Is.EqualTo(1));
            Assert.That(data.musicVolume, Is.EqualTo(1));
            Assert.That(data.effectsVolume, Is.Zero);
            Assert.That(data.mouseSensitivity, Is.EqualTo(.25));
            Assert.That(data.fieldOfView, Is.EqualTo(90));
            Assert.That(data.textureQuality, Is.EqualTo(2));
            Assert.That(data.shadows, Is.Zero);
            Assert.That(data.antiAliasing, Is.Zero);
            Assert.That(data.frameLimit, Is.EqualTo(60));
        }

        [Test]
        public void SaveRoundTripPreservesPreferencesIncludingMuteAndUnlimited()
        {
            var data = new GameSettingsData {
                quality = 4, lodBias = 2.3f, shadowDistance = 77f, antiAliasing = 8,
                masterVolume = 0, musicVolume = .4f, effectsVolume = .6f,
                mouseSensitivity = 1.6f, invertY = true, fieldOfView = 72, showFps = true,
                frameLimit = -1, vSync = false, fullscreen = false
            };
            var loaded = GameSettingsData.Decode(JsonUtility.ToJson(data), new GameSettingsData(), 6);
            Assert.That(JsonUtility.ToJson(loaded), Is.EqualTo(JsonUtility.ToJson(data)));
        }

        [TestCase(0f, -80f)]
        [TestCase(.5f, -6.0206f)]
        [TestCase(1f, 0f)]
        public void VolumeConversionSupportsMuteAndLogarithmicGain(float linear, float decibels)
        {
            Assert.That(GameSettings.LinearToDecibels(linear), Is.EqualTo(decibels).Within(.0002f));
        }
    }
}
