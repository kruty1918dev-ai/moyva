using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitRecruitmentService :
        IUnitRecruitmentService,
        IUnitRecruitmentStateStore,
        ITurnParticipant,
        IInitializable,
        IDisposable
    {
        private readonly UnitRecruitmentQueueStateMachine _queue = new();
        private readonly IEconomyInfoMediator _economy;
        private readonly ITurnService _turns;
        private readonly SignalBus _signalBus;
        private readonly UnitRecruitmentBuildingContextResolver _buildingContext;
        private readonly UnitRecruitmentDeploymentService _deployment;

        [Inject]
        public UnitRecruitmentService(
            IUnitClassConfig unitClassConfig,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IConstructionSaveSnapshotSource constructionSnapshot = null,
            [InjectOptional] IConstructionLifecycle constructionLifecycle = null,
            [InjectOptional] IEconomyInfoMediator economy = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] SignalBus signalBus = null,
            [InjectOptional] IUnitFactory unitFactory = null,
            [InjectOptional] IUnitService unitService = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IObjectsMapService objectsMap = null,
            [InjectOptional] IUnitPlacementValidator placementValidator = null)
        {
            _economy = economy;
            _turns = turns;
            _signalBus = signalBus;

            _buildingContext =
                new UnitRecruitmentBuildingContextResolver(
                    unitClassConfig,
                    buildingRegistry,
                    constructionSnapshot,
                    constructionLifecycle);

            _deployment =
                new UnitRecruitmentDeploymentService(
                    _queue,
                    _buildingContext,
                    turns,
                    signalBus,
                    unitFactory,
                    unitService,
                    ownership,
                    placementValidator);
        }

        public int TurnOrder => 30;

        public void Initialize()
            => _signalBus?.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

        public void Dispose()
            => _signalBus?.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

        public bool TryEnqueue(string ownerId, Vector2Int recruitingBuildingPosition, string unitTypeId, out string reason)
        {
            reason = null;
            string owner = NormalizeRequiredId(ownerId);
            string unitType = NormalizeRequiredId(unitTypeId);
            if (owner == null)
            {
                reason = "Recruitment owner is empty.";
                return false;
            }
            if (unitType == null)
            {
                reason = "Unit type is empty.";
                return false;
            }
            if (_turns == null)
            {
                reason = "Turn authority is unavailable for recruitment.";
                return false;
            }
            if (!_turns.CanOwnerAct(owner, out reason))
                return false;
            if (!_buildingContext.TryResolveEnqueue(
                    recruitingBuildingPosition,
                    owner,
                    unitType,
                    out string recruitingBuildingId,
                    out UnitRecruitmentBuildingModule recruitmentModule,
                    out UnitRecruitmentRecipeDefinition recipe,
                    out reason))
            {
                return false;
            }

            int capacity = Math.Max(1, recruitmentModule.QueueCapacity);
            if (!_queue.CanEnqueue(owner, recruitingBuildingPosition, capacity, out reason))
                return false;

            Dictionary<string, float> costs = BuildCostMap(recipe.Costs);
            if (!TryConsumeRecruitmentCosts(owner, recruitingBuildingPosition, costs, out reason))
                return false;

            UnitRecruitmentQueueItemSnapshot enqueued = _queue.EnqueueValidated(
                owner,
                recruitingBuildingPosition,
                recruitingBuildingId,
                unitType,
                Math.Max(1, recipe.TrainingTurns),
                Math.Max(1L, _turns.GlobalTurn));

            if (!_turns.TryRecordAction(owner, "unit-recruit-enqueue"))
            {
                Debug.LogWarning(
                    "[UnitRecruitment] Queue commit succeeded but turn action telemetry was rejected after commit.");
            }

            FireQueueChanged(enqueued);
            return true;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetQueue(string ownerId, Vector2Int recruitingBuildingPosition)
        {
            string owner = NormalizeRequiredId(ownerId);
            return owner == null
                ? Array.Empty<UnitRecruitmentQueueItemSnapshot>()
                : _queue.GetQueue(owner, recruitingBuildingPosition);
        }

        public bool TryPeekReady(string ownerId, Vector2Int recruitingBuildingPosition, out UnitRecruitmentQueueItemSnapshot item)
        {
            string owner = NormalizeRequiredId(ownerId);
            if (owner != null)
                return _queue.TryPeekReady(owner, recruitingBuildingPosition, out item);
            item = default;
            return false;
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> GetReadyItems(string ownerId)
        {
            string owner = NormalizeRequiredId(ownerId);
            return owner == null
                ? Array.Empty<UnitRecruitmentQueueItemSnapshot>()
                : _queue.GetReadyHeads(owner);
        }

        public IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> GetDeploymentTiles(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId)
        {
            return _deployment.GetDeploymentTiles(
                ownerId,
                recruitingBuildingPosition,
                queueId);
        }

        public bool TryDeployReady(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId,
            Vector2Int targetPosition,
            out string unitId,
            out string reason)
        {
            return _deployment.TryDeployReady(
                ownerId,
                recruitingBuildingPosition,
                queueId,
                targetPosition,
                out unitId,
                out reason);
        }

        public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> CaptureState()
            => _queue.CaptureAll();

        public void RestoreState(IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items)
        {
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> before =
                _signalBus == null
                    ? Array.Empty<UnitRecruitmentQueueItemSnapshot>()
                    : _queue.CaptureAll();

            _queue.RestoreAll(items);

            if (_signalBus != null)
                FireRestoreQueueNotifications(before, _queue.CaptureAll());
        }

        public void OnTurnStarted(TurnContext context)
        {
            string owner = NormalizeRequiredId(context.Faction.OwnerId);
            if (owner == null)
                return;

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyBefore =
                _queue.GetReadyHeads(owner);
            var readyIdsBefore = new HashSet<long>();
            for (int index = 0; index < readyBefore.Count; index++)
                readyIdsBefore.Add(readyBefore[index].QueueId);

            bool progressed = _queue.AdvanceOwnerTurn(
                owner,
                context.GlobalTurn);

            if (!progressed)
                return;

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> state =
                _queue.CaptureAll();
            for (int index = 0; index < state.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = state[index];
                if (!string.Equals(
                        item.OwnerId,
                        owner,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                FireQueueChanged(item);
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyAfter =
                _queue.GetReadyHeads(owner);
            for (int index = 0; index < readyAfter.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot ready = readyAfter[index];
                if (readyIdsBefore.Contains(ready.QueueId))
                    continue;

                _signalBus?.Fire(new UnitRecruitmentReadySignal
                {
                    OwnerId = ready.OwnerId,
                    BuildingPosition = ready.RecruitingBuildingPosition,
                    QueueId = ready.QueueId,
                    UnitTypeId = ready.UnitTypeId,
                });
            }

            // P24A: ready recruitment jobs deliberately persist in the queue.
            // Unit creation happens only through TryDeployReady after an explicit
            // participant tile selection.
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }



        private void FireQueueChanged(
            UnitRecruitmentQueueItemSnapshot item)
        {
            _signalBus?.Fire(new UnitRecruitmentQueueChangedSignal
            {
                OwnerId = item.OwnerId,
                BuildingPosition = item.RecruitingBuildingPosition,
                QueueId = item.QueueId,
                UnitTypeId = item.UnitTypeId,
                CompletedTurns = item.CompletedTurns,
                TrainingTurns = item.TrainingTurns,
                IsReady = item.IsReady,
            });
        }

        private void FireRestoreQueueNotifications(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> before,
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> after)
        {
            var keys = new List<RecruitmentQueueSignalKey>();
            AddAffectedQueueKeys(before, keys);
            AddAffectedQueueKeys(after, keys);
            keys.Sort(RecruitmentQueueSignalKey.Compare);

            for (int index = 0; index < keys.Count; index++)
            {
                RecruitmentQueueSignalKey key = keys[index];
                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                    _queue.GetQueue(key.OwnerId, key.Position);

                if (queue != null && queue.Count > 0)
                {
                    FireQueueChanged(queue[0]);
                    continue;
                }

                _signalBus?.Fire(new UnitRecruitmentQueueChangedSignal
                {
                    OwnerId = key.OwnerId,
                    BuildingPosition = key.Position,
                    QueueId = 0,
                    UnitTypeId = string.Empty,
                    CompletedTurns = 0,
                    TrainingTurns = 0,
                    IsReady = false,
                });
            }
        }

        private static void AddAffectedQueueKeys(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items,
            List<RecruitmentQueueSignalKey> keys)
        {
            if (items == null || keys == null)
                return;

            for (int index = 0; index < items.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = items[index];
                var key = new RecruitmentQueueSignalKey(
                    item.OwnerId,
                    item.RecruitingBuildingPosition);

                bool exists = false;
                for (int keyIndex = 0; keyIndex < keys.Count; keyIndex++)
                {
                    if (keys[keyIndex].Equals(key))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    keys.Add(key);
            }
        }


        private readonly struct RecruitmentQueueSignalKey : IEquatable<RecruitmentQueueSignalKey>
        {
            public RecruitmentQueueSignalKey(string ownerId, Vector2Int position)
            {
                OwnerId = ownerId ?? string.Empty;
                Position = position;
            }

            public string OwnerId { get; }
            public Vector2Int Position { get; }

            public bool Equals(RecruitmentQueueSignalKey other)
                => string.Equals(OwnerId, other.OwnerId, StringComparison.Ordinal)
                   && Position == other.Position;

            public override bool Equals(object obj)
                => obj is RecruitmentQueueSignalKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((OwnerId != null
                        ? StringComparer.Ordinal.GetHashCode(OwnerId)
                        : 0) * 397) ^ Position.GetHashCode();
                }
            }

            public static int Compare(
                RecruitmentQueueSignalKey left,
                RecruitmentQueueSignalKey right)
            {
                int owner = string.CompareOrdinal(left.OwnerId, right.OwnerId);
                if (owner != 0)
                    return owner;

                int x = left.Position.x.CompareTo(right.Position.x);
                return x != 0 ? x : left.Position.y.CompareTo(right.Position.y);
            }
        }


        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            if (_queue.RemoveBuildingQueues(signal.Position))
            {
                Debug.Log($"[UnitRecruitment] Dropped paid recruitment queue at demolished building {signal.Position}.");
                _signalBus?.Fire(new UnitRecruitmentQueueChangedSignal
                {
                    OwnerId = signal.OwnerId,
                    BuildingPosition = signal.Position,
                    QueueId = 0,
                    UnitTypeId = string.Empty,
                    CompletedTurns = 0,
                    TrainingTurns = 0,
                    IsReady = false,
                });
            }
        }



        internal static Dictionary<string, float> BuildCostMap(IReadOnlyList<BuildingResourceAmount> source)
        {
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            if (source == null)
                return result;
            for (int index = 0; index < source.Count; index++)
            {
                BuildingResourceAmount entry = source[index];
                string resourceId = NormalizeRequiredId(entry?.ResourceId);
                if (resourceId == null || entry.Amount <= 0)
                    continue;
                if (result.TryGetValue(resourceId, out float current))
                    result[resourceId] = current + entry.Amount;
                else
                    result.Add(resourceId, entry.Amount);
            }
            return result;
        }

        private bool TryConsumeRecruitmentCosts(
            string ownerId,
            Vector2Int buildingPosition,
            IReadOnlyDictionary<string, float> costs,
            out string reason)
        {
            reason = null;
            if (costs == null || costs.Count == 0)
                return true;
            if (_economy == null)
            {
                reason = "Economy is unavailable for recruitment costs.";
                return false;
            }
            if (!_economy.OwnerHasAnyWarehouse(ownerId))
                return _economy.TryConsumeOwnerPoolResources(ownerId, costs, out reason);

            if (!_economy.TryResolveConstructionSettlement(
                    buildingPosition,
                    ownerId,
                    out EconomySettlementContext settlement)
                || string.IsNullOrWhiteSpace(settlement.SettlementId)
                || !string.Equals(NormalizeRequiredId(settlement.OwnerId), ownerId, StringComparison.Ordinal))
            {
                reason = "No owned settlement is available to fund recruitment at this building.";
                return false;
            }
            return _economy.TryConsumeSettlementResources(settlement.SettlementId, costs, out reason);
        }



        private static string NormalizeRequiredId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
