using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    // IInitializable означає, що Zenject викличе цей метод відразу після завантаження сцени
    internal sealed class TestUnitSpawner : IInitializable
    {
        private readonly IUnitFactory _unitFactory;
        private readonly ISaveService _saveService;

        public TestUnitSpawner(IUnitFactory unitFactory, ISaveService saveService)
        {
            _unitFactory = unitFactory;
            _saveService = saveService;
        }

        public void Initialize()
        {
            if (_saveService.HasSave(0))
            {
                Debug.Log("[Bootstrap] Знайдено збереження (слот 0) — завантажуємо юнітів з сейву.");
                _saveService.Load(0);
            }
            else
            {
                Debug.Log("[Bootstrap] Збереження не знайдено — стартуємо тестовий спавн юнітів.");
                SpawnSampleUnits();
            }
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