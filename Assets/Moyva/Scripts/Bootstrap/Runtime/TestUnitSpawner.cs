using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class TestUnitSpawner : IInitializable
    {
        private readonly ISaveService _saveService;
        private readonly ISaveInspectorService _saveInspectorService;

        public TestUnitSpawner(
            ISaveService saveService,
            ISaveInspectorService saveInspectorService)
        {
            _saveService = saveService;
            _saveInspectorService = saveInspectorService;
        }

        public void Initialize()
        {
            if (!SavePlayModeOptions.AutoLoadEnabled)
            {
                Debug.Log("[Bootstrap] Auto Load вимкнено — стартуємо як нову гру.");
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
            }
        }
    }
}