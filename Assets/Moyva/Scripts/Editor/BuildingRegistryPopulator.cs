using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    /// <summary>
    /// Legacy entry point kept so older menus/tests do not recreate inline building data.
    /// Building data now lives in BuildingDefinition assets and is managed through the Odin Build Designer.
    /// </summary>
    public static class BuildingRegistryPopulator
    {
        [MenuItem("Moyva/Tools/Construction/Open Building Designer", priority = 30)]
        private static void OpenBuildingDesigner()
        {
            EditorApplication.ExecuteMenuItem("Moyva/Tools/Building Designer");
        }

        [MenuItem("Moyva/Tools/Construction/Migrate Legacy Building Registry", priority = 31)]
        private static void OpenMigrationTool()
        {
            if (!EditorApplication.ExecuteMenuItem("Moyva/Tools/Construction/Building Migration"))
                EditorApplication.ExecuteMenuItem("Moyva/Tools/Building Designer");
        }

        [System.Obsolete("Building registries are asset-backed. Use the Odin Build Designer or migration tool instead.")]
        public static void PopulateAndSave(BuildingRegistrySO registry)
        {
            if (registry == null)
                return;

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.LogWarning(
                "[BuildingRegistryPopulator] Legacy inline population is disabled. " +
                "Use BuildingDefinition assets through Moyva/Tools/Building Designer.",
                registry);
        }
    }
}
