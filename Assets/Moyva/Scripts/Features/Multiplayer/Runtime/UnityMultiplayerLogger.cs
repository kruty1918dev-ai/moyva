using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Runtime-safe logger that delegates to UnityEngine.Debug.
    /// </summary>
    public sealed class UnityMultiplayerLogger : IMultiplayerLogger
    {
        private const string Tag = "[Multiplayer]";

        public void Info(string message) => Debug.Log($"{Tag} {message}");
        public void Warn(string message) => Debug.LogWarning($"{Tag} {message}");
        public void Error(string message) => Debug.LogError($"{Tag} {message}");
        public void Trace(string message) => Debug.Log($"{Tag} TRACE {message}");
    }
}
