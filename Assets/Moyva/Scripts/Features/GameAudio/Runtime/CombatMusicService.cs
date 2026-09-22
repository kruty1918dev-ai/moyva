using System;
using Kruty1918.Audio;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Перемикає сценовий музичний профіль у epic-режим, поки триває бій:
    /// кожна атака IUnitCombatService вмикає epic-трек і відновлює таймер тиші;
    /// після combatMusicQuietSeconds без бою музика повертається до default.
    /// Вимикається наприкінці гри та при Dispose.
    /// </summary>
    public sealed class CombatMusicService : IInitializable, ITickable, IDisposable
    {
        private readonly IMusicService _music;
        private readonly AudioFeedbackConfig _config;
        private readonly IUnitCombatService _combat;
        private readonly SignalBus _signalBus;

        private float _quietAt;
        private bool _armed;

        /// <summary>Створює сервіс бойової музики із залежностями аудіо та стану бою.</summary>
        public CombatMusicService(
            [InjectOptional] IMusicService music,
            [InjectOptional] AudioFeedbackConfig config,
            [InjectOptional] IUnitCombatService combat,
            [InjectOptional] SignalBus signalBus)
        {
            _music = music;
            _config = config;
            _combat = combat;
            _signalBus = signalBus;
        }

        /// <summary>Підписує сервіс на бойові події.</summary>
        public void Initialize()
        {
            if (_music == null || _combat == null
                || _config == null || !_config.combatMusicEnabled)
                return;

            _armed = true;
            _combat.AttackStarted += OnCombatActivity;
            _combat.AttackResolved += OnCombatActivity;
            _signalBus?.Subscribe<GameEndedSignal>(OnGameEnded);
        }

        /// <summary>Оновлює бойову музику за станом бою.</summary>
        public void Tick()
        {
            if (!_armed || !_music.IsEpicActive)
                return;

            if (Time.unscaledTime >= _quietAt)
                _music.DisableEpicMusic();
        }

        /// <summary>Зупиняє бойову музику та звільняє ресурси.</summary>
        public void Dispose()
        {
            if (!_armed)
                return;

            _armed = false;
            _combat.AttackStarted -= OnCombatActivity;
            _combat.AttackResolved -= OnCombatActivity;
            _signalBus?.TryUnsubscribe<GameEndedSignal>(OnGameEnded);

            if (_music != null && _music.IsEpicActive)
                _music.DisableEpicMusic();
        }

        private void OnCombatActivity(string attackerId, string defenderId)
            => MarkCombat();

        private void OnCombatActivity(UnitAttackResult result)
            => MarkCombat();

        private void MarkCombat()
        {
            _music.EnableEpicMusic();
            _quietAt = Time.unscaledTime + Mathf.Max(1f, _config.combatMusicQuietSeconds);
        }

        private void OnGameEnded(GameEndedSignal evt)
        {
            if (_music.IsEpicActive)
                _music.DisableEpicMusic();
        }
    }
}
