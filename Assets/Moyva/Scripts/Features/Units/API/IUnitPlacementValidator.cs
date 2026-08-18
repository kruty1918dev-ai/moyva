using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Shared authoritative unit placement rules used by recruitment deployment
    /// and movement terrain validation.
    /// </summary>
    public interface IUnitPlacementValidator
    {
        /// <summary>
        /// Validates grid existence and terrain restrictions, but intentionally
        /// ignores occupancy so movement can apply its construction-sharing rules.
        /// </summary>
        bool IsTerrainAllowed(Vector2Int position, out string reason);

        /// <summary>
        /// Validates a final deployment tile including occupancy.
        /// </summary>
        bool CanDeployUnit(
            string unitTypeId,
            Vector2Int position,
            out string reason);
    }
}
