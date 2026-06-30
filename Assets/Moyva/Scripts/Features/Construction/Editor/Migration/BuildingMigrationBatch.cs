using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public static class BuildingMigrationBatch
    {
        private const string DefaultOutputFolder = "Assets/Moyva/SO/Construction/Buildings";

        public static void MigrateFirstRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingRegistrySO");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogError("[BuildingMigrationBatch] No BuildingRegistrySO asset found.");
                return;
            }

            string registryPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(registryPath);
            var report = BuildingMigrationUtility.MigrateLegacyRegistry(registry, DefaultOutputFolder, addLegacyFogRevealModules: true);
            Debug.Log($"[BuildingMigrationBatch] Migrated '{registryPath}': {report}\n{string.Join("\n", report.Messages)}");
        }
    }
}
