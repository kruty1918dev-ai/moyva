using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitService
    {
        float GetStamina(string unitId);
        bool TryGetUnitPosition(string unitId, out UnityEngine.Vector2Int position);
        GameObject GetUnitObject(string unitId);
    }
}