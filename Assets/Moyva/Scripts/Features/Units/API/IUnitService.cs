using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitService
    {
        float GetStamina(string unitId);
        bool TryGetUnitPosition(string unitId, out UnityEngine.Vector2Int position);
        // Метод для зміни ходу (Turn System буде його викликати)
        void ProcessTurnUpdate();
        GameObject GetUnitObject(string unitId);
    }
}