using UnityEngine;

namespace Kruty1918.Moyva.Faction.API
{
    /// <summary>
    /// Незмінні дані про одну фракцію в поточній сесії гри.
    /// Створюється FactionInstaller із GameSessionConfigSO.
    /// </summary>
    public sealed class FactionDefinition
    {
        public FactionId   FactionId          { get; }
        public FactionType FactionType        { get; }

        /// <summary>TypeId юніта, якого бот спавнить за замовчуванням (напр. "warrior").</summary>
        public string      DefaultUnitTypeId  { get; }

        /// <summary>Стартова позиція фракції на гриді.</summary>
        public Vector2Int  StartPosition      { get; }

        /// <summary>Колір команди для UI / візуальної ідентифікації.</summary>
        public Color       TeamColor          { get; }

        public FactionDefinition(
            FactionId   factionId,
            FactionType factionType,
            string      defaultUnitTypeId,
            Vector2Int  startPosition,
            Color       teamColor)
        {
            FactionId         = factionId;
            FactionType       = factionType;
            DefaultUnitTypeId = defaultUnitTypeId;
            StartPosition     = startPosition;
            TeamColor         = teamColor;
        }
    }
}
