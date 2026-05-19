using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Settlement registry service: manages settlement storage, lookup, and position-to-settlement mapping.
    /// </summary>
    internal interface ISettlementRegistry
    {
        /// <summary>Get read-only view of all settlements.</summary>
        IReadOnlyDictionary<string, EconomySettlementState> AllSettlements { get; }

        /// <summary>Get settlement by ID.</summary>
        EconomySettlementState GetSettlement(string settlementId);

        /// <summary>Try get settlement by world position.</summary>
        bool TryGetSettlementByPosition(Vector2Int position, out EconomySettlementState state);

        /// <summary>Try find nearest active settlement for owner at position.</summary>
        bool TryFindNearestSettlement(Vector2Int position, string ownerId, out EconomySettlementState state);

        /// <summary>Register newly created settlement.</summary>
        void RegisterSettlement(EconomySettlementState state, Vector2Int townHallPosition);

        /// <summary>Unregister settlement (e.g., when townhall demolished).</summary>
        void UnregisterSettlement(string settlementId);

        /// <summary>Map building position to settlement and building metadata.</summary>
        void RegisterBuildingPosition(Vector2Int position, string settlementId, string buildingId, string ownerId);

        /// <summary>Remove building position mapping.</summary>
        void UnregisterBuildingPosition(Vector2Int position);

        /// <summary>Try get building at position with owner.</summary>
        bool TryGetBuildingAtPosition(Vector2Int position, out string buildingId, out string ownerId);

        /// <summary>Get settlement name or fallback to ID.</summary>
        string GetSettlementNameOrFallback(string settlementId);
    }
}
