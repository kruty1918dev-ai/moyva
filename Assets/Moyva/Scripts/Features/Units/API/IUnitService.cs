using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitService
    {
        float GetStamina(string unitId);
        void SetStamina(string unitId, float stamina);
        int GetHitPoints(string unitId);
        bool TryApplyDamage(string unitId, int damage, out int remainingHitPoints, out bool destroyed);
        bool TryGetUnitPosition(string unitId, out UnityEngine.Vector2Int position);
        GameObject GetUnitObject(string unitId);

        /// <summary>Повертає ідентифікатори всіх активних юнітів.</summary>
        IReadOnlyCollection<string> GetAllUnitIds();

        /// <summary>Повертає typeId (наприклад, "warrior") для відновлення юніта при завантаженні.</summary>
        string GetUnitTypeId(string unitId);
    }
}