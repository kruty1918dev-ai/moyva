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
            bool hasSave = _saveService.HasSave(slot);
            if (hasSave)
            {
                bool hasSavedWorld = _saveInspectorService.HasBlock(slot, "Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule");
                if (!hasSavedWorld)
                    Debug.LogWarning($"[Bootstrap] Save slot {slot} не має блоку генератора. SaveService спробує .bak перед завантаженням.");

                Debug.Log($"[Bootstrap] Знайдено збереження — запускаємо завантаження слота {slot}.");
                _saveService.Load(slot);
            }
            else
            {
                Debug.Log("[Bootstrap] Валідного save-блоку генератора немає — буде створена нова гра після побудови світу.");
            }
        }
    }
}