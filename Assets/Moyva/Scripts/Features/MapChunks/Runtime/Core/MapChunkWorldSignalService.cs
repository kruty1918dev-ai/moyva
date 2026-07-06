using System;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapChunkWorldSignalService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapChunkVisibilityService _visibility;
        private readonly IMapVisualChunkDiscoveryService _discovery;

        public MapChunkWorldSignalService(
            SignalBus signalBus,
            IMapChunkLayoutService layout,
            IMapChunkVisibilityService visibility,
            [InjectOptional] IMapVisualChunkDiscoveryService discovery = null)
        {
            _signalBus = signalBus;
            _layout = layout;
            _visibility = visibility;
            _discovery = discovery;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            var bounds = signal.HasMapWorldBounds
                ? new Bounds(signal.MapWorldBoundsCenter, Abs(signal.MapWorldBoundsSize))
                : default;

            _layout.Configure(
                signal.Width,
                signal.Height,
                signal.CellSize > 0.0001f ? signal.CellSize : 1f,
                signal.HasMapWorldBounds,
                bounds);

            _visibility.ResetVisibility();
            _discovery?.RequestDiscovery();
        }

        private static Vector3 Abs(Vector3 value)
            => new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
