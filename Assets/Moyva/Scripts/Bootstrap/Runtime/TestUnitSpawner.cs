using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    // IInitializable означає, що Zenject викличе цей метод відразу після завантаження сцени
    internal sealed class TestUnitSpawner : IInitializable
    {
        private readonly IUnitFactory _unitFactory;

        public TestUnitSpawner(IUnitFactory unitFactory)
        {
            _unitFactory = unitFactory;
        }

        public void Initialize()
        {
            Debug.Log("[Bootstrap] Початок тестового спавну юнітів...");

            // Спавнимо кілька юнітів у різних точках для тесту
            SpawnSampleUnits();
        }

        private void SpawnSampleUnits()
        {
            // Юніт 1: Центр
            _unitFactory.CreateUnit("warrior", new Vector2Int(5, 5));

            // Юніт 2: Трохи далі
            _unitFactory.CreateUnit("warrior", new Vector2Int(7, 3));

            // Юніт 3: В кутку
            _unitFactory.CreateUnit("warrior", new Vector2Int(2, 8));

            Debug.Log("[Bootstrap] Тестові юніти створені.");
        }
    }
}