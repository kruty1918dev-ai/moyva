using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public bool TryDemolishAt(Vector2Int position)
        {
            if (!CanActiveOwnerAct(out string turnReason))
            {
                Debug.LogWarning($"[Construction] Demolition rejected: {turnReason}");
                return false;
            }

            if (!_isActive || !IsDemolishMode)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: active={_isActive}, demolish={IsDemolishMode}");
                return false;
            }

            position = _footprints.ResolveOrigin(position);
            if (!TryResolveCommittedBuildingForOwner(
                    position,
                    _activeOwnerId,
                    out Vector2Int origin,
                    out string buildingId,
                    out string ownershipReason))
            {
                _lastActionMessage = ownershipReason;
                Debug.LogWarning(
                    $"[Construction] TryDemolishAt({position}) rejected: {ownershipReason}");
                return false;
            }

            if (_pendingDemolitionPositions.Contains(position))
            {
                for (int i = _pendingDemolitions.Count - 1; i >= 0; i--)
                {
                    if (_pendingDemolitions[i].Position == position)
                    {
                        var pendingBuildingId = _pendingDemolitions[i].BuildingId;
                        _pendingDemolitions.RemoveAt(i);
                        _pendingDemolitionPositions.Remove(position);

                        _signalBus.Fire(new BuildingPreviewChangedSignal
                        {
                            Position = position,
                            BuildingId = pendingBuildingId,
                            PreviewState = BuildingPreviewState.None
                        });

                        if (VerboseLogs)
                            Debug.Log($"[Construction] Pending demolition unmarked for '{pendingBuildingId}' at {position}. pendingCount={_pendingDemolitions.Count}");

                        return true;
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: pending position exists but mark entry was not found.");
                return true;
            }

            _pendingDemolitions.Add(new PendingDemolition(position, buildingId));
            _pendingDemolitionPositions.Add(position);

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = buildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] Pending demolition marked for '{buildingId}' at {position}. pendingCount={_pendingDemolitions.Count}");

            return true;
        }

        public IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings()
        {
            string activeOwner = NormalizeOwnerId(_activeOwnerId);
            var snapshot = new Dictionary<Vector2Int, string>();
            foreach (var pair in _factionPlacedBuildings)
            {
                if (string.Equals(
                        NormalizeOwnerId(pair.Value.FactionId),
                        activeOwner,
                        System.StringComparison.Ordinal))
                {
                    snapshot[pair.Key] = pair.Value.BuildingId;
                }
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (!snapshot.ContainsKey(pair.Key))
                    snapshot[pair.Key] = pair.Value;
            }

            return new ReadOnlyDictionary<Vector2Int, string>(snapshot);
        }

        private void ConfirmPendingDemolitions()
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm demolish requested. count={_pendingDemolitions.Count}");

            for (int i = 0; i < _pendingDemolitions.Count; i++)
            {
                var demolition = _pendingDemolitions[i];
                var pos = demolition.Position;
                var id = demolition.BuildingId;

                if (!TryDemolishByFaction(
                        pos,
                        _activeOwnerId))
                {
                    continue;
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] Confirm demolished '{id}' at {pos}");
            }

            _pendingDemolitions.Clear();
            _pendingDemolitionPositions.Clear();

            if (VerboseLogs)
                Debug.Log("[Construction] Confirm demolish completed.");
        }
    }
}
