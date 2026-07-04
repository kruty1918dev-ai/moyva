using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за реєстрацію та зміну gameplay reveal sources.
    /// </summary>
    public interface IFogVisionSourceRegistry
    {
        void RegisterUnit(string unitId, Vector2Int position, int visionRange);
        void UpdateUnitVisionRange(string unitId, int visionRange);
        void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape);
        void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null);
        void UpdateUnitPosition(string unitId, Vector2Int newPosition);
        void UnregisterUnit(string unitId);
    }
}
