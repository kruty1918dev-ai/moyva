using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Тимчасова stub-реалізація save provider-а для fog explored snapshot.
    /// Не зберігає дані фактично і підходить лише як fallback binding для нової гри.
    /// </summary>
    internal sealed class FogSaveDataStub : IFogSaveDataProvider
    {
        /// <summary>
        /// Повертає відсутній explored snapshot для сценарію нової гри.
        /// </summary>
        /// <returns>Завжди <see langword="null"/>.</returns>
        public bool[,] LoadExploredData()
        {
            Debug.Log("[FogOfWar][STUB] LoadExploredData() -> null (new game).");
            return null;
        }

        /// <summary>
        /// Ігнорує запит на збереження explored snapshot і лише пише діагностичний лог.
        /// </summary>
        /// <param name="explored">Snapshot, який намагаються зберегти.</param>
        public void SaveExploredData(bool[,] explored)
        {
            Debug.Log("[FogOfWar][STUB] SaveExploredData() not implemented. See docs/systems/fog-of-war/save-system-stub.md");
        }
    }
}
