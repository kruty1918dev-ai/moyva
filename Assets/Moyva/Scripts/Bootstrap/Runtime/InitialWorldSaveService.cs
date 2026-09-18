using System;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap
{
    internal sealed class InitialWorldSaveService : IInitializable, IDisposable
    {
        private readonly ISaveService _saveService;
        private readonly SignalBus _signalBus;
        private bool _saved;

        public InitialWorldSaveService(ISaveService saveService, SignalBus signalBus)
        {
            _saveService = saveService;
            _signalBus = signalBus;
        }

        public void Initialize()
            => _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);

        public void Dispose()
            => _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            if (_saved
                || GameLaunchContext.Mode != GameLaunchMode.MenuNewGame
                || !GameLaunchContext.IsAutoSaveEnabled())
            {
                return;
            }

            _saved = true;
            _saveService.Save(GameLaunchContext.SaveSlot);
        }
    }
}
