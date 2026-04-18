using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Зберігає обчислену стартову позицію гравця.
    /// Встановлюється <see cref="StartingPositionInitializer"/>;
    /// зчитується <see cref="BootstrapGameInitializer"/> для розміщення замку.
    /// </summary>
    internal sealed class BootstrapStartingPositionState
    {
        /// <summary>Чи була стартова позиція вже обчислена на цій сесії.</summary>
        public bool IsSet { get; private set; }

        /// <summary>Тайлова позиція стартового ядра.</summary>
        public Vector2Int StartPosition { get; private set; }

        public void Set(Vector2Int position)
        {
            StartPosition = position;
            IsSet = true;
        }
    }
}
