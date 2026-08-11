using System;
using Newtonsoft.Json;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Plain C# base for migrated Moyva configuration objects.
    /// It intentionally does NOT derive from UnityEngine.Object or ScriptableObject.
    /// </summary>
    [Serializable]
    public abstract class MoyvaJsonConfigObject
    {
        [JsonIgnore] public string JsonId { get; internal set; } = string.Empty;
        [JsonIgnore] public string JsonSchema { get; internal set; } = string.Empty;
        [JsonIgnore] public int JsonVersion { get; internal set; } = 1;
        [JsonIgnore] public string JsonSourcePath { get; internal set; } = string.Empty;

        // Compatibility for old config code that used UnityEngine.Object.name.
        // This is runtime metadata only and is not an authoring source.
        [JsonIgnore] public string name { get; set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(JsonId)
                ? GetType().Name
                : $"{GetType().Name}({JsonId})";
        }
    }
}
