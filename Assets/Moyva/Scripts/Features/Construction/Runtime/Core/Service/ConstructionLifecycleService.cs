using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionLifecycleService :
        IConstructionLifecycle,
        ITurnParticipant,
        ISaveModule,
        IInitializable,
        IDisposable
    {
        private const int SaveMagic = unchecked((int)0x434C4946);
        private const int SaveVersion = 2;
        private const int LegacySaveVersion = 1;
        private const int MaxSavedStates = 100000;

        private readonly SignalBus _signals;
        private readonly IBuildingRegistry _registry;
        private readonly ITurnService _turns;
        private readonly IConstructionSaveSnapshotSource _placementSnapshots;
        private readonly ConstructionLifecycleStateMachine _state = new();

        public ConstructionLifecycleService(
            SignalBus signals,
            IBuildingRegistry registry,
            ITurnService turns,
            [InjectOptional] IConstructionSaveSnapshotSource placementSnapshots = null)
        {
            _signals = signals;
            _registry = registry;
            _turns = turns;
            _placementSnapshots = placementSnapshots;
        }

        public int TurnOrder => 20;

        public void Initialize()
        {
            _signals.Subscribe<BuildingPlacedSignal>(OnPlaced);
            _signals.Subscribe<BuildingDemolishedSignal>(OnDemolished);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<BuildingPlacedSignal>(OnPlaced);
            _signals.TryUnsubscribe<BuildingDemolishedSignal>(OnDemolished);
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

        public void OnTurnStarted(TurnContext context)
        {
            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition>
                transitions = _state.AdvanceOwnerTurn(
                    context.Faction.OwnerId,
                    context.GlobalTurn);
            PublishOperational(transitions);
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        private void OnPlaced(BuildingPlacedSignal signal)
        {
            BuildingDefinition definition =
                _registry?.GetById(signal.BuildingId);
            int required = Math.Max(0, definition?.BuildTurns ?? 0);
            long globalTurn = Math.Max(0L, _turns?.GlobalTurn ?? 0L);
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

        private void OnDemolished(BuildingDemolishedSignal signal)
        {
            _state.Remove(signal.Position);
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
            }
        }

        public void OnLoad(ISaveContext context)
        {
            int magic = context.Reader.ReadInt32();
            int version = context.Reader.ReadInt32();
            if (magic != SaveMagic
                || (version != LegacySaveVersion && version != SaveVersion))
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
                string buildingId = version >= SaveVersion
                    ? context.Reader.ReadString()
                    : string.Empty;
                string ownerId = context.Reader.ReadString();
                int required = context.Reader.ReadInt32();
                int completed = context.Reader.ReadInt32();
                long placedTurn = context.Reader.ReadInt64();

                restored.Add(
                    new ConstructionLifecycleStateMachine.SavedState(
                        position,
                        buildingId,
                        ownerId,
                        required,
                        completed,
                        placedTurn));
            }

            IReadOnlyList<ConstructionLifecycleStateMachine.OperationalTransition>
                transitions = _state.Restore(
                    restored,
                    ResolvePlacedBuildingId);
            PublishOperational(transitions);
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
                Debug.LogWarning(
                    $"[ConstructionLifecycle] Operational transition for unknown building " +
                    $"'{transition.BuildingId}' at {transition.Position} was deferred.");
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
