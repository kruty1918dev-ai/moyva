using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.InputRouting.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Notifications.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;
using Kruty1918.Localization;
using Kruty1918.UIActions.API;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    [TestFixture]
    public class UnitRecruitmentDeploymentControllerTests
    {
        private const string OwnerId = "owner-a";
        private const long ReadyQueueId = 7;
        private static readonly Vector2Int BuildingPos = new(4, 4);

        private DiContainer _container;
        private SignalBus _signals;
        private FakeTurnService _turns;
        private FakeRecruitmentService _recruitment;
        private FakeOverlay _overlay;
        private FakeInputPolicy _inputPolicy;
        private FakeGameMode _gameMode;
        private FakeNotifications _notifications;
        private UnitRecruitmentDeploymentController _controller;
        private int _panelClosedCount;

        [SetUp]
        public void SetUp()
        {
            _panelClosedCount = 0;
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<UnitRecruitmentReadyIndicatorClickedSignal>()
                .OptionalSubscriber();
            _container.DeclareSignal<UnitRecruitmentQueueChangedSignal>()
                .OptionalSubscriber();
            _container.DeclareSignal<UnitRecruitmentDeployedSignal>()
                .OptionalSubscriber();
            _container.DeclareSignal<UnitRecruitmentReadySignal>()
                .OptionalSubscriber();
            _container.DeclareSignal<UnitRecruitmentCommandRejectedSignal>()
                .OptionalSubscriber();
            _container.DeclareSignal<GameModeChangedSignal>()
                .OptionalSubscriber();
            _container.DeclareSignal<WorldInfoPanelClosedSignal>()
                .OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();
            _signals.Subscribe<WorldInfoPanelClosedSignal>(
                () => _panelClosedCount++);

            _turns = new FakeTurnService();
            _recruitment = new FakeRecruitmentService();
            _overlay = new FakeOverlay();
            _inputPolicy = new FakeInputPolicy();
            _gameMode = new FakeGameMode();
            _notifications = new FakeNotifications();

            _controller = new UnitRecruitmentDeploymentController(
                _signals,
                _turns,
                _recruitment,
                new FakeUnitConfigs(),
                new FakeGridProjection(),
                gridOverlay: _overlay,
                inputPolicy: _inputPolicy,
                gameModeService: _gameMode,
                notifications: _notifications);
            _controller.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyIfExists("UnitDeploymentControls");
            DestroyIfExists("UnitDeploymentPreviewRoot");
            _controller?.Dispose();
            _controller = null;
        }

        [Test]
        public void ReadyClick_StartsSession_AcquiresInputAndOverlay_ClosesPanel()
        {
            EnqueueReadyItem();
            FireReadyClick();

            Assert.IsTrue(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(1, _overlay.AcquireCalls);
            Assert.AreEqual(GridActionOverlayOwner.Deployment, _overlay.ActiveOwner);
            Assert.AreEqual(1, _inputPolicy.AcquireCalls);
            Assert.AreEqual(1, _inputPolicy.ActiveBlocks);
            Assert.AreEqual(1, _panelClosedCount);
            Assert.AreEqual(0, _notifications.Shown.Count);
        }

        [Test]
        public void ReadyClick_WhenOverlayBusy_RollsBackAndWarns()
        {
            _overlay.AcquireResult = false;
            EnqueueReadyItem();
            FireReadyClick();

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(GridActionOverlayOwner.None, _overlay.ActiveOwner);
            Assert.AreEqual(0, _inputPolicy.ActiveBlocks);
            Assert.AreEqual(0, _panelClosedCount);
            AssertNotification(
                GameplayNotificationKind.Warning,
                "Another map action is already in progress.");
        }

        [Test]
        public void ReadyClick_WhenNotOwnersTurn_WarnsAndDoesNotStart()
        {
            _turns.CanAct = false;
            _turns.ActReason = "It is not your turn.";
            EnqueueReadyItem();
            FireReadyClick();

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(0, _overlay.AcquireCalls);
            AssertNotification(
                GameplayNotificationKind.Warning,
                "It is not your turn.");
        }

        [Test]
        public void ReadyClick_InNonNormalMode_WarnsAndDoesNotStart()
        {
            _gameMode.CurrentMode = GameModeType.Construction;
            EnqueueReadyItem();
            FireReadyClick();

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(0, _overlay.AcquireCalls);
            AssertNotification(
                GameplayNotificationKind.Warning,
                "Unit deployment is not available right now.");
        }

        [Test]
        public void ReadyClick_WhenItemNotReady_WarnsAndDoesNotStart()
        {
            _recruitment.Queue.Add(new UnitRecruitmentQueueItemSnapshot(
                ReadyQueueId,
                OwnerId,
                BuildingPos,
                "militia",
                completedTurns: 0,
                trainingTurns: 2,
                enqueuedGlobalTurn: 1,
                UnitRecruitmentQueueStatus.Training));
            FireReadyClick();

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            AssertNotification(
                GameplayNotificationKind.Warning,
                "This recruitment is no longer ready to deploy.");
        }

        [Test]
        public void ReadyClick_ForOtherOwner_IsIgnored()
        {
            EnqueueReadyItem();
            _signals.Fire(new UnitRecruitmentReadyIndicatorClickedSignal
            {
                OwnerId = "owner-b",
                QueueId = ReadyQueueId,
                UnitTypeId = "militia",
                RecruitingBuildingPosition = BuildingPos,
            });

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(0, _notifications.Shown.Count);
        }

        [Test]
        public void CancelAction_EndsSession_ReleasesOverlayAndInputBlock()
        {
            EnqueueReadyItem();
            FireReadyClick();

            UiActionResult result = _controller.Execute(
                new UiActionRequest(UiActionIds.Deployment.Cancel));

            Assert.AreEqual(UiActionStatus.Performed, result.Status);
            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(1, _overlay.ReleaseCalls);
            Assert.AreEqual(GridActionOverlayOwner.None, _overlay.ActiveOwner);
            Assert.AreEqual(0, _inputPolicy.ActiveBlocks);
        }

        [Test]
        public void Cancel_WithoutSession_IsRejected()
        {
            UiActionResult result = _controller.Execute(
                new UiActionRequest(UiActionIds.Deployment.Cancel));

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(UiActionReason.WrongContext, result.Reason);
        }

        [Test]
        public void Confirm_WithoutSelection_IsRejected()
        {
            EnqueueReadyItem();
            FireReadyClick();

            UiActionResult result = _controller.Execute(
                new UiActionRequest(UiActionIds.Deployment.Confirm));

            Assert.AreEqual(UiActionStatus.Rejected, result.Status);
            Assert.AreEqual(UiActionReason.NoSelection, result.Reason);
        }

        [Test]
        public void TurnEnds_CancelsSession_AndNotifies()
        {
            EnqueueReadyItem();
            FireReadyClick();

            _turns.CanAct = false;
            _turns.ActReason = "Your turn has ended.";
            _turns.RaiseStateChanged();

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(1, _overlay.ReleaseCalls);
            AssertNotification(
                GameplayNotificationKind.Info,
                "Your turn has ended.");
        }

        [Test]
        public void QueueChanged_ItemNoLongerReady_EndsSession()
        {
            EnqueueReadyItem();
            FireReadyClick();
            _recruitment.Queue.Clear();

            _signals.Fire(new UnitRecruitmentQueueChangedSignal
            {
                OwnerId = OwnerId,
                BuildingPosition = BuildingPos,
                QueueId = ReadyQueueId,
                UnitTypeId = "militia",
            });

            Assert.IsFalse(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(1, _overlay.ReleaseCalls);
            Assert.AreEqual(0, _inputPolicy.ActiveBlocks);
        }

        [Test]
        public void ReadyClick_SecondReadyItem_ReplacesActiveSession()
        {
            EnqueueReadyItem();
            FireReadyClick();

            _recruitment.Queue.Add(new UnitRecruitmentQueueItemSnapshot(
                queueId: 8,
                ownerId: OwnerId,
                recruitingBuildingPosition: BuildingPos,
                unitTypeId: "archer",
                completedTurns: 1,
                trainingTurns: 1,
                enqueuedGlobalTurn: 2,
                UnitRecruitmentQueueStatus.Training));
            _signals.Fire(new UnitRecruitmentReadyIndicatorClickedSignal
            {
                OwnerId = OwnerId,
                QueueId = 8,
                UnitTypeId = "archer",
                RecruitingBuildingPosition = BuildingPos,
            });

            Assert.IsTrue(_controller.IsTurnBlocked(out _));
            Assert.AreEqual(2, _overlay.AcquireCalls);
            Assert.AreEqual(1, _overlay.ReleaseCalls);
            Assert.AreEqual(GridActionOverlayOwner.Deployment, _overlay.ActiveOwner);
        }

        private void EnqueueReadyItem()
        {
            _recruitment.Queue.Add(new UnitRecruitmentQueueItemSnapshot(
                ReadyQueueId,
                OwnerId,
                BuildingPos,
                "militia",
                completedTurns: 2,
                trainingTurns: 2,
                enqueuedGlobalTurn: 1,
                UnitRecruitmentQueueStatus.Training));
            _recruitment.Tiles.Add(new UnitRecruitmentDeploymentTileSnapshot(
                new Vector2Int(5, 4), true, null));
        }

        private void FireReadyClick()
        {
            _signals.Fire(new UnitRecruitmentReadyIndicatorClickedSignal
            {
                OwnerId = OwnerId,
                QueueId = ReadyQueueId,
                UnitTypeId = "militia",
                RecruitingBuildingPosition = BuildingPos,
            });
        }

        [Test]
        public void ClientConfirm_SendsSingleDeployRequest_UntilRejected()
        {
            var roles = new FakeRoleResolver { Role = LocalGameplayRole.Client };
            var remote = new FakeRemoteRecruitment();
            RebuildController(roles, remote);
            EnqueueReadyItem();
            FireReadyClick();
            _controller.SelectTile(new Vector2Int(5, 4));

            var request = new UiActionRequest(UiActionIds.Deployment.Confirm);
            _controller.Execute(request);
            _controller.Execute(request);
            _controller.Execute(request);

            Assert.AreEqual(
                1,
                remote.DeployRequests,
                "Awaiting a host answer must not resend the deploy request.");
            Assert.AreEqual(0, _recruitment.DeployCalls);
        }

        [Test]
        public void ClientConfirm_HostRejectionReEnablesConfirm()
        {
            var roles = new FakeRoleResolver { Role = LocalGameplayRole.Client };
            var remote = new FakeRemoteRecruitment();
            RebuildController(roles, remote);
            EnqueueReadyItem();
            FireReadyClick();
            _controller.SelectTile(new Vector2Int(5, 4));
            _controller.Execute(
                new UiActionRequest(UiActionIds.Deployment.Confirm));

            _signals.Fire(new UnitRecruitmentCommandRejectedSignal
            {
                Reason = "Tile occupied by host-side order.",
            });
            _controller.Execute(
                new UiActionRequest(UiActionIds.Deployment.Confirm));

            Assert.AreEqual(2, remote.DeployRequests);
        }

        private void RebuildController(
            ILocalGameplayRoleResolver roles,
            IUnitRecruitmentRemoteCommandRequester remote)
        {
            _controller.Dispose();
            _controller = new UnitRecruitmentDeploymentController(
                _signals,
                _turns,
                _recruitment,
                new FakeUnitConfigs(),
                new FakeGridProjection(),
                gridOverlay: _overlay,
                inputPolicy: _inputPolicy,
                gameModeService: _gameMode,
                remoteRecruitment: remote,
                roleResolver: roles,
                notifications: _notifications);
            _controller.Initialize();
        }

        private void AssertNotification(
            GameplayNotificationKind kind,
            string messagePart)
        {
            foreach (GameplayNotificationRequest request in _notifications.Shown)
            {
                if (request.Kind == kind
                    && request.Message != null
                    && request.Message.Contains(messagePart))
                {
                    return;
                }
            }

            Assert.Fail(
                $"Expected {kind} notification containing '{messagePart}'. " +
                $"Shown: {string.Join(" | ", _notifications.Shown.ConvertAll(r => r.Message))}");
        }

        private static void DestroyIfExists(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing);
        }

        private sealed class FakeTurnService : ITurnService
        {
            public event Action StateChanged;
            public bool CanAct = true;
            public string ActReason;

            public TurnPhase Phase => TurnPhase.AwaitingInput;
            public int Round => 1;
            public long GlobalTurn => 1;
            public int ActionsThisTurn => 0;
            public string ActiveOwnerId => OwnerId;
            public string LocalOwnerId => OwnerId;
            public IReadOnlyList<TurnFaction> Factions
                => Array.Empty<TurnFaction>();

            public bool IsOwnerActive(string ownerId) => CanAct;

            public bool CanOwnerAct(string ownerId, out string reason)
            {
                reason = ActReason;
                return CanAct;
            }

            public bool TryRecordAction(string ownerId, string actionId) => true;

            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                reason = null;
                return true;
            }

            public void RaiseStateChanged() => StateChanged?.Invoke();
        }

        private sealed class FakeRecruitmentService : IUnitRecruitmentService
        {
            public readonly List<UnitRecruitmentQueueItemSnapshot> Queue = new();
            public readonly List<UnitRecruitmentDeploymentTileSnapshot> Tiles = new();
            public int DeployCalls;
            public bool DeployResult = true;
            public string DeployRejectReason;

            public bool TryEnqueue(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                string unitTypeId,
                out string reason)
            {
                reason = null;
                return false;
            }

            public bool TryCancel(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId,
                out string reason)
            {
                reason = null;
                return false;
            }

            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(
                string ownerId,
                Vector2Int recruitingBuildingPosition) => Queue;

            public bool TryPeekReady(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                out UnitRecruitmentQueueItemSnapshot item)
            {
                foreach (UnitRecruitmentQueueItemSnapshot candidate in Queue)
                {
                    if (candidate.IsReady)
                    {
                        item = candidate;
                        return true;
                    }
                }

                item = default;
                return false;
            }

            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyItems(
                string ownerId)
            {
                var ready = new List<UnitRecruitmentQueueItemSnapshot>();
                foreach (UnitRecruitmentQueueItemSnapshot item in Queue)
                    if (item.IsReady)
                        ready.Add(item);
                return ready;
            }

            public IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot>
                GetDeploymentTiles(
                    string ownerId,
                    Vector2Int recruitingBuildingPosition,
                    long queueId) => Tiles;

            public bool TryDeployReady(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId,
                Vector2Int targetPosition,
                out string unitId,
                out string reason)
            {
                DeployCalls++;
                unitId = DeployResult ? "unit-1" : null;
                reason = DeployResult ? null : DeployRejectReason;
                return DeployResult;
            }
        }

        private sealed class FakeRoleResolver : ILocalGameplayRoleResolver
        {
            public LocalGameplayRole Role = LocalGameplayRole.Offline;

            public LocalGameplayRoleSnapshot Resolve()
                => new LocalGameplayRoleSnapshot(Role, OwnerId);
        }

        private sealed class FakeRemoteRecruitment
            : IUnitRecruitmentRemoteCommandRequester
        {
            public int DeployRequests;
            public bool RequestResult = true;
            public string RequestReason;

            public bool TryRequestEnqueue(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                string unitTypeId,
                out string reason)
            {
                reason = null;
                return RequestResult;
            }

            public bool TryRequestCancel(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId,
                out string reason)
            {
                reason = null;
                return RequestResult;
            }

            public bool TryRequestDeploy(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId,
                Vector2Int targetPosition,
                out string reason)
            {
                DeployRequests++;
                reason = RequestReason;
                return RequestResult;
            }
        }

        private sealed class FakeOverlay : IGridActionOverlayService
        {
            public bool AcquireResult = true;
            public int AcquireCalls;
            public int ReleaseCalls;
            public GridActionOverlayOwner ActiveOwner { get; private set; }
                = GridActionOverlayOwner.None;

            public bool HasActiveOwner
                => ActiveOwner != GridActionOverlayOwner.None;

            public bool Acquire(GridActionOverlayOwner owner)
            {
                AcquireCalls++;
                if (!AcquireResult)
                    return false;
                ActiveOwner = owner;
                return true;
            }

            public void Show(
                GridActionOverlayOwner owner,
                IReadOnlyList<GridActionOverlayCell> cells)
            {
            }

            public void Update(
                GridActionOverlayOwner owner,
                IReadOnlyList<GridActionOverlayCell> cells)
            {
            }

            public void Hide(GridActionOverlayOwner owner)
            {
            }

            public void Release(GridActionOverlayOwner owner)
            {
                ReleaseCalls++;
                if (ActiveOwner == owner)
                    ActiveOwner = GridActionOverlayOwner.None;
            }
        }

        private sealed class FakeInputPolicy : IGameplayInputPolicy
        {
            public int AcquireCalls;
            public int ActiveBlocks;

            public bool CanProcess(
                GameplayInputKind inputKind,
                Vector2 screenPosition,
                int pointerId = -1) => true;

            public bool IsPointerOverUi(
                Vector2 screenPosition,
                int pointerId = -1,
                bool interactiveOnly = true) => false;

            public bool TryBeginPointerCapture(
                GameplayInputKind inputKind,
                Vector2 screenPosition,
                int pointerId = -1) => true;

            public void EndPointerCapture(
                GameplayInputKind inputKind,
                int pointerId = -1)
            {
            }

            public IDisposable AcquireBlock(
                GameplayInputKind inputMask,
                object owner)
            {
                AcquireCalls++;
                ActiveBlocks++;
                return new Block(this);
            }

            private sealed class Block : IDisposable
            {
                private FakeInputPolicy _policy;

                public Block(FakeInputPolicy policy) => _policy = policy;

                public void Dispose()
                {
                    if (_policy == null)
                        return;
                    _policy.ActiveBlocks--;
                    _policy = null;
                }
            }
        }

        private sealed class FakeGameMode : IGameModeService
        {
            public GameModeType CurrentMode { get; set; } = GameModeType.Normal;

            public void SetMode(GameModeType newMode) => CurrentMode = newMode;
        }

        private sealed class FakeNotifications : IGameplayNotificationService
        {
            public readonly List<GameplayNotificationRequest> Shown = new();

            public void Show(
                string message,
                GameplayNotificationKind kind = GameplayNotificationKind.Info,
                float? holdDuration = null,
                string dedupKey = null)
                => Shown.Add(new GameplayNotificationRequest(
                    message, kind, holdDuration, dedupKey));

            public void Show(GameplayNotificationRequest request)
                => Shown.Add(request);

            public void Clear() => Shown.Clear();
        }

        private sealed class FakeUnitConfigs : IUnitClassConfig
        {
            public UnitClassConfig GetConfig(string typeId) => null;
        }

        private sealed class FakeGridProjection : IGridProjection
        {
            public GridProjectionMode ProjectionMode => default;
            public GridTopology Topology => default;
            public GridWorldPlane WorldPlane => GridWorldPlane.XZ;

            public Vector3 GridToWorld(Vector2Int gridPosition)
                => new(gridPosition.x, 0f, gridPosition.y);

            public Vector3 GridToWorld(
                Vector2Int gridPosition,
                float elevation,
                float layerOffset = 0f)
                => new(gridPosition.x, elevation + layerOffset, gridPosition.y);

            public Vector2Int WorldToGrid(Vector3 worldPosition)
                => new(
                    Mathf.RoundToInt(worldPosition.x),
                    Mathf.RoundToInt(worldPosition.z));

            public IEnumerable<Vector2Int> GetNeighborCandidates(
                Vector2Int gridPosition)
            {
                yield return gridPosition + Vector2Int.up;
                yield return gridPosition + Vector2Int.down;
                yield return gridPosition + Vector2Int.left;
                yield return gridPosition + Vector2Int.right;
            }

            public float GetStepDistance(Vector2Int from, Vector2Int to) => 1f;

            public float EstimateDistance(Vector2Int from, Vector2Int to)
                => Vector2Int.Distance(from, to);

            public Bounds GetWorldBounds(int width, int height)
                => new(Vector3.zero, new Vector3(width, 1f, height));
        }
    }
}
