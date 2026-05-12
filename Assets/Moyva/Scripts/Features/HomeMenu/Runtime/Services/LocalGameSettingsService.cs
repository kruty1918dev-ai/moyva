using System;
using System.IO;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Audio;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Зберігає та застосовує локальні налаштування гри: ім'я гравця, гучності та mute.
    /// Гучності маршрутизуються через AudioMixerBindingsSO (за наявності), інакше — лише AudioListener.volume для master.
    /// Версія файлу = 2 (додані Music/Sfx/Ui/IsMuted).
    /// </summary>
    internal sealed class LocalGameSettingsService : ILocalGameSettingsService, IAudioSettingsService, IInitializable
    {
        private const int Version = 2;
        private const float DefaultMaster = 1f;
        private const float DefaultMusic = 0.7f;
        private const float DefaultSfx = 0.9f;
        private const float DefaultUi = 0.9f;

        private readonly string _filePath;

        [InjectOptional] private ISaveService _saveService;
        [InjectOptional] private AudioMixerBindingsSO _mixerBindings;

        public event Action<LocalGameSettings> OnSettingsChanged;
        public event Action<float, float, float> OnVolumesChanged;

        public LocalGameSettings Settings { get; private set; }
        public string PlayerName => Settings.PlayerName;
        public float MasterVolume => Settings.MasterVolume;
        public float MusicVolume => Settings.MusicVolume;
        public float SfxVolume => Settings.SfxVolume;
        public float UiVolume => Settings.UiVolume;
        public bool IsMuted => Settings.IsMuted;

        public LocalGameSettingsService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, MultiplayerClientScope.BuildScopedFileName("home_menu_settings.dat"));
            Settings = CreateDefaultSettings();
        }

        public void Initialize()
        {
            Settings = LoadOrCreate();
            ApplyAll(Settings);
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
            OnVolumesChanged?.Invoke(Settings.MasterVolume, Settings.MusicVolume, Settings.SfxVolume);
        }

        public void SetPlayerName(string playerName)
        {
            var normalized = NormalizePlayerName(playerName);
            if (string.Equals(Settings.PlayerName, normalized, StringComparison.Ordinal)) return;
            Settings = Settings.WithPlayerName(normalized);
            SaveAndNotify();
        }

        public void SetMasterVolume(float volume)
        {
            var v = Mathf.Clamp01(volume);
            if (Mathf.Approximately(Settings.MasterVolume, v)) return;
            Settings = Settings.WithMaster(v);
            ApplyAll(Settings);
            SaveAndNotify();
        }

        public void SetMusicVolume(float volume)
        {
            var v = Mathf.Clamp01(volume);
            if (Mathf.Approximately(Settings.MusicVolume, v)) return;
            Settings = Settings.WithMusic(v);
            ApplyAll(Settings);
            SaveAndNotify();
        }

        public void SetSfxVolume(float volume)
        {
            var v = Mathf.Clamp01(volume);
            if (Mathf.Approximately(Settings.SfxVolume, v)) return;
            Settings = Settings.WithSfx(v);
            ApplyAll(Settings);
            SaveAndNotify();
        }

        public void SetUiVolume(float volume)
        {
            var v = Mathf.Clamp01(volume);
            if (Mathf.Approximately(Settings.UiVolume, v)) return;
            Settings = Settings.WithUi(v);
            ApplyAll(Settings);
            SaveAndNotify();
        }

        public void SetMuted(bool isMuted)
        {
            if (Settings.IsMuted == isMuted) return;
            Settings = Settings.WithMuted(isMuted);
            ApplyAll(Settings);
            SaveAndNotify();
        }

        public void DeleteAllSaves()
        {
            if (_saveService == null)
            {
                Debug.LogWarning("[LocalGameSettingsService] ISaveService is not available; cannot delete saves.");
                return;
            }
            for (int slot = 0; slot <= 99; slot++)
                _saveService.Delete(slot);
        }

        private LocalGameSettings LoadOrCreate()
        {
            if (!File.Exists(_filePath))
                return Settings;

            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new BinaryReader(stream);

                var version = reader.ReadInt32();
                if (version == 1)
                {
                    var name = NormalizePlayerName(reader.ReadString());
                    var master = Mathf.Clamp01(reader.ReadSingle());
                    return new LocalGameSettings(name, master, DefaultMusic, DefaultSfx, DefaultUi, false);
                }
                if (version == Version)
                {
                    var name = NormalizePlayerName(reader.ReadString());
                    var master = Mathf.Clamp01(reader.ReadSingle());
                    var music = Mathf.Clamp01(reader.ReadSingle());
                    var sfx = Mathf.Clamp01(reader.ReadSingle());
                    var ui = Mathf.Clamp01(reader.ReadSingle());
                    var muted = reader.ReadBoolean();
                    return new LocalGameSettings(name, master, music, sfx, ui, muted);
                }
                return CreateDefaultSettings();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LocalGameSettingsService] Failed to load settings: {e.Message}. Defaults will be used.");
                return CreateDefaultSettings();
            }
        }

        private void SaveAndNotify()
        {
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
            OnVolumesChanged?.Invoke(Settings.MasterVolume, Settings.MusicVolume, Settings.SfxVolume);
        }

        private void Save(LocalGameSettings s)
        {
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var stream = File.Create(_filePath);
                using var writer = new BinaryWriter(stream);
                writer.Write(Version);
                writer.Write(NormalizePlayerName(s.PlayerName));
                writer.Write(Mathf.Clamp01(s.MasterVolume));
                writer.Write(Mathf.Clamp01(s.MusicVolume));
                writer.Write(Mathf.Clamp01(s.SfxVolume));
                writer.Write(Mathf.Clamp01(s.UiVolume));
                writer.Write(s.IsMuted);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalGameSettingsService] Failed to save settings: {e.Message}");
            }
        }

        private LocalGameSettings CreateDefaultSettings()
        {
            var defMaster = _mixerBindings != null ? _mixerBindings.defaultMaster : DefaultMaster;
            var defMusic = _mixerBindings != null ? _mixerBindings.defaultMusic : DefaultMusic;
            var defSfx = _mixerBindings != null ? _mixerBindings.defaultSfx : DefaultSfx;
            var defUi = _mixerBindings != null ? _mixerBindings.defaultUi : DefaultUi;
            return new LocalGameSettings(MultiplayerClientScope.CreateDefaultPlayerName(), defMaster, defMusic, defSfx, defUi, false);
        }

        private static string NormalizePlayerName(string playerName)
        {
            var value = string.IsNullOrWhiteSpace(playerName) ? MultiplayerClientScope.CreateDefaultPlayerName() : playerName.Trim();
            return value.Length <= 24 ? value : value.Substring(0, 24);
        }

        /// <summary>Застосувати всі гучності з урахуванням mute. Якщо mute — усі канали ставимо в -80 dB.</summary>
        private void ApplyAll(LocalGameSettings s)
        {
            // Глобальний AudioListener.volume — щоб master працював навіть без AudioMixer.
            AudioListener.volume = s.IsMuted ? 0f : Mathf.Clamp01(s.MasterVolume);

            if (_mixerBindings == null || _mixerBindings.mixer == null) return;

            ApplyMixerParam(_mixerBindings.masterParameter, s.IsMuted ? 0f : s.MasterVolume);
            ApplyMixerParam(_mixerBindings.musicParameter, s.IsMuted ? 0f : s.MusicVolume);
            ApplyMixerParam(_mixerBindings.sfxParameter, s.IsMuted ? 0f : s.SfxVolume);
            ApplyMixerParam(_mixerBindings.uiParameter, s.IsMuted ? 0f : s.UiVolume);
        }

        private void ApplyMixerParam(string paramName, float linear01)
        {
            if (string.IsNullOrEmpty(paramName) || _mixerBindings.mixer == null) return;
            // 0..1 → -80..0 dB (логарифмічно).
            float db = linear01 <= 0.0001f ? -80f : Mathf.Log10(Mathf.Max(0.0001f, linear01)) * 20f;
            try { _mixerBindings.mixer.SetFloat(paramName, db); }
            catch (Exception e) { Debug.LogWarning($"[LocalGameSettingsService] SetFloat('{paramName}') failed: {e.Message}"); }
        }
    }
}
