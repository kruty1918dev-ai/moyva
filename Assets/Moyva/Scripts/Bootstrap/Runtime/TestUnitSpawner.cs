using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    // IInitializable означає, що Zenject викличе цей метод відразу після завантаження сцени
    internal sealed class TestUnitSpawner : IInitializable, System.IDisposable
    {
        private readonly IUnitFactory _unitFactory;
        private readonly ISaveService _saveService;
        private readonly ISaveInspectorService _saveInspectorService;
        private readonly SignalBus _signalBus;
        private bool _shouldSpawnSamples;

        public TestUnitSpawner(
            IUnitFactory unitFactory,
            ISaveService saveService,
            ISaveInspectorService saveInspectorService,
            SignalBus signalBus)
        {
            _unitFactory = unitFactory;
            _saveService = saveService;
            _saveInspectorService = saveInspectorService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);

            if (!SavePlayModeOptions.AutoLoadEnabled)
            {
                Debug.Log("[Bootstrap] Auto Load вимкнено — стартуємо як нову гру.");
                _shouldSpawnSamples = true;
                return;
            }

            bool hasSavedWorld = _saveService.HasSave(0) &&
                _saveInspectorService.HasBlock(0, "Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule");
            if (hasSavedWorld)
            {
                Debug.Log("[Bootstrap] Знайдено збереження зі збереженими даними генератора — запускаємо завантаження слота 0.");
                _saveService.Load(0);
            }
            else
            {
                Debug.Log("[Bootstrap] Валідного save-блоку генератора немає — буде створена нова гра після побудови світу.");
                _shouldSpawnSamples = true;
            }
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<WorldBuiltSignal>(OnWorldBuilt);
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
        {
            if (!_shouldSpawnSamples)
                return;

            _shouldSpawnSamples = false;
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