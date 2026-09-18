using System;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class BotController : IInitializable, ITickable, IDisposable
    {
        private readonly ITurnService _turns;
        private readonly IGameplayInputPolicy _input;
        private readonly ITurnAuthorityPolicy _authority;
        private readonly IBotDecisionOrchestrator _orchestrator;
        private readonly float _presentationDelay;
        private IDisposable _inputBlock;
        private long _observedTurn = -1;
        private float _delay;

        public BotController(ITurnService turns, IGameplayInputPolicy input,
            [InjectOptional] ITurnAuthorityPolicy authority = null,
            [InjectOptional] IBotDecisionOrchestrator orchestrator = null,
            [InjectOptional] BotRuntimeConfig config = null)
        {
            _turns = turns;
            _input = input;
            _authority = authority;
            _orchestrator = orchestrator;
            _presentationDelay = config?.visibleDelay ?? 0.35f;
        }

        public void Initialize()
        {
            _turns.StateChanged += RefreshControl;
            RefreshControl();
        }

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

            _orchestrator?.BeginTurn(_turns.ActiveOwnerId);
            _orchestrator?.Tick(deltaSeconds);
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

        public void Dispose()
        {
            _orchestrator?.Cancel();
            _turns.StateChanged -= RefreshControl;
            _inputBlock?.Dispose();
            _inputBlock = null;
        }
    }
}
