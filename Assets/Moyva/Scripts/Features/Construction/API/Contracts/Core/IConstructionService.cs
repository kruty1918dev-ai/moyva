using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Compatibility facade for integrations that still combine interactive,
    /// persistence, and authoritative construction workflows.
    /// </summary>
    public interface IConstructionService :
        IConstructionSessionCommands,
        IConstructionPlacedBuildingQuery
    {
        void RestoreFromSave(Vector2Int position, string buildingId);

        bool TryDirectPlace(
            string buildingId,
            Vector2Int position,
            string placedByFactionId);

        bool TryDemolishByFaction(
            Vector2Int position,
            string factionId);
    }
}
