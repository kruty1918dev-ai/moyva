using System;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitTurnParticipant : ITurnParticipant, IDisposable
    {
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitClassConfig _configs;
        private readonly IGameplayProgressClock _progressClock;

        public UnitTurnParticipant(
            IUnitService units,
            IUnitOwnershipQuery ownership,
            IUnitClassConfig configs,
            [InjectOptional] IGameplayProgressClock progressClock = null)
        {
            _units = units;
            _ownership = ownership;
            _configs = configs;
            _progressClock = progressClock;
            if (_progressClock != null)
                _progressClock.Progressed += OnProgressed;
        }

        public int TurnOrder => 100;

        public void OnTurnStarted(TurnContext context)
            => RestoreOwnerStamina(context.Faction.OwnerId);

        private void OnProgressed(GameplayProgressTick tick)
        {
            if (!tick.IsRealtime)
                return;

            RestoreOwnerStamina(tick.OwnerId);
        }

        private void RestoreOwnerStamina(string ownerId)
        {
            foreach (string unitId in _units.GetAllUnitIds())
            {
                if (!string.Equals(_ownership.GetUnitOwnerId(unitId), ownerId, StringComparison.Ordinal))
                    continue;

                string typeId = _units.GetUnitTypeId(unitId);
                UnitClassConfig config = string.IsNullOrWhiteSpace(typeId) ? null : _configs.GetConfig(typeId);
                if (config != null)
                    _units.SetStamina(unitId, config.MovementPointsPerTurn);
            }
        }

        public void OnTurnEnding(TurnContext context) { }
        public void OnRoundCompleted(int completedRound) { }

        public void Dispose()
        {
            if (_progressClock != null)
                _progressClock.Progressed -= OnProgressed;
        }
    }
}
