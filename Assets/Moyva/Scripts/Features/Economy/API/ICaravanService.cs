using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    public enum CaravanCargoOperation { Load, Unload, CollectLoot }

    public readonly struct CaravanTransferResult
    {
        private CaravanTransferResult(bool succeeded, string reason)
        { Succeeded = succeeded; Reason = reason ?? string.Empty; }

        public bool Succeeded { get; }
        public string Reason { get; }
        public static CaravanTransferResult Success() => new(true, string.Empty);
        public static CaravanTransferResult Rejected(string reason) => new(false, reason);
    }

    public readonly struct CaravanCargoRequest
    {
        public CaravanCargoRequest(string ownerId, string unitId, CaravanCargoOperation operation,
            string settlementId, string warehouseKey, IReadOnlyDictionary<string, float> resources)
        {
            OwnerId = ownerId;
            UnitId = unitId;
            Operation = operation;
            SettlementId = settlementId;
            WarehouseKey = warehouseKey;
            Resources = resources;
        }

        public string OwnerId { get; }
        public string UnitId { get; }
        public CaravanCargoOperation Operation { get; }
        public string SettlementId { get; }
        public string WarehouseKey { get; }
        public IReadOnlyDictionary<string, float> Resources { get; }
    }

    public readonly struct CaravanUnitSnapshot
    {
        public CaravanUnitSnapshot(string ownerId, Vector2Int position, float capacity)
        { OwnerId = ownerId; Position = position; Capacity = capacity; }

        public string OwnerId { get; }
        public Vector2Int Position { get; }
        public float Capacity { get; }
    }

    public readonly struct CaravanCargoSnapshot
    {
        public CaravanCargoSnapshot(CaravanUnitSnapshot unit, IReadOnlyDictionary<string, float> resources)
        { Unit = unit; Resources = resources; }

        public CaravanUnitSnapshot Unit { get; }
        public IReadOnlyDictionary<string, float> Resources { get; }
    }

    public readonly struct CaravanRouteTransferCommitted
    {
        public CaravanRouteTransferCommitted(CaravanCargoRequest request, CaravanRoutePhase nextPhase)
        { Request = request; NextPhase = nextPhase; }

        public CaravanCargoRequest Request { get; }
        public CaravanRoutePhase NextPhase { get; }
    }

    // Composition adapts unit identity and authority without Economy owning movement.
    public interface ICaravanGameplayAccess
    {
        event Action ProgressAvailable;
        bool IsAuthoritative { get; }
        bool TryGetWagon(string unitId, out CaravanUnitSnapshot unit);
        bool CanCommand(string ownerId, string unitId, out string reason);
        bool CanAccessWarehouse(string unitId, Vector2Int origin, out string reason);
        Task<CaravanTransferResult> MoveToWarehouseAsync(string unitId, Vector2Int origin, CancellationToken token);
    }

    public interface ICaravanService
    {
        event Action Changed;
        event Action<CaravanRouteTransferCommitted> RouteTransferCommitted;
        bool TryGetCargo(string ownerId, string unitId, out CaravanCargoSnapshot snapshot);
        IReadOnlyDictionary<string, float> GetLootAtWagon(string ownerId, string unitId);
        CaravanTransferResult CanExecute(CaravanCargoRequest request);
        CaravanTransferResult Execute(CaravanCargoRequest request);
        CaravanTransferResult SetRoute(CaravanRouteRequest request);
        CaravanTransferResult CanSetRoute(CaravanRouteRequest request);
        CaravanTransferResult StopRoute(string ownerId, string unitId);
        bool TryGetRoute(string ownerId, string unitId, out CaravanRouteSnapshot snapshot);
        CaravanTransferResult CanFoundSettlement(string ownerId, string unitId, string buildingId, out Vector2Int position);
        CaravanTransferResult FoundSettlement(string ownerId, string unitId, string buildingId);
        CaravanTransferResult ApplyConfirmedTransfer(CaravanCargoRequest request);
        CaravanTransferResult ApplyConfirmedRouteTransfer(CaravanCargoRequest request, CaravanRoutePhase nextPhase);
        CaravanTransferResult ApplyConfirmedSetRoute(CaravanRouteRequest request);
        CaravanTransferResult ApplyConfirmedStopRoute(string ownerId, string unitId);
        CaravanTransferResult ApplyConfirmedFoundSettlement(string ownerId, string unitId, string buildingId, Vector2Int position);
    }

    public interface ICaravanRemoteCommandRequester
    {
        event Action<CaravanRemoteCommandResult> CommandRejected;

        bool TryRequestExecute(CaravanCargoRequest request, out string reason);
        bool TryRequestSetRoute(CaravanRouteRequest request, out string reason);
        bool TryRequestStopRoute(string ownerId, string unitId, out string reason);
        bool TryRequestFoundSettlement(string ownerId, string unitId, string buildingId, out string reason);
    }

    public readonly struct CaravanRemoteCommandResult
    {
        public CaravanRemoteCommandResult(
            CaravanCommandKind command,
            string ownerId,
            string unitId,
            string requestId,
            string reason)
        {
            Command = command;
            OwnerId = ownerId ?? string.Empty;
            UnitId = unitId ?? string.Empty;
            RequestId = requestId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public CaravanCommandKind Command { get; }
        public string OwnerId { get; }
        public string UnitId { get; }
        public string RequestId { get; }
        public string Reason { get; }
    }

    public enum CaravanCommandKind
    {
        Transfer = 0,
        StartRoute = 1,
        StopRoute = 2,
        FoundSettlement = 3,
    }
}
