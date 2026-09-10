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
        ZoomOut
    }

    public struct PlayerControlSettingsData
    {
        public float MouseSensitivity;
        public float MovementSpeed;
        public float OrbitSpeed;
        public float ZoomSpeed;
        public Dictionary<PlayerControlAction, string> Bindings;

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
                Bindings = NormalizeBindings(Bindings, defaults.Bindings)
            };
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

            // A corrupt/old layout must not trigger multiple actions for the same chord.
            if (new HashSet<string>(result.Values, StringComparer.OrdinalIgnoreCase).Count != result.Count)
                return new Dictionary<PlayerControlAction, string>(fallback);

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
    }

    internal sealed class PlayerControlSettingsService : IPlayerControlSettingsService, IInitializable
    {
        private const int Version = 1;
        private readonly string _filePath;

        public PlayerControlSettingsData Settings { get; private set; }
        public event Action<PlayerControlSettingsData> OnSettingsChanged;

        public PlayerControlSettingsService([InjectOptional] IClientInstanceScope clientScope = null)
        {
            clientScope ??= ClientInstanceScope.Default;
            _filePath = Path.Combine(
                Application.persistentDataPath,
                clientScope.BuildScopedFileName("player_controls.dat"));
            Settings = PlayerControlSettingsData.CreateDefault().Normalized();
        }

        public void Initialize()
        {
            Settings = LoadOrCreate();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        public bool TrySetBinding(PlayerControlAction action, string controlPath, out PlayerControlAction conflictingAction)
        {
            conflictingAction = default;
            if (!Enum.IsDefined(typeof(PlayerControlAction), action)
                || !PlayerControlBinding.TryParse(controlPath, out var binding))
                return false;

            var next = Clone(Settings);
            string normalizedPath = binding.CanonicalPath;
            foreach (var pair in next.Bindings)
            {
                if (pair.Key == action)
                    continue;
                if (!string.Equals(pair.Value, normalizedPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                conflictingAction = pair.Key;
                return false;
            }

            next.Bindings[action] = normalizedPath;
            Update(next);
            return true;
        }

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

        private void Update(PlayerControlSettingsData next)
        {
            next = next.Normalized();
            if (AreEquivalent(Settings, next))
                return;

            Settings = next;
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
                if (reader.ReadInt32() != Version)
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
                using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
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
