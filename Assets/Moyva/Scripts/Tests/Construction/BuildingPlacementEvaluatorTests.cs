#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
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

        private sealed class RecordingRuleEvaluator :
            IBuildingPlacementRuleEvaluator
        {
            private readonly string _name;
            private readonly List<string> _calls;
            private readonly BuildingPlacementBlocker _blocker;

            public RecordingRuleEvaluator(
                string name,
                List<string> calls,
                BuildingPlacementBlocker blocker = null)
            {
                _name = name;
                _calls = calls;
                _blocker = blocker;
            }

            public BuildingPlacementBlocker Evaluate(
                BuildingPlacementEvaluationRequest request,
                BuildingDefinition definition)
            {
                _calls.Add(_name);
                return _blocker;
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
        public void Evaluate_InfluenceCenter_AllowsNearbyCenter_WhenOverlapRuleIsDisabled()
        {
            BuildingDefinition center = TownHall("town-hall", 2);
            center.BlockIfTownHallAlreadyInRange = false;
            var registry = new TestBuildingRegistry(center);
            var occupants = new Dictionary<Vector2Int, string>
            {
                [Vector2Int.zero] = center.Id,
            };

            var result = Evaluate(registry, center.Id, Vector2Int.right, occupants);

            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.InfluenceZoneBlocked);
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

        [Test]
        public void Evaluate_BlocksWhenCurrentTileDoesNotMatchRequiredTerrain()
        {
            var registry = new TestBuildingRegistry(new BuildingDefinition
            {
                Id = "fisher-hut",
                DisplayName = "Fisher Hut",
                Category = BuildingCategory.Civilian,
                RequiredTerrainIds = new[] { "water" },
                UseCustomTownHallRules = true,
                RequireTownHallInRange = false,
            });

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = "fisher-hut",
                Position = Vector2Int.zero,
                IsOccupied = _ => false,
                GetTileId = _ => "grass",
            });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TerrainBlocked);
            Assert.AreEqual(BuildingPlacementBlockerKind.Terrain, result.Blockers[0].Kind);
        }

        [Test]
        public void CanPlace_MatchesDetailedEvaluation()
        {
            var registry = new TestBuildingRegistry(House("house"));
            var occupied = new HashSet<Vector2Int> { Vector2Int.right };
            var request = new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = "house",
                Position = Vector2Int.zero,
                MinSpacing = 1,
                IsOccupied = occupied.Contains,
                GetOccupantId = position => occupied.Contains(position) ? "blocker" : null,
                GetTileId = _ => "grass",
            };

            Assert.AreEqual(
                BuildingPlacementEvaluator.Evaluate(request).IsValid,
                BuildingPlacementEvaluator.CanPlace(request));

            occupied.Clear();
            Assert.AreEqual(
                BuildingPlacementEvaluator.Evaluate(request).IsValid,
                BuildingPlacementEvaluator.CanPlace(request));
        }

        [Test]
        public void Evaluate_FootprintCrossingMapBoundary_ReportsExactBlockedCell()
        {
            var definition = WideBuilding("warehouse", new Vector2Int(2, 2));
            var registry = new TestBuildingRegistry(definition);

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = new Vector2Int(1, 0),
                IsOccupied = _ => false,
                IsTerrainBlocked = position => position.x >= 2,
                SkipInfluenceRules = true,
            });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TerrainBlocked);
            CollectionAssert.AreEqual(
                new[]
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                },
                result.FootprintPositions);
            Assert.AreEqual(new Vector2Int(2, 0), result.Blockers[0].Position);
        }

        [Test]
        public void Evaluate_PendingWideFootprint_BlocksCandidateOnSecondaryCell()
        {
            var wide = WideBuilding("wide", new Vector2Int(2, 1));
            var candidate = House("house");
            var registry = new TestBuildingRegistry(wide, candidate);

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = candidate.Id,
                Position = new Vector2Int(11, 10),
                IsOccupied = _ => false,
                PendingPlacements = new[]
                {
                    new BuildingPlacementSimulationEntry(new Vector2Int(10, 10), wide.Id),
                },
                SkipInfluenceRules = true,
            });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TileOccupied);
            Assert.AreEqual(wide.Id, result.Blockers[0].BuildingId);
            Assert.AreEqual(new Vector2Int(11, 10), result.Blockers[0].Position);
        }

        [Test]
        public void Evaluate_FlatFootprint_BlocksDifferentTerrainLevel()
        {
            var definition = WideBuilding("keep", new Vector2Int(2, 2));
            var registry = new TestBuildingRegistry(definition);

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                IsOccupied = _ => false,
                IsTerrainBlocked = _ => false,
                GetTerrainLevel = position => position == new Vector2Int(1, 1) ? 2 : 1,
                SkipInfluenceRules = true,
            });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TerrainBlocked);
            Assert.AreEqual(new Vector2Int(1, 1), result.Blockers[0].Position);
        }

        [Test]
        public void Evaluate_DuplicateFootprintCell_IsConfigurationBlocked()
        {
            var definition = WideBuilding("invalid", new Vector2Int(2, 1));
            definition.Footprint.OccupiedCells = new[] { Vector2Int.zero, Vector2Int.zero };
            var registry = new TestBuildingRegistry(definition);

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                IsOccupied = _ => false,
                SkipInfluenceRules = true,
            });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ConfigurationBlocked);
            Assert.AreEqual(BuildingPlacementBlockerKind.Configuration, result.Blockers[0].Kind);
        }

        [Test]
        public void Evaluate_TileRequirementModule_UsesAndSemanticsAndDistinctWholeFootprintTiles()
        {
            BuildingDefinition definition = WideBuilding(
                "bridge",
                new Vector2Int(2, 1));
            definition.Modules.Add(new TileRequirementBuildingModule
            {
                Requirements = new[]
                {
                    new TileRequirementDefinition
                    {
                        TerrainTag = "water",
                        Radius = 1,
                        MinimumTileCount = 2,
                    },
                    new TileRequirementDefinition
                    {
                        TerrainTag = "road",
                        Radius = 1,
                        MinimumTileCount = 1,
                    },
                },
            });
            var registry = new TestBuildingRegistry(definition);
            var water = new HashSet<Vector2Int>
            {
                // This cell is inside the Chebyshev neighborhood of both
                // footprint cells and must only be counted once.
                new Vector2Int(0, 1),
            };
            var roads = new HashSet<Vector2Int> { new Vector2Int(2, 0) };
            BuildingPlacementEvaluationRequest request = new()
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                TileExists = _ => true,
                IsOccupied = _ => false,
                HasTerrainTag = (position, tag) =>
                    (tag == "water" && water.Contains(position))
                    || (tag == "road" && roads.Contains(position)),
                SkipInfluenceRules = true,
            };

            BuildingPlacementEvaluationResult oneDistinctWater =
                BuildingPlacementEvaluator.Evaluate(request);
            Assert.IsFalse(oneDistinctWater.IsValid);
            Assert.IsTrue(oneDistinctWater.AdjacencyBlocked);

            water.Add(new Vector2Int(2, 1));
            Assert.IsTrue(BuildingPlacementEvaluator.Evaluate(request).IsValid);

            roads.Clear();
            BuildingPlacementEvaluationResult missingSecondRequirement =
                BuildingPlacementEvaluator.Evaluate(request);
            Assert.IsFalse(missingSecondRequirement.IsValid);
            Assert.AreEqual(
                BuildingPlacementBlockerKind.Adjacency,
                missingSecondRequirement.Blockers[0].Kind);
        }

        [Test]
        public void Evaluate_DisabledTileRequirementModule_SuppressesLegacyRequirements()
        {
            BuildingDefinition definition = House("free-placement");
            definition.PlacementRules = new BuildingPlacementRules
            {
                NearbyTileRequirements = new[]
                {
                    new TileRequirementDefinition
                    {
                        TerrainTag = "water",
                        Radius = 1,
                        MinimumTileCount = 1,
                    },
                },
            };
            definition.Modules.Add(new TileRequirementBuildingModule
            {
                MergeMode = PlacementRuleMergeMode.Disabled,
            });
            var registry = new TestBuildingRegistry(definition);

            BuildingPlacementEvaluationResult result =
                BuildingPlacementEvaluator.Evaluate(
                    new BuildingPlacementEvaluationRequest
                    {
                        BuildingRegistry = registry,
                        BuildingId = definition.Id,
                        Position = Vector2Int.zero,
                        TileExists = _ => true,
                        IsOccupied = _ => false,
                        HasTerrainTag = (_, _) => false,
                        SkipInfluenceRules = true,
                    });

            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.AdjacencyBlocked);
        }

        [Test]
        public void Evaluate_TerrainOverride_ReplacesGlobalTerrainPolicy()
        {
            BuildingDefinition definition = House("quarry");
            definition.Modules.Add(new TerrainPlacementRuleModule
            {
                MergeMode = PlacementRuleMergeMode.Override,
                AllowedTerrainTags = new[] { "land" },
                AllowedTerrainLevels = new[] { 1 },
                AllowHills = true,
            });
            var registry = new TestBuildingRegistry(definition);
            int terrainLevel = 1;
            BuildingPlacementEvaluationRequest request = new()
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                TileExists = _ => true,
                IsOccupied = _ => false,
                IsTerrainBlocked = _ => true,
                GetTileId = _ => "raised-grass",
                GetTerrainLevel = _ => terrainLevel,
                HasTerrainTag = (_, tag) => tag == "land",
                SkipInfluenceRules = true,
            };

            Assert.IsTrue(
                BuildingPlacementEvaluator.Evaluate(request).IsValid,
                "Override must replace the global terrain callback.");

            terrainLevel = 2;
            BuildingPlacementEvaluationResult blocked =
                BuildingPlacementEvaluator.Evaluate(request);
            Assert.IsFalse(blocked.IsValid);
            Assert.IsTrue(blocked.TerrainBlocked);
        }

        [Test]
        public void Evaluate_DisabledTerrainRule_DoesNotDisableTechnicalMapBounds()
        {
            BuildingDefinition definition = WideBuilding(
                "edge-building",
                new Vector2Int(2, 1));
            definition.Modules.Add(new TerrainPlacementRuleModule
            {
                MergeMode = PlacementRuleMergeMode.Disabled,
            });
            var registry = new TestBuildingRegistry(definition);

            BuildingPlacementEvaluationResult result =
                BuildingPlacementEvaluator.Evaluate(
                    new BuildingPlacementEvaluationRequest
                    {
                        BuildingRegistry = registry,
                        BuildingId = definition.Id,
                        Position = Vector2Int.zero,
                        TileExists = position => position == Vector2Int.zero,
                        IsOccupied = _ => false,
                        IsTerrainBlocked = _ => true,
                        SkipInfluenceRules = true,
                    });

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TerrainBlocked);
            Assert.AreEqual(Vector2Int.right, result.Blockers[0].Position);
        }

        [Test]
        public void Evaluate_FogModule_AllowsExploredButNotUnexplored()
        {
            BuildingDefinition definition = House("scout-post");
            definition.Modules.Add(new FogPlacementRuleModule
            {
                MergeMode = PlacementRuleMergeMode.Override,
                Visibility = FogPlacementVisibility.ExploredOrVisible,
            });
            var registry = new TestBuildingRegistry(definition);
            FogStateType fogState = FogStateType.Explored;
            BuildingPlacementEvaluationRequest request = new()
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                IsOccupied = _ => false,
                GetFogState = _ => fogState,
                SkipInfluenceRules = true,
            };

            Assert.IsTrue(BuildingPlacementEvaluator.Evaluate(request).IsValid);

            fogState = FogStateType.Unexplored;
            BuildingPlacementEvaluationResult blocked =
                BuildingPlacementEvaluator.Evaluate(request);
            Assert.IsFalse(blocked.IsValid);
            Assert.IsTrue(blocked.FogBlocked);
        }

        [Test]
        public void Evaluate_SpacingModule_DisabledAndOverrideAreAuthoritative()
        {
            BuildingDefinition definition = House("house");
            var spacing = new SpacingPlacementRuleModule
            {
                MergeMode = PlacementRuleMergeMode.Disabled,
                MinimumSpacing = 1,
            };
            definition.Modules.Add(spacing);
            var registry = new TestBuildingRegistry(definition);
            var occupied = new HashSet<Vector2Int> { Vector2Int.right };
            BuildingPlacementEvaluationRequest request = new()
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                MinSpacing = 2,
                IsOccupied = occupied.Contains,
                GetOccupantId = position =>
                    occupied.Contains(position) ? "neighbor" : null,
                SkipInfluenceRules = true,
            };

            Assert.IsTrue(BuildingPlacementEvaluator.Evaluate(request).IsValid);

            spacing.MergeMode = PlacementRuleMergeMode.Override;
            BuildingPlacementEvaluationResult blocked =
                BuildingPlacementEvaluator.Evaluate(request);
            Assert.IsFalse(blocked.IsValid);
            Assert.IsTrue(blocked.SpacingBlocked);
        }

        [Test]
        public void Evaluate_InfluenceUsesSettlementCenterMarkerWithoutTypeChecks()
        {
            var center = House("village-anchor");
            center.Modules.Add(new SettlementCenterBuildingModule
            {
                InfluenceRadius = 2,
            });
            var dependent = House("house");
            dependent.Modules.Add(
                new SettlementInfluenceRequirementBuildingModule
                {
                    MergeMode = PlacementRuleMergeMode.Override,
                    RequiresInfluence = true,
                });
            var registry = new TestBuildingRegistry(center, dependent);
            var occupants = new Dictionary<Vector2Int, string>
            {
                [Vector2Int.zero] = center.Id,
            };

            BuildingPlacementEvaluationResult inRange = Evaluate(
                registry,
                dependent.Id,
                new Vector2Int(2, 0),
                occupants);
            BuildingPlacementEvaluationResult outOfRange = Evaluate(
                registry,
                dependent.Id,
                new Vector2Int(3, 0),
                occupants);

            Assert.IsTrue(inRange.IsValid);
            Assert.IsFalse(outOfRange.IsValid);
            Assert.IsTrue(outOfRange.InfluenceZoneBlocked);
        }

        [Test]
        public void Evaluate_AuthoritativeInfluenceRequirement_BlocksWhenRegistryHasNoCenter()
        {
            BuildingDefinition dependent = House("house");
            dependent.Modules.Add(
                new SettlementInfluenceRequirementBuildingModule
                {
                    MergeMode = PlacementRuleMergeMode.Override,
                    RequiresInfluence = true,
                });
            var registry = new TestBuildingRegistry(dependent);

            BuildingPlacementEvaluationResult result = Evaluate(
                registry,
                dependent.Id,
                Vector2Int.zero,
                new Dictionary<Vector2Int, string>());

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.InfluenceZoneBlocked);
            Assert.AreEqual(
                BuildingPlacementBlockerKind.InfluenceRequired,
                result.Blockers[0].Kind);
            StringAssert.Contains(
                nameof(SettlementCenterBuildingModule),
                result.Blockers[0].Message);
        }

        [Test]
        public void Evaluate_AuthoritativeInfluenceRequirement_BlocksWhenCenterRadiusIsZero()
        {
            BuildingDefinition center = TownHall("town-hall", 0);
            BuildingDefinition dependent = House("house");
            dependent.Modules.Add(
                new SettlementInfluenceRequirementBuildingModule
                {
                    MergeMode = PlacementRuleMergeMode.Override,
                    RequiresInfluence = true,
                });
            var registry = new TestBuildingRegistry(center, dependent);

            BuildingPlacementEvaluationResult result = Evaluate(
                registry,
                dependent.Id,
                Vector2Int.right,
                new Dictionary<Vector2Int, string>());

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.InfluenceZoneBlocked);
            Assert.AreEqual(
                BuildingPlacementBlockerKind.InfluenceRequired,
                result.Blockers[0].Kind);
            StringAssert.Contains(
                "додатного радіуса",
                result.Blockers[0].Message);
        }

        [Test]
        public void Evaluate_LegacyInfluenceFallback_StillPassesWithoutCenter()
        {
            BuildingDefinition legacyDependent = House("legacy-house");
            var registry = new TestBuildingRegistry(legacyDependent);

            BuildingPlacementEvaluationResult result = Evaluate(
                registry,
                legacyDependent.Id,
                Vector2Int.zero,
                new Dictionary<Vector2Int, string>());

            Assert.IsTrue(result.IsValid);
            Assert.IsFalse(result.InfluenceZoneBlocked);
        }

        [Test]
        public void TownHallAndCastleTypeModules_DoNotCreateInfluenceWithoutMarker()
        {
            var townHall = House("legacy-town-hall-type");
            townHall.Modules.Add(
                new TownHallBuildingModule
                {
                    BuildRadius = 5,
                });
            var castle = House("legacy-castle-type");
            castle.Modules.Add(
                new CastleBuildingModule
                {
                    ExclusionRadius = 5,
                });

            Assert.IsFalse(
                BuildingPlacementEvaluator.IsInfluenceCenter(
                    townHall));
            Assert.IsFalse(
                BuildingDefinitionCapabilities.IsSettlementCenter(
                    townHall));
            Assert.IsFalse(
                BuildingPlacementEvaluator.IsInfluenceCenter(
                    castle));
            Assert.IsFalse(
                BuildingDefinitionCapabilities.IsSettlementCenter(
                    castle));
        }

        [Test]
        public void Evaluate_InfluenceCoverage_IsScopedToPlacementOwner()
        {
            BuildingDefinition center = TownHall("town-hall", 3);
            BuildingDefinition dependent = House("house");
            dependent.Modules.Add(
                new SettlementInfluenceRequirementBuildingModule
                {
                    MergeMode =
                        PlacementRuleMergeMode.Override,
                    RequiresInfluence = true,
                });
            var registry =
                new TestBuildingRegistry(center, dependent);
            var occupants =
                new Dictionary<Vector2Int, string>
                {
                    [Vector2Int.zero] = center.Id,
                };
            var owners =
                new Dictionary<Vector2Int, string>
                {
                    [Vector2Int.zero] = "owner-b",
                };

            BuildingPlacementEvaluationResult foreignCoverage =
                Evaluate(
                    registry,
                    dependent.Id,
                    Vector2Int.right,
                    occupants,
                    ownerId: "owner-a",
                    occupantOwners: owners);
            BuildingPlacementEvaluationResult ownedCoverage =
                Evaluate(
                    registry,
                    dependent.Id,
                    Vector2Int.right,
                    occupants,
                    ownerId: "owner-b",
                    occupantOwners: owners);

            Assert.IsFalse(foreignCoverage.IsValid);
            Assert.IsTrue(
                foreignCoverage.InfluenceZoneBlocked);
            Assert.IsTrue(ownedCoverage.IsValid);
        }

        [Test]
        public void Evaluate_InfluenceCenterOverlap_IsGlobalAcrossOwners()
        {
            BuildingDefinition center = TownHall("town-hall", 2);
            var registry = new TestBuildingRegistry(center);
            var occupants =
                new Dictionary<Vector2Int, string>
                {
                    [Vector2Int.zero] = center.Id,
                };
            var owners =
                new Dictionary<Vector2Int, string>
                {
                    [Vector2Int.zero] = "owner-b",
                };

            BuildingPlacementEvaluationResult result =
                Evaluate(
                    registry,
                    center.Id,
                    new Vector2Int(4, 0),
                    occupants,
                    ownerId: "owner-a",
                    occupantOwners: owners);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.InfluenceZoneBlocked);
            Assert.AreEqual(
                BuildingPlacementBlockerKind.InfluenceOverlap,
                result.Blockers[0].Kind);
        }

        [Test]
        public void Evaluate_RegisteredRuleEvaluators_RunInOrderAndStopAtFirstBlocker()
        {
            BuildingDefinition definition = House("custom");
            var registry = new TestBuildingRegistry(definition);
            var calls = new List<string>();
            var request = new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = definition.Id,
                Position = Vector2Int.zero,
                IsOccupied = _ => false,
                SkipInfluenceRules = true,
                RuleEvaluators =
                    new IBuildingPlacementRuleEvaluator[]
                    {
                        new RecordingRuleEvaluator("first", calls),
                        new RecordingRuleEvaluator(
                            "second",
                            calls,
                            new BuildingPlacementBlocker
                            {
                                Kind = BuildingPlacementBlockerKind.Fog,
                                Message = "Custom fog policy.",
                                Position = Vector2Int.zero,
                                BuildingId = definition.Id,
                            }),
                        new RecordingRuleEvaluator("third", calls),
                    },
            };

            BuildingPlacementEvaluationResult result =
                BuildingPlacementEvaluator.Evaluate(request);

            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.FogBlocked);
            Assert.AreEqual("Custom fog policy.", result.Blockers[0].Message);
            CollectionAssert.AreEqual(
                new[] { "first", "second" },
                calls);
        }

        private static BuildingPlacementEvaluationResult Evaluate(
            IBuildingRegistry registry,
            string buildingId,
            Vector2Int position,
            Dictionary<Vector2Int, string> occupants,
            IReadOnlyList<BuildingPlacementSimulationEntry> pendingPlacements = null,
            string ownerId = null,
            IReadOnlyDictionary<Vector2Int, string> occupantOwners = null)
        {
            return BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = registry,
                BuildingId = buildingId,
                OwnerId = ownerId,
                Position = position,
                MinSpacing = 0,
                TownHallBuildRadius = 2,
                IsOccupied = occupants.ContainsKey,
                GetOccupantId = tile => occupants.TryGetValue(tile, out var occupantId) ? occupantId : null,
                GetOccupantOwnerId =
                    occupantOwners == null
                        ? null
                        : tile =>
                            occupantOwners.TryGetValue(
                                tile,
                                out string occupantOwnerId)
                                ? occupantOwnerId
                                : null,
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
                    new SettlementCenterBuildingModule
                    {
                        InfluenceRadius = radius,
                    },
                },
            };
        }

        private static BuildingDefinition WideBuilding(string id, Vector2Int size)
        {
            return new BuildingDefinition
            {
                Id = id,
                DisplayName = id,
                Category = BuildingCategory.Civilian,
                Footprint = new BuildingFootprint
                {
                    Size = size,
                    Anchor = BuildingFootprintAnchor.SouthWest,
                    RequiresFlatGround = true,
                },
                UseCustomTownHallRules = true,
                RequireTownHallInRange = false,
            };
        }
    }
}

#endif
