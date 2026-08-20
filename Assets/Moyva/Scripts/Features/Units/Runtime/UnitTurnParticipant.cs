using System;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitTurnParticipant : ITurnParticipant
    {
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitClassConfig _configs;

        public UnitTurnParticipant(
            IUnitService units,
            IUnitOwnershipQuery ownership,
            IUnitClassConfig configs)
        {
            _units = units;
            _ownership = ownership;
            _configs = configs;
        }

        public int TurnOrder => 100;

        public void OnTurnStarted(TurnContext context)
        {
            foreach (string unitId in _units.GetAllUnitIds())
            {
                if (!string.Equals(_ownership.GetUnitOwnerId(unitId), context.Faction.OwnerId, StringComparison.Ordinal))
                    continue;

                string typeId = _units.GetUnitTypeId(unitId);
                UnitClassConfig config = string.IsNullOrWhiteSpace(typeId) ? null : _configs.GetConfig(typeId);
                if (config != null)
                    _units.SetStamina(unitId, config.MovementPointsPerTurn);
            }
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }
    }
}
