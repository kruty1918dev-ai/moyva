using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    internal static class TileWorldCreatorRegistrySyncUtility
    {
        public static bool SyncTerrainIds(
            TileRegistrySO tileRegistry,
            TileWorldCreatorIdMappingSO mapping,
            bool updateExisting,
            out int added,
            out int updated,
            out int skipped)
            => SyncTerrainIds(tileRegistry, mapping, updateExisting, false, out added, out updated, out skipped);

        public static bool RebuildTerrainIds(
            TileRegistrySO tileRegistry,
            TileWorldCreatorIdMappingSO mapping,
            out int added,
            out int updated,
            out int skipped)
            => SyncTerrainIds(tileRegistry, mapping, true, true, out added, out updated, out skipped);

        private static bool SyncTerrainIds(
            TileRegistrySO tileRegistry,
            TileWorldCreatorIdMappingSO mapping,
            bool updateExisting,
            bool rebuildFromMapping,
            out int added,
            out int updated,
            out int skipped)
        {
            added = 0;
            updated = 0;
            skipped = 0;

            if (tileRegistry == null || mapping == null)
                return false;

            var serializedRegistry = new SerializedObject(tileRegistry);
            var definitions = serializedRegistry.FindProperty("_definitions");
            if (definitions == null)
                return false;

            if (rebuildFromMapping)
                definitions.ClearArray();

            foreach (var entry in mapping.TerrainLayers)
            {
                if (entry == null || !entry.HasExactId)
                {
                    skipped++;
                    continue;
                }

                int existingIndex = FindDefinitionIndex(definitions, entry.IdPattern);
                if (existingIndex >= 0)
                {
                    if (!updateExisting)
                    {
                        skipped++;
                        continue;
                    }

                    WriteDefinition(definitions.GetArrayElementAtIndex(existingIndex), entry);
                    updated++;
                    continue;
                }

                int newIndex = definitions.arraySize;
                definitions.InsertArrayElementAtIndex(newIndex);
                WriteDefinition(definitions.GetArrayElementAtIndex(newIndex), entry);
                added++;
            }

            serializedRegistry.ApplyModifiedProperties();
            EditorUtility.SetDirty(tileRegistry);
            AssetDatabase.SaveAssets();
            return true;
        }

        private static int FindDefinitionIndex(SerializedProperty definitions, string id)
        {
            for (int i = 0; i < definitions.arraySize; i++)
            {
                var element = definitions.GetArrayElementAtIndex(i);
                var idProperty = element.FindPropertyRelative("_id");
                if (idProperty != null && idProperty.stringValue == id)
                    return i;
            }

            return -1;
        }

        private static void WriteDefinition(SerializedProperty element, TileWorldCreatorIdMappingSO.LayerMapping entry)
        {
            element.FindPropertyRelative("_id").stringValue = entry.IdPattern;
            element.FindPropertyRelative("_movementCost").floatValue = entry.MovementCost;
            element.FindPropertyRelative("_tileWorldCreatorPreset").objectReferenceValue = entry.TilePreset;
            element.FindPropertyRelative("_tileWorldCreatorBlueprintLayerGuid").stringValue = entry.BlueprintLayerGuid;
            element.FindPropertyRelative("_tileWorldCreatorBlueprintLayerName").stringValue = entry.BlueprintLayerName;
            element.FindPropertyRelative("_visualPrefab").objectReferenceValue = entry.RegistryVisualPrefab != null
                ? entry.RegistryVisualPrefab
                : ResolvePresetVisualPrefab(entry.TilePreset);
        }

        private static GameObject ResolvePresetVisualPrefab(TilePreset preset)
        {
            if (preset == null)
                return null;

            if (preset.gridtype == TilePreset.GridType.dual)
            {
                if (preset.DUALGRD_fillTile != null) return preset.DUALGRD_fillTile;
                if (preset.DUALGRD_edgeTile != null) return preset.DUALGRD_edgeTile;
                if (preset.DUALGRD_cornerTile != null) return preset.DUALGRD_cornerTile;
                if (preset.DUALGRD_invertedCornerTile != null) return preset.DUALGRD_invertedCornerTile;
                return preset.DUALGRD_doubleInteriorCornerTile;
            }

            if (preset.NRMGRD_fillTile != null) return preset.NRMGRD_fillTile;
            if (preset.NRMGRD_singleTile != null) return preset.NRMGRD_singleTile;
            if (preset.NRMGRD_edgeFillTile != null) return preset.NRMGRD_edgeFillTile;
            if (preset.NRMGRD_cornerFillTile != null) return preset.NRMGRD_cornerFillTile;
            return preset.NRMGRD_interiorCornerTile;
        }
    }
}