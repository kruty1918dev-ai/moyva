using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
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

        public IReadOnlyList<SpawnPositionAssignment> SpawnAssignments => _spawnAssignments;

        private readonly List<Vector2Int> _startPositions = new List<Vector2Int>();
        private readonly Dictionary<string, Vector2Int> _playerStartPositions = new Dictionary<string, Vector2Int>();
        private readonly List<SpawnPositionAssignment> _spawnAssignments = new List<SpawnPositionAssignment>();

        public void Set(Vector2Int position)
        {
            _startPositions.Clear();
            _playerStartPositions.Clear();
            _spawnAssignments.Clear();
            _startPositions.Add(position);
            _spawnAssignments.Add(new SpawnPositionAssignment { SlotIndex = 0, Position = position });
            StartPosition = position;
            IsSet = true;
        }

        public void Set(IReadOnlyList<Vector2Int> positions, IReadOnlyList<string> playerIds = null)
        {
            _startPositions.Clear();
            _playerStartPositions.Clear();
            _spawnAssignments.Clear();
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

            for (int index = 0; index < _startPositions.Count; index++)
            {
                string playerId = playerIds != null && index < playerIds.Count ? playerIds[index] : string.Empty;
                _spawnAssignments.Add(new SpawnPositionAssignment
                {
                    SlotIndex = index,
                    ParticipantId = playerId,
                    Position = _startPositions[index],
                });
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

        public void Set(SpawnPositionAssignment[] assignments)
        {
            _startPositions.Clear();
            _playerStartPositions.Clear();
            _spawnAssignments.Clear();

            if (assignments != null)
            {
                for (int index = 0; index < assignments.Length; index++)
                {
                    SpawnPositionAssignment assignment = assignments[index];
                    _spawnAssignments.Add(assignment);
                    _startPositions.Add(assignment.Position);

                    if (!string.IsNullOrEmpty(assignment.ParticipantId))
                        _playerStartPositions[assignment.ParticipantId] = assignment.Position;
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
