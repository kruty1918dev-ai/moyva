using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionLifecycleService : IConstructionLifecycle, ITurnParticipant, ISaveModule, IInitializable, IDisposable
    {
        private const int SaveMagic = unchecked((int)0x434C4946);
        private const int SaveVersion = 1;
        private sealed class State
        {
            public string OwnerId;
            public int Required;
            public int Completed;
            public long PlacedTurn;
        }

        private readonly SignalBus _signals;
        private readonly IBuildingRegistry _registry;
        private readonly ITurnService _turns;
        private readonly Dictionary<Vector2Int, State> _states = new();

        public ConstructionLifecycleService(SignalBus signals, IBuildingRegistry registry, ITurnService turns)
        {
            _signals = signals;
            _registry = registry;
            _turns = turns;
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
            => !_states.TryGetValue(position, out State state) || state.Completed >= state.Required;

        public bool TryGetProgress(Vector2Int position, out int completedTurns, out int requiredTurns)
        {
            if (_states.TryGetValue(position, out State state))
            {
                completedTurns = state.Completed;
                requiredTurns = state.Required;
                return true;
            }

            completedTurns = requiredTurns = 0;
            return false;
        }

        public void OnTurnStarted(TurnContext context)
        {
            foreach (State state in _states.Values)
            {
                if (state.Completed >= state.Required
                    || state.PlacedTurn >= context.GlobalTurn
                    || !string.Equals(state.OwnerId, context.Faction.OwnerId, StringComparison.Ordinal))
                    continue;
                state.Completed++;
                if (state.Completed >= state.Required)
                    FireOperational(state);
            }
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        private void OnPlaced(BuildingPlacedSignal signal)
        {
            BuildingDefinition definition = _registry.GetById(signal.BuildingId);
            int required = Mathf.Max(0, definition?.BuildTurns ?? 0);
            _states[signal.Position] = new State
            {
                OwnerId = string.IsNullOrWhiteSpace(signal.OwnerId) ? "player_0" : signal.OwnerId.Trim(),
                Required = required,
                Completed = 0,
                PlacedTurn = _turns.GlobalTurn,
            };
        }

        private void OnDemolished(BuildingDemolishedSignal signal) => _states.Remove(signal.Position);

        public void OnSave(ISaveContext context)
        {
            context.Writer.Write(SaveMagic);
            context.Writer.Write(SaveVersion);
            context.Writer.Write(_states.Count);
            foreach (KeyValuePair<Vector2Int, State> pair in _states)
            {
                context.Writer.Write(pair.Key.x);
                context.Writer.Write(pair.Key.y);
                context.Writer.Write(pair.Value.OwnerId ?? string.Empty);
                context.Writer.Write(pair.Value.Required);
                context.Writer.Write(pair.Value.Completed);
                context.Writer.Write(pair.Value.PlacedTurn);
            }
        }

        public void OnLoad(ISaveContext context)
        {
            if (context.Reader.ReadInt32() != SaveMagic || context.Reader.ReadInt32() != SaveVersion)
                throw new System.IO.InvalidDataException("Unsupported construction lifecycle save block.");
            _states.Clear();
            int count = context.Reader.ReadInt32();
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = new(context.Reader.ReadInt32(), context.Reader.ReadInt32());
                var state = new State
                {
                    OwnerId = context.Reader.ReadString(),
                    Required = context.Reader.ReadInt32(),
                    Completed = context.Reader.ReadInt32(),
                    PlacedTurn = context.Reader.ReadInt64(),
                };
                _states[position] = state;
                if (state.Completed >= state.Required)
                    FireOperational(state, position);
            }
        }

        private void FireOperational(State state, Vector2Int? knownPosition = null)
        {
            Vector2Int position = knownPosition ?? FindPosition(state);
            if (_registry.GetById(ResolveBuildingId(position)) is BuildingDefinition definition)
                _signals.Fire(new BuildingOperationalSignal { BuildingId = definition.Id, Position = position, OwnerId = state.OwnerId });
        }

        private Vector2Int FindPosition(State wanted)
        {
            foreach (KeyValuePair<Vector2Int, State> pair in _states)
                if (ReferenceEquals(pair.Value, wanted))
                    return pair.Key;
            return default;
        }

        private string ResolveBuildingId(Vector2Int position)
        {
            if (_registry == null)
                return string.Empty;
            // Registry definitions use IDs, while placement ownership lives in ConstructionService.
            // BuildingOperationalSignal is repaired below by looking up the saved placement.
            return FindPlacedBuildingId(position);
        }

        [InjectOptional] private IConstructionService _construction;

        private string FindPlacedBuildingId(Vector2Int position)
        {
            if (_construction is IConstructionSaveSnapshotSource source)
                foreach (ConstructionSavedPlacement placement in source.GetSavedPlacements())
                    if (placement.Position == position)
                        return placement.BuildingId;
            return string.Empty;
        }
    }
}
