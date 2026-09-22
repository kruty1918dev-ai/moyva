using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.UIActions.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// Action-level coverage of the construction lifecycle (P024/P025):
    /// Enter/NumpadEnter resolve to ConfirmPlacement, a drained pending queue
    /// exits construction mode through GameModeChangeRequestedSignal, a queue
    /// that survives the confirm request (client authority) keeps the mode,
    /// and the initial-castle guard blocks cancellation.
    /// </summary>
    [TestFixture]
    public sealed class ConstructionInputActionTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private FakeSessionCommands _session;
        private ConstructionInputService _input;
        private List<GameModeChangeRequestedSignal> _modeRequests;
        private int _confirmRequests;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<GameModeChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<GameModeChangeRequestedSignal>().OptionalSubscriber();
            _container.DeclareSignal<PlaceBuildingConfirmRequestSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingSelectionChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingPreviewChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingPreviewMovedSignal>().OptionalSubscriber();
            _container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<GridTileChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<FogStateChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            _modeRequests = new List<GameModeChangeRequestedSignal>();
            _confirmRequests = 0;
            _signals.Subscribe<GameModeChangeRequestedSignal>(
                signal => _modeRequests.Add(signal));
            _signals.Subscribe<PlaceBuildingConfirmRequestSignal>(
                _ => _confirmRequests++);

            _session = new FakeSessionCommands();
            _input = new ConstructionInputService(
                _session,
                new FakeWallTopologyService(),
                new FakeWallPathfinder(),
                new WallHandleController(_signals),
                new FakeObjectsMap(),
                new FakeScreenToGrid(),
                new FakePointerInput(),
                inputSettingsProvider: null,
                new FakeGridService(),
                gridGeometry: null,
                new FakePlacementQuery(),
                new BuildModeGridStateController(),
                uiHitTester: null,
                inputPolicy: null,
                uiContexts: null,
                _signals);
            _input.Initialize();
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Construction,
            });
        }

        [TearDown]
        public void TearDown()
        {
            _input?.Dispose();
            _container?.UnbindAll();
        }

        private UiActionResult Execute(UiActionId actionId)
            => _input.Execute(new UiActionRequest(
                actionId,
                UiActionSource.Hotkey,
                "test"));

        [Test]
        public void ConfirmPlacement_PendingDrainedSynchronously_RequestsNormalMode()
        {
            _session.Pending[new Vector2Int(3, 3)] = "house";
            // Emulate local/host authority: the commit drains the queue while
            // the confirm request is being handled.
            _signals.Subscribe<PlaceBuildingConfirmRequestSignal>(
                _ => _session.Pending.Clear());

            UiActionResult result = Execute(UiActionIds.Construction.ConfirmPlacement);

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(1, _confirmRequests);
            Assert.AreEqual(1, _modeRequests.Count);
            Assert.AreEqual(GameModeType.Normal, _modeRequests[0].RequestedMode);
        }

        [Test]
        public void ConfirmPlacement_PendingSurvives_StaysInConstructionMode()
        {
            // Client authority: the host has not answered yet, pending entries
            // remain, so the mode must not be exited prematurely.
            _session.Pending[new Vector2Int(3, 3)] = "house";

            UiActionResult result = Execute(UiActionIds.Construction.ConfirmPlacement);

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(1, _confirmRequests);
            Assert.AreEqual(0, _modeRequests.Count);
        }

        [Test]
        public void ConfirmPlacement_NoPendingWork_RejectedWithoutSignals()
        {
            UiActionResult result = Execute(UiActionIds.Construction.ConfirmPlacement);

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(0, _confirmRequests);
            Assert.AreEqual(0, _modeRequests.Count);
        }

        [Test]
        public void ConfirmPlacement_OutsideConstructionMode_Rejected()
        {
            _signals.Fire(new GameModeChangedSignal
            {
                NewMode = GameModeType.Normal,
            });
            _session.Pending[new Vector2Int(3, 3)] = "house";

            UiActionResult result = Execute(UiActionIds.Construction.ConfirmPlacement);

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(0, _confirmRequests);
        }

        [Test]
        public void CancelPlacement_ActivePlacement_CancelsWithoutModeExit()
        {
            _session.State = BuildingPlacementState.Placing;
            _session.Pending[new Vector2Int(3, 3)] = "house";

            UiActionResult result = Execute(UiActionIds.Construction.CancelPlacement);

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(1, _session.CancelCalls);
            Assert.AreEqual(0, _modeRequests.Count);
        }

        [Test]
        public void CancelPlacement_NoActiveAction_RequestsNormalMode()
        {
            UiActionResult result = Execute(UiActionIds.Construction.CancelPlacement);

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.AreEqual(0, _session.CancelCalls);
            Assert.AreEqual(1, _modeRequests.Count);
            Assert.AreEqual(GameModeType.Normal, _modeRequests[0].RequestedMode);
        }

        [Test]
        public void CancelPlacement_InitialCastleRequired_RejectedAndConsumed()
        {
            _session.InitialCastleRequired = true;
            _session.State = BuildingPlacementState.Placing;
            _session.Pending[new Vector2Int(3, 3)] = "castle";

            UiActionResult result = Execute(UiActionIds.Construction.CancelPlacement);

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.IsTrue(result.Consumed);
            StringAssert.Contains("castle", result.Details);
            Assert.AreEqual(0, _session.CancelCalls);
            Assert.AreEqual(0, _modeRequests.Count);
        }

        private sealed class FakeSessionCommands :
            IConstructionSessionCommands,
            IConstructionBootstrapQuery
        {
            public readonly Dictionary<Vector2Int, string> Pending = new();
            public bool InitialCastleRequired;
            public int CancelCalls;

            public BuildingPlacementState State { get; set; }
                = BuildingPlacementState.Idle;
            public bool IsDemolishMode { get; set; }
            public int PendingDemolitionCount { get; set; }
            public string LastActionMessage = string.Empty;

            public void SelectBuilding(string buildingId)
            {
                SelectedBuildingId = buildingId;
                State = BuildingPlacementState.Placing;
            }

            public string SelectedBuildingId;
            public string GetSelectedBuildingId() => SelectedBuildingId;
            public void SetActiveOwner(string ownerId) => ActiveOwner = ownerId;
            public string ActiveOwner = "p1";
            public string GetActiveOwner() => ActiveOwner;

            public bool TryPreviewAt(Vector2Int position)
            {
                Pending[position] = SelectedBuildingId ?? "house";
                return true;
            }

            public bool HasPendingPlacementAt(Vector2Int position)
                => Pending.ContainsKey(position);

            public bool TryGetPendingBuildingIdAt(
                Vector2Int position, out string buildingId)
                => Pending.TryGetValue(position, out buildingId);

            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => Pending;

            public bool TryMovePendingPlacement(
                Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position)
                => Pending.Remove(position);
            public bool TryGetPendingPlacementStatus(
                Vector2Int position,
                out ConstructionPendingPlacementStatus status)
            {
                status = default;
                return false;
            }

            public ConstructionResourceProjection GetResourceProjection(
                Vector2Int position) => ConstructionResourceProjection.Empty;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(
                string buildingId)
                => new Dictionary<string, float>();

            public void Confirm() => Pending.Clear();
            public void Cancel()
            {
                CancelCalls++;
                Pending.Clear();
                State = BuildingPlacementState.Idle;
            }
            public void UndoLast() { }
            public void RedoLast() { }
            public void ToggleDemolishMode() => IsDemolishMode = !IsDemolishMode;
            public bool TryDemolishAt(Vector2Int position) => false;
            public string GetLastActionMessage() => LastActionMessage;

            public bool RequiresInitialCastle(
                string ownerId, out string castleBuildingId)
            {
                castleBuildingId = InitialCastleRequired ? "castle" : null;
                return InitialCastleRequired;
            }

            public bool IsCastleBuilding(string buildingId)
                => buildingId == "castle";
        }

        private sealed class FakeWallTopologyService : IWallTopologyService
        {
            public bool IsWallOrGate(string buildingId) => false;
            public bool IsWall(string buildingId) => false;
            public bool IsGate(string buildingId) => false;
            public bool TryBuildPlacedMask(
                Vector2Int position, string buildingId,
                out WallCollectionDefinition collection,
                out TopologyNeighborMask mask)
            {
                collection = null;
                mask = default;
                return false;
            }
            public bool TryBuildPreviewMask(
                Vector2Int position, string buildingId,
                out WallCollectionDefinition collection,
                out TopologyNeighborMask mask)
            {
                collection = null;
                mask = default;
                return false;
            }
            public bool IsHorizontalWallSegment(
                Vector2Int position, WallCollectionDefinition collection,
                bool includePendingNeighbors) => false;
        }

        private sealed class FakeWallPathfinder : IWallPathfinder
        {
            public IReadOnlyList<Vector2Int> BuildPath(
                Vector2Int startPosition, Vector2Int endPosition)
                => Array.Empty<Vector2Int>();
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            public bool IsOccupied(Vector2Int position) => false;
            public bool TryGetOccupant(Vector2Int position, out string occupantId)
            {
                occupantId = null;
                return false;
            }
            public void Register(Vector2Int position, string occupantId) { }
            public void Move(Vector2Int from, Vector2Int to) { }
            public void Unregister(Vector2Int position) { }
            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                position = default;
                return false;
            }
        }

        private sealed class FakeScreenToGrid : IScreenToGridConverter
        {
            public Vector2Int ScreenToGrid(Vector2 screenPosition)
                => Vector2Int.zero;
            public Vector2Int WorldToGrid(Vector2 worldPosition)
                => Vector2Int.zero;
        }

        private sealed class FakePointerInput : IConstructionPointerInputSource
        {
            public ConstructionPointerSnapshot ReadPointerSnapshot()
                => ConstructionPointerSnapshot.None;
        }

        private sealed class FakeGridService : IGridService
        {
            public string GetTileData(Vector2Int position) => null;
            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = null;
                return false;
            }
            public void SetTileData(Vector2Int position, string tileTypeId) { }
            public int GridWidth => 100;
            public int GridHeight => 100;
        }

        private sealed class FakePlacementQuery : IConstructionPlacementQuery
        {
            public ConstructionPlacementQueryResult EvaluatePlacement(
                ConstructionPlacementQueryRequest request)
                => new ConstructionPlacementQueryResult(
                    isSpatiallyValid: true,
                    resourcesValid: true,
                    isGateReplacement: false);
        }
    }
}
