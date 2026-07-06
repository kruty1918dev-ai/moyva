using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using System;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class MapFogChunkCoverageRefreshService : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly FogOfWarService _fogService;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapChunkVisibilityService _visibility;
        private int _lastVersion = -1;

        public MapFogChunkCoverageRefreshService(
            SignalBus signalBus,
            FogOfWarService fogService,
            IMapChunkLayoutService layout,
            IMapChunkVisibilityService visibility)
        {
            _signalBus = signalBus;
            _fogService = fogService;
            _layout = layout;
            _visibility = visibility;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            RefreshIfReady(force: true);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Tick()
        {
            RefreshIfReady(force: false);
        }

        private void RefreshIfReady(bool force)
        {
            if (_fogService == null || !_fogService.IsReady || !_layout.IsConfigured)
                return;

            if (!force && _lastVersion == _fogService.Version)
                return;

            _lastVersion = _fogService.Version;
            _visibility.RefreshFogCoverage();
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal _)
        {
            _lastVersion = -1;
        }
    }
}
