using System;
using Kruty1918.Moyva.Faction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Faction.Runtime
{
    /// <summary>
    /// Один слот фракції в конфігурації сесії.
    /// Кожен слот відповідає локальному або мережевому гравцю.
    /// </summary>
    [Serializable]
    public sealed class FactionSlot
    {
        [Tooltip("Унікальний рядковий ідентифікатор фракції, наприклад 'player_0'.")]
        [SerializeField] private string _factionId = "faction_0";

        [SerializeField] private FactionType _type = FactionType.Human;

        [Tooltip("TypeId стартового юніта, наприклад 'warrior'.")]
        [SerializeField] private string _defaultUnitTypeId = "warrior";

        [SerializeField] private Vector2Int _startPosition = Vector2Int.zero;

        [SerializeField] private Color _teamColor = Color.white;

        public string      FactionId         => _factionId;
        public FactionType Type              => _type;
        public string      DefaultUnitTypeId => _defaultUnitTypeId;
        public Vector2Int  StartPosition     => _startPosition;
        public Color       TeamColor         => _teamColor;
    }
}
