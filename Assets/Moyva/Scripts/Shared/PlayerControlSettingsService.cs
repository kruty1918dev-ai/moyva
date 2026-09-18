using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Shared.Common;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Shared.Controls
{
    public enum PlayerControlAction
    {
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        RotateLeft,
        RotateRight,
        ZoomIn,
        ZoomOut,
        PrimarySelect,
        SecondarySelect
    }

    public struct PlayerControlSettingsData
    {
        public float MouseSensitivity;
        public float MovementSpeed;
        public float OrbitSpeed;
        public float ZoomSpeed;
        public Dictionary<PlayerControlAction, string> Bindings;
        public ProfileDocument Devices;

        public PlayerControlProfile Profile(ControlProfile profile)
            => Devices?.Profiles?.Find(item => item.Profile == profile);

        public static PlayerControlSettingsData CreateDefault()
        {
            return new PlayerControlSettingsData
            {
                MouseSensitivity = 1f,
                MovementSpeed = 1f,
                OrbitSpeed = 1f,
                ZoomSpeed = 1f,
                Bindings = new Dictionary<PlayerControlAction, string>
                {
                    { PlayerControlAction.MoveForward, "<Keyboard>/w" },
                    { PlayerControlAction.MoveBackward, "<Keyboard>/s" },
                    { PlayerControlAction.MoveLeft, "<Keyboard>/a" },
                    { PlayerControlAction.MoveRight, "<Keyboard>/d" },
                    { PlayerControlAction.RotateLeft, "<Keyboard>/q" },
                    { PlayerControlAction.RotateRight, "<Keyboard>/e" },
                    { PlayerControlAction.ZoomIn, "<Keyboard>/equals" },
                    { PlayerControlAction.ZoomOut, "<Keyboard>/minus" },
                    { PlayerControlAction.PrimarySelect, "<Mouse>/leftButton" },
                    { PlayerControlAction.SecondarySelect, "<Mouse>/rightButton" },
                }
            };
        }

        public PlayerControlSettingsData Normalized()
        {
            var defaults = CreateDefault();
            return new PlayerControlSettingsData
            {
                MouseSensitivity = ClampMultiplier(MouseSensitivity),
                MovementSpeed = ClampMultiplier(MovementSpeed),
                OrbitSpeed = ClampMultiplier(OrbitSpeed),
                ZoomSpeed = ClampMultiplier(ZoomSpeed),
                Bindings = NormalizeBindings(Bindings, defaults.Bindings),
                Devices = NormalizeDevices(Devices, Bindings)
            };
        }

        private static ProfileDocument NormalizeDevices(ProfileDocument source, Dictionary<PlayerControlAction, string> legacy)
        {
            var result = source == null ? new ProfileDocument() : JsonUtility.FromJson<ProfileDocument>(JsonUtility.ToJson(source));
            var defaults = PlayerControlProfile.Defaults();
            result.Profiles ??= new List<PlayerControlProfile>();
            result.Profiles.RemoveAll(item => item == null || item.Profile == ControlProfile.Auto || !Enum.IsDefined(typeof(ControlProfile), item.Profile));
            if (!Enum.IsDefined(typeof(ControlProfile), result.Selection)) result.Selection = ControlProfile.Auto;
            if (!Enum.IsDefined(typeof(PointerInterpretation), result.PointerMode)) result.PointerMode = PointerInterpretation.Auto;
            foreach (var fallback in defaults)
            {
                var profile = result.Profiles.Find(item => item != null && item.Profile == fallback.Profile);
                if (profile == null)
                {
                    profile = fallback.Copy();
                    if (legacy != null && (profile.Profile == ControlProfile.KeyboardMouse || profile.Profile == ControlProfile.KeyboardTouchpad))
                        foreach (var pair in legacy)
                            if (PlayerControlBinding.TryParse(pair.Value, out var parsed)) profile.SetBinding(pair.Key, parsed.CanonicalPath);
                    if (profile.Profile == ControlProfile.KeyboardTouchpad)
                    {
                        // Retain a legacy Space shortcut by resolving an unused pointer-drag chord.
                        foreach (string chord in new[] { "<Touchpad>/spaceDrag", "Ctrl+<Touchpad>/spaceDrag", "Shift+<Touchpad>/spaceDrag", "Ctrl+Shift+<Touchpad>/spaceDrag", "<Keyboard>/backquote" })
                        {
                            bool conflict = false;
                            foreach (var path in profile.ToBindings().Values)
                                if (PlayerControlBinding.Conflicts(path, chord) || PlayerControlBinding.Conflicts(path, "Alt+" + chord)) { conflict = true; break; }
                            if (!conflict) { profile.PanBinding = chord; profile.OrbitBinding = "Alt+" + chord; break; }
                        }
                    }
                    result.Profiles.Add(profile);
                }
                profile.Sensitivity = ClampMultiplier(profile.Sensitivity);
                profile.Deadzone = float.IsNaN(profile.Deadzone) ? 0.2f : Mathf.Clamp(profile.Deadzone, 0.05f, 0.8f);
                profile.Bindings ??= new List<ProfileBinding>();
                foreach (var binding in fallback.Bindings)
                    if (!profile.ToBindings().ContainsKey(binding.Action)) profile.SetBinding(binding.Action, binding.Path);
                profile.PanBinding = PlayerControlBinding.TryParse(profile.PanBinding, out var pan) ? pan.CanonicalPath : fallback.PanBinding;
                profile.OrbitBinding = PlayerControlBinding.TryParse(profile.OrbitBinding, out var orbit) ? orbit.CanonicalPath : fallback.OrbitBinding;
            }
            return result;
        }

        private static Dictionary<PlayerControlAction, string> NormalizeBindings(
            Dictionary<PlayerControlAction, string> source,
            Dictionary<PlayerControlAction, string> fallback)
        {
            var result = new Dictionary<PlayerControlAction, string>();
            foreach (PlayerControlAction action in Enum.GetValues(typeof(PlayerControlAction)))
            {
                string value = null;
                source?.TryGetValue(action, out value);
                if (!PlayerControlBinding.TryParse(value, out var binding))
                {
                    fallback.TryGetValue(action, out value);
                    PlayerControlBinding.TryParse(value, out binding);
                }
                result[action] = binding.CanonicalPath;
            }

            return result;
        }

        private static float ClampMultiplier(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 1f;
            return Mathf.Clamp(value, 0.25f, 3f);
        }
    }

    public interface IPlayerControlSettingsService
    {
        PlayerControlSettingsData Settings { get; }
        event Action<PlayerControlSettingsData> OnSettingsChanged;

        bool TrySetBinding(PlayerControlAction action, string controlPath, out PlayerControlAction conflictingAction);
        void SetMouseSensitivity(float value);
        void SetMovementSpeed(float value);
        void SetOrbitSpeed(float value);
        void SetZoomSpeed(float value);
        void ResetToDefaults();
        void ConfigureDevices(ControlProfile selection, PointerInterpretation pointerMode);
        bool TrySetProfileBinding(ControlProfile profile, PlayerControlAction action, string path, out PlayerControlAction conflict);
        void SetProfileOptions(PlayerControlProfile profile);
        void ResetProfile(ControlProfile profile);
    }

    internal sealed class PlayerControlSettingsService : IPlayerControlSettingsService, IInitializable
    {
        private const int Version = 2;
        private readonly IInputDeviceContext _devices;
        private readonly string _filePath;

        public PlayerControlSettingsData Settings { get; private set; }
        public event Action<PlayerControlSettingsData> OnSettingsChanged;

        public PlayerControlSettingsService([InjectOptional] IClientInstanceScope clientScope = null,
            [InjectOptional] IInputDeviceContext devices = null)
        {
            _devices = devices;
            clientScope ??= ClientInstanceScope.Default;
            _filePath = Path.Combine(
                Application.persistentDataPath,
                clientScope.BuildScopedFileName("player_controls.dat"));
            Settings = PlayerControlSettingsData.CreateDefault().Normalized();
        }

        public void Initialize()
        {
            Settings = LoadOrCreate();
            ApplyDeviceSelection();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        public bool TrySetBinding(PlayerControlAction action, string controlPath, out PlayerControlAction conflictingAction)
            => TrySetProfileBinding(ControlProfile.KeyboardMouse, action, controlPath, out conflictingAction);

        public void SetMouseSensitivity(float value)
        {
            var next = Clone(Settings);
            next.MouseSensitivity = value;
            Update(next);
        }

        public void SetMovementSpeed(float value)
        {
            var next = Clone(Settings);
            next.MovementSpeed = value;
            Update(next);
        }

        public void SetOrbitSpeed(float value)
        {
            var next = Clone(Settings);
            next.OrbitSpeed = value;
            Update(next);
        }

        public void SetZoomSpeed(float value)
        {
            var next = Clone(Settings);
            next.ZoomSpeed = value;
            Update(next);
        }

        public void ResetToDefaults() => Update(PlayerControlSettingsData.CreateDefault());

        private void ApplyDeviceSelection()
            => _devices?.Configure(Settings.Devices.Selection, Settings.Devices.PointerMode, Settings.Profile(ControlProfile.Gamepad)?.Deadzone ?? 0.3f);

        public void ConfigureDevices(ControlProfile selection, PointerInterpretation pointerMode)
        {
            var next = Clone(Settings);
            next.Devices.Selection = selection; next.Devices.PointerMode = pointerMode;
            Update(next);
        }

        public bool TrySetProfileBinding(ControlProfile profile, PlayerControlAction action, string path, out PlayerControlAction conflict)
        {
            conflict = default;
            if (!PlayerControlBinding.TryParse(path, out var binding) || !Enum.IsDefined(typeof(PlayerControlAction), action)) return false;
            var next = Clone(Settings);
            var target = next.Profile(profile);
            if (target == null) return false;
            foreach (var pair in target.ToBindings())
                if (pair.Key != action && PlayerControlBinding.Conflicts(pair.Value, binding.CanonicalPath))
                { conflict = pair.Key; return false; }
            if (PlayerControlBinding.Conflicts(target.PanBinding, binding.CanonicalPath) || PlayerControlBinding.Conflicts(target.OrbitBinding, binding.CanonicalPath)) return false;
            target.SetBinding(action, binding.CanonicalPath);
            if (profile == ControlProfile.KeyboardMouse) next.Bindings[action] = binding.CanonicalPath;
            Update(next); return true;
        }

        public void SetProfileOptions(PlayerControlProfile profile)
        {
            if (profile == null || profile.Profile == ControlProfile.Auto) return;
            var next = Clone(Settings);
            var target = next.Profile(profile.Profile);
            if (target == null) return;
            if (PlayerControlBinding.Conflicts(profile.PanBinding, profile.OrbitBinding)) return;
            foreach (var path in target.ToBindings().Values)
                if (PlayerControlBinding.Conflicts(path, profile.PanBinding) || PlayerControlBinding.Conflicts(path, profile.OrbitBinding)) return;
            target.Sensitivity = profile.Sensitivity; target.Deadzone = profile.Deadzone;
            target.EdgePan = profile.EdgePan; target.Gestures = profile.Gestures;
            target.PanBinding = profile.PanBinding; target.OrbitBinding = profile.OrbitBinding;
            Update(next);
        }

        public void ResetProfile(ControlProfile profile)
        {
            var next = Clone(Settings);
            var defaults = PlayerControlProfile.Defaults().Find(item => item.Profile == profile);
            if (defaults == null) return;
            next.Devices.Profiles.RemoveAll(item => item.Profile == profile);
            next.Devices.Profiles.Add(defaults);
            if (profile == ControlProfile.KeyboardMouse) next.Bindings = defaults.ToBindings();
            Update(next);
        }

        private void Update(PlayerControlSettingsData next)
        {
            next = next.Normalized();
            if (AreEquivalent(Settings, next))
                return;

            Settings = next;
            ApplyDeviceSelection();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        private PlayerControlSettingsData LoadOrCreate()
        {
            if (!File.Exists(_filePath))
                return PlayerControlSettingsData.CreateDefault().Normalized();

            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new BinaryReader(stream);
                int version = reader.ReadInt32();
                if (version != 1 && version != Version)
                    return PlayerControlSettingsData.CreateDefault().Normalized();

                var settings = new PlayerControlSettingsData
                {
                    MouseSensitivity = reader.ReadSingle(),
                    MovementSpeed = reader.ReadSingle(),
                    OrbitSpeed = reader.ReadSingle(),
                    ZoomSpeed = reader.ReadSingle(),
                    Bindings = new Dictionary<PlayerControlAction, string>()
                };

                int count = Mathf.Clamp(reader.ReadInt32(), 0, 64);
                for (int i = 0; i < count; i++)
                {
                    var action = (PlayerControlAction)reader.ReadInt32();
                    var path = reader.ReadString();
                    settings.Bindings[action] = path;
                }

                if (version == 1 && !File.Exists(_filePath + ".v1.bak")) File.Copy(_filePath, _filePath + ".v1.bak");
                if (version >= 2) settings.Devices = JsonUtility.FromJson<ProfileDocument>(reader.ReadString());
                return settings.Normalized();
            }
            catch
            {
                return PlayerControlSettingsData.CreateDefault().Normalized();
            }
        }

        private void Save(PlayerControlSettingsData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                string temporaryPath = _filePath + ".tmp";
                using (var stream = File.Open(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                using var writer = new BinaryWriter(stream);
                writer.Write(Version);
                writer.Write(data.MouseSensitivity);
                writer.Write(data.MovementSpeed);
                writer.Write(data.OrbitSpeed);
                writer.Write(data.ZoomSpeed);
                writer.Write(data.Bindings?.Count ?? 0);
                if (data.Bindings == null)
                    return;

                foreach (var pair in data.Bindings)
                {
                    writer.Write((int)pair.Key);
                    writer.Write(pair.Value ?? string.Empty);
                }
                writer.Write(JsonUtility.ToJson(data.Devices));
                }
                if (File.Exists(_filePath)) File.Replace(temporaryPath, _filePath, _filePath + ".bak");
                else File.Move(temporaryPath, _filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Controls] Failed to save player controls: {e.Message}");
            }
        }

        private static PlayerControlSettingsData Clone(PlayerControlSettingsData source)
        {
            return new PlayerControlSettingsData
            {
                Devices = source.Devices == null ? null : JsonUtility.FromJson<ProfileDocument>(JsonUtility.ToJson(source.Devices)),
                MouseSensitivity = source.MouseSensitivity,
                MovementSpeed = source.MovementSpeed,
                OrbitSpeed = source.OrbitSpeed,
                ZoomSpeed = source.ZoomSpeed,
                Bindings = new Dictionary<PlayerControlAction, string>(
                    source.Bindings ?? PlayerControlSettingsData.CreateDefault().Bindings)
            };
        }

        private static bool AreEquivalent(PlayerControlSettingsData first, PlayerControlSettingsData second)
        {
            if (JsonUtility.ToJson(first.Devices) != JsonUtility.ToJson(second.Devices)) return false;
            if (!Mathf.Approximately(first.MouseSensitivity, second.MouseSensitivity) ||
                !Mathf.Approximately(first.MovementSpeed, second.MovementSpeed) ||
                !Mathf.Approximately(first.OrbitSpeed, second.OrbitSpeed) ||
                !Mathf.Approximately(first.ZoomSpeed, second.ZoomSpeed))
                return false;

            foreach (PlayerControlAction action in Enum.GetValues(typeof(PlayerControlAction)))
            {
                string a = null;
                string b = null;
                first.Bindings?.TryGetValue(action, out a);
                second.Bindings?.TryGetValue(action, out b);
                if (!string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}
