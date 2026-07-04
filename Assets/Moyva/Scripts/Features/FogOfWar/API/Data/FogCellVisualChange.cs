using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Описує зміну visual contribution однієї fog-клітинки.
    /// Renderer використовує old/new state, щоб прибрати клітинку зі старого cluster-а
    /// і додати її до нового без повної перебудови карти.
    /// </summary>
    public readonly struct FogCellVisualChange
    {
        public FogCellVisualChange(
            Vector2Int cell,
            FogStateType oldState,
            FogStateType newState,
            int oldHeightKey,
            int newHeightKey)
        {
            Cell = cell;
            OldState = oldState;
            NewState = newState;
            OldHeightKey = oldHeightKey;
            NewHeightKey = newHeightKey;
        }

        public Vector2Int Cell { get; }
        public FogStateType OldState { get; }
        public FogStateType NewState { get; }
        public int OldHeightKey { get; }
        public int NewHeightKey { get; }

        public bool HasVisualDelta
            => OldState != NewState || OldHeightKey != NewHeightKey;
    }
}
