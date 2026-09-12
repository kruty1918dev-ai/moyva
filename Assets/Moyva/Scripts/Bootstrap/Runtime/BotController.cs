using System;
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
        private IDisposable _inputBlock;
        private long _observedTurn = -1;
        private float _delay;

        public BotController(ITurnService turns, IGameplayInputPolicy input,
            [InjectOptional] ITurnAuthorityPolicy authority = null)
        {
            _turns = turns;
            _input = input;
            _authority = authority;
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

            _delay = 0.35f;
            _turns.TryEndTurn(_turns.ActiveOwnerId, out _);
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
                    _delay = 0.35f;
                }
            }
            else
            {
                _inputBlock?.Dispose();
                _inputBlock = null;
                _observedTurn = -1;
            }
        }

        public void Dispose()
        {
            _turns.StateChanged -= RefreshControl;
            _inputBlock?.Dispose();
            _inputBlock = null;
        }
    }
}
