using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Центральна точка підбору типу об'єкта за контекстом мапи.
    /// </summary>
    public interface IObjectTypePicker
    {
        IReadOnlyList<TopologyCaseType> SupportedCases { get; }

        /// <summary>
        /// Повертає найбільш релевантний BuildingId з урахуванням маски сусідів і налаштованих варіацій.
        /// </summary>
        bool TryPickId(string sourceBuildingId, TopologyNeighborMask mask, out string resolvedBuildingId);
    }
}
