using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Сервіс для керування підтвердженими будівлями на карті
    /// </summary>
    public interface IBuildingService
    {
        /// <summary>Список всіх підтверджених будівель</summary>
        IReadOnlyList<PlacedBuilding> GetAllBuildings();

        /// <summary>Отримати будівлю на позиції. Повертає null якщо нема.</summary>
        PlacedBuilding GetBuilding(Vector2Int position);

        /// <summary>Знищити будівлю за ID</summary>
        void DestroyBuilding(string buildingId);

        /// <summary>
        /// Зареєструвати підтверджену будівлю (викликається BuildingPlacementService при підтвердженні)
        /// </summary>
        void RegisterBuilding(string buildingId, string typeId, Vector2Int position, GameObject go);
    }
}
