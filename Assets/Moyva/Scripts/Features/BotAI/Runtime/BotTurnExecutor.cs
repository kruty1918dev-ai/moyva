using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Single turn-scoped executor for bot gameplay mutations.
    ///
    /// It deliberately issues commands through the same authoritative construction,
    /// recruitment and movement APIs used by normal gameplay. The executor never ends
    /// the turn itself: TurnBotDriver retries ITurnService.TryEndTurn on later ticks,
    /// allowing ITurnBlocker implementations (notably movement) to settle first.
    /// </summary>
    public sealed class BotTurnExecutor : IBotTurnExecutor
    {
        internal const int MaxMutatingActionsPerTurn = 6;
        internal const int MaxMoveActionsPerTurn = 4;
        internal const int BarrackSearchRadius = 6;
        internal const string BarrackBuildingId = "barrack";
        internal const string FallbackUnitTypeId = "warrior";

        private readonly ITurnService _turns;
        private readonly IFactionRegistry _factions;
        private readonly IConstructionService _construction;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitMovementService _movement;
        private readonly IGridService _grid;
        private readonly IObjectsMapService _objectsMap;
        private readonly IFogOfWarServiceRegistry _fogRegistry;
        private readonly HashSet<BotTurnEpoch> _startedEpochs = new();

        [Inject]
        public BotTurnExecutor(
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IFactionRegistry factions = null,
            [InjectOptional] IConstructionService construction = null,
            [InjectOptional] IUnitRecruitmentService recruitment = null,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IUnitMovementService movement = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IObjectsMapService objectsMap = null,
            [InjectOptional] IFogOfWarServiceRegistry fogRegistry = null)
        {
            _turns = turns;
            _factions = factions;
            _construction = construction;
            _recruitment = recruitment;
            _units = units;
            _ownership = ownership;
            _movement = movement;
            _grid = grid;
            _objectsMap = objectsMap;
            _fogRegistry = fogRegistry;
        }

        internal int StartedEpochCount => _startedEpochs.Count;

        public bool TryBeginTurn(string ownerId, long globalTurn, out string reason)
        {
            reason = null;
            string owner = NormalizeId(ownerId);
            if (owner == null)
            {
                reason = "Bot turn owner is empty.";
                return false;
            }
            if (globalTurn < 1)
            {
                reason = "Bot global turn must be positive.";
                return false;
            }
            if (_turns == null)
            {
                reason = "Turn authority is unavailable for bot execution.";
                return false;
            }
            if (_turns.GlobalTurn != globalTurn)
            {
                reason = $"Bot turn epoch mismatch: requested {globalTurn}, authoritative {_turns.GlobalTurn}.";
                return false;
            }
            if (_turns.Phase != TurnPhase.AwaitingInput)
            {
                reason = $"Bot execution requires AwaitingInput, current phase is {_turns.Phase}.";
                return false;
            }
            if (!_turns.IsActiveFactionBot)
            {
                reason = "The active faction is not a bot.";
                return false;
            }
            if (!string.Equals(NormalizeId(_turns.ActiveOwnerId), owner, StringComparison.Ordinal))
            {
                reason = $"Bot owner '{owner}' is not the active owner '{_turns.ActiveOwnerId}'.";
                return false;
            }
            if (!_turns.CanOwnerAct(owner, out reason))
                return false;

            var epoch = new BotTurnEpoch(owner, globalTurn);
            if (_startedEpochs.Contains(epoch))
            {
                reason = null;
                return true;
            }

            // Claim the epoch before the first mutation. If a synchronous gameplay
            // signal re-enters the executor, the same turn cannot issue actions twice.
            _startedEpochs.Add(epoch);
            PruneOldEpochs(globalTurn);

            try
            {
                ExecuteTurn(owner, globalTurn);
                reason = null;
                return true;
            }
            catch (Exception exception)
            {
                // Keep the epoch claimed after a partial failure. Retrying the same
                // turn could duplicate a successful command that preceded the fault.
                reason = $"Bot turn execution failed safely: {exception.Message}";
                Debug.LogError($"[BotTurnExecutor] owner={owner} globalTurn={globalTurn}: {exception}");
                return false;
            }
        }

        private void ExecuteTurn(string ownerId, long globalTurn)
        {
            var budget = new BotTurnBudget(MaxMutatingActionsPerTurn);
            ResolveBotProfile(ownerId, out Vector2Int startPosition, out string unitTypeId);

            bool hasBarrack = TryFindOwnedBuilding(
                ownerId,
                BarrackBuildingId,
                out Vector2Int barrackPosition);

            if (!hasBarrack
                && budget.HasRemaining
                && TryPlaceBarrack(ownerId, startPosition, out barrackPosition))
            {
                budget.TrySpend();
                hasBarrack = true;
            }

            if (hasBarrack
                && budget.HasRemaining
                && _recruitment != null
                && _recruitment.TryEnqueue(
                    ownerId,
                    barrackPosition,
                    unitTypeId,
                    out _))
            {
                // IUnitRecruitmentService owns economy consumption and turn-action
                // telemetry. The executor budget only limits command fan-out.
                budget.TrySpend();
            }

            if (!budget.HasRemaining)
                return;

            IssueDeterministicMoves(ownerId, globalTurn, budget);
        }

        private void ResolveBotProfile(
            string ownerId,
            out Vector2Int startPosition,
            out string unitTypeId)
        {
            startPosition = Vector2Int.zero;
            unitTypeId = FallbackUnitTypeId;

            if (_factions != null)
            {
                IReadOnlyList<FactionDefinition> all = _factions.GetAll();
                if (all != null)
                {
                    for (int index = 0; index < all.Count; index++)
                    {
                        FactionDefinition faction = all[index];
                        if (faction == null
                            || !string.Equals(
                                NormalizeId(faction.FactionId.Value),
                                ownerId,
                                StringComparison.Ordinal))
                        {
                            continue;
                        }

                        startPosition = faction.StartPosition;
                        string configuredType = NormalizeId(faction.DefaultUnitTypeId);
                        if (configuredType != null)
                            unitTypeId = configuredType;
                        return;
                    }
                }
            }

            IReadOnlyList<TurnFaction> turnFactions = _turns?.Factions;
            if (turnFactions == null)
                return;

            for (int index = 0; index < turnFactions.Count; index++)
            {
                if (!string.Equals(
                        NormalizeId(turnFactions[index].OwnerId),
                        ownerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                startPosition = turnFactions[index].StartPosition;
                return;
            }
        }

        private bool TryFindOwnedBuilding(
            string ownerId,
            string buildingId,
            out Vector2Int position)
        {
            position = default;
            if (_construction is not IConstructionSaveSnapshotSource source)
                return false;

            IReadOnlyList<ConstructionSavedPlacement> placements = source.GetSavedPlacements();
            if (placements == null)
                return false;

            bool found = false;
            Vector2Int best = default;
            for (int index = 0; index < placements.Count; index++)
            {
                ConstructionSavedPlacement placement = placements[index];
                if (!string.Equals(NormalizeId(placement.OwnerId), ownerId, StringComparison.Ordinal)
                    || !string.Equals(placement.BuildingId, buildingId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!found || ComparePosition(placement.Position, best) < 0)
                {
                    best = placement.Position;
                    found = true;
                }
            }

            position = best;
            return found;
        }

        private bool TryPlaceBarrack(
            string ownerId,
            Vector2Int center,
            out Vector2Int position)
        {
            position = default;
            if (_construction == null)
                return false;

            List<Vector2Int> candidates = BuildRingCandidates(center, BarrackSearchRadius);
            for (int index = 0; index < candidates.Count; index++)
            {
                Vector2Int candidate = candidates[index];
                if (_grid != null && !_grid.ContainsCell(candidate))
                    continue;

                if (_construction.TryDirectPlace(BarrackBuildingId, candidate, ownerId))
                {
                    position = candidate;
                    return true;
                }
            }

            return false;
        }

        private void IssueDeterministicMoves(
            string ownerId,
            long globalTurn,
            BotTurnBudget budget)
        {
            if (_units == null
                || _ownership == null
                || _movement == null
                || _grid == null
                || _objectsMap == null)
            {
                return;
            }

            IReadOnlyCollection<string> unitIds = _units.GetAllUnitIds();
            if (unitIds == null || unitIds.Count == 0)
                return;

            var allUnitIds = new List<string>(unitIds);
            allUnitIds.Sort(StringComparer.Ordinal);

            var ownedUnitIds = new List<string>();
            var enemyPositions = new List<Vector2Int>();
            IFogOfWarService fog = null;
            _fogRegistry?.TryGetFor(ownerId, out fog);

            for (int index = 0; index < allUnitIds.Count; index++)
            {
                string unitId = allUnitIds[index];
                string unitOwner = NormalizeId(_ownership.GetUnitOwnerId(unitId));
                if (!_units.TryGetUnitPosition(unitId, out Vector2Int unitPosition))
                    continue;

                if (string.Equals(unitOwner, ownerId, StringComparison.Ordinal))
                {
                    ownedUnitIds.Add(unitId);
                    continue;
                }

                if (unitOwner == null)
                    continue;
                if (fog != null && !fog.IsVisible(unitPosition))
                    continue;
                enemyPositions.Add(unitPosition);
            }

            enemyPositions.Sort(ComparePosition);
            var reservedTargets = new HashSet<Vector2Int>();
            int issuedMoves = 0;

            for (int index = 0;
                 index < ownedUnitIds.Count
                 && budget.HasRemaining
                 && issuedMoves < MaxMoveActionsPerTurn;
                 index++)
            {
                string unitId = ownedUnitIds[index];
                if (!_units.TryGetUnitPosition(unitId, out Vector2Int current))
                    continue;

                Vector2Int? enemy = FindNearestEnemy(current, enemyPositions);
                if (!TryChooseMoveTarget(
                        current,
                        enemy,
                        globalTurn,
                        index,
                        reservedTargets,
                        out Vector2Int target))
                {
                    continue;
                }

                reservedTargets.Add(target);
                _ = _movement.MoveUnitAsync(unitId, target, CancellationToken.None);
                budget.TrySpend();
                issuedMoves++;
            }
        }

        private bool TryChooseMoveTarget(
            Vector2Int current,
            Vector2Int? enemy,
            long globalTurn,
            int unitOrdinal,
            HashSet<Vector2Int> reservedTargets,
            out Vector2Int target)
        {
            Vector2Int[] candidates = BuildPreferredMoveCandidates(
                current,
                enemy,
                globalTurn,
                unitOrdinal);

            for (int index = 0; index < candidates.Length; index++)
            {
                Vector2Int candidate = candidates[index];
                if (!_grid.ContainsCell(candidate)
                    || !_grid.TryGetTileData(candidate, out string tileId)
                    || string.IsNullOrWhiteSpace(tileId)
                    || _objectsMap.IsOccupied(candidate)
                    || reservedTargets.Contains(candidate))
                {
                    continue;
                }

                target = candidate;
                return true;
            }

            target = current;
            return false;
        }

        private static Vector2Int? FindNearestEnemy(
            Vector2Int from,
            IReadOnlyList<Vector2Int> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return null;

            Vector2Int best = enemies[0];
            int bestDistance = ManhattanDistance(from, best);
            for (int index = 1; index < enemies.Count; index++)
            {
                Vector2Int candidate = enemies[index];
                int distance = ManhattanDistance(from, candidate);
                if (distance < bestDistance
                    || (distance == bestDistance && ComparePosition(candidate, best) < 0))
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }
            return best;
        }

        internal static List<Vector2Int> BuildRingCandidates(
            Vector2Int center,
            int maxRadius)
        {
            var result = new List<Vector2Int>();
            int radiusLimit = Math.Max(0, maxRadius);
            for (int radius = 1; radius <= radiusLimit; radius++)
            {
                int minX = center.x - radius;
                int maxX = center.x + radius;
                int minY = center.y - radius;
                int maxY = center.y + radius;

                for (int x = minX; x <= maxX; x++)
                    result.Add(new Vector2Int(x, maxY));
                for (int y = maxY - 1; y >= minY; y--)
                    result.Add(new Vector2Int(maxX, y));
                for (int x = maxX - 1; x >= minX; x--)
                    result.Add(new Vector2Int(x, minY));
                for (int y = minY + 1; y < maxY; y++)
                    result.Add(new Vector2Int(minX, y));
            }
            return result;
        }

        internal static Vector2Int[] BuildPreferredMoveCandidates(
            Vector2Int current,
            Vector2Int? enemy,
            long globalTurn,
            int unitOrdinal)
        {
            Vector2Int[] offsets =
            {
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down,
            };

            int rotation = PositiveModulo(globalTurn + unitOrdinal, offsets.Length);
            var candidates = new Vector2Int[offsets.Length];
            for (int index = 0; index < offsets.Length; index++)
                candidates[index] = current + offsets[(rotation + index) % offsets.Length];

            if (!enemy.HasValue)
                return candidates;

            // Stable insertion sort by distance to the target. The rotated order
            // remains the tie-breaker, so identical inputs are deterministic.
            for (int index = 1; index < candidates.Length; index++)
            {
                Vector2Int value = candidates[index];
                int valueDistance = ManhattanDistance(value, enemy.Value);
                int cursor = index - 1;
                while (cursor >= 0
                       && ManhattanDistance(candidates[cursor], enemy.Value) > valueDistance)
                {
                    candidates[cursor + 1] = candidates[cursor];
                    cursor--;
                }
                candidates[cursor + 1] = value;
            }

            return candidates;
        }

        internal static int ComparePosition(Vector2Int left, Vector2Int right)
        {
            int x = left.x.CompareTo(right.x);
            return x != 0 ? x : left.y.CompareTo(right.y);
        }

        private static int ManhattanDistance(Vector2Int left, Vector2Int right)
            => Mathf.Abs(left.x - right.x) + Mathf.Abs(left.y - right.y);

        private static int PositiveModulo(long value, int divisor)
        {
            if (divisor <= 0)
                return 0;
            long result = value % divisor;
            return (int)(result < 0 ? result + divisor : result);
        }

        private void PruneOldEpochs(long currentGlobalTurn)
        {
            long minimum = Math.Max(1L, currentGlobalTurn - 64L);
            _startedEpochs.RemoveWhere(epoch => epoch.GlobalTurn < minimum);
        }

        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        internal readonly struct BotTurnEpoch : IEquatable<BotTurnEpoch>
        {
            public BotTurnEpoch(string ownerId, long globalTurn)
            {
                OwnerId = ownerId ?? string.Empty;
                GlobalTurn = globalTurn;
            }

            public string OwnerId { get; }
            public long GlobalTurn { get; }

            public bool Equals(BotTurnEpoch other)
                => GlobalTurn == other.GlobalTurn
                   && string.Equals(OwnerId, other.OwnerId, StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is BotTurnEpoch other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((OwnerId != null ? StringComparer.Ordinal.GetHashCode(OwnerId) : 0) * 397)
                           ^ GlobalTurn.GetHashCode();
                }
            }
        }

        internal sealed class BotTurnBudget
        {
            public BotTurnBudget(int limit)
            {
                Remaining = Math.Max(0, limit);
            }

            public int Remaining { get; private set; }
            public bool HasRemaining => Remaining > 0;

            public bool TrySpend()
            {
                if (Remaining <= 0)
                    return false;
                Remaining--;
                return true;
            }
        }
    }
}
