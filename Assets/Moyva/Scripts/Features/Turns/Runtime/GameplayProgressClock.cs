using System;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Turns.Runtime
{
    internal sealed class GameplayProgressClock : IGameplayProgressClock, ITickable
    {
        private const float DefaultSandboxRoundSeconds = 10f;

        private readonly ICalendarService _calendar;
        private readonly LazyInject<ITurnService> _turns;
        private float _accumulator;

        public GameplayProgressClock(
            ICalendarService calendar,
            [InjectOptional] LazyInject<ITurnService> turns = null)
        {
            _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
            _turns = turns;
            SandboxRoundSeconds = DefaultSandboxRoundSeconds;
            Speed = 1f;
        }

        public event Action<GameplayProgressTick> Progressed;

        public GameplayProgressMode Mode { get; private set; } = GameplayProgressMode.TurnBased;
        public bool IsRealtime => Mode == GameplayProgressMode.SandboxRealtime;
        public float SandboxRoundSeconds { get; private set; }
        public float Speed { get; private set; }
        public long CurrentSequence => ResolveSequence();

        public void Configure(GameplayProgressMode mode, float sandboxRoundSeconds, float speed)
        {
            Mode = mode;
            SandboxRoundSeconds = Mathf.Max(0.1f, sandboxRoundSeconds);
            SetSpeed(speed);
            _accumulator = 0f;
        }

        public void SetSpeed(float speed)
            => Speed = Mathf.Clamp(speed, 0.1f, 8f);

        public void Tick()
        {
            if (!IsRealtime || Time.timeScale <= 0f
                || _turns?.Value?.Phase == TurnPhase.Completed)
                return;

            _accumulator += Time.deltaTime * Speed;
            if (_accumulator < SandboxRoundSeconds)
                return;

            float elapsed = SandboxRoundSeconds;
            _accumulator -= SandboxRoundSeconds;

            _calendar.AdvanceTurn();

            Progressed?.Invoke(new GameplayProgressTick(
                Mode,
                ResolveOwnerId(),
                CurrentSequence,
                elapsed));
        }

        private long ResolveSequence()
        {
            int hoursPerTurn = Mathf.Max(1, _calendar.Config.HoursPerTurn);
            return Math.Max(1L, (_calendar.TotalHoursSinceEpoch / hoursPerTurn) + 1L);
        }

        private string ResolveOwnerId()
        {
            ITurnService turns = _turns?.Value;
            string ownerId = turns?.LocalOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = turns?.ActiveOwnerId;
            return string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
        }
    }
}
