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
