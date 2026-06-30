using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public static class BuildingMigrationUtility
    {
        public static BuildingMigrationReport MigrateLegacyRegistry(
            BuildingRegistrySO registry,
            string outputFolder,
            bool addLegacyFogRevealModules)
        {
            var report = new BuildingMigrationReport();
            if (registry == null)
            {
                report.Messages.Add("Registry is null.");
                return report;
            }

            outputFolder = NormalizeFolder(outputFolder);
            EnsureFolder(outputFolder);

            var legacy = registry.LegacyBuildings;
            report.LegacyDefinitions = legacy.Length;

            var assets = new List<BuildingDefinitionAsset>(registry.BuildingAssets);
            var assetsById = BuildAssetMap(assets);

            for (int i = 0; i < legacy.Length; i++)
            {
                var definition = legacy[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                {
                    report.Messages.Add($"Skipped legacy entry [{i}] because it is null or has no ID.");
                    continue;
                }

                if (!assetsById.TryGetValue(definition.Id, out var asset) || asset == null)
                {
                    asset = ScriptableObject.CreateInstance<BuildingDefinitionAsset>();
                    asset.ApplyLegacy(definition);
                    MaybeAddLegacyFogReveal(asset, definition, addLegacyFogRevealModules, report);

                    string path = AssetDatabase.GenerateUniqueAssetPath(
                        $"{outputFolder}/{SanitizeFileName(definition.Id)}.asset");
                    AssetDatabase.CreateAsset(asset, path);
                    assets.Add(asset);
                    assetsById[definition.Id] = asset;
                    report.CreatedAssets++;
                    report.Messages.Add($"Created {path}");
                }
                else
                {
                    report.ReusedAssets++;
                    report.Messages.Add($"Reused existing asset for '{definition.Id}'.");
                }
            }

            registry.SetBuildingAssets(assets);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return report;
        }

        private static Dictionary<string, BuildingDefinitionAsset> BuildAssetMap(IEnumerable<BuildingDefinitionAsset> assets)
        {
            var result = new Dictionary<string, BuildingDefinitionAsset>(StringComparer.OrdinalIgnoreCase);
            if (assets == null)
                return result;

            foreach (var asset in assets)
            {
                if (asset == null || string.IsNullOrWhiteSpace(asset.Id))
                    continue;

                if (!result.ContainsKey(asset.Id))
                    result.Add(asset.Id, asset);
            }

            return result;
        }

        private static void MaybeAddLegacyFogReveal(
            BuildingDefinitionAsset asset,
            BuildingDefinition legacy,
            bool enabled,
            BuildingMigrationReport report)
        {
            if (!enabled || asset == null || legacy == null)
                return;

            var runtime = asset.ToRuntimeDefinition();
            if (BuildingDefinitionCapabilities.TryGetFogReveal(runtime, out _))
                return;

            int radius = ResolveLegacyFogRevealRadius(legacy);
            if (radius <= 0)
                return;

            asset.Modules.Add(new FogRevealBuildingModule
            {
                RevealRadius = radius,
                RevealOnBuilt = true,
                RevealWhileActive = true,
                OnlyAfterConstructionComplete = true,
            });
            EditorUtility.SetDirty(asset);
            report.AddedFogRevealModules++;
        }

        private static int ResolveLegacyFogRevealRadius(BuildingDefinition definition)
        {
            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return Math.Max(3, BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0));
            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return Math.Max(2, BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0));
            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return 2;
            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return 1;
            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return 3;
            return 1;
        }

        private static string NormalizeFolder(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
                return "Assets/Moyva/SO/Construction/Buildings";

            outputFolder = outputFolder.Replace('\\', '/').TrimEnd('/');
            return outputFolder.StartsWith("Assets/", StringComparison.Ordinal)
                ? outputFolder
                : "Assets/Moyva/SO/Construction/Buildings";
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '-');
            return string.IsNullOrWhiteSpace(value) ? "BuildingDefinition" : value.Trim();
        }
    }
}
