using System;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Combat.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Серверний сервіс здоров'я будівель.
    ///
    /// Реєструє <see cref="IHealth"/> для кожної будівлі при отриманні <see cref="BuildingPlacedSignal"/>
    /// та видаляє при знищенні (через <see cref="IHealth.OnDestroyed"/>).
    ///
    /// EntityId формату: "{BuildingId}@{x},{y}" — унікальний, бо на одній клітинці може стояти
    /// лише одна будівля.
    /// </summary>
    internal sealed class BuildingHealthService : IInitializable, IDisposable
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IHealthRegistry _healthRegistry;
        private readonly SignalBus _signalBus;

        public BuildingHealthService(
            IBuildingRegistry buildingRegistry,
            IHealthRegistry healthRegistry,
            SignalBus signalBus)
        {
            _buildingRegistry = buildingRegistry ?? throw new ArgumentNullException(nameof(buildingRegistry));
            _healthRegistry   = healthRegistry   ?? throw new ArgumentNullException(nameof(healthRegistry));
            _signalBus        = signalBus         ?? throw new ArgumentNullException(nameof(signalBus));
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            var definition = _buildingRegistry.GetById(signal.BuildingId);
            if (definition == null)
            {
                Debug.LogWarning($"[BuildingHealthService] Визначення будівлі '{signal.BuildingId}' не знайдено; health не буде зареєстроване.");
                return;
            }

            int maxHp = Mathf.Max(1, definition.MaxHp);
            string entityId = BuildingEntityId(signal.BuildingId, signal.Position);

            var health = new HealthComponent();
            health.Initialize(entityId, maxHp);
            health.OnDestroyed += OnBuildingDestroyed;

            _healthRegistry.Register(health);
            Debug.Log($"[BuildingHealthService] Будівля '{signal.BuildingId}' @ {signal.Position} зареєстрована з HP {maxHp}.");
        }

        private void OnBuildingDestroyed(string entityId)
        {
            _healthRegistry.Unregister(entityId);
            Debug.Log($"[BuildingHealthService] Будівля '{entityId}' знищена та видалена з реєстру.");
        }

        /// <summary>
        /// Генерує унікальний EntityId для будівлі за її типом і позицією на grid.
        /// Формат: "barracks@3,5"
        /// </summary>
        public static string BuildingEntityId(string buildingId, Vector2Int position)
            => $"{buildingId}@{position.x},{position.y}";
    }
}
