using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class ConstructionLifecycleStateMachineTests
    {
        private sealed class FixedLifecycle : IConstructionLifecycle
        {
            private readonly bool _isOperational;

            public FixedLifecycle(bool isOperational)
            {
                _isOperational = isOperational;
            }

            public bool IsOperational(Vector2Int position)
                => _isOperational;

            public bool TryGetProgress(
                Vector2Int position,
                out int completedTurns,
                out int requiredTurns)
            {
                completedTurns = _isOperational ? 1 : 0;
                requiredTurns = 1;
                return !_isOperational;
            }
        }

        private ConstructionLifecycleStateMachine _state;

        [SetUp]
        public void SetUp()
        {
            _state = new ConstructionLifecycleStateMachine();
        }

        [Test]
        public void ZeroTurnBuilding_BecomesOperationalImmediately()
        {
            _state.RegisterPlacement(
                new Vector2Int(2, 3), "storage", "p1", 0, 7, null,
                out var transition);

            Assert.IsTrue(transition.IsValid);
            Assert.AreEqual("storage", transition.BuildingId);
            Assert.IsTrue(_state.IsOperational(new Vector2Int(2, 3)));
        }

        [Test]
        public void InstantBuilding_BuildTurnsZero_IsImmediatelyOperational()
        {
            Vector2Int position = new(3, 4);

            _state.RegisterPlacement(
                position,
                "castle-01",
                "p1",
                0,
                12,
                null,
                out ConstructionLifecycleStateMachine.OperationalTransition
                    transition);

            Assert.IsTrue(transition.IsValid);
            Assert.AreEqual("castle-01", transition.BuildingId);
            Assert.IsTrue(_state.IsOperational(position));
            Assert.IsTrue(_state.TryGetProgress(position, out int completed, out int required));
            Assert.AreEqual(0, completed);
            Assert.AreEqual(0, required);
        }

        [Test]
        public void InstantBuilding_DoesNotShowConstructionVariant()
        {
            var definition = new BuildingDefinition
            {
                Id = "castle-01",
                BuildTurns = 0,
                Prefab = new GameObject("castle-base"),
                Presentation = new BuildingRuntimePresentationConfig
                {
                    Variants = new BuildingPresentationVariants
                    {
                        ConstructionPrefab = new GameObject("building_scaffolding"),
                    },
                },
            };

            try
            {
                Assert.IsFalse(
                    ConstructionPlacedVisualSignalHandler.ShouldUseConstructionVisual(
                        definition,
                        new FixedLifecycle(false),
                        Vector2Int.zero));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition.Presentation.Variants.ConstructionPrefab);
            }
        }

        [Test]
        public void TimedBuilding_UsesConstructionVariantUntilOperational()
        {
            var definition = new BuildingDefinition
            {
                Id = "barrack",
                BuildTurns = 3,
            };

            Assert.IsTrue(
                ConstructionPlacedVisualSignalHandler.ShouldUseConstructionVisual(
                    definition,
                    new FixedLifecycle(false),
                    Vector2Int.zero));
            Assert.IsFalse(
                ConstructionPlacedVisualSignalHandler.ShouldUseConstructionVisual(
                    definition,
                    new FixedLifecycle(true),
                    Vector2Int.zero));
        }

        [Test]
        public void CastlePreset_BuildTurnsIsZero()
        {
            string json = File.ReadAllText(
                "Assets/Moyva/Presets/Buildings/castle-01.json");

            StringAssert.Contains("\"id\": \"castle-01\"", json);
            Assert.IsTrue(
                Regex.IsMatch(
                    json,
                    "\"construction\"\\s*:\\s*\\{[\\s\\S]*?\"buildTurns\"\\s*:\\s*0\\b"),
                "castle-01 preset must use buildTurns=0 for instant construction.");
        }

        [Test]
        public void Placement_DoesNotConsumeItsOwnGlobalTurn()
        {
            Vector2Int position = new(1, 1);
            _state.RegisterPlacement(position, "barn", "p1", 2, 10, null, out _);

            Assert.AreEqual(0, _state.AdvanceOwnerTurn("p1", 10).Count);
            Assert.IsTrue(_state.TryGetProgress(position, out int completed, out int required));
            Assert.AreEqual(0, completed);
            Assert.AreEqual(2, required);
        }

        [Test]
        public void OwnerTurn_AdvancesOnlyOwnedConstruction()
        {
            Vector2Int p1 = new(1, 1);
            Vector2Int p2 = new(2, 2);
            _state.RegisterPlacement(p1, "barn", "p1", 2, 4, null, out _);
            _state.RegisterPlacement(p2, "stable", "p2", 2, 4, null, out _);

            _state.AdvanceOwnerTurn("p1", 5);

            _state.TryGetProgress(p1, out int first, out _);
            _state.TryGetProgress(p2, out int second, out _);
            Assert.AreEqual(1, first);
            Assert.AreEqual(0, second);
        }

        [Test]
        public void FinalRequiredTurn_EmitsOperationalOnce()
        {
            Vector2Int position = new(4, 4);
            _state.RegisterPlacement(position, "stable", "p1", 2, 1, null, out _);

            Assert.AreEqual(0, _state.AdvanceOwnerTurn("p1", 2).Count);
            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition> completed =
                _state.AdvanceOwnerTurn("p1", 3);
            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual(position, completed[0].Position);
            Assert.AreEqual(0, _state.AdvanceOwnerTurn("p1", 4).Count);
        }

        [Test]
        public void DuplicatePlacement_DoesNotResetProgress()
        {
            Vector2Int position = new(5, 5);
            _state.RegisterPlacement(position, "barn", "p1", 3, 1, null, out _);
            _state.AdvanceOwnerTurn("p1", 2);

            bool created = _state.RegisterPlacement(
                position, "barn", "p1", 3, 2, null, out var duplicateTransition);

            Assert.IsFalse(created);
            Assert.IsFalse(duplicateTransition.IsValid);
            _state.TryGetProgress(position, out int completed, out _);
            Assert.AreEqual(1, completed);
        }

        [Test]
        public void ReplacementAtSameOrigin_ResetsLifecycle()
        {
            Vector2Int position = new(5, 5);
            _state.RegisterPlacement(position, "wall", "p1", 3, 1, null, out _);
            _state.AdvanceOwnerTurn("p1", 2);

            _state.RegisterPlacement(position, "gate", "p1", 2, 2, null, out _);

            _state.TryGetProgress(position, out int completed, out int required);
            Assert.AreEqual(0, completed);
            Assert.AreEqual(2, required);
        }

        [Test]
        public void Relocation_PreservesPartialProgressAndRemovesSource()
        {
            Vector2Int source = new(2, 2);
            Vector2Int target = new(8, 8);
            _state.RegisterPlacement(source, "unique", "p1", 3, 1, null, out _);
            _state.AdvanceOwnerTurn("p1", 2);

            _state.RegisterPlacement(
                target, "unique", "p1", 3, 2, source, out _);

            Assert.IsFalse(_state.TryGetProgress(source, out _, out _));
            Assert.IsTrue(_state.TryGetProgress(target, out int completed, out int required));
            Assert.AreEqual(1, completed);
            Assert.AreEqual(3, required);
        }

        [Test]
        public void Relocation_OfOperationalBuilding_ReannouncesNewOrigin()
        {
            Vector2Int source = new(2, 2);
            Vector2Int target = new(9, 9);
            _state.RegisterPlacement(source, "unique", "p1", 1, 1, null, out _);
            Assert.AreEqual(1, _state.AdvanceOwnerTurn("p1", 2).Count);

            _state.RegisterPlacement(
                target, "unique", "p1", 1, 2, source, out var moved);

            Assert.IsTrue(moved.IsValid);
            Assert.AreEqual(target, moved.Position);
            Assert.IsTrue(_state.IsOperational(target));
        }

        [Test]
        public void Demolition_RemovesLifecycleState()
        {
            Vector2Int position = new(4, 7);
            _state.RegisterPlacement(position, "barn", "p1", 2, 1, null, out _);

            Assert.IsTrue(_state.Remove(position));
            Assert.IsFalse(_state.TryGetProgress(position, out _, out _));
            Assert.IsTrue(_state.IsOperational(position));
        }

        [Test]
        public void CaptureSorted_IsDeterministicByPosition()
        {
            _state.RegisterPlacement(new Vector2Int(9, 1), "c", "p1", 2, 1, null, out _);
            _state.RegisterPlacement(new Vector2Int(1, 9), "a", "p1", 2, 1, null, out _);
            _state.RegisterPlacement(new Vector2Int(1, 2), "b", "p1", 2, 1, null, out _);

            IReadOnlyList<ConstructionLifecycleStateMachine.SavedState> saved =
                _state.CaptureSorted();

            Assert.AreEqual(new Vector2Int(1, 2), saved[0].Position);
            Assert.AreEqual(new Vector2Int(1, 9), saved[1].Position);
            Assert.AreEqual(new Vector2Int(9, 1), saved[2].Position);
        }

        [Test]
        public void Restore_CompletedStatePublishesWhenBuildingIdentityIsKnown()
        {
            Vector2Int position = new(3, 3);
            var saved = new[]
            {
                new ConstructionLifecycleStateMachine.SavedState(
                    position, "windmill", "p1", 2, 2, 4),
            };

            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition> transitions =
                _state.Restore(saved, null);

            Assert.AreEqual(1, transitions.Count);
            Assert.AreEqual("windmill", transitions[0].BuildingId);
        }

        [Test]
        public void LegacyRestore_ResolvesMissingBuildingIdentity()
        {
            Vector2Int position = new(3, 3);
            var saved = new[]
            {
                new ConstructionLifecycleStateMachine.SavedState(
                    position, string.Empty, "p1", 1, 1, 4),
            };

            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition> transitions =
                _state.Restore(saved, p => p == position ? "windmill" : string.Empty);

            Assert.AreEqual(1, transitions.Count);
            Assert.AreEqual("windmill", transitions[0].BuildingId);
        }

        [Test]
        public void LegacyRestoreBeforePlacement_ReconcilesWithoutResettingProgress()
        {
            Vector2Int position = new(3, 3);
            var saved = new[]
            {
                new ConstructionLifecycleStateMachine.SavedState(
                    position, string.Empty, "p1", 2, 1, 4),
            };
            _state.Restore(saved, _ => string.Empty);

            bool created = _state.RegisterPlacement(
                position, "windmill", "p1", 2, 5, null, out _);

            Assert.IsFalse(created);
            _state.TryGetProgress(position, out int completed, out int required);
            Assert.AreEqual(1, completed);
            Assert.AreEqual(2, required);
        }

        [Test]
        public void Restore_DuplicatePositionsKeepsFirstRecordDeterministically()
        {
            Vector2Int position = new(1, 1);
            var saved = new[]
            {
                new ConstructionLifecycleStateMachine.SavedState(position, "first", "p1", 3, 1, 2),
                new ConstructionLifecycleStateMachine.SavedState(position, "second", "p1", 1, 1, 2),
            };

            _state.Restore(saved, null);

            Assert.IsTrue(_state.TryGetSavedState(position, out var actual));
            Assert.AreEqual("first", actual.BuildingId);
            Assert.AreEqual(3, actual.Required);
        }

        [Test]
        public void Restore_CarriesAlreadyPublishedMarkerFromPlacementRestore()
        {
            Vector2Int position = new(6, 6);
            _state.RegisterPlacement(position, "storage", "p1", 0, 4, null, out var immediate);
            Assert.IsTrue(immediate.IsValid);

            var saved = new[]
            {
                new ConstructionLifecycleStateMachine.SavedState(position, "storage", "p1", 0, 0, 4),
            };

            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition> transitions =
                _state.Restore(saved, null);

            Assert.AreEqual(0, transitions.Count);
        }
    }
}
