using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class BuildingDefinitionAssetRegistryTests
    {
        [Test]
        public void Registry_PrefersAssetDefinitionOverLegacyInlineDefinition()
        {
            var asset = ScriptableObject.CreateInstance<BuildingDefinitionAsset>();
            var registry = ScriptableObject.CreateInstance<BuildingRegistrySO>();
            try
            {
                asset.Identity.Id = "house";
                asset.Identity.DisplayName = "Asset House";
                asset.Identity.Category = BuildingCategory.Civilian;
                asset.Footprint.Size = new Vector2Int(2, 1);
                asset.Footprint.Anchor = BuildingFootprintAnchor.SouthWest;
                asset.Footprint.OccupiedCells = new[] { Vector2Int.zero, Vector2Int.right };
                asset.Placement.RequiredTerrainIds = new[] { "grass" };
                asset.Modules.Add(new FogRevealBuildingModule { RevealRadius = 2 });

                registry.Buildings = new[]
                {
                    new BuildingDefinition
                    {
                        Id = "house",
                        DisplayName = "Legacy House",
                        Category = BuildingCategory.Military,
                    },
                };
                registry.SetBuildingAssets(new[] { asset });

                var definition = registry.GetById("house");

                Assert.NotNull(definition);
                Assert.AreEqual("Asset House", definition.DisplayName);
                Assert.AreEqual(BuildingCategory.Civilian, definition.Category);
                CollectionAssert.AreEqual(new[] { "grass" }, definition.RequiredTerrainIds);
                Assert.AreEqual(new Vector2Int(2, 1), definition.Footprint.Size);
                CollectionAssert.AreEqual(
                    new[] { Vector2Int.zero, Vector2Int.right },
                    definition.Footprint.OccupiedCells);
                Assert.AreNotSame(asset.Footprint, definition.Footprint);
                Assert.AreNotSame(asset.Footprint.OccupiedCells, definition.Footprint.OccupiedCells);
                Assert.AreEqual(2, BuildingDefinitionCapabilities.GetFogRevealRadius(definition));
                Assert.AreEqual(1, registry.GetAll().Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
                UnityEngine.Object.DestroyImmediate(registry);
            }
        }

        [Test]
        public void ApplyLegacy_CopiesFootprintWithoutSharingArrays()
        {
            var asset = ScriptableObject.CreateInstance<BuildingDefinitionAsset>();
            try
            {
                var legacy = new BuildingDefinition
                {
                    Id = "legacy-wide",
                    DisplayName = "Legacy Wide",
                    Footprint = new BuildingFootprint
                    {
                        Size = new Vector2Int(3, 1),
                        Anchor = BuildingFootprintAnchor.Custom,
                        CustomAnchor = Vector2Int.right,
                        OccupiedCells = new[] { Vector2Int.zero, Vector2Int.right },
                        EntranceCells = new[] { new Vector2Int(2, 0) },
                    },
                };

                asset.ApplyLegacy(legacy);

                Assert.AreEqual(legacy.Footprint.Size, asset.Footprint.Size);
                Assert.AreEqual(legacy.Footprint.Anchor, asset.Footprint.Anchor);
                CollectionAssert.AreEqual(legacy.Footprint.OccupiedCells, asset.Footprint.OccupiedCells);
                CollectionAssert.AreEqual(legacy.Footprint.EntranceCells, asset.Footprint.EntranceCells);
                Assert.AreNotSame(legacy.Footprint, asset.Footprint);
                Assert.AreNotSame(legacy.Footprint.OccupiedCells, asset.Footprint.OccupiedCells);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void PlacementEvaluator_AllowsFog_WhenDefinitionCanPlaceInFog()
        {
            var registry = new TestRegistry(new BuildingDefinition
            {
                Id = "scout-camp",
                DisplayName = "Scout Camp",
                CanPlaceInFog = true,
                UseCustomTownHallRules = true,
                RequireTownHallInRange = false,
            });

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = "scout-camp",
                Position = Vector2Int.zero,
                IsFogBlocked = _ => true,
            });

            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.FogBlocked);
        }

        private sealed class TestRegistry : IBuildingRegistry
        {
            private readonly Dictionary<string, BuildingDefinition> _definitions = new Dictionary<string, BuildingDefinition>(StringComparer.Ordinal);

            public TestRegistry(params BuildingDefinition[] definitions)
            {
                foreach (var definition in definitions)
                {
                    if (definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                        _definitions[definition.Id] = definition;
                }
            }

            public BuildingDefinition[] GetAll()
            {
                var result = new BuildingDefinition[_definitions.Count];
                _definitions.Values.CopyTo(result, 0);
                return result;
            }

            public BuildingDefinition GetById(string id)
                => id != null && _definitions.TryGetValue(id, out var definition) ? definition : null;

            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.FindAll(GetAll(), definition => definition != null && definition.Category == category);

            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();

            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
                => null;
        }
    }
}
