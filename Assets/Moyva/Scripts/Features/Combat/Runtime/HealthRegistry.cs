using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Combat.API;

namespace Kruty1918.Moyva.Combat.Runtime
{
    /// <summary>
    /// Стандартна реалізація <see cref="IHealthRegistry"/>.
    /// Зберігає активні (не знищені) health-компоненти у словнику за EntityId.
    ///
    /// Реєстрація/видалення thread-safe не гарантована — вся взаємодія відбувається
    /// на main thread у рамках Unity game loop.
    /// </summary>
    public sealed class HealthRegistry : IHealthRegistry
    {
        private readonly Dictionary<string, IHealth> _registry = new Dictionary<string, IHealth>(64);

        /// <inheritdoc/>
        public int Count => _registry.Count;

        /// <inheritdoc/>
        public void Register(IHealth health)
        {
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (string.IsNullOrWhiteSpace(health.EntityId))
                throw new ArgumentException("IHealth.EntityId не може бути порожнім.");

            _registry[health.EntityId] = health;
        }

        /// <inheritdoc/>
        public void Unregister(string entityId)
        {
            if (!string.IsNullOrWhiteSpace(entityId))
                _registry.Remove(entityId);
        }

        /// <inheritdoc/>
        public IHealth Get(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId)) return null;
            _registry.TryGetValue(entityId, out var h);
            return h is { IsDestroyed: false } ? h : null;
        }

        /// <inheritdoc/>
        public bool TryGet(string entityId, out IHealth health)
        {
            health = Get(entityId);
            return health != null;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IHealth> GetAll()
        {
            // Повертаємо знімок лише живих сутностей
            var result = new List<IHealth>(_registry.Count);
            foreach (var kv in _registry)
            {
                if (!kv.Value.IsDestroyed)
                    result.Add(kv.Value);
            }
            return result;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IHealth> GetMany(IEnumerable<string> entityIds)
        {
            if (entityIds == null) return Array.Empty<IHealth>();

            var result = new List<IHealth>();
            foreach (var id in entityIds)
            {
                if (TryGet(id, out var h))
                    result.Add(h);
            }
            return result;
        }
    }
}
