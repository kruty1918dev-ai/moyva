using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Zenject;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class CaravanGameplayAccess : ICaravanGameplayAccess, IInitializable, IDisposable
    {
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitClassConfig _configs;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _clock;
        private readonly IGameStateService _state;
        private readonly IConstructionPortfolioQuery _portfolio;
        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionLifecycle _lifecycle;
        private readonly IPathfinder _pathfinder;
        private readonly IUnitMovementService _movement;
        private readonly IUnitTraversalPolicy _traversal;
        private long _lastTurn;
        private TurnPhase _lastPhase;
        public event Action ProgressAvailable;

        public CaravanGameplayAccess(IUnitService units, IUnitOwnershipQuery ownership,
            IUnitClassConfig configs, ILocalGameplayRoleResolver roles, ITurnService turns,
            IGameplayProgressClock clock, IConstructionPortfolioQuery portfolio,
            IBuildingRegistry buildings, IPathfinder pathfinder, IUnitMovementService movement,
            IUnitTraversalPolicy traversal, [InjectOptional] IConstructionLifecycle lifecycle = null,
            [InjectOptional] IGameStateService state = null)
        {
            _units = units; _ownership = ownership; _configs = configs;
            _roles = roles; _turns = turns; _clock = clock; _state = state;
            _portfolio = portfolio; _buildings = buildings; _lifecycle = lifecycle;
            _pathfinder = pathfinder; _movement = movement; _traversal = traversal;
        }

        public static void Install(DiContainer container)
            => container.BindInterfacesAndSelfTo<CaravanGameplayAccess>().AsSingle().NonLazy();

        public void Initialize()
        {
            _lastTurn = _turns.GlobalTurn;
            _lastPhase = _turns.Phase;
            _turns.StateChanged += OnTurnChanged;
            _clock.Progressed += OnProgressed;
        }

        public void Dispose()
        {
            _turns.StateChanged -= OnTurnChanged;
            _clock.Progressed -= OnProgressed;
        }

        private void OnTurnChanged()
        {
            bool opened = _turns.Phase == TurnPhase.AwaitingInput
                && (_lastTurn != _turns.GlobalTurn || _lastPhase != _turns.Phase);
            _lastTurn = _turns.GlobalTurn; _lastPhase = _turns.Phase;
            if (opened) ProgressAvailable?.Invoke();
        }

        private void OnProgressed(GameplayProgressTick tick)
        {
            if (tick.IsRealtime) ProgressAvailable?.Invoke();
        }

        public bool IsAuthoritative => _roles.Resolve().IsAuthoritative;

        public bool TryGetWagon(string unitId, out CaravanUnitSnapshot unit)
        {
            unit = default;
            if (string.IsNullOrWhiteSpace(unitId) || !_units.TryGetUnitPosition(unitId, out var position))
                return false;
            var config = _configs.GetConfig(_units.GetUnitTypeId(unitId));
            if (config == null || float.IsNaN(config.CargoCapacity)
                || float.IsInfinity(config.CargoCapacity) || config.CargoCapacity <= 0) return false;
            unit = new CaravanUnitSnapshot(_ownership.GetUnitOwnerId(unitId), position, config.CargoCapacity);
            return true;
        }

        public bool CanCommand(string ownerId, string unitId, out string reason)
        {
            reason = string.Empty;
            if (!IsAuthoritative) reason = "Cargo commands must be confirmed by the host.";
            else if (!TryGetWagon(unitId, out var unit) || string.IsNullOrWhiteSpace(ownerId)
                || !string.Equals(ownerId, unit.OwnerId, StringComparison.Ordinal))
                reason = "Select a wagon belonging to your kingdom.";
            else if (_state != null && _state.CurrentState != GameStateType.Playing)
                reason = "Cargo operations are unavailable while the game is paused or finished.";
            else if (_clock.IsRealtime)
            {
                string localOwner = string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                    ? _roles.Resolve().PlayerId : _turns.LocalOwnerId;
                if (!string.Equals(ownerId, localOwner, StringComparison.Ordinal))
                    reason = "This wagon belongs to another kingdom.";
            }
            else if (!_turns.CanOwnerAct(ownerId, out reason)) return false;
            return string.IsNullOrEmpty(reason);
        }

        public bool CanAccessWarehouse(string unitId, Vector2Int origin, out string reason)
        {
            reason = "Move the wagon beside this warehouse first.";
            if (!TryGetWagon(unitId, out var unit)) return false;
            if (_lifecycle != null && !_lifecycle.IsOperational(origin))
            {
                reason = "This warehouse is not operational yet.";
                return false;
            }
            foreach (var placement in _portfolio.GetOwnerPlacements(unit.OwnerId))
            {
                if (placement.Position != origin) continue;
                var definition = _buildings.GetById(placement.BuildingId);
                for (int i = 0; i < BuildingFootprintUtility.GetOccupiedCellCount(definition); i++)
                {
                    var cell = BuildingFootprintUtility.GetOccupiedCell(definition, origin, i, placement.Rotation);
                    if (Math.Abs((long)unit.Position.x - cell.x) <= 1
                        && Math.Abs((long)unit.Position.y - cell.y) <= 1)
                    { reason = string.Empty; return true; }
                }
            }
            return false;
        }
    }
}
