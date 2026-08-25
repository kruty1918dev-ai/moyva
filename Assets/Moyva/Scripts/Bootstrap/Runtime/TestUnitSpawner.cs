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
                return;
            }

            int slot = GameLaunchContext.SaveSlot;
            bool hasSave = _saveService.HasSave(slot);
            if (hasSave)
            {
                bool hasSavedWorld = _saveInspectorService.HasBlock(slot, "Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule");
                _saveService.Load(slot);
            }
            else
            {
            }
        }
    }
}