using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>Юніт-тести <see cref="AudioSettingsService"/> (логіка без AudioMixer).</summary>
    public sealed class AudioSettingsServiceTests
    {
        private AudioMixerBindingsSO _bindings;
        private AudioSettingsService _sut;

        [SetUp]
        public void SetUp()
        {
            _bindings = ScriptableObject.CreateInstance<AudioMixerBindingsSO>();
            // Очищуємо PlayerPrefs по ключам сервісу для детермінованих тестів.
            foreach (AudioChannel c in System.Enum.GetValues(typeof(AudioChannel)))
                PlayerPrefs.DeleteKey($"Moyva.Audio.{c}");
            PlayerPrefs.DeleteKey("Moyva.Audio.Muted");

            _sut = new AudioSettingsService(_bindings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_bindings);
        }

        [Test]
        public void SetVolume_Clamps_And_FiresEvent()
        {
            AudioChannel capturedChannel = AudioChannel.Master;
            float capturedValue = -1f;
            _sut.VolumeChanged += (c, v) => { capturedChannel = c; capturedValue = v; };

            _sut.SetVolume(AudioChannel.Music, 1.5f);

            Assert.AreEqual(AudioChannel.Music, capturedChannel);
            Assert.AreEqual(1f, capturedValue);
            Assert.AreEqual(1f, _sut.GetVolume(AudioChannel.Music));
        }

        [Test]
        public void SetVolume_NegativeIsClampedToZero()
        {
            _sut.SetVolume(AudioChannel.Sfx, -0.5f);
            Assert.AreEqual(0f, _sut.GetVolume(AudioChannel.Sfx));
        }

        [Test]
        public void SetMuted_FiresEventOnlyOnChange()
        {
            int eventCount = 0;
            _sut.MuteChanged += _ => eventCount++;

            _sut.SetMuted(true);
            _sut.SetMuted(true);      // дубль — має бути проігнорований
            _sut.SetMuted(false);

            Assert.AreEqual(2, eventCount);
            Assert.IsFalse(_sut.IsMuted);
        }

        [Test]
        public void LinearToDb_Converts_01_To_KnownValues()
        {
            Assert.That(AudioSettingsService.LinearToDb(1f), Is.EqualTo(0f).Within(0.0001f));
            // log10(0.5) * 20 ≈ -6.0206
            Assert.That(AudioSettingsService.LinearToDb(0.5f), Is.EqualTo(-6.0206f).Within(0.01f));
            // 0 → clamp до 0.0001 → log10(1e-4)*20 = -80 dB
            Assert.That(AudioSettingsService.LinearToDb(0f), Is.EqualTo(-80f).Within(0.01f));
        }

        [Test]
        public void Save_And_Load_RoundTrip()
        {
            _sut.SetVolume(AudioChannel.Master, 0.3f);
            _sut.SetVolume(AudioChannel.Music,  0.4f);
            _sut.SetVolume(AudioChannel.Sfx,    0.5f);
            _sut.SetVolume(AudioChannel.Ui,     0.6f);
            _sut.SetMuted(true);
            _sut.Save();

            var fresh = new AudioSettingsService(_bindings);
            fresh.Load();

            Assert.AreEqual(0.3f, fresh.GetVolume(AudioChannel.Master));
            Assert.AreEqual(0.4f, fresh.GetVolume(AudioChannel.Music));
            Assert.AreEqual(0.5f, fresh.GetVolume(AudioChannel.Sfx));
            Assert.AreEqual(0.6f, fresh.GetVolume(AudioChannel.Ui));
            Assert.IsTrue(fresh.IsMuted);
        }
    }
}
