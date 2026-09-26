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
        SecondarySelect,
        /// <summary>Варіант FocusSelected.</summary>
        FocusSelected,
        /// <summary>Held while navigating — boosts camera movement speed.</summary>
        Sprint,
        /// <summary>Focuses the camera on the local player's capital/castle.</summary>
        FocusCapital
    }

    public struct PlayerControlSettingsData
    {
        public float MouseSensitivity;
        public float MovementSpeed;
        public float OrbitSpeed;
        public float ZoomSpeed;
        /// <summary>камери ефектів — bool.</summary>
        public bool CameraEffects;
        /// <summary>камери тряски Intensity — float.</summary>
        public float CameraShakeIntensity;
        /// <summary>плавного камери фокус — bool.</summary>
        public bool SmoothCameraFocus;
        /// <summary>автоматичного камери фокус — bool.</summary>
        public bool AutomaticCameraFocus;
        /// <summary>Reduce камери руху — bool.</summary>
        public bool ReduceCameraMotion;
        /// <summary>зменшеного UI/HTML руху (меню, HUD, підказки, позначення) — bool.</summary>
        public bool ReduceMotion;
        /// <summary>зум Toward пальців — bool.</summary>
        public bool ZoomTowardFingers;
        /// <summary>показувати журнал клавіатури у кутку екрана — bool.</summary>
        public bool InputLogEnabled;
        /// <summary>чутливість прокручування ігрових списків — float.</summary>
        public float ScrollSensitivity;
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
                CameraEffects = true,
                CameraShakeIntensity = 0.5f,
                SmoothCameraFocus = true,
                AutomaticCameraFocus = false,
                ReduceCameraMotion = false,
                ReduceMotion = false,
                ZoomTowardFingers = true,
                InputLogEnabled = true,
                ScrollSensitivity = 0.5f,
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
                    { PlayerControlAction.FocusSelected, "<Keyboard>/f" },
                    { PlayerControlAction.Sprint, "<Keyboard>/shift" },
                    { PlayerControlAction.FocusCapital, "<Keyboard>/h" },
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
                CameraEffects = CameraEffects,
                CameraShakeIntensity = float.IsNaN(CameraShakeIntensity) || float.IsInfinity(CameraShakeIntensity)
                    ? defaults.CameraShakeIntensity
                    : Mathf.Clamp01(CameraShakeIntensity),
                SmoothCameraFocus = SmoothCameraFocus,
                AutomaticCameraFocus = AutomaticCameraFocus,
                ReduceCameraMotion = ReduceCameraMotion,
                ReduceMotion = ReduceMotion,
                ZoomTowardFingers = ZoomTowardFingers,
                InputLogEnabled = InputLogEnabled,
                ScrollSensitivity = float.IsNaN(ScrollSensitivity) || float.IsInfinity(ScrollSensitivity)
                    ? defaults.ScrollSensitivity
                    : Mathf.Clamp(ScrollSensitivity, 0.25f, 3f),
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
        /// <summary>
        /// Modifier bits held by the active profile's Sprint binding. Consumers reading
        /// other bindings may ignore these bits so sprinting does not block them.
        /// </summary>
        PlayerControlModifiers SprintModifierMask { get; }
        event Action<PlayerControlSettingsData> OnSettingsChanged;

        bool TrySetBinding(PlayerControlAction action, string controlPath, out PlayerControlAction conflictingAction);
        void SetMouseSensitivity(float value);
        void SetMovementSpeed(float value);
        void SetOrbitSpeed(float value);
        void SetZoomSpeed(float value);
        /// <summary>Встановлює камери ефектів.</summary>
        void SetCameraEffects(bool value);
        /// <summary>Встановлює камери тряски Intensity.</summary>
        void SetCameraShakeIntensity(float value);
        /// <summary>Встановлює плавного камери фокус.</summary>
        void SetSmoothCameraFocus(bool value);
        /// <summary>Встановлює автоматичного камери фокус.</summary>
        void SetAutomaticCameraFocus(bool value);
        /// <summary>Встановлює Reduce камери руху.</summary>
        void SetReduceCameraMotion(bool value);
        /// <summary>Встановлює зменшення UI/HTML руху.</summary>
        void SetReduceMotion(bool value);
        /// <summary>Встановлює зум Toward пальців.</summary>
        void SetZoomTowardFingers(bool value);
        /// <summary>Вмикає/вимикає журнал клавіатури у кутку екрана.</summary>
        void SetInputLogEnabled(bool value);
        /// <summary>Встановлює чутливість прокручування ігрових списків.</summary>
        void SetScrollSensitivity(float value);
        void ResetToDefaults();
        void ConfigureDevices(ControlProfile selection, PointerInterpretation pointerMode);
        bool TrySetProfileBinding(ControlProfile profile, PlayerControlAction action, string path, out PlayerControlAction conflict);
        void SetProfileOptions(PlayerControlProfile profile);
        void ResetProfile(ControlProfile profile);
    }

    internal sealed class PlayerControlSettingsService : IPlayerControlSettingsService, IInitializable
    {
        private const int Version = 4;
        private readonly IInputDeviceContext _devices;
        private readonly string _filePath;
        private bool _sprintMaskDirty = true;
        private ControlProfile _sprintMaskProfile;
        private PlayerControlModifiers _sprintModifierMask;

        public PlayerControlSettingsData Settings { get; private set; }

        public PlayerControlModifiers SprintModifierMask
        {
            get
            {
                var profile = _devices?.ActiveProfile ?? ControlProfile.KeyboardMouse;
                if (_sprintMaskDirty || profile != _sprintMaskProfile)
                {
                    _sprintMaskDirty = false;
                    _sprintMaskProfile = profile;
                    _sprintModifierMask = ResolveSprintModifierMask(profile);
                }
                return _sprintModifierMask;
            }
        }

        private PlayerControlModifiers ResolveSprintModifierMask(ControlProfile profile)
        {
            var bindings = Settings.Profile(profile)?.ToBindings() ?? Settings.Bindings;
            return bindings != null
                && bindings.TryGetValue(PlayerControlAction.Sprint, out var path)
                && PlayerControlBinding.TryParse(path, out var binding)
                ? binding.ModifierMask
                : PlayerControlModifiers.None;
        }
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

        /// <summary>Встановлює камери ефектів.</summary>
        public void SetCameraEffects(bool value)
        {
            var next = Clone(Settings);
            next.CameraEffects = value;
            Update(next);
        }

        /// <summary>Встановлює камери тряски Intensity.</summary>
        public void SetCameraShakeIntensity(float value)
        {
            var next = Clone(Settings);
            next.CameraShakeIntensity = value;
            Update(next);
        }

        /// <summary>Встановлює плавного камери фокус.</summary>
        public void SetSmoothCameraFocus(bool value)
        {
            var next = Clone(Settings);
            next.SmoothCameraFocus = value;
            Update(next);
        }

        /// <summary>Встановлює автоматичного камери фокус.</summary>
        public void SetAutomaticCameraFocus(bool value)
        {
            var next = Clone(Settings);
            next.AutomaticCameraFocus = value;
            Update(next);
        }

        /// <summary>Встановлює Reduce камери руху.</summary>
        public void SetReduceCameraMotion(bool value)
        {
            var next = Clone(Settings);
            next.ReduceCameraMotion = value;
            Update(next);
        }

        /// <summary>Встановлює зменшення UI/HTML руху.</summary>
        public void SetReduceMotion(bool value)
        {
            var next = Clone(Settings);
            next.ReduceMotion = value;
            Update(next);
        }

        /// <summary>Встановлює зум Toward пальців.</summary>
        public void SetZoomTowardFingers(bool value)
        {
            var next = Clone(Settings);
            next.ZoomTowardFingers = value;
            Update(next);
        }

        /// <summary>Вмикає/вимикає журнал клавіатури у кутку екрана.</summary>
        public void SetInputLogEnabled(bool value)
        {
            var next = Clone(Settings);
            next.InputLogEnabled = value;
            Update(next);
        }

        /// <summary>Встановлює чутливість прокручування ігрових списків.</summary>
        public void SetScrollSensitivity(float value)
        {
            var next = Clone(Settings);
            next.ScrollSensitivity = value;
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
            _sprintMaskDirty = true;
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
                if (version < 1 || version > Version)
                    return PlayerControlSettingsData.CreateDefault().Normalized();

                var defaults = PlayerControlSettingsData.CreateDefault();
                var settings = new PlayerControlSettingsData
                {
                    MouseSensitivity = reader.ReadSingle(),
                    MovementSpeed = reader.ReadSingle(),
                    OrbitSpeed = reader.ReadSingle(),
                    ZoomSpeed = reader.ReadSingle(),
                    CameraEffects = defaults.CameraEffects,
                    CameraShakeIntensity = defaults.CameraShakeIntensity,
                    SmoothCameraFocus = defaults.SmoothCameraFocus,
                    AutomaticCameraFocus = defaults.AutomaticCameraFocus,
                    ReduceCameraMotion = defaults.ReduceCameraMotion,
                    ZoomTowardFingers = defaults.ZoomTowardFingers,
                    InputLogEnabled = defaults.InputLogEnabled,
                    ScrollSensitivity = defaults.ScrollSensitivity,
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
                if (version >= 3)
                {
                    byte flags = reader.ReadByte();
                    settings.CameraEffects = (flags & 1) != 0;
                    settings.SmoothCameraFocus = (flags & 2) != 0;
                    settings.AutomaticCameraFocus = (flags & 4) != 0;
                    settings.ReduceCameraMotion = (flags & 8) != 0;
                    settings.ZoomTowardFingers = (flags & 16) != 0;
                    // Bit 32 (ReduceMotion) extends the v3 flags byte in place —
                    // files written by older builds simply read it as unset.
                    settings.ReduceMotion = (flags & 32) != 0;
                    // Bit 64 stores InputLog *disabled* so older files read enabled.
                    settings.InputLogEnabled = (flags & 64) == 0;
                    settings.CameraShakeIntensity = reader.ReadSingle();
                    if (version >= 4) settings.ScrollSensitivity = reader.ReadSingle();
                }
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
                byte flags = 0;
                if (data.CameraEffects) flags |= 1;
                if (data.SmoothCameraFocus) flags |= 2;
                if (data.AutomaticCameraFocus) flags |= 4;
                if (data.ReduceCameraMotion) flags |= 8;
                if (data.ZoomTowardFingers) flags |= 16;
                if (data.ReduceMotion) flags |= 32;
                if (!data.InputLogEnabled) flags |= 64;
                writer.Write(flags);
                writer.Write(data.CameraShakeIntensity);
                writer.Write(data.ScrollSensitivity);
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
                CameraEffects = source.CameraEffects,
                CameraShakeIntensity = source.CameraShakeIntensity,
                SmoothCameraFocus = source.SmoothCameraFocus,
                AutomaticCameraFocus = source.AutomaticCameraFocus,
                ReduceCameraMotion = source.ReduceCameraMotion,
                ReduceMotion = source.ReduceMotion,
                ZoomTowardFingers = source.ZoomTowardFingers,
                InputLogEnabled = source.InputLogEnabled,
                ScrollSensitivity = source.ScrollSensitivity,
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
                !Mathf.Approximately(first.ZoomSpeed, second.ZoomSpeed) ||
                first.CameraEffects != second.CameraEffects ||
                !Mathf.Approximately(first.CameraShakeIntensity, second.CameraShakeIntensity) ||
                first.SmoothCameraFocus != second.SmoothCameraFocus ||
                first.AutomaticCameraFocus != second.AutomaticCameraFocus ||
                first.ReduceCameraMotion != second.ReduceCameraMotion ||
                first.ReduceMotion != second.ReduceMotion ||
                first.ZoomTowardFingers != second.ZoomTowardFingers ||
                first.InputLogEnabled != second.InputLogEnabled ||
                !Mathf.Approximately(first.ScrollSensitivity, second.ScrollSensitivity))
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
