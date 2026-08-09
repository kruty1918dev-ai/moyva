using System;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    internal sealed class BuildingPresetAutoApplyCoordinator : AssetPostprocessor
    {
        private static bool _running;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (Application.isBatchMode
                || (!ContainsBuildingJson(importedAssets) && !ContainsBuildingJson(movedAssets)))
                return;

            EditorApplication.delayCall -= TryApply;
            EditorApplication.delayCall += TryApply;
        }

        private static void TryApply()
        {
            if (_running || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall -= TryApply;
                EditorApplication.delayCall += TryApply;
                return;
            }
            _running=true;
            try { BuildingPresetBatchService.ApplyAll("json-import"); }
            catch (System.Exception ex) { Debug.LogError($"[MoyvaBuildingPresets] AUTO_APPLY_FAILED: {ex.Message}"); }
            finally { _running=false; }
        }

        private static bool ContainsBuildingJson(string[] paths)
        {
            if (paths == null)
                return false;

            string prefix = BuildingPresetPaths.PresetRoot + "/";
            for (int index = 0; index < paths.Length; index++)
            {
                string path = paths[index];
                if (!string.IsNullOrWhiteSpace(path)
                    && path.StartsWith(prefix, StringComparison.Ordinal)
                    && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
