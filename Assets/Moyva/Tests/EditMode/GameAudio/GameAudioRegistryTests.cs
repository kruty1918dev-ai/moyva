using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.JsonConfig;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.GameAudio.Tests
{
    /// <summary>
    /// Фокусні EditMode-перевірки JSON-driven аудіо-шару:
    /// реєстр ключів, канали, bus-групи, конфігурація ambience/feedback,
    /// гучності каналів і duck-енвеолопи AudioService.
    /// </summary>
    public sealed class GameAudioRegistryTests
    {
        private AudioService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new AudioService(null, null);
            _service.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _service = null;
        }

        [Test]
        public void Registry_Loads_All_Keys_Resolve_To_Clips()
        {
            var registry = JsonConfigRuntime.Get<AudioRegistrySO>("moyvaaudioregistry");
            Assert.NotNull(registry, "audio-registry preset must resolve");
            Assert.GreaterOrEqual(registry.Sounds.Length, 40, "expected full sound set");

            foreach (var sound in registry.Sounds)
            {
                Assert.NotNull(sound);
                Assert.IsFalse(string.IsNullOrWhiteSpace(sound.Key));
                Assert.NotNull(sound.Clip, $"sound '{sound.Key}' has no clip resolved");
            }
        }

        [Test]
        public void Registry_BusGroups_Cover_All_Playable_Buses()
        {
            var registry = JsonConfigRuntime.Get<AudioRegistrySO>("moyvaaudioregistry");
            Assert.NotNull(registry);
            Assert.NotNull(registry.GetBusGroup(AudioBus.Music), "Music bus group");
            Assert.NotNull(registry.GetBusGroup(AudioBus.Sfx), "Sfx bus group");
            Assert.NotNull(registry.GetBusGroup(AudioBus.Ui), "Ui bus group");
            Assert.NotNull(registry.GetBusGroup(AudioBus.Ambience), "Ambience bus group");
        }

        [Test]
        public void Play_KnownKey_Returns_ValidHandle()
        {
            var handle = _service.Play("ui-click");
            Assert.IsTrue(handle.IsValid, "ui-click must resolve and play");
            Assert.NotNull(handle.Source);
            handle.Stop();
        }

        [Test]
        public void Play_UnknownKey_Returns_InvalidHandle_And_Warns_Once()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
                new System.Text.RegularExpressions.Regex("Unknown sound key"));
            var first = _service.Play("definitely-not-a-key");
            var second = _service.Play("definitely-not-a-key");

            Assert.IsFalse(first.IsValid);
            Assert.IsFalse(second.IsValid);
        }

        [Test]
        public void ChannelVolume_Scales_Active_Source()
        {
            var handle = _service.Play("amb-water-stream");
            Assert.IsTrue(handle.IsValid, "amb-water-stream must play");
            float before = handle.Source.volume;

            _service.SetChannelVolume("world", 0.5f);
            Assert.Less(handle.Source.volume, before, "channel volume must scale active sources");

            _service.SetChannelVolume("world", 1f);
            handle.Stop();
        }

        [Test]
        public void DuckBus_Envelope_Reduces_Then_Recovers()
        {
            // attack=0, hold>0 → множник одразу цільовий.
            _service.DuckBus(AudioBus.Ambience, 0.3f, 0f, 10f, 5f);
            Assert.AreEqual(0.3f, _service.GetBusDuckMultiplier(AudioBus.Ambience), 0.001f);
            Assert.AreEqual(1f, _service.GetBusDuckMultiplier(AudioBus.Sfx), 0.001f);
        }

        [Test]
        public void SetPlaybackScale_Updates_Source_Volume()
        {
            var handle = _service.Play("amb-wind-deep",
                new AudioPlayOptions(volumeScale: 0.5f, loopOverride: true));
            Assert.IsTrue(handle.IsValid);
            float before = handle.Source.volume;

            _service.SetPlaybackScale(handle, 0.5f);
            Assert.Less(handle.Source.volume, before, "playback scale must reduce source volume");
            handle.Stop();
        }

        [Test]
        public void Ambience_Config_Resolves_From_Json()
        {
            var config = JsonConfigRuntime.Get<AudioAmbienceConfig>("moyvaaudioambience");
            Assert.NotNull(config, "audio-ambience preset must resolve");
            Assert.NotNull(config.beds);
            Assert.NotNull(config.emitters);
            Assert.Greater(config.beds.Length, 0, "at least one ambience bed expected");
        }

        [Test]
        public void Feedback_Config_SoundKeys_Exist_In_Registry()
        {
            var config = JsonConfigRuntime.Get<AudioFeedbackConfig>("moyvaaudiofeedback");
            var registry = JsonConfigRuntime.Get<AudioRegistrySO>("moyvaaudioregistry");
            Assert.NotNull(config);
            Assert.NotNull(registry);

            foreach (var rule in config.eventSounds)
            {
                Assert.IsTrue(registry.TryGet(rule.soundKey, out _),
                    $"eventSound '{rule.eventName}' → missing key '{rule.soundKey}'");
            }

            foreach (var rule in config.uiActionSounds)
            {
                Assert.IsTrue(registry.TryGet(rule.soundKey, out _),
                    $"uiActionSound → missing key '{rule.soundKey}'");
            }
        }
    }
}
