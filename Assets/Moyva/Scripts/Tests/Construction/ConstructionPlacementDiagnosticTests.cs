using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class ConstructionPlacementDiagnosticTests
    {
        [Test]
        public void Formatter_IncludesActionSourceAndConcreteBlockerContext()
        {
            var blockers = new List<BuildingPlacementBlocker>
            {
                new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Position = new Vector2Int(8, 4),
                    BuildingId = "castle-01",
                    Message = "edge terrain tile",
                },
            };
            var diagnostic = new ConstructionPlacementDiagnostic(
                attemptId: 42,
                source: ConstructionPlacementAttemptSource.PointerClick,
                buildingId: "castle-01",
                position: new Vector2Int(8, 4),
                contextPosition: new Vector2Int(8, 4),
                ownerId: "player-a",
                isSpatiallyValid: false,
                resourcesValid: false,
                isGateReplacement: false,
                reasonCode: "terrain",
                reason: "Footprint contains a terrain cell blocked for construction.",
                tileId: "grass",
                terrainLevel: 1,
                terrainReason: "edge terrain tile",
                fogState: "Visible",
                perPlayerLimit: 1,
                existingOwnedCount: 0,
                pendingOwnedCount: 0,
                ignoredPendingPosition: null,
                ignoredOccupiedPosition: null,
                blockers);

            string line =
                ConstructionPlacementDiagnosticFormatter.FormatSingleLine(diagnostic);

            StringAssert.Contains("[MoyvaPlacementAttempt]", line);
            StringAssert.Contains("source=PointerClick", line);
            StringAssert.Contains("building='castle-01'", line);
            StringAssert.Contains("terrainLevel=1", line);
            StringAssert.Contains("terrainReason='edge terrain tile'", line);
            StringAssert.Contains("Terrain@(8, 4)", line);
        }

        [Test]
        public void ResolveReasonCode_PreservesExplicitStableCode()
        {
            string code =
                ConstructionPlacementDiagnosticFormatter.ResolveReasonCode(
                    evaluationResult: null,
                    isSpatiallyValid: false,
                    resourcesValid: false,
                    explicitCode: "per-player-limit");

            Assert.AreEqual("per-player-limit", code);
        }

        [Test]
        public void Diagnostic_CopiesBlockerCollection()
        {
            var blockers = new List<BuildingPlacementBlocker>
            {
                new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = "limit",
                },
            };
            var diagnostic = new ConstructionPlacementDiagnostic(
                1,
                ConstructionPlacementAttemptSource.PointerClick,
                "castle-01",
                Vector2Int.zero,
                Vector2Int.zero,
                "owner",
                false,
                false,
                false,
                "per-player-limit",
                "limit",
                "grass",
                0,
                null,
                "Visible",
                1,
                1,
                0,
                null,
                null,
                blockers);

            blockers.Clear();

            Assert.AreEqual(1, diagnostic.Blockers.Count);
            Assert.AreEqual(
                BuildingPlacementBlockerKind.Configuration,
                diagnostic.Blockers[0].Kind);
        }
    }
}
