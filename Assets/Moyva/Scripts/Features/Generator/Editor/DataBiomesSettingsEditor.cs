using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(DataBiomesSettings))]
    public class DataBiomesSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawValidationWarnings((DataBiomesSettings)target);
            base.OnInspectorGUI();
        }

        private static void DrawValidationWarnings(DataBiomesSettings settings)
        {
            var knownIds = LoadKnownTileIds();
            if (knownIds.Count == 0) return;

            var unknownIds = new HashSet<string>();

            if (!string.IsNullOrEmpty(settings.DefaultTileID) && !knownIds.Contains(settings.DefaultTileID))
                unknownIds.Add(settings.DefaultTileID);

            if (settings.Biomes != null)
            {
                foreach (var biome in settings.Biomes)
                {
                    if (!string.IsNullOrEmpty(biome.TileID) && !knownIds.Contains(biome.TileID))
                        unknownIds.Add(biome.TileID);
                }
            }

            if (unknownIds.Count > 0)
            {
                var sb = new StringBuilder("Невідомі Tile ID у BiomesSettings:\n");
                foreach (var id in unknownIds)
                    sb.AppendLine($"  • {id}");
                EditorGUILayout.HelpBox(sb.ToString().TrimEnd(), MessageType.Warning);
            }
        }

        private static HashSet<string> _cachedKnownIds;
        private static double _knownIdsTime;

        private static HashSet<string> LoadKnownTileIds()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_cachedKnownIds != null && now - _knownIdsTime < 2.0)
                return _cachedKnownIds;

            _cachedKnownIds = LoadKnownTileIdsInternal();
            _knownIdsTime = now;
            return _cachedKnownIds;
        }

        private static HashSet<string> LoadKnownTileIdsInternal()
        {
            var result = new HashSet<string>();
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return result;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registry = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
            if (registry?.Definitions == null) return result;

            foreach (var def in registry.Definitions)
                if (!string.IsNullOrEmpty(def.Id))
                    result.Add(def.Id);

            return result;
        }
    }
}
