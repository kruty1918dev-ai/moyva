using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionPlacedBuildingQuery
    {
        IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings();
        /// <summary>Повертає позиції та id усіх будівель, що належать зазначеному власнику.</summary>
        IReadOnlyDictionary<Vector2Int, string> GetPlacedBuildings(string ownerId);
        bool HasPlacedBuilding(string buildingId, string ownerId = null);
    }

    public readonly struct ConstructionSavedPlacement
    {
        public ConstructionSavedPlacement(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation = ConstructionRotation.Degrees0)
        {
            Position = position;
            BuildingId = buildingId;
            OwnerId = ownerId;
            Rotation = rotation;
        }

        public Vector2Int Position { get; }
        public string BuildingId { get; }
        public string OwnerId { get; }
        public ConstructionRotation Rotation { get; }
    }

    public interface IConstructionSaveSnapshotSource
    {
        IReadOnlyList<ConstructionSavedPlacement> GetSavedPlacements();
    }

    public interface IConstructionPortfolioQuery
    {
        IReadOnlyList<ConstructionSavedPlacement> GetOwnerPlacements(string ownerId);
    }

    public interface IConstructionSaveRestorer
    {
        void RestoreFromSave(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation = ConstructionRotation.Degrees0);
    }

    public interface IConstructionModuleStatePersistence
    {
        string StateKey { get; }
        byte[] CaptureState();
        void RestoreState(byte[] payload);
    }

    public interface IConstructionObserverStateSource
    {
        byte[] CaptureObserverState(string ownerId, ISet<Vector2Int> visibleBuildings);
    }
}
