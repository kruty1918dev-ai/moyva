using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// P059: while the supply panel stays open, deliveries and external
    /// spending must refresh the numbers without losing the player's
    /// source/wagon selection, and covered deficits must not be offered again.
    /// </summary>
    [TestFixture]
    public class GameplaySupplyPanelTests
    {
        private static readonly Vector2Int Position = new(10, 11);
        private const string OwnerId = "player_0";

        private GameplayHtmlState _state;
        private FakeConstructionCommands _construction;
        private FakeSupplyService _supply;
        private GameplaySupplyPanel _panel;

        [SetUp]
        public void SetUp()
        {
            _state = new GameplayHtmlState();
            _construction = new FakeConstructionCommands();
            _supply = new FakeSupplyService();
            _panel = new GameplaySupplyPanel(_construction, _supply,
                new FakeRoleResolver(), new FakeTurnService(), _state);
            _panel.Initialize();
            _state.OpenSupplyPanel(Position, "lumber-camp");
        }

        [TearDown]
        public void TearDown() => _panel.Dispose();

        private static ConstructionSupplyEvaluation Eval(
            float required, float local, float delivered)
            => new(true, "target", "Target", Position, null,
                new List<ConstructionSupplyResourceLine>
                {
                    new("wood", required, local, delivered,
                        Math.Max(0f, required - local)),
                },
                new List<ConstructionSupplySourceSnapshot>
                {
                    new("source", "Source", "0:0",
                        new Dictionary<string, float> { ["wood"] = 200f }, 14.1f),
                },
                new List<ConstructionSupplyWagonSnapshot>
                {
                    new("wagon-1", new Vector2Int(1, 0), 120f, 0f, false, "Idle"),
                });

        [Test]
        public void Capture_KeepsSourceAndWagonSelection_WhenDeliveryUpdatesNumbers()
        {
            _supply.Evaluation = Eval(required: 50f, local: 0f, delivered: 0f);
            var first = _panel.Capture();

            Assert.AreEqual(0, first.SourceIndex);
            Assert.AreEqual(0, first.WagonIndex);
            Assert.IsTrue(first.CanDispatch, first.DispatchUnavailableReason);

            // Half the wood arrives while the panel is open.
            _supply.Evaluation = Eval(required: 50f, local: 25f, delivered: 25f);
            _supply.RaiseChanged();
            Assert.IsTrue(_state.Dirty, "A supply change must mark the HUD dirty.");

            var second = _panel.Capture();
            Assert.AreEqual(0, second.SourceIndex, "Source selection must survive a refresh.");
            Assert.AreEqual(0, second.WagonIndex, "Wagon selection must survive a refresh.");
            Assert.AreEqual(25f, second.Resources[0].Delivered, 0.001f);
            Assert.AreEqual(25f, second.Resources[0].Deficit, 0.001f);
            Assert.IsTrue(second.CanDispatch, "The remaining deficit must stay dispatchable.");
        }

        [Test]
        public void Capture_FulfilledNeed_IsNotOfferedForDispatch()
        {
            _supply.Evaluation = Eval(required: 50f, local: 0f, delivered: 0f);
            _panel.Capture();

            // The last delivery covered the whole deficit.
            _supply.Evaluation = Eval(required: 50f, local: 50f, delivered: 50f);
            var snapshot = _panel.Capture();

            Assert.IsFalse(snapshot.CanDispatch);
            Assert.AreEqual("The settlement already covers this construction.",
                snapshot.DispatchUnavailableReason);
            Assert.IsNull(snapshot.PlannedShipment,
                "A fulfilled need must not show a fresh shipment plan.");
        }

        [Test]
        public void Capture_SourceEmptiedByOtherSpend_DropsItFromOffers()
        {
            _supply.Evaluation = Eval(required: 50f, local: 0f, delivered: 0f);
            var first = _panel.Capture();
            Assert.AreEqual(1, first.Sources.Length);

            // Another action drained the source stock — no usable source remains.
            _supply.Evaluation = new ConstructionSupplyEvaluation(true, "target", "Target",
                Position, null,
                new List<ConstructionSupplyResourceLine>
                {
                    new("wood", 50f, 0f, 0f, 50f),
                },
                new List<ConstructionSupplySourceSnapshot>(),
                new List<ConstructionSupplyWagonSnapshot>
                {
                    new("wagon-1", new Vector2Int(1, 0), 120f, 0f, false, "Idle"),
                });
            var second = _panel.Capture();

            Assert.AreEqual(0, second.Sources.Length);
            Assert.AreEqual(-1, second.SourceIndex);
            Assert.IsFalse(second.CanDispatch);
            Assert.AreEqual("Select a source warehouse with the missing resources.",
                second.DispatchUnavailableReason);
        }

        private sealed class FakeConstructionCommands : IConstructionSessionCommands
        {
            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public readonly Dictionary<string, float> Costs = new() { ["wood"] = 50f };

            public void SelectBuilding(string buildingId) { }
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) { }
            public string GetActiveOwner() => OwnerId;
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => true;
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            { buildingId = "lumber-camp"; return true; }
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => new Dictionary<Vector2Int, string>();
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => false;
            public bool TryGetPendingPlacementStatus(Vector2Int position,
                out ConstructionPendingPlacementStatus status)
            {
                status = new ConstructionPendingPlacementStatus(position, "lumber-camp",
                    "target", "Target", true, false, null);
                return true;
            }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position) => null;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(string buildingId) => Costs;
            public void Confirm() { }
            public void Cancel() { }
            public void UndoLast() { }
            public void RedoLast() { }
            public void ToggleDemolishMode() { }
            public bool TryDemolishAt(Vector2Int position) => false;
            public string GetLastActionMessage() => null;
        }

        private sealed class FakeSupplyService : IConstructionSupplyService
        {
            public ConstructionSupplyEvaluation Evaluation;
            public IReadOnlyDictionary<string, float> Preview =
                new Dictionary<string, float>();

            public event Action Changed;
            public void RaiseChanged() => Changed?.Invoke();

            public ConstructionSupplyEvaluation Evaluate(string ownerId, string buildingId,
                Vector2Int position, IReadOnlyDictionary<string, float> requiredCosts)
                => Evaluation;

            public IReadOnlyDictionary<string, float> PreviewShipment(
                ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts) => Preview;

            public CaravanTransferResult DispatchSupply(ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts)
                => CaravanTransferResult.Success();
            public IReadOnlyList<ConstructionSupplyOrderSnapshot> GetOrders(string ownerId)
                => Array.Empty<ConstructionSupplyOrderSnapshot>();
            public bool TryGetOrderAt(Vector2Int position, out ConstructionSupplyOrderSnapshot snapshot)
            { snapshot = default; return false; }
            public IReadOnlyDictionary<string, float> GetResourcesForPlacement(
                string settlementId, Vector2Int position) => new Dictionary<string, float>();
            public void CancelOrderAt(Vector2Int position) { }
            public CaravanTransferResult ApplyConfirmedDispatch(ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts)
                => CaravanTransferResult.Success();
            public void ApplyConfirmedCancelOrder(string ownerId, string unitId, Vector2Int position) { }
        }

        private sealed class FakeTurnService : ITurnService
        {
            public event Action StateChanged { add { } remove { } }
            public TurnPhase Phase => TurnPhase.AwaitingInput;
            public int Round => 1;
            public long GlobalTurn => 1;
            public int ActionsThisTurn => 0;
            public string ActiveOwnerId => OwnerId;
            public string LocalOwnerId => OwnerId;
            public IReadOnlyList<TurnFaction> Factions => Array.Empty<TurnFaction>();
            public bool IsOwnerActive(string ownerId) => true;
            public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return true; }
            public bool TryRecordAction(string ownerId, string actionId) => true;
            public bool TryEndTurn(string requesterOwnerId, out string reason) { reason = null; return true; }
        }

        private sealed class FakeRoleResolver : ILocalGameplayRoleResolver
        {
            public LocalGameplayRoleSnapshot Resolve()
                => new(LocalGameplayRole.Offline, OwnerId);
        }
    }
}
