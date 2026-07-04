using System;
using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Runtime state-store для replay startup world signals усередині gameplay-сцени.
    /// Не замінює SignalBus, а лише робить останній world/spawn snapshot доступним для late subscribers.
    /// </summary>
    public sealed class WorldGenerationSignalState : IWorldGenerationSignalState
    {
        private const string LogTag = "[WorldGenerationSignalState]";

        private bool _hasWorldGeneratedData;
        private WorldGeneratedDataSignal _worldGeneratedData;
        private bool _hasWorldSpawnPositions;
        private WorldSpawnPositionsSignal _worldSpawnPositions;
        private long _currentWorldSequence;
        private string _currentSessionId = string.Empty;
        private long _nextWorldSequence = 1;
        private int _nextWorldRevision = 1;
        private int _nextSpawnRevision = 1;

        public long BeginWorldSnapshotCycle(string sessionId)
        {
            _currentWorldSequence = _nextWorldSequence++;
            _currentSessionId = NormalizeSessionId(sessionId, _currentWorldSequence);
            _hasWorldGeneratedData = false;
            _worldGeneratedData = default;
            _hasWorldSpawnPositions = false;
            _worldSpawnPositions = default;
            Debug.Log($"{LogTag} BeginWorldSnapshotCycle sequence={_currentWorldSequence}, session='{_currentSessionId}'.");
            return _currentWorldSequence;
        }

        public void Clear()
        {
            _hasWorldGeneratedData = false;
            _worldGeneratedData = default;
            _hasWorldSpawnPositions = false;
            _worldSpawnPositions = default;
            _currentWorldSequence = 0;
            _currentSessionId = string.Empty;
            Debug.Log($"{LogTag} Clear snapshots.");
        }

        public bool TryGetCurrentWorldIdentity(out long startupSequence, out string sessionId)
        {
            startupSequence = _currentWorldSequence;
            sessionId = _currentSessionId;
            return startupSequence > 0;
        }

        public WorldGeneratedDataSignal StoreWorldGeneratedData(WorldGeneratedDataSignal signal)
        {
            signal = NormalizeWorldGeneratedSignal(signal);
            _worldGeneratedData = CloneWorldGeneratedDataSignal(signal);
            _hasWorldGeneratedData = true;
            Debug.Log($"{LogTag} Store WorldGeneratedData sequence={signal.StartupSequence}, revision={signal.SnapshotRevision}, source={signal.Source}, frame={signal.PublishedFrame}, session='{signal.StartupSessionId}', map={signal.Width}x{signal.Height}.");
            return CloneWorldGeneratedDataSignal(signal);
        }

        public bool TryGetWorldGeneratedData(out WorldGeneratedDataSignal signal)
        {
            if (!_hasWorldGeneratedData)
            {
                signal = default;
                return false;
            }

            signal = CloneWorldGeneratedDataSignal(_worldGeneratedData);
            return true;
        }

        public bool TryStoreWorldSpawnPositions(WorldSpawnPositionsSignal signal, out WorldSpawnPositionsSignal storedSignal)
        {
            signal = NormalizeWorldSpawnPositionsSignal(signal);
            if (!CanStoreSpawnSignal(signal, out string rejectReason))
            {
                Debug.LogWarning($"{LogTag} Reject WorldSpawnPositions sequence={signal.StartupSequence}, source={signal.Source}, session='{signal.StartupSessionId}', reason={rejectReason}.");
                storedSignal = default;
                return false;
            }

            _worldSpawnPositions = CloneWorldSpawnPositionsSignal(signal);
            _hasWorldSpawnPositions = true;
            storedSignal = CloneWorldSpawnPositionsSignal(signal);
            Debug.Log($"{LogTag} Store WorldSpawnPositions sequence={signal.StartupSequence}, revision={signal.SnapshotRevision}, source={signal.Source}, frame={signal.PublishedFrame}, session='{signal.StartupSessionId}', assignments={signal.Assignments?.Length ?? 0}.");
            return true;
        }

        public bool TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal signal)
        {
            if (!_hasWorldSpawnPositions)
            {
                signal = default;
                return false;
            }

            signal = CloneWorldSpawnPositionsSignal(_worldSpawnPositions);
            return true;
        }

        private static WorldGeneratedDataSignal CloneWorldGeneratedDataSignal(WorldGeneratedDataSignal signal)
        {
            return new WorldGeneratedDataSignal
            {
                StartupSequence = signal.StartupSequence,
                SnapshotRevision = signal.SnapshotRevision,
                StartupSessionId = signal.StartupSessionId,
                Source = signal.Source,
                PublishedFrame = signal.PublishedFrame,
                PublishedAtUtcTicks = signal.PublishedAtUtcTicks,
                Width = signal.Width,
                Height = signal.Height,
                GridTopology = signal.GridTopology,
                ProjectionMode = signal.ProjectionMode,
                RenderMode = signal.RenderMode,
                NeighborhoodMode = signal.NeighborhoodMode,
                CellSize = signal.CellSize,
                HasMapWorldBounds = signal.HasMapWorldBounds,
                MapWorldBoundsCenter = signal.MapWorldBoundsCenter,
                MapWorldBoundsSize = signal.MapWorldBoundsSize,
                TileMap = Clone2DArray(signal.TileMap),
                ObjectMap = Clone2DArray(signal.ObjectMap),
                HeightMap = Clone2DArray(signal.HeightMap),
                TerrainLevelMap = Clone2DArray(signal.TerrainLevelMap),
            };
        }

        private static WorldSpawnPositionsSignal CloneWorldSpawnPositionsSignal(WorldSpawnPositionsSignal signal)
        {
            return new WorldSpawnPositionsSignal
            {
                StartupSequence = signal.StartupSequence,
                SnapshotRevision = signal.SnapshotRevision,
                StartupSessionId = signal.StartupSessionId,
                Source = signal.Source,
                PublishedFrame = signal.PublishedFrame,
                PublishedAtUtcTicks = signal.PublishedAtUtcTicks,
                Assignments = signal.Assignments != null
                    ? (SpawnPositionAssignment[])signal.Assignments.Clone()
                    : null,
            };
        }

        private WorldGeneratedDataSignal NormalizeWorldGeneratedSignal(WorldGeneratedDataSignal signal)
        {
            EnsureWorldIdentity(ref signal.StartupSequence, ref signal.StartupSessionId);
            signal.Source = signal.Source == WorldGeneratedDataSource.Unknown
                ? WorldGeneratedDataSource.GeneratedHost
                : signal.Source;
            signal.PublishedFrame = signal.PublishedFrame > 0 ? signal.PublishedFrame : Time.frameCount;
            signal.PublishedAtUtcTicks = signal.PublishedAtUtcTicks > 0 ? signal.PublishedAtUtcTicks : DateTime.UtcNow.Ticks;
            signal.SnapshotRevision = _nextWorldRevision++;
            return signal;
        }

        private WorldSpawnPositionsSignal NormalizeWorldSpawnPositionsSignal(WorldSpawnPositionsSignal signal)
        {
            EnsureWorldIdentity(ref signal.StartupSequence, ref signal.StartupSessionId);
            signal.Source = signal.Source == WorldSpawnPositionsSource.Unknown
                ? WorldSpawnPositionsSource.GeneratedHost
                : signal.Source;
            signal.PublishedFrame = signal.PublishedFrame > 0 ? signal.PublishedFrame : Time.frameCount;
            signal.PublishedAtUtcTicks = signal.PublishedAtUtcTicks > 0 ? signal.PublishedAtUtcTicks : DateTime.UtcNow.Ticks;
            signal.SnapshotRevision = _nextSpawnRevision++;
            return signal;
        }

        private void EnsureWorldIdentity(ref long startupSequence, ref string sessionId)
        {
            if (startupSequence <= 0)
            {
                if (_currentWorldSequence <= 0)
                    BeginWorldSnapshotCycle(sessionId);

                startupSequence = _currentWorldSequence;
            }

            if (string.IsNullOrWhiteSpace(sessionId))
                sessionId = NormalizeSessionId(_currentSessionId, startupSequence);

            if (startupSequence != _currentWorldSequence || !string.Equals(sessionId, _currentSessionId, StringComparison.Ordinal))
            {
                if (_currentWorldSequence > 0)
                {
                    _hasWorldGeneratedData = false;
                    _worldGeneratedData = default;
                    _hasWorldSpawnPositions = false;
                    _worldSpawnPositions = default;
                }

                _currentWorldSequence = startupSequence;
                _currentSessionId = NormalizeSessionId(sessionId, startupSequence);
            }
        }

        private bool CanStoreSpawnSignal(WorldSpawnPositionsSignal incoming, out string rejectReason)
        {
            rejectReason = null;

            if (incoming.Assignments == null || incoming.Assignments.Length == 0)
            {
                rejectReason = "empty-assignments";
                return false;
            }

            if (_currentWorldSequence > 0 && incoming.StartupSequence < _currentWorldSequence)
            {
                rejectReason = $"stale-sequence-current={_currentWorldSequence}";
                return false;
            }

            if (_hasWorldSpawnPositions)
            {
                if (incoming.StartupSequence < _worldSpawnPositions.StartupSequence)
                {
                    rejectReason = "stale-sequence";
                    return false;
                }

                if (incoming.StartupSequence == _worldSpawnPositions.StartupSequence
                    && string.Equals(incoming.StartupSessionId, _worldSpawnPositions.StartupSessionId, StringComparison.Ordinal))
                {
                    int incomingPriority = ResolveSpawnSourcePriority(incoming.Source);
                    int existingPriority = ResolveSpawnSourcePriority(_worldSpawnPositions.Source);
                    if (incomingPriority < existingPriority)
                    {
                        rejectReason = $"lower-priority-source existing={_worldSpawnPositions.Source}";
                        return false;
                    }

                    if (incomingPriority == existingPriority && HaveSameAssignments(incoming.Assignments, _worldSpawnPositions.Assignments))
                    {
                        rejectReason = "duplicate-payload";
                        return false;
                    }
                }
            }

            return true;
        }

        private static int ResolveSpawnSourcePriority(WorldSpawnPositionsSource source)
        {
            return source switch
            {
                WorldSpawnPositionsSource.SavedGame => 400,
                WorldSpawnPositionsSource.MultiplayerSync => 300,
                WorldSpawnPositionsSource.GeneratedHost => 200,
                WorldSpawnPositionsSource.DirectGameplayTest => 100,
                _ => 0,
            };
        }

        private static bool HaveSameAssignments(SpawnPositionAssignment[] left, SpawnPositionAssignment[] right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null || left.Length != right.Length)
                return false;

            for (int index = 0; index < left.Length; index++)
            {
                if (left[index].SlotIndex != right[index].SlotIndex
                    || !string.Equals(left[index].ParticipantId, right[index].ParticipantId, StringComparison.Ordinal)
                    || left[index].IsBot != right[index].IsBot
                    || left[index].Position != right[index].Position)
                {
                    return false;
                }
            }

            return true;
        }

        private static string NormalizeSessionId(string sessionId, long startupSequence)
        {
            return string.IsNullOrWhiteSpace(sessionId)
                ? $"world-session-{startupSequence}"
                : sessionId.Trim();
        }

        private static T[,] Clone2DArray<T>(T[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var copy = new T[width, height];
            Array.Copy(source, copy, source.Length);
            return copy;
        }
    }
}
