using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class TestUnitSpawner : IInitializable
    {
        private readonly ISaveService _saveService;

        public TestUnitSpawner(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public void Initialize()
        {
            if (!GameLaunchContext.IsAutoLoadEnabled())
            {
                return;
            }

            // Load also attempts the backup; failure must stop startup, not generate a new world.
            _saveService.Load(GameLaunchContext.SaveSlot);
        }
    }
}
