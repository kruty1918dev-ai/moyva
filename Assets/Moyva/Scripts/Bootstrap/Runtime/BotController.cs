using System;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.InputRouting.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>Контролер бот-опонента: визначає ходи бота через телеметрію та політику вводу, виконує їх у хід бота.</summary>
    internal sealed class BotController : IInitializable, ITickable, IDisposable
    {
        private readonly ITurnService _turns;
        private readonly IGameplayInputPolicy _input;
        private readonly ITurnAuthorityPolicy _authority;
        private readonly Func<IBotDecisionOrchestrator> _orchestratorProvider;
        private readonly float _presentationDelay;
        private IBotDecisionOrchestrator _orchestrator;
        private IDisposable _inputBlock;
        private long _observedTurn = -1;
        private float _delay;

        /// <summary>Створює контролер із сервісом ходів, політикою вводу та пайплайном бота.</summary>
        public BotController(ITurnService turns, IGameplayInputPolicy input,
            [InjectOptional] ITurnAuthorityPolicy authority = null,
            [InjectOptional] Func<IBotDecisionOrchestrator> orchestratorProvider = null,
            [InjectOptional] BotRuntimeConfig config = null)
        {
            _turns = turns;
            _input = input;
            _authority = authority;
            _orchestratorProvider = orchestratorProvider;
            _presentationDelay = config?.visibleDelay ?? 0.35f;
        }

        /// <summary>Ініціалізує контролер бота.</summary>
        public void Initialize()
        {
            _turns.StateChanged += RefreshControl;
            RefreshControl();
        }

        /// <summary>Прокачує логіку бота в хід його ходу.</summary>
        public void Tick()
        {
            if (Time.timeScale > 0f)
                Advance(Time.deltaTime);
        }

        internal void Advance(float deltaSeconds)
        {
            RefreshControl();
            if (!IsBotTurn() || _turns.Phase != TurnPhase.AwaitingInput)
                return;

            _delay -= deltaSeconds;
            if (_delay > 0f)
                return;

            // Resolve lazily: on save-load the bot identity is restored during
            // the load pass, after container construction.
            var orchestrator = _orchestrator ??= _orchestratorProvider?.Invoke();
            orchestrator?.BeginTurn(_turns.ActiveOwnerId);
            orchestrator?.Tick(deltaSeconds);
        }

        private bool IsBotTurn()
            => (_authority?.IsAuthoritative ?? true)
                && _turns.Phase != TurnPhase.Completed
                && GameLaunchContext.GetPlayerController(_turns.ActiveOwnerId) == PlayerControllerType.Bot;

        private void RefreshControl()
        {
            if (IsBotTurn())
            {
                _inputBlock ??= _input.AcquireBlock(GameplayInputKind.All, this);
                if (_observedTurn != _turns.GlobalTurn)
                {
                    _observedTurn = _turns.GlobalTurn;
                    _delay = _presentationDelay;
                }
            }
            else
            {
                _inputBlock?.Dispose();
                _inputBlock = null;
                _observedTurn = -1;
                _orchestrator?.Cancel();
            }
        }

        /// <summary>Звільняє ресурси контролера.</summary>
        public void Dispose()
        {
            _orchestrator?.Cancel();
            _turns.StateChanged -= RefreshControl;
            _inputBlock?.Dispose();
            _inputBlock = null;
        }
    }
}
