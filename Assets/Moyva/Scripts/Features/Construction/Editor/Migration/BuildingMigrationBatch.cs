using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public static class BuildingMigrationBatch
    {
        private const string DefaultOutputFolder = "Assets/Moyva/Data/ScriptableObjects/Construction/Buildings";

        [MenuItem(
            "Moyva/Tools/Construction/Migrate Placement Rules To Modules",
            priority = 34)]
        public static void MigratePlacementRulesToModules()
        {
            int migratedAssetCount =
                MigrateAllPlacementRulesToModules(saveAssets: true);
            Debug.Log(
                $"[BuildingPlacementMigration] Migrated {migratedAssetCount} building definition asset(s) to version {BuildingPlacementModuleMigrationUtility.CurrentVersion}.");
        }

        /// <summary>
        /// Explicit batch entry point for CI or editor automation.
        /// This method is never called automatically by runtime or import hooks.
        /// </summary>
        public static int MigrateAllPlacementRulesToModules(
            bool saveAssets)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:BuildingDefinitionAsset",
                new[] { "Assets" });
            Array.Sort(guids, StringComparer.Ordinal);

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(
                "Migrate Building Placement Rules");
            int migratedAssetCount = 0;
            try
            {
                for (int index = 0; index < guids.Length; index++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(
                        guids[index]);
                    BuildingDefinitionAsset asset =
                        AssetDatabase.LoadAssetAtPath<
                            BuildingDefinitionAsset>(path);
                    BuildingPlacementModuleMigrationResult result =
                        BuildingPlacementModuleMigrationUtility.Migrate(
                            asset);
                    if (result.Migrated)
                        migratedAssetCount++;
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }

            if (saveAssets && migratedAssetCount > 0)
                AssetDatabase.SaveAssets();
            return migratedAssetCount;
        }

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
