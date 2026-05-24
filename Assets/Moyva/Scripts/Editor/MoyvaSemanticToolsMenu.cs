using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.EditorTools
{
    internal static class MoyvaSemanticToolsMenu
    {
        private static void OpenLegacyMenu(string legacyPath)
        {
            if (!EditorApplication.ExecuteMenuItem(legacyPath))
            {
                Debug.LogWarning($"[MoyvaSemanticToolsMenu] Legacy menu path not found: {legacyPath}");
            }
        }

        // Designers
        [MenuItem("Moyva/Tools/Designers/Developer Hub", priority = 0)]
        private static void OpenDeveloperHubDesigner() => OpenLegacyMenu("Moyva/Tools/Developer Hub");

        [MenuItem("Moyva/Tools/Designers/Project Settings", priority = 9)]
        private static void OpenProjectSettings() => OpenLegacyMenu("Moyva/Project/Global Settings");

        [MenuItem("Moyva/Tools/Designers/Unit Designer", priority = 10)]
        private static void OpenUnitDesigner() => OpenLegacyMenu("Moyva/Tools/Unit Designer");

        [MenuItem("Moyva/Tools/Designers/Building Designer", priority = 11)]
        private static void OpenBuildingDesigner() => OpenLegacyMenu("Moyva/Tools/Building Designer");

        [MenuItem("Moyva/Tools/Designers/Economy Designer", priority = 12)]
        private static void OpenEconomyDesigner() => OpenLegacyMenu("Moyva/Tools/Редактор Економіки");

        [MenuItem("Moyva/Tools/Designers/Audio Designer", priority = 13)]
        private static void OpenAudioDesigner() => OpenLegacyMenu("Moyva/Tools/Audio Designer");

        [MenuItem("Moyva/Tools/Designers/Save System Designer", priority = 14)]
        private static void OpenSaveSystemDesigner() => OpenLegacyMenu("Moyva/Save System/Designer Tool");

        [MenuItem("Moyva/Tools/Designers/Graph Editor", priority = 15)]
        private static void OpenGraphEditor() => OpenLegacyMenu("Moyva/Graph Editor");

        [MenuItem("Moyva/Tools/Designers/Multiplayer Config Hub", priority = 16)]
        private static void OpenMultiplayerConfigHub() => OpenLegacyMenu("Moyva/Multiplayer/Config Hub");

        [MenuItem("Moyva/Tools/Designers/Calendar Config Hub", priority = 17)]
        private static void OpenCalendarConfigHub() => OpenLegacyMenu("Moyva/Calendar/Config Hub");

        [MenuItem("Moyva/Tools/Designers/World Defaults", priority = 18)]
        private static void OpenWorldDefaults() => OpenLegacyMenu("Moyva/World/Базові налаштування світу");

        [MenuItem("Moyva/Tools/Designers/Bootstrap Installer Config", priority = 19)]
        private static void OpenBootstrapInstallerConfig() => OpenLegacyMenu("Moyva/Bootstrap/Installer Config Editor");

        // Validation
        [MenuItem("Moyva/Tools/Validation/Topology Resolver", priority = 200)]
        private static void OpenTopologyResolver() => OpenLegacyMenu("Moyva/Construction/Topology Resolver Editor");

        [MenuItem("Moyva/Tools/Validation/Construction UI Setup", priority = 201)]
        private static void OpenConstructionUiSetup() => OpenLegacyMenu("Moyva/Construction/UI Setup Tool");

        [MenuItem("Moyva/Tools/Validation/Wall Registry Editor", priority = 202)]
        private static void OpenWallRegistryEditor() => OpenLegacyMenu("Moyva/Construction/Wall Registry Editor");

        [MenuItem("Moyva/Tools/Validation/Fog Vision Tuner", priority = 203)]
        private static void OpenFogVisionTuner() => OpenLegacyMenu("Moyva/Tools/Fog of War/Vision Tuner");

        // Diagnostics
        [MenuItem("Moyva/Tools/Diagnostics/Registry Hub", priority = 400)]
        private static void OpenRegistryHub() => OpenLegacyMenu("Moyva/Tools/Registry Hub");

        [MenuItem("Moyva/Tools/Diagnostics/Registry Factory (Legacy)", priority = 401)]
        private static void OpenRegistryFactory() => OpenLegacyMenu("Moyva/Tools/Registry Factory (Legacy)");

        [MenuItem("Moyva/Tools/Diagnostics/Gameplay Startup Graphics", priority = 402)]
        private static void OpenGameplayStartupGraphics() => OpenLegacyMenu("Moyva/Tools/Gameplay Startup Graphics");

        [MenuItem("Moyva/Tools/Diagnostics/Player Spawn Preview", priority = 403)]
        private static void OpenPlayerSpawnPreview() => OpenLegacyMenu("Moyva/Bootstrap/Дизайнер стартового спавну");

        [MenuItem("Moyva/Tools/Diagnostics/Prefab 2D Preview Setup", priority = 404)]
        private static void OpenPrefabPreviewSetup() => OpenLegacyMenu("Moyva/Windows/Prefab 2D Preview Setup");

        [MenuItem("Moyva/Tools/Diagnostics/Force 2D on Selected Prefab", priority = 405)]
        private static void OpenForce2DOnSelectedPrefab() => OpenLegacyMenu("Moyva/Setup/Force 2D on Selected Prefab");

        [MenuItem("Moyva/Tools/Diagnostics/Gizmo/Show Sprite Bounds", priority = 406)]
        private static void ShowSpriteBoundsGizmo() => OpenLegacyMenu("Moyva/Gizmo/Show Sprite Bounds");

        [MenuItem("Moyva/Tools/Diagnostics/Gizmo/Hide 3D Bounds", priority = 407)]
        private static void Hide3DBoundsGizmo() => OpenLegacyMenu("Moyva/Gizmo/Hide 3D Bounds");
    }
}
