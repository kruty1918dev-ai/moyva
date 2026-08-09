using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    [InitializeOnLoad]
    internal static class BuildingPresetAutoApplyCoordinator
    {
        private static bool _running;
        static BuildingPresetAutoApplyCoordinator()
        {
            if (!Application.isBatchMode) EditorApplication.delayCall += TryApply;
        }
        private static void TryApply()
        {
            if (_running || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            _running=true;
            try { BuildingPresetBatchService.ApplyAll("auto-domain-reload"); }
            catch (System.Exception ex) { Debug.LogError($"[MoyvaBuildingPresets] AUTO_APPLY_FAILED: {ex.Message}"); }
            finally { _running=false; }
        }
    }
}
