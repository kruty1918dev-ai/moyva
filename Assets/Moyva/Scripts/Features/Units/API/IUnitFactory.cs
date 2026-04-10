using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitFactory
    {
        /// <summary>Створює юніт без прив'язки до фракції (зворотна сумісність).</summary>
        string CreateUnit(string typeId, Vector2Int gridPosition);

        /// <summary>
        /// Створює юніт і прив'язує його до вказаної фракції.
        /// <paramref name="ownerId"/> — це FactionId.Value (або null для "без власника").
        /// </summary>
        string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId);
    }
}