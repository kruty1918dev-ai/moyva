using System.Collections.Generic;
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

        /// <summary>Усі зарезервовані стартові позиції для multiplayer host.</summary>
        public IReadOnlyList<Vector2Int> StartPositions => _startPositions;

        /// <summary>Стартові позиції, прив'язані до PlayerId учасників multiplayer-сесії.</summary>
        public IReadOnlyDictionary<string, Vector2Int> PlayerStartPositions => _playerStartPositions;

        private readonly List<Vector2Int> _startPositions = new List<Vector2Int>();
        private readonly Dictionary<string, Vector2Int> _playerStartPositions = new Dictionary<string, Vector2Int>();

        public void Set(Vector2Int position)
        {
            _startPositions.Clear();
            _playerStartPositions.Clear();
            _startPositions.Add(position);
            StartPosition = position;
            IsSet = true;
        }

        public void Set(IReadOnlyList<Vector2Int> positions, IReadOnlyList<string> playerIds = null)
        {
            _startPositions.Clear();
            _playerStartPositions.Clear();
            if (positions != null)
                _startPositions.AddRange(positions);

            if (playerIds != null)
            {
                int assignedCount = Mathf.Min(playerIds.Count, _startPositions.Count);
                for (int index = 0; index < assignedCount; index++)
                {
                    if (!string.IsNullOrEmpty(playerIds[index]))
                        _playerStartPositions[playerIds[index]] = _startPositions[index];
                }
            }

            if (_startPositions.Count > 0)
            {
                StartPosition = _startPositions[0];
                IsSet = true;
            }
            else
            {
                StartPosition = Vector2Int.zero;
                IsSet = false;
            }
        }
    }
}
