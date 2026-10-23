using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionLifecycleService :
        IConstructionLifecycle,
        IConstructionOperationalRestore,
        IConstructionRealtimeProgress,
        IConstructionModuleStatePersistence,
        IConstructionObserverStateSource,
        ITickable,
        ITurnParticipant,
        ISaveModule,
        IInitializable,
        IDisposable
    {
        private const int SaveMagic = unchecked((int)0x434C4946);
        private const int SaveVersion = 3;
        private const int LegacySaveVersion = 1;
        private const int MaxSavedStates = 100000;
        private const string LifecycleStateKey = "construction-lifecycle.v1";

        private readonly SignalBus _signals;
        private readonly IBuildingRegistry _registry;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _progressClock;
        private readonly IConstructionSaveSnapshotSource _placementSnapshots;
        private readonly ConstructionLifecycleStateMachine _state = new();
        private readonly IEconomyInfoMediator _economy;
        private double _lastGameplaySeconds;

        public ConstructionLifecycleService(
            SignalBus signals,
            IBuildingRegistry registry,
            ITurnService turns,
            [InjectOptional] IGameplayProgressClock progressClock = null,
            [InjectOptional] IConstructionSaveSnapshotSource placementSnapshots = null,
            [InjectOptional] IEconomyInfoMediator economy = null)
        {
            _signals = signals;
            _registry = registry;
            _turns = turns;
            _progressClock = progressClock;
            _placementSnapshots = placementSnapshots;
            _economy = economy;
        }

        public int TurnOrder => 20;

        public void Initialize()
        {
            _signals.Subscribe<BuildingPlacedSignal>(OnPlaced);
            _signals.Subscribe<BuildingDemolishedSignal>(OnDemolished);
            _signals.Subscribe<BuildingOwnershipTransferredSignal>(OnBuildingOwnershipTransferred);
            _lastGameplaySeconds = _progressClock?.ElapsedGameplaySeconds ?? 0d;
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<BuildingPlacedSignal>(OnPlaced);
            _signals.TryUnsubscribe<BuildingDemolishedSignal>(OnDemolished);
            _signals.TryUnsubscribe<BuildingOwnershipTransferredSignal>(OnBuildingOwnershipTransferred);

        }

        public bool IsOperational(Vector2Int position)
            => _state.IsOperational(position);

        public bool TryGetProgress(
            Vector2Int position,
            out int completedTurns,
            out int requiredTurns)
            => _state.TryGetProgress(
                position,
                out completedTurns,
                out requiredTurns);

        public bool TryRestoreOperational(Vector2Int position)
        {
            if (!_state.TryRestoreOperational(position, out var transition))
                return false;
            PublishOperational(transition);
            return true;
        }

        public void OnTurnStarted(TurnContext context)
        {
            if (_progressClock?.IsRealtime == true) return;
            AdvanceOwnerProgress(context.Faction.OwnerId, context.GlobalTurn);
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        private void OnPlaced(BuildingPlacedSignal signal)
        {
            Tick();
            BuildingDefinition definition =
                _registry?.GetById(signal.BuildingId);
            int required = Math.Max(0, definition?.BuildTurns ?? 0);
            long globalTurn = ResolvePlacementSequence();
            Vector2Int? relocationSource =
                signal.HasRelocationSource
                && signal.RelocationSourcePosition != signal.Position
                    ? signal.RelocationSourcePosition
                    : null;

            _state.RegisterPlacement(
                signal.Position,
                signal.BuildingId,
                signal.OwnerId,
                required,
                globalTurn,
                relocationSource,
                out ConstructionLifecycleStateMachine.OperationalTransition
                    operational);

            PublishOperational(operational);
        }

        public void Tick()
        {
            double now = _progressClock?.ElapsedGameplaySeconds ?? 0d;
            float delta = (float)Math.Max(0d, now - _lastGameplaySeconds);
            _lastGameplaySeconds = now;
            if (_progressClock?.IsRealtime != true || delta <= 0f) return;
            PublishOperational(_state.AdvanceRealtime(delta / _progressClock.SandboxRoundSeconds, ResolveConstructionSpeed));
        }

        private float ResolveConstructionSpeed(string ownerId, Vector2Int position)
        {
            if (_economy == null) return 1f;
            var population = _economy.GetRecruitmentPopulation(ownerId, position);
            // The initial settlement must be constructible before it has residents.
            return population.Total == 0 ? 0.25f : Mathf.Clamp(population.ConstructionSpeed, 0.05f, 1f);
        }

        public bool TryGetRealtimeProgress(Vector2Int position, out float progress, out float remainingSeconds)
        {
            progress = 0f;
            remainingSeconds = 0f;
            if (_progressClock?.IsRealtime != true || !_state.TryGetSavedState(position, out var state))
                return false;
            float completed = state.Completed + state.PartialWork;
            progress = state.Required == 0 ? 1f : Mathf.Clamp01(completed / state.Required);
            remainingSeconds = Math.Max(0f, state.Required - completed)
                * _progressClock.SandboxRoundSeconds / ResolveConstructionSpeed(state.OwnerId, position);
            return true;
        }

        private void AdvanceOwnerProgress(string ownerId, long sequence)
        {
            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition>
                transitions = _state.AdvanceOwnerTurn(
                    ownerId,
                    sequence);
            PublishOperational(transitions);
        }

        private long ResolvePlacementSequence()
        {
            if (_progressClock != null && _progressClock.IsRealtime)
                return Math.Max(1L, _progressClock.CurrentSequence);

            return Math.Max(0L, _turns?.GlobalTurn ?? 0L);
        }

        private void OnDemolished(BuildingDemolishedSignal signal)
        {
            _state.Remove(signal.Position);
        }

        private void OnBuildingOwnershipTransferred(
            BuildingOwnershipTransferredSignal signal)
        {
            if (!_state.TryTransferOwner(
                    signal.Position,
                    signal.PreviousOwnerId,
                    signal.NewOwnerId,
                    out string reason)
                && Debug.isDebugBuild)
            {
                Debug.LogWarning(
                    $"[ConstructionLifecycle] Could not transfer owner for '{signal.BuildingId}' at {signal.Position}: {reason}");
            }
        }

        public void OnSave(ISaveContext context)
        {
            IReadOnlyList<ConstructionLifecycleStateMachine.SavedState> snapshot =
                _state.CaptureSorted();

            context.Writer.Write(SaveMagic);
            context.Writer.Write(SaveVersion);
            context.Writer.Write(snapshot.Count);

            for (int index = 0; index < snapshot.Count; index++)
            {
                ConstructionLifecycleStateMachine.SavedState item =
                    snapshot[index];
                context.Writer.Write(item.Position.x);
                context.Writer.Write(item.Position.y);
                context.Writer.Write(item.BuildingId ?? string.Empty);
                context.Writer.Write(item.OwnerId ?? string.Empty);
                context.Writer.Write(item.Required);
                context.Writer.Write(item.Completed);
                context.Writer.Write(item.PlacedTurn);
                context.Writer.Write(item.PartialWork);
            }
        }

        public void OnLoad(ISaveContext context)
        {
            int magic = context.Reader.ReadInt32();
            int version = context.Reader.ReadInt32();
            if (magic != SaveMagic
                || (version != LegacySaveVersion && version != 2 && version != SaveVersion))
            {
                throw new InvalidDataException(
                    $"Unsupported construction lifecycle save block: magic={magic}, version={version}.");
            }

            int count = context.Reader.ReadInt32();
            if (count < 0 || count > MaxSavedStates)
            {
                throw new InvalidDataException(
                    $"Construction lifecycle state count {count} is outside 0..{MaxSavedStates}.");
            }

            var restored =
                new List<ConstructionLifecycleStateMachine.SavedState>(count);

            for (int index = 0; index < count; index++)
            {
                var position = new Vector2Int(
                    context.Reader.ReadInt32(),
                    context.Reader.ReadInt32());
                string buildingId = version >= 2
                    ? context.Reader.ReadString()
                    : string.Empty;
                string ownerId = context.Reader.ReadString();
                int required = context.Reader.ReadInt32();
                int completed = context.Reader.ReadInt32();
                long placedTurn = context.Reader.ReadInt64();
                float partialWork = version >= 3 ? context.Reader.ReadSingle() : 0f;
                if (float.IsNaN(partialWork) || float.IsInfinity(partialWork) || partialWork < 0f || partialWork >= 1f)
                    throw new InvalidDataException("Invalid construction progress fraction.");

                restored.Add(
                    new ConstructionLifecycleStateMachine.SavedState(
                        position,
                        buildingId,
                        ownerId,
                        required,
                        completed,
                        placedTurn, partialWork));
            }

            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition>
                transitions = _state.Restore(
                    restored,
                    ResolvePlacedBuildingId);
            _lastGameplaySeconds = _progressClock?.ElapsedGameplaySeconds ?? 0d;
            PublishOperational(transitions);
        }

        // ── World-state replication module ───────────────────────────────────
        // The save block above covers persistence; this keyed module payload
        // carries the same per-building progress to observer peers through the
        // world-state snapshot, so a client joining mid-build keeps the host's
        // completed/partial work instead of restarting construction at zero.

        public string StateKey => LifecycleStateKey;

        public byte[] CaptureState()
            => SerializeLifecycleStates(_state.CaptureSorted());

        public byte[] CaptureObserverState(
            string ownerId,
            ISet<Vector2Int> visibleBuildings)
        {
            if (visibleBuildings == null)
                throw new ArgumentNullException(nameof(visibleBuildings));

            IReadOnlyList<ConstructionLifecycleStateMachine.SavedState> all =
                _state.CaptureSorted();
            var visible =
                new List<ConstructionLifecycleStateMachine.SavedState>(all.Count);
            for (int index = 0; index < all.Count; index++)
            {
                if (visibleBuildings.Contains(all[index].Position))
                    visible.Add(all[index]);
            }

            return SerializeLifecycleStates(visible);
        }

        public void RestoreState(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return;

            List<ConstructionLifecycleStateMachine.SavedState> restored =
                DeserializeLifecycleStates(payload);
            if (restored == null)
                return;

            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition>
                transitions = _state.Restore(restored, ResolvePlacedBuildingId);
            _lastGameplaySeconds = _progressClock?.ElapsedGameplaySeconds ?? 0d;
            PublishOperational(transitions);
        }

        private static byte[] SerializeLifecycleStates(
            IReadOnlyList<ConstructionLifecycleStateMachine.SavedState> states)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(1);
            writer.Write(states?.Count ?? 0);
            if (states != null)
            {
                for (int index = 0; index < states.Count; index++)
                {
                    var item = states[index];
                    writer.Write(item.Position.x);
                    writer.Write(item.Position.y);
                    writer.Write(item.BuildingId ?? string.Empty);
                    writer.Write(item.OwnerId ?? string.Empty);
                    writer.Write(item.Required);
                    writer.Write(item.Completed);
                    writer.Write(item.PlacedTurn);
                    writer.Write(item.PartialWork);
                }
            }

            return stream.ToArray();
        }

        private static List<ConstructionLifecycleStateMachine.SavedState>
            DeserializeLifecycleStates(byte[] payload)
        {
            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            int version = reader.ReadInt32();
            if (version != 1)
                return null;

            int count = reader.ReadInt32();
            if (count < 0 || count > MaxSavedStates)
                return null;

            var restored =
                new List<ConstructionLifecycleStateMachine.SavedState>(count);
            for (int index = 0; index < count; index++)
            {
                var position = new Vector2Int(
                    reader.ReadInt32(),
                    reader.ReadInt32());
                string buildingId = reader.ReadString();
                string ownerId = reader.ReadString();
                int required = reader.ReadInt32();
                int completed = reader.ReadInt32();
                long placedTurn = reader.ReadInt64();
                float partialWork = reader.ReadSingle();
                if (float.IsNaN(partialWork) || float.IsInfinity(partialWork)
                    || partialWork < 0f || partialWork >= 1f)
                {
                    return null;
                }

                restored.Add(
                    new ConstructionLifecycleStateMachine.SavedState(
                        position,
                        buildingId,
                        ownerId,
                        required,
                        completed,
                        placedTurn,
                        partialWork));
            }

            return restored;
        }

        private string ResolvePlacedBuildingId(Vector2Int position)
        {
            IReadOnlyList<ConstructionSavedPlacement> placements =
                _placementSnapshots?.GetSavedPlacements();
            if (placements == null)
                return string.Empty;

            for (int index = 0; index < placements.Count; index++)
            {
                ConstructionSavedPlacement placement = placements[index];
                if (placement.Position == position)
                    return placement.BuildingId ?? string.Empty;
            }

            return string.Empty;
        }

        private void PublishOperational(
            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition>
                transitions)
        {
            if (transitions == null)
                return;

            for (int index = 0; index < transitions.Count; index++)
                PublishOperational(transitions[index]);
        }

        private void PublishOperational(
            ConstructionLifecycleStateMachine.OperationalTransition transition)
        {
            if (!transition.IsValid)
                return;

            BuildingDefinition definition =
                _registry?.GetById(transition.BuildingId);
            if (definition == null)
            {
                return;
            }

            _signals.Fire(
                new BuildingOperationalSignal
                {
                    BuildingId = definition.Id,
                    Position = transition.Position,
                    OwnerId = transition.OwnerId,
                });
        }
    }
}
