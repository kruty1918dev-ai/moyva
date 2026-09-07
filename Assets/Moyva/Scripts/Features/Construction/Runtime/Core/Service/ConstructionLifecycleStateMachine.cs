using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Deterministic, Unity-object-free construction-progress model.
    /// It owns construction progress identity while presentation/signals stay in
    /// <see cref="ConstructionLifecycleService"/>.
    /// </summary>
    internal sealed class ConstructionLifecycleStateMachine
    {
        internal readonly struct SavedState
        {
            public SavedState(
                Vector2Int position,
                string buildingId,
                string ownerId,
                int required,
                int completed,
                long placedTurn)
            {
                Position = position;
                BuildingId = buildingId ?? string.Empty;
                OwnerId = NormalizeOwner(ownerId);
                Required = Math.Max(0, required);
                Completed = Math.Max(0, completed);
                PlacedTurn = Math.Max(0L, placedTurn);
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
            public string OwnerId { get; }
            public int Required { get; }
            public int Completed { get; }
            public long PlacedTurn { get; }
        }

        internal readonly struct OperationalTransition
        {
            public OperationalTransition(
                Vector2Int position,
                string buildingId,
                string ownerId)
            {
                Position = position;
                BuildingId = buildingId ?? string.Empty;
                OwnerId = NormalizeOwner(ownerId);
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
            public string OwnerId { get; }
            public bool IsValid => !string.IsNullOrWhiteSpace(BuildingId);
        }

        private sealed class State
        {
            public string BuildingId;
            public string OwnerId;
            public int Required;
            public int Completed;
            public long PlacedTurn;
            public bool OperationalPublished;
        }

        private readonly Dictionary<Vector2Int, State> _states = new();

        public int Count => _states.Count;

        public bool IsOperational(Vector2Int position)
            => !_states.TryGetValue(position, out State state)
               || state.Completed >= state.Required;

        public bool TryGetProgress(
            Vector2Int position,
            out int completedTurns,
            out int requiredTurns)
        {
            if (_states.TryGetValue(position, out State state))
            {
                completedTurns = state.Completed;
                requiredTurns = state.Required;
                return true;
            }

            completedTurns = 0;
            requiredTurns = 0;
            return false;
        }

        public bool TryGetSavedState(
            Vector2Int position,
            out SavedState saved)
        {
            if (!_states.TryGetValue(position, out State state))
            {
                saved = default;
                return false;
            }

            saved = ToSavedState(position, state);
            return true;
        }

        /// <summary>
        /// Registers a committed placement. Duplicate placement notifications for
        /// the same identity never reset progress. Relocations move the lifecycle
        /// state from source to destination and preserve progress.
        /// </summary>
        public bool RegisterPlacement(
            Vector2Int position,
            string buildingId,
            string ownerId,
            int requiredTurns,
            long placedTurn,
            Vector2Int? relocationSource,
            out OperationalTransition operational)
        {
            operational = default;
            string normalizedBuilding = NormalizeBuilding(buildingId);
            string normalizedOwner = NormalizeOwner(ownerId);
            int normalizedRequired = Math.Max(0, requiredTurns);
            long normalizedPlacedTurn = Math.Max(0L, placedTurn);

            if (relocationSource.HasValue
                && relocationSource.Value != position
                && _states.TryGetValue(relocationSource.Value, out State moving))
            {
                _states.Remove(relocationSource.Value);

                bool sameIdentity =
                    string.IsNullOrWhiteSpace(moving.BuildingId)
                    || string.Equals(
                        moving.BuildingId,
                        normalizedBuilding,
                        StringComparison.Ordinal);

                if (!sameIdentity)
                {
                    moving = CreateState(
                        normalizedBuilding,
                        normalizedOwner,
                        normalizedRequired,
                        normalizedPlacedTurn);
                }
                else
                {
                    moving.BuildingId = normalizedBuilding;
                    moving.OwnerId = normalizedOwner;
                    moving.Required = normalizedRequired;
                    moving.Completed = Math.Min(
                        moving.Completed,
                        normalizedRequired);
                    // Relocation is not a new build action. Preserve PlacedTurn so
                    // moving a half-built structure cannot steal or lose a turn.
                }

                _states[position] = moving;
                if (moving.Completed >= moving.Required
                    && !string.IsNullOrWhiteSpace(moving.BuildingId))
                {
                    // Operational consumers are position based. A completed
                    // structure moved to another origin must announce the new origin.
                    moving.OperationalPublished = true;
                    operational = ToOperational(position, moving);
                }

                return true;
            }

            if (_states.TryGetValue(position, out State existing))
            {
                bool missingIdentity =
                    string.IsNullOrWhiteSpace(existing.BuildingId);
                bool sameIdentity = missingIdentity
                    || (string.Equals(
                            existing.BuildingId,
                            normalizedBuilding,
                            StringComparison.Ordinal)
                        && string.Equals(
                            existing.OwnerId,
                            normalizedOwner,
                            StringComparison.Ordinal));

                if (sameIdentity)
                {
                    existing.BuildingId = normalizedBuilding;
                    existing.OwnerId = normalizedOwner;
                    existing.Required = normalizedRequired;
                    existing.Completed = Math.Min(
                        existing.Completed,
                        normalizedRequired);
                    if (existing.Completed >= existing.Required
                        && !existing.OperationalPublished
                        && !string.IsNullOrWhiteSpace(existing.BuildingId))
                    {
                        existing.OperationalPublished = true;
                        operational = ToOperational(position, existing);
                    }

                    return false;
                }
            }

            State created = CreateState(
                normalizedBuilding,
                normalizedOwner,
                normalizedRequired,
                normalizedPlacedTurn);
            _states[position] = created;

            if (created.Required == 0
                && !string.IsNullOrWhiteSpace(created.BuildingId))
            {
                created.OperationalPublished = true;
                operational = ToOperational(position, created);
            }

            return true;
        }

        public bool Remove(Vector2Int position)
            => _states.Remove(position);

        public bool TryTransferOwner(
            Vector2Int position,
            string previousOwnerId,
            string newOwnerId,
            out string reason)
        {
            reason = string.Empty;
            if (!_states.TryGetValue(position, out State state))
            {
                reason = "Construction lifecycle state not found.";
                return false;
            }

            string previous = NormalizeOwner(previousOwnerId);
            string next = NormalizeOwner(newOwnerId);
            if (!string.Equals(state.OwnerId, previous, StringComparison.Ordinal))
            {
                reason = $"Construction lifecycle belongs to '{state.OwnerId}', not '{previous}'.";
                return false;
            }

            state.OwnerId = next;
            return true;
        }

        public IReadOnlyList<OperationalTransition> AdvanceOwnerTurn(
            string ownerId,
            long globalTurn)
        {
            string normalizedOwner = NormalizeOwner(ownerId);
            var transitions = new List<OperationalTransition>();
            var positions = new List<Vector2Int>(_states.Keys);
            positions.Sort(ComparePosition);

            for (int index = 0; index < positions.Count; index++)
            {
                Vector2Int position = positions[index];
                State state = _states[position];
                if (state.Completed >= state.Required
                    || state.PlacedTurn >= globalTurn
                    || !string.Equals(
                        state.OwnerId,
                        normalizedOwner,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                state.Completed++;
                if (state.Completed >= state.Required
                    && !state.OperationalPublished
                    && !string.IsNullOrWhiteSpace(state.BuildingId))
                {
                    state.OperationalPublished = true;
                    transitions.Add(ToOperational(position, state));
                }
            }

            return transitions;
        }

        public IReadOnlyList<SavedState> CaptureSorted()
        {
            var positions = new List<Vector2Int>(_states.Keys);
            positions.Sort(ComparePosition);
            var result = new List<SavedState>(positions.Count);

            for (int index = 0; index < positions.Count; index++)
            {
                Vector2Int position = positions[index];
                result.Add(ToSavedState(position, _states[position]));
            }

            return result;
        }

        /// <summary>
        /// Replaces the authoritative state from save data. If placement restore
        /// already fired before this module's load block, its publication marker is
        /// carried across so BuildOperational is not duplicated in the same load.
        /// </summary>
        public IReadOnlyList<OperationalTransition> Restore(
            IReadOnlyList<SavedState> savedStates,
            Func<Vector2Int, string> missingBuildingResolver)
        {
            var previous = new Dictionary<Vector2Int, State>(_states);
            _states.Clear();
            var transitions = new List<OperationalTransition>();

            if (savedStates == null)
                return transitions;

            for (int index = 0; index < savedStates.Count; index++)
            {
                SavedState saved = savedStates[index];
                if (_states.ContainsKey(saved.Position))
                    continue;

                string buildingId = NormalizeBuilding(saved.BuildingId);
                if (string.IsNullOrWhiteSpace(buildingId)
                    && missingBuildingResolver != null)
                {
                    buildingId = NormalizeBuilding(
                        missingBuildingResolver(saved.Position));
                }

                var restored = new State
                {
                    BuildingId = buildingId,
                    OwnerId = NormalizeOwner(saved.OwnerId),
                    Required = Math.Max(0, saved.Required),
                    Completed = Math.Min(
                        Math.Max(0, saved.Completed),
                        Math.Max(0, saved.Required)),
                    PlacedTurn = Math.Max(0L, saved.PlacedTurn),
                };

                if (previous.TryGetValue(saved.Position, out State previousState)
                    && previousState.OperationalPublished
                    && (string.IsNullOrWhiteSpace(restored.BuildingId)
                        || string.IsNullOrWhiteSpace(previousState.BuildingId)
                        || string.Equals(
                            previousState.BuildingId,
                            restored.BuildingId,
                            StringComparison.Ordinal)))
                {
                    restored.OperationalPublished = true;
                }

                _states[saved.Position] = restored;

                if (restored.Completed >= restored.Required
                    && !restored.OperationalPublished
                    && !string.IsNullOrWhiteSpace(restored.BuildingId))
                {
                    restored.OperationalPublished = true;
                    transitions.Add(
                        ToOperational(saved.Position, restored));
                }
            }

            return transitions;
        }

        private static State CreateState(
            string buildingId,
            string ownerId,
            int required,
            long placedTurn)
            => new State
            {
                BuildingId = buildingId,
                OwnerId = ownerId,
                Required = required,
                Completed = 0,
                PlacedTurn = placedTurn,
                OperationalPublished = false,
            };

        private static SavedState ToSavedState(
            Vector2Int position,
            State state)
            => new SavedState(
                position,
                state.BuildingId,
                state.OwnerId,
                state.Required,
                state.Completed,
                state.PlacedTurn);

        private static OperationalTransition ToOperational(
            Vector2Int position,
            State state)
            => new OperationalTransition(
                position,
                state.BuildingId,
                state.OwnerId);

        private static int ComparePosition(
            Vector2Int left,
            Vector2Int right)
        {
            int byX = left.x.CompareTo(right.x);
            return byX != 0 ? byX : left.y.CompareTo(right.y);
        }

        private static string NormalizeBuilding(string buildingId)
            => string.IsNullOrWhiteSpace(buildingId)
                ? string.Empty
                : buildingId.Trim();

        private static string NormalizeOwner(string ownerId)
            => string.IsNullOrWhiteSpace(ownerId)
                ? "player_0"
                : ownerId.Trim();
    }
}
