using System;
using Kruty1918.Moyva.Combat.API;

namespace Kruty1918.Moyva.Combat.Runtime
{
    /// <summary>
    /// Базова реалізація <see cref="IHealth"/>, придатна для вбудовування у будь-яку сутність.
    ///
    /// Використання:
    ///   - Створіть екземпляр (через new або Zenject factory).
    ///   - Викличте <see cref="Initialize"/> під час spawn сутності.
    ///   - Збережіть у поле класу (unit / building); реєструйте у <see cref="IHealthRegistry"/>.
    ///
    /// Сервер-авторитетність:
    ///   Клас сам по собі не перевіряє, хто його викликає — цей контракт забезпечується
    ///   на рівні архітектури: лише серверні сервіси мають посилання на екземпляри IHealth.
    /// </summary>
    public sealed class HealthComponent : IHealth
    {
        private string _entityId;
        private int _currentHp;
        private int _maxHp;
        private int _armor;
        private bool _initialized;

        /// <inheritdoc/>
        public string EntityId => _entityId;

        /// <inheritdoc/>
        public int CurrentHp => _currentHp;

        /// <inheritdoc/>
        public int MaxHp => _maxHp;

        /// <inheritdoc/>
        public bool IsDestroyed => _currentHp <= 0;

        /// <inheritdoc/>
        public event Action<string, int, int, int> OnHealthChanged;

        /// <inheritdoc/>
        public event Action<string> OnDestroyed;

        /// <inheritdoc/>
        public void Initialize(string entityId, int maxHp)
            => Initialize(entityId, maxHp, armor: 0);

        public void Initialize(
            string entityId,
            int maxHp,
            int armor)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("EntityId не може бути порожнім.", nameof(entityId));
            if (maxHp < 1)
                throw new ArgumentOutOfRangeException(nameof(maxHp), "MaxHp має бути >= 1.");

            _entityId = entityId;
            _maxHp = maxHp;
            _currentHp = maxHp;
            _armor = Math.Max(0, armor);
            _initialized = true;
        }

        public void RestoreCurrentHp(int currentHp)
        {
            if (!_initialized)
                return;

            int previous = _currentHp;
            _currentHp =
                Math.Max(
                    0,
                    Math.Min(
                        _maxHp,
                        currentHp));

            if (_currentHp != previous)
            {
                OnHealthChanged?.Invoke(
                    _entityId,
                    previous,
                    _currentHp,
                    _maxHp);
            }
        }

        public void Reconfigure(
            int maxHp,
            int armor,
            bool preserveHealthRatio = true)
        {
            if (!_initialized)
                return;

            int nextMaxHp = Math.Max(1, maxHp);
            int previousMaxHp = Math.Max(1, _maxHp);
            int previousHp = _currentHp;

            _armor = Math.Max(0, armor);
            _maxHp = nextMaxHp;

            if (!IsDestroyed)
            {
                if (preserveHealthRatio)
                {
                    double ratio =
                        Math.Max(
                            0d,
                            Math.Min(
                                1d,
                                (double)previousHp
                                / previousMaxHp));
                    _currentHp =
                        Math.Max(
                            1,
                            Math.Min(
                                _maxHp,
                                (int)Math.Round(
                                    ratio * _maxHp)));
                }
                else
                {
                    _currentHp =
                        Math.Min(
                            previousHp,
                            _maxHp);
                }
            }

            if (_currentHp != previousHp
                || _maxHp != previousMaxHp)
            {
                OnHealthChanged?.Invoke(
                    _entityId,
                    previousHp,
                    _currentHp,
                    _maxHp);
            }
        }

        /// <inheritdoc/>
        public void TakeDamage(int amount)
        {
            if (!_initialized || IsDestroyed) return;
            if (amount <= 0) return;

            int effectiveDamage =
                Math.Max(0, amount - _armor);
            if (effectiveDamage <= 0)
                return;

            int oldHp = _currentHp;
            _currentHp =
                Math.Max(0, _currentHp - effectiveDamage);

            OnHealthChanged?.Invoke(_entityId, oldHp, _currentHp, _maxHp);

            if (_currentHp == 0)
                OnDestroyed?.Invoke(_entityId);
        }

        /// <inheritdoc/>
        public void Heal(int amount)
        {
            if (!_initialized || IsDestroyed) return;
            if (amount <= 0) return;

            int oldHp = _currentHp;
            _currentHp = Math.Min(_maxHp, _currentHp + amount);

            if (_currentHp != oldHp)
                OnHealthChanged?.Invoke(_entityId, oldHp, _currentHp, _maxHp);
        }

        /// <inheritdoc/>
        public void Kill()
        {
            if (!_initialized || IsDestroyed) return;

            int oldHp = _currentHp;
            _currentHp = 0;

            OnHealthChanged?.Invoke(_entityId, oldHp, 0, _maxHp);
            OnDestroyed?.Invoke(_entityId);
        }
    }
}
