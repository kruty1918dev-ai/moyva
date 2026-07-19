using System;
using System.IO;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public static class GraphPresetIO
    {
        private const string FileExtension = "graphpreset";
        internal const int CurrentVersion = 3;

        // JsonUtility requires a root object — cannot serialize a plain class at top level
        [Serializable]
        private sealed class PresetWrapper
        {
            public GraphPreset preset;
        }

        public static string ShowExportPanel(string defaultName = "NodePreset") =>
            EditorUtility.SaveFilePanel(
                "Export Graph Preset",
                Application.dataPath,
                defaultName,
                FileExtension);

        public static string ShowImportPanel() =>
            EditorUtility.OpenFilePanel(
                "Import Graph Preset",
                Application.dataPath,
                FileExtension);

        public static void WriteToFile(GraphPreset preset, string path)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path must not be empty.", nameof(path));

            preset.version = CurrentVersion;
            var wrapper = new PresetWrapper { preset = preset };
            var json = JsonUtility.ToJson(wrapper, prettyPrint: true);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }

        public static GraphPreset ReadFromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Preset file not found: {path}");

            var json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var wrapper = new PresetWrapper();
            JsonUtility.FromJsonOverwrite(json, wrapper);

            if (wrapper.preset == null)
                throw new InvalidDataException("File does not contain a valid GraphPreset.");

            int sourceVersion = Mathf.Max(1, wrapper.preset.version);
            if (sourceVersion > CurrentVersion)
            {
                Debug.LogWarning($"[GraphPresetIO] Preset version {sourceVersion} is newer than supported ({CurrentVersion}). " +
                                 "Some data may not load correctly.");
            }
            else
            {
                // v1/v2 omit explicit TWC modifier fields. Import code retains
                // its serialized-json fallback, so migration is lossless.
                wrapper.preset.version = CurrentVersion;
            }

            return wrapper.preset;
        }
    }
}
