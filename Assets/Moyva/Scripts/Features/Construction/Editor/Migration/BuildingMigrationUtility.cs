using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public readonly struct BuildingPlacementModuleMigrationResult
    {
        public BuildingPlacementModuleMigrationResult(
            bool migrated,
            int previousVersion,
            int currentVersion,
            int addedModuleCount)
        {
            Migrated = migrated;
            PreviousVersion = previousVersion;
            CurrentVersion = currentVersion;
            AddedModuleCount = addedModuleCount;
        }

        public bool Migrated { get; }
        public int PreviousVersion { get; }
        public int CurrentVersion { get; }
        public int AddedModuleCount { get; }
    }

    /// <summary>
    /// Explicit editor migration from the compatibility placement fields to
    /// authoritative placement modules. Runtime never invokes this migration.
    /// </summary>
    public static class BuildingPlacementModuleMigrationUtility
    {
        public const int CurrentVersion = 1;

        private const string VersionPropertyName =
            "_placementModuleMigrationVersion";

        public static bool RequiresMigration(BuildingDefinitionAsset asset)
        {
            return asset != null
                && asset.PlacementModuleMigrationVersion < CurrentVersion;
        }

        public static BuildingPlacementModuleMigrationResult Migrate(
            BuildingDefinitionAsset asset,
            bool registerUndo = true)
        {
            if (asset == null)
            {
                return new BuildingPlacementModuleMigrationResult(
                    migrated: false,
                    previousVersion: 0,
                    currentVersion: CurrentVersion,
                    addedModuleCount: 0);
            }

            int previousVersion = asset.PlacementModuleMigrationVersion;
            if (previousVersion >= CurrentVersion)
            {
                return new BuildingPlacementModuleMigrationResult(
                    migrated: false,
                    previousVersion,
                    previousVersion,
                    addedModuleCount: 0);
            }

            if (registerUndo)
            {
                Undo.RegisterCompleteObjectUndo(
                    asset,
                    "Migrate Building Placement Rules");
            }

            asset.Modules ??= new List<BuildingModuleDefinition>();
            BuildingPlacementRules legacy =
                asset.Placement ?? new BuildingPlacementRules();
            int addedModuleCount = 0;

            if (!ContainsModule<TerrainPlacementRuleModule>(asset.Modules)
                && HasLegacyTerrainOverride(legacy))
            {
                asset.Modules.Add(CreateTerrainModule(asset, legacy));
                addedModuleCount++;
            }

            if (legacy.CanPlaceInFog
                && !ContainsModule<FogPlacementRuleModule>(asset.Modules))
            {
                asset.Modules.Add(new FogPlacementRuleModule
                {
                    MergeMode = PlacementRuleMergeMode.Override,
                    Visibility = legacy.CanPlaceInFog
                        ? FogPlacementVisibility.Any
                        : FogPlacementVisibility.Visible,
                });
                addedModuleCount++;
            }

            if (!ContainsModule<
                    SettlementInfluenceRequirementBuildingModule>(
                    asset.Modules))
            {
                asset.Modules.Add(
                    CreateSettlementInfluenceRequirementModule(legacy));
                addedModuleCount++;
            }

            if (legacy.CreatesSettlementInfluence
                && !ContainsModule<SettlementCenterBuildingModule>(
                    asset.Modules))
            {
                asset.Modules.Add(new SettlementCenterBuildingModule
                {
                    InfluenceRadius = Mathf.Max(
                        0,
                        legacy.InfluenceRadius),
                    MinimumDistanceFromOtherCenters = Mathf.Max(
                        0,
                        legacy.MinDistanceFromSettlementCenters),
                });
                addedModuleCount++;
            }

            TileRequirementDefinition[] requirements =
                BuildTileRequirements(legacy);
            if (requirements.Length > 0
                && !ContainsModule<TileRequirementBuildingModule>(
                    asset.Modules))
            {
                asset.Modules.Add(new TileRequirementBuildingModule
                {
                    Requirements = requirements,
                    MergeMode = PlacementRuleMergeMode.Override,
                });
                addedModuleCount++;
            }

            SetMigrationVersion(asset, CurrentVersion);
            asset.NotifyEditorDataChanged();
            EditorUtility.SetDirty(asset);

            return new BuildingPlacementModuleMigrationResult(
                migrated: true,
                previousVersion,
                CurrentVersion,
                addedModuleCount);
        }

        private static TerrainPlacementRuleModule CreateTerrainModule(
            BuildingDefinitionAsset asset,
            BuildingPlacementRules legacy)
        {
            return new TerrainPlacementRuleModule
            {
                MergeMode = PlacementRuleMergeMode.Override,
                AllowedTerrainIds = CloneStrings(
                    legacy.RequiredTerrainIds),
                RequiredNeighborOffsets =
                    legacy.RequiredNeighborOffsets != null
                        ? (Vector2Int[])legacy.RequiredNeighborOffsets
                            .Clone()
                        : Array.Empty<Vector2Int>(),
                AllowHills = true,
                RequiresFlatGround =
                    asset.Footprint?.RequiresFlatGround == true,
            };
        }

        private static SettlementInfluenceRequirementBuildingModule
            CreateSettlementInfluenceRequirementModule(
                BuildingPlacementRules legacy)
        {
            bool hasInfluenceRule =
                legacy.RequiresSettlementInfluence
                || legacy.BlockIfSettlementCenterInRange;
            return new SettlementInfluenceRequirementBuildingModule
            {
                MergeMode = hasInfluenceRule
                    ? PlacementRuleMergeMode.Override
                    : PlacementRuleMergeMode.Disabled,
                RequiresInfluence =
                    legacy.RequiresSettlementInfluence,
                BlockOverlappingCenters =
                    legacy.BlockIfSettlementCenterInRange,
                MaximumDistanceToCenter =
                    legacy.RequiresSettlementInfluence
                        ? ResolveLegacyProximityRadius(legacy)
                        : 0,
            };
        }

        private static int ResolveLegacyProximityRadius(
            BuildingPlacementRules legacy)
        {
            return legacy.MinDistanceFromSettlementCenters > 0
                ? legacy.MinDistanceFromSettlementCenters
                : Mathf.Max(0, legacy.InfluenceRadius);
        }

        private static TileRequirementDefinition[] BuildTileRequirements(
            BuildingPlacementRules legacy)
        {
            var result = new List<TileRequirementDefinition>();
            TileRequirementDefinition[] explicitRequirements =
                legacy.NearbyTileRequirements;
            if (explicitRequirements != null)
            {
                for (int index = 0;
                     index < explicitRequirements.Length;
                     index++)
                {
                    TileRequirementDefinition requirement =
                        explicitRequirements[index];
                    if (requirement == null)
                        continue;

                    result.Add(CloneRequirement(requirement));
                }
            }

            AddSemanticRequirement(
                result,
                legacy.RequiresWaterNearby,
                "water");
            AddSemanticRequirement(
                result,
                legacy.RequiresForestNearby,
                "forest");
            AddSemanticRequirement(
                result,
                legacy.RequiresMountainNearby,
                "mountain");
            AddSemanticRequirement(
                result,
                legacy.RequiresRoadNearby,
                "road");
            return result.ToArray();
        }

        private static void AddSemanticRequirement(
            List<TileRequirementDefinition> target,
            bool enabled,
            string terrainTag)
        {
            if (!enabled
                || ContainsEquivalentSemanticRequirement(
                    target,
                    terrainTag))
            {
                return;
            }

            target.Add(new TileRequirementDefinition
            {
                TerrainTag = terrainTag,
                Radius = 1,
                MinimumTileCount = 1,
            });
        }

        private static bool ContainsEquivalentSemanticRequirement(
            IReadOnlyList<TileRequirementDefinition> requirements,
            string terrainTag)
        {
            for (int index = 0; index < requirements.Count; index++)
            {
                TileRequirementDefinition requirement =
                    requirements[index];
                if (requirement != null
                    && string.Equals(
                        requirement.TerrainTag?.Trim(),
                        terrainTag,
                        StringComparison.OrdinalIgnoreCase)
                    && Mathf.Max(0, requirement.Radius) == 1
                    && Mathf.Max(1, requirement.MinimumTileCount) == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static TileRequirementDefinition CloneRequirement(
            TileRequirementDefinition source)
        {
            return new TileRequirementDefinition
            {
                TileId = source.TileId,
                TerrainTag = source.TerrainTag,
                Radius = source.Radius,
                MinimumTileCount = source.MinimumTileCount,
            };
        }

        private static bool HasLegacyTerrainOverride(
            BuildingPlacementRules legacy)
        {
            // Flat-ground remains a footprint invariant and neighbor offsets
            // continue through the compatibility adapter. Neither one may create
            // an Override module on its own, because that would bypass global
            // terrain buildability (for example the water build-block profile).
            return HasValues(legacy.RequiredTerrainIds);
        }

        private static bool HasValues(IReadOnlyList<string> values)
        {
            if (values == null)
                return false;

            for (int index = 0; index < values.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                    return true;
            }

            return false;
        }

        private static string[] CloneStrings(
            IReadOnlyList<string> source)
        {
            if (source == null || source.Count == 0)
                return Array.Empty<string>();

            var result = new string[source.Count];
            for (int index = 0; index < source.Count; index++)
                result[index] = source[index];
            return result;
        }

        private static bool ContainsModule<TModule>(
            IReadOnlyList<BuildingModuleDefinition> modules)
            where TModule : BuildingModuleDefinition
        {
            if (modules == null)
                return false;

            for (int index = 0; index < modules.Count; index++)
            {
                if (modules[index] is TModule)
                    return true;
            }

            return false;
        }

        private static void SetMigrationVersion(
            BuildingDefinitionAsset asset,
            int version)
        {
            var serializedAsset = new SerializedObject(asset);
            SerializedProperty property = serializedAsset.FindProperty(
                VersionPropertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Serialized migration version field '{VersionPropertyName}' was not found.");
            }

            property.intValue = version;
            serializedAsset.ApplyModifiedPropertiesWithoutUndo();
        }
    }

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
