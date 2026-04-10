using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Базова реалізація AI-мозку для однієї Bot-фракції.
    ///
    /// Поточна логіка (MVP):
    ///   — якщо у фракції немає жодного живого юніта, спавнить одного на StartPosition.
    ///
    /// Розширення: замінити тіло Tick() на повноцінне дерево рішень або GOAP.
    /// </summary>
    internal sealed class BotBrain : IBotController
    {
        public FactionId FactionId => _definition.FactionId;

        private readonly FactionDefinition       _definition;
        private readonly IUnitFactory            _unitFactory;
        private readonly IFactionOwnershipService _ownership;

        /// <summary>
        /// Zenject inject: _definition передається як extra arg через container.Instantiate,
        /// решта — стандартні DI-залежності.
        /// </summary>
        [Inject]
        public BotBrain(
            FactionDefinition        definition,
            IUnitFactory             unitFactory,
            IFactionOwnershipService ownership)
        {
            _definition  = definition;
            _unitFactory = unitFactory;
            _ownership   = ownership;
        }

        public void Tick()
        {
            var myUnits = _ownership.GetUnitIds(_definition.FactionId);

            if (myUnits.Count == 0)
            {
                SpawnStartUnit();
            }

            // TODO: розширити логіку — економіка, атака, захист тощо.
        }

        private void SpawnStartUnit()
        {
            if (string.IsNullOrEmpty(_definition.DefaultUnitTypeId))
            {
                Debug.LogWarning($"[BotBrain:{_definition.FactionId}] DefaultUnitTypeId не вказано — спавн пропущено.");
                return;
            }

            string unitId = _unitFactory.CreateUnit(
                _definition.DefaultUnitTypeId,
                _definition.StartPosition,
                _definition.FactionId.Value);

            if (!string.IsNullOrEmpty(unitId))
                Debug.Log($"[BotBrain:{_definition.FactionId}] Spawned unit '{unitId}' at {_definition.StartPosition}.");
        }
    }
}
