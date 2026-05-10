using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class BuildingPlacementEvaluatorTests
    {
        private sealed class TestBuildingRegistry : IBuildingRegistry
        {
            private readonly Dictionary<string, BuildingDefinition> _definitions = new Dictionary<string, BuildingDefinition>(StringComparer.Ordinal);

            public TestBuildingRegistry(params BuildingDefinition[] definitions)
            {
                for (int index = 0; index < definitions.Length; index++)
                {
                    var definition = definitions[index];
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
            {
                return id != null && _definitions.TryGetValue(id, out var definition) ? definition : null;
            }

            public BuildingDefinition[] GetByCategory(BuildingCategory category)
            {
                var result = new List<BuildingDefinition>();
                foreach (var definition in _definitions.Values)
                {
                    if (definition.Category == category)
                        result.Add(definition);
                }

                return result.ToArray();
            }

            public WallCollectionDefinition[] GetWallCollections()
            {
                return Array.Empty<WallCollectionDefinition>();
            }

            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
            {
                return null;
            }
        }

        [Test]
        public void Evaluate_NonCenterBuilding_RequiresInfluenceCenterInRange()
        {
            var registry = new TestBuildingRegistry(TownHall("town-hall", 2), House("house"));
            var occupants = new Dictionary<Vector2Int, string>
            {
                [Vector2Int.zero] = "town-hall",
            };

            var inRange = Evaluate(registry, "house", new Vector2Int(2, 0), occupants);
            var outOfRange = Evaluate(registry, "house", new Vector2Int(3, 0), occupants);

            Assert.IsTrue(inRange.IsValid);
            Assert.IsFalse(outOfRange.IsValid);
            Assert.IsTrue(outOfRange.InfluenceZoneBlocked);
            Assert.AreEqual(BuildingPlacementBlockerKind.InfluenceRequired, outOfRange.Blockers[0].Kind);
        }

        [Test]
        public void Evaluate_InfluenceCenter_BlocksOverlappingCenterRadius()
        {
            var registry = new TestBuildingRegistry(TownHall("town-hall", 2));
            var occupants = new Dictionary<Vector2Int, string>
            {
                [Vector2Int.zero] = "town-hall",
            };

            var overlapping = Evaluate(registry, "town-hall", new Vector2Int(4, 0), occupants);
            var separated = Evaluate(registry, "town-hall", new Vector2Int(5, 0), occupants);

            Assert.IsFalse(overlapping.IsValid);
            Assert.IsTrue(overlapping.InfluenceZoneBlocked);
            Assert.AreEqual(BuildingPlacementBlockerKind.InfluenceOverlap, overlapping.Blockers[0].Kind);
            Assert.IsTrue(separated.IsValid);
        }

        [Test]
        public void Evaluate_PendingInfluenceCenter_CoversSameSessionBuilding()
        {
            var registry = new TestBuildingRegistry(TownHall("town-hall", 3), House("house"));
            var pending = new[]
            {
                new BuildingPlacementSimulationEntry(new Vector2Int(5, 5), "town-hall"),
            };

            var result = Evaluate(
                registry,
                "house",
                new Vector2Int(8, 5),
                new Dictionary<Vector2Int, string>(),
                pendingPlacements: pending);

            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.InfluenceZoneBlocked);
        }

        [Test]
        public void Evaluate_ReportsSpacingBeforeFog()
        {
            var registry = new TestBuildingRegistry(House("house"));
            var occupants = new Dictionary<Vector2Int, string>
            {
                [new Vector2Int(1, 0)] = "blocker",
            };

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = "house",
                Position = Vector2Int.zero,
                MinSpacing = 1,
                TownHallBuildRadius = 0,
                IsOccupied = occupants.ContainsKey,
                GetOccupantId = position => occupants.TryGetValue(position, out var buildingId) ? buildingId : null,
                IsFogBlocked = _ => true,
            });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.SpacingBlocked);
            Assert.IsFalse(result.FogBlocked);
            Assert.AreEqual(BuildingPlacementBlockerKind.Spacing, result.Blockers[0].Kind);
        }

        private static BuildingPlacementEvaluationResult Evaluate(
            IBuildingRegistry registry,
            string buildingId,
            Vector2Int position,
            Dictionary<Vector2Int, string> occupants,
            IReadOnlyList<BuildingPlacementSimulationEntry> pendingPlacements = null)
        {
            return BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = buildingId,
                Position = position,
                MinSpacing = 0,
                TownHallBuildRadius = 2,
                IsOccupied = occupants.ContainsKey,
                GetOccupantId = tile => occupants.TryGetValue(tile, out var occupantId) ? occupantId : null,
                PendingPlacements = pendingPlacements,
            });
        }

        private static BuildingDefinition House(string id)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = id,
                Category = BuildingCategory.Civilian,
            };
        }

        private static BuildingDefinition TownHall(string id, int radius)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = id,
                Category = BuildingCategory.Civilian,
                RequireTownHallInRange = false,
                BlockIfTownHallAlreadyInRange = true,
                Modules = new List<BuildingModuleDefinition>
                {
                    new TownHallBuildingModule
                    {
                        BuildRadius = radius,
                    },
                },
            };
        }
    }
}