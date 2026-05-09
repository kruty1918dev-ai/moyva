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
            if (!GameLaunchContext.IsAutoLoadEnabled())
            {
                Debug.Log("[Bootstrap] Auto Load вимкнено — стартуємо як нову гру.");
                return;
            }

            int slot = GameLaunchContext.SaveSlot;
            bool hasSavedWorld = _saveService.HasSave(slot) &&
                _saveInspectorService.HasBlock(slot, "Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule");
            if (hasSavedWorld)
            {
                Debug.Log($"[Bootstrap] Знайдено збереження зі збереженими даними генератора — запускаємо завантаження слота {slot}.");
                _saveService.Load(slot);
            }
            else
            {
                Debug.Log("[Bootstrap] Валідного save-блоку генератора немає — буде створена нова гра після побудови світу.");
            }
        }
    }
}