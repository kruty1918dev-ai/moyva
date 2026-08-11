using System.Collections.Generic;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
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
[System.Serializable]
public sealed class GameSessionConfigSO : MoyvaJsonConfigObject
    {
        [SerializeField]
        private List<FactionSlot> _factions = new List<FactionSlot>();

        public IReadOnlyList<FactionSlot> Factions => _factions;
    }
}
