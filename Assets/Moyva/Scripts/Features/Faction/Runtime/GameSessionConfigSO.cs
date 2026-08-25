using System.Collections.Generic;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Faction.Runtime
{
    /// <summary>
    /// ScriptableObject — конфігурація сесії гри.
    /// Визначає локальних і мережевих учасників сесії.
    /// </summary>
[System.Serializable]
public sealed class GameSessionConfigSO : MoyvaJsonConfigObject
    {
        [SerializeField]
        private List<FactionSlot> _factions = new List<FactionSlot>();

        public IReadOnlyList<FactionSlot> Factions => _factions;
    }
}
