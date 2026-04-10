using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Faction.Runtime
{
    /// <summary>
    /// ScriptableObject — конфігурація сесії гри.
    /// Визначає кількість і тип учасників: люди, боти, мережеві гравці.
    ///
    /// Приклад слотів:
    ///   player_0 (Human), player_1 (Human), bot_0 (Bot), bot_1 (Bot)  → 2v2
    ///   player_0 (Human), bot_0 (Bot)                                  → 1v1 з ботом
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Session/Game Session Config", fileName = "GameSessionConfig")]
    public sealed class GameSessionConfigSO : ScriptableObject
    {
        [SerializeField]
        private List<FactionSlot> _factions = new List<FactionSlot>();

        public IReadOnlyList<FactionSlot> Factions => _factions;
    }
}
