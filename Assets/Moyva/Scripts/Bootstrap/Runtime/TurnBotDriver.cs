using System;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Recruitment;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class TurnBotDriver : ITickable
    {
        private readonly ITurnService _turns;
        private readonly IConstructionService _construction;
        private readonly IRecruitmentService _recruitment;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitMovementService _movement;
        private readonly IGridService _grid;
        private long _actedTurn;

        public TurnBotDriver(
            ITurnService turns,
            IConstructionService construction,
            IRecruitmentService recruitment,
            IUnitService units,
            IUnitOwnershipQuery ownership,
            IUnitMovementService movement,
            IGridService grid)
        {
            _turns = turns;
            _construction = construction;
            _recruitment = recruitment;
            _units = units;
            _ownership = ownership;
            _movement = movement;
            _grid = grid;
        }

        public void Tick()
        {
            if (_turns.Phase != TurnPhase.AwaitingInput || !_turns.IsActiveFactionBot)
                return;

            string ownerId = _turns.ActiveOwnerId;
            if (_actedTurn != _turns.GlobalTurn)
            {
                _actedTurn = _turns.GlobalTurn;
                ExecuteTurn(ownerId);
                return;
            }

            _turns.TryEndTurn(ownerId, out _);
        }

        private void ExecuteTurn(string ownerId)
        {
            Vector2Int start = ResolveStart(ownerId);
            if (!TryFindOwnedBuilding(ownerId, "barrack", out Vector2Int barrack))
            {
                if (TryPlaceBarrack(ownerId, start, out barrack))
                    _turns.TryRecordAction(ownerId, "bot-building-place");
            }

            if (TryFindOwnedBuilding(ownerId, "barrack", out barrack))
                _recruitment.TryEnqueue(barrack, "warrior", ownerId, out _);

            foreach (string unitId in _units.GetAllUnitIds())
            {
                if (!string.Equals(_ownership.GetUnitOwnerId(unitId), ownerId, StringComparison.Ordinal)
                    || !_units.TryGetUnitPosition(unitId, out Vector2Int position))
                    continue;
                Vector2Int target = FindMoveTarget(position);
                if (target != position)
                    _ = _movement.MoveUnitAsync(unitId, target, CancellationToken.None);
            }
        }

        private Vector2Int ResolveStart(string ownerId)
        {
            foreach (TurnFaction faction in _turns.Factions)
                if (string.Equals(faction.OwnerId, ownerId, StringComparison.Ordinal))
                    return faction.StartPosition;
            return Vector2Int.zero;
        }

        private bool TryFindOwnedBuilding(string ownerId, string buildingId, out Vector2Int position)
        {
            if (_construction is IConstructionSaveSnapshotSource source)
            {
                foreach (ConstructionSavedPlacement placement in source.GetSavedPlacements())
                {
                    if (string.Equals(placement.OwnerId, ownerId, StringComparison.Ordinal)
                        && string.Equals(placement.BuildingId, buildingId, StringComparison.Ordinal))
                    {
                        position = placement.Position;
                        return true;
                    }
                }
            }
            position = default;
            return false;
        }

        private bool TryPlaceBarrack(string ownerId, Vector2Int center, out Vector2Int position)
        {
            for (int radius = 1; radius <= 6; radius++)
            for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
            {
                if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radius)
                    continue;
                Vector2Int candidate = center + new Vector2Int(x, y);
                if (_construction.TryDirectPlace("barrack", candidate, ownerId))
                {
                    position = candidate;
                    return true;
                }
            }
            position = default;
            return false;
        }

        private Vector2Int FindMoveTarget(Vector2Int position)
        {
            Vector2Int[] offsets = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };
            for (int index = 0; index < offsets.Length; index++)
            {
                Vector2Int candidate = position + offsets[index];
                if (_grid.TryGetTileData(candidate, out string tileId) && !string.IsNullOrWhiteSpace(tileId))
                    return candidate;
            }
            return position;
        }
    }
}
