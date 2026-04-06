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

            if (wrapper.preset.version > 2)
                Debug.LogWarning($"[GraphPresetIO] Preset version {wrapper.preset.version} is newer than supported (2). " +
                                 "Some data may not load correctly.");

            return wrapper.preset;
        }
    }
}
