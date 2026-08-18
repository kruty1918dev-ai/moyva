using System;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualChunkDiscoveryService : IMapVisualChunkDiscoveryService, IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IMapChunkSettingsProvider _settings;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkDiscoveryRebuildService _rebuildService;
        private bool _requested = true;
        private float _discoveryUntil;
        private float _nextDiscoveryAt;
        private int _lastRequestFrame = -1;

        public MapVisualChunkDiscoveryService(
            SignalBus signalBus,
            IMapChunkSettingsProvider settings,
            IMapChunkLayoutService layout,
            IMapVisualChunkDiscoveryRebuildService rebuildService)
        {
            _signalBus = signalBus;
            _settings = settings;
            _layout = layout;
            _rebuildService = rebuildService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Tick()
        {
            if (!CanRunDiscovery())
                return;

            _rebuildService.Rebuild();
            _requested = false;

            if (_settings.RepeatVisualChunkDiscoveryDuringStartup)
            {
                _nextDiscoveryAt =
                    Time.unscaledTime
                    + _settings.VisualDiscoveryIntervalSeconds;
            }
            else
            {
                _discoveryUntil =
                    float.NegativeInfinity;

                _nextDiscoveryAt =
                    float.PositiveInfinity;
            }
        }

        public void RequestDiscovery()
        {
            if (_lastRequestFrame == Time.frameCount)
                return;

            _lastRequestFrame =
                Time.frameCount;

            _requested =
                true;

            _discoveryUntil =
                _settings.RepeatVisualChunkDiscoveryDuringStartup
                    ? Time.unscaledTime
                      + _settings.VisualPartitionDurationSeconds
                    : Time.unscaledTime;

            _nextDiscoveryAt =
                0f;
        }

        private bool CanRunDiscovery()
        {
            if (!_settings.EnableVisualChunkDiscovery || !_layout.IsConfigured)
                return false;

            return ShouldRunDiscovery(
                _requested,
                Time.unscaledTime,
                _nextDiscoveryAt,
                _discoveryUntil);
        }

        internal static bool ShouldRunDiscovery(
            bool requested,
            float currentTime,
            float nextDiscoveryAt,
            float discoveryUntil)
            => requested
               || currentTime <= discoveryUntil && currentTime >= nextDiscoveryAt;

        private void OnWorldBuilt(WorldBuiltSignal _) => RequestDiscovery();
        private void OnWorldGenerated(WorldGeneratedDataSignal _) => RequestDiscovery();
    }
}
