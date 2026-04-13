using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// FSM-реалізація AI-мозку для однієї Bot-фракції.
    ///
    /// Стани:
    ///   Idle      → немає юнітів або щойно ініціалізовано.
    ///   Expanding → бот намагається нарощувати кількість юнітів.
    ///   Attacking → достатньо юнітів для атаки (placeholder).
    ///   Defending → мало юнітів, бот переходить у режим захисту.
    /// </summary>
    internal sealed class BotBrain : IBotController
    {
        public FactionId FactionId   => _definition.FactionId;
        public BotState  CurrentState { get; private set; } = BotState.Idle;

        private readonly FactionDefinition        _definition;
        private readonly IUnitFactory             _unitFactory;
        private readonly IFactionOwnershipService _ownership;
        private readonly IBotDifficultySettings   _settings;

        /// <summary>
        /// Zenject inject: _definition передається як extra arg через container.Instantiate,
        /// решта — стандартні DI-залежності.
        /// </summary>
        [Inject]
        public BotBrain(
            FactionDefinition        definition,
            IUnitFactory             unitFactory,
            IFactionOwnershipService ownership,
            IBotDifficultySettings   settings)
        {
            _definition  = definition;
            _unitFactory = unitFactory;
            _ownership   = ownership;
            _settings    = settings;
        }

        public void Tick()
        {
            var myUnits = _ownership.GetUnitIds(_definition.FactionId);
            int unitCount = myUnits.Count;

            switch (CurrentState)
            {
                case BotState.Idle:
                    TransitionToExpanding();
                    break;

                case BotState.Expanding:
                    TickExpanding(unitCount);
                    break;

                case BotState.Attacking:
                    TickAttacking(unitCount);
                    break;

                case BotState.Defending:
                    TickDefending(unitCount);
                    break;
            }
        }

        private void TransitionToExpanding()
        {
            CurrentState = BotState.Expanding;
            Debug.Log($"[BotBrain:{_definition.FactionId}] → Expanding");
        }

        private void TickExpanding(int unitCount)
        {
            if (unitCount >= _settings.AttackThreshold)
            {
                CurrentState = BotState.Attacking;
                Debug.Log($"[BotBrain:{_definition.FactionId}] → Attacking ({unitCount} юнітів)");
                return;
            }

            SpawnStartUnit();
        }

        private void TickAttacking(int unitCount)
        {
            if (unitCount <= _settings.DefendThreshold)
            {
                CurrentState = BotState.Defending;
                Debug.Log($"[BotBrain:{_definition.FactionId}] → Defending ({unitCount} юнітів)");
                return;
            }

            // Placeholder: логіка атаки буде розширена пізніше.
            Debug.Log($"[BotBrain:{_definition.FactionId}] Готовий до атаки ({unitCount} юнітів).");
        }

        private void TickDefending(int unitCount)
        {
            if (unitCount < _settings.DefendThreshold)
            {
                CurrentState = BotState.Expanding;
                Debug.Log($"[BotBrain:{_definition.FactionId}] → Expanding (мало юнітів: {unitCount})");
                return;
            }

            // Placeholder: логіка захисту буде розширена пізніше.
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
