using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.Controls
{
    [Serializable]
    public sealed class PlayerControlProfile
    {
        public ControlProfile Profile;
        public float Sensitivity = 1f;
        public float Deadzone = 0.2f;
        public bool EdgePan;
        public bool Gestures = true;
        public string PanBinding;
        public string OrbitBinding;
        public List<ProfileBinding> Bindings = new();

        public PlayerControlProfile Copy() => JsonUtility.FromJson<PlayerControlProfile>(JsonUtility.ToJson(this));
        public Dictionary<PlayerControlAction, string> ToBindings()
        {
            var result = new Dictionary<PlayerControlAction, string>();
            foreach (var entry in Bindings)
                if (Enum.IsDefined(typeof(PlayerControlAction), entry.Action) && PlayerControlBinding.TryParse(entry.Path, out var parsed))
                    result[entry.Action] = parsed.CanonicalPath;
            return result;
        }
        public void SetBinding(PlayerControlAction action, string path)
        {
            Bindings.RemoveAll(entry => entry.Action == action);
            Bindings.Add(new ProfileBinding { Action = action, Path = path });
        }
        public static List<PlayerControlProfile> Defaults()
        {
            var asset = Resources.Load<TextAsset>("Controls/device-profiles");
            if (asset == null) throw new InvalidOperationException("Missing Controls/device-profiles JSON preset.");
            return JsonUtility.FromJson<ProfileDocument>(asset.text).Profiles;
        }
    }
    [Serializable]
    public sealed class ProfileBinding { public PlayerControlAction Action; public string Path; }
    [Serializable]
    public sealed class ProfileDocument
    {
        public ControlProfile Selection;
        public PointerInterpretation PointerMode;
        public List<PlayerControlProfile> Profiles = new();
    }
}
