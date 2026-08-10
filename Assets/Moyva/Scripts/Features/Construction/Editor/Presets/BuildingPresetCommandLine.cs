using System;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    public static class BuildingPresetCommandLine
    {
        [MenuItem("Moyva/Construction/JSON Presets/Apply All")]
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

        [MenuItem("Moyva/Construction/JSON Presets/Validate All")]
        public static void ValidateAll()
        {
            try
            {
                BuildingPresetBatchResult result =
                    BuildingPresetBatchService.ValidatePack("editor-menu");
                Debug.Log(
                    $"[MoyvaBuildingPresets] VALIDATION_OK pack={result.PackId} presets={result.PresetCount}");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MoyvaBuildingPresets] VALIDATION_FAILED: {ex}");
            }
        }
    }
}
