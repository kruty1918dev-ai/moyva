using System;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    public static class BuildingPresetCommandLine
    {
        public static void ApplyAll()
        {
            try
            {
                BuildingPresetBatchResult result=BuildingPresetBatchService.ApplyAll("batchmode");
                Debug.Log($"[MoyvaBuildingPresets] COMMAND_OK pack={result.PackId} changed={result.ChangedCount}");
            }
            catch(Exception ex)
            {
                Debug.LogError($"[MoyvaBuildingPresets] COMMAND_FAILED: {ex}"); EditorApplication.Exit(23);
            }
        }
    }
}
