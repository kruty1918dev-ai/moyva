using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Позиційні loop-емітери світу: вода/ліс із тайлів (кластеризовано),
    /// робочі будівлі (ферма, кузня, млин) за подіями placement/demolish.
    /// Чутність гейтиться zoom-фокусом — при віддаленні камери емітери затихають.
    /// </summary>
    public sealed class AmbientWorldAudioService : IInitializable, ITickable, IDisposable
    {
        private sealed class Emitter
        {
            public AudioAmbienceEmitter Config;
            public AudioHandle Handle;
            public Vector2Int GridPosition;
            public bool FromBuilding;
            public float CurrentScale = 1f;
        }

        private readonly IAudioService _audio;
        private readonly AudioAmbienceConfig _config;
        private readonly AudioZoomFocusService _zoom;
        private readonly IGridService _grid;
        private readonly IGridProjection _projection;
        private readonly SignalBus _signalBus;
        private readonly List<Emitter> _emitters = new List<Emitter>();

        public AmbientWorldAudioService(
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioAmbienceConfig config,
            [InjectOptional] AudioZoomFocusService zoom,
            [InjectOptional] IGridService grid,
            [InjectOptional] IGridProjection projection,
            [InjectOptional] SignalBus signalBus)
        {
            _audio = audio;
            _config = config;
            _zoom = zoom;
            _grid = grid;
            _projection = projection;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            if (_signalBus != null)
            {
                _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
                _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            }
        }

        public void Tick()
        {
            if (_emitters.Count == 0 || _zoom == null)
                return;

            for (int i = _emitters.Count - 1; i >= 0; i--)
            {
                var emitter = _emitters[i];
                if (!emitter.Handle.IsValid || emitter.Handle.Source == null)
                {
                    _emitters.RemoveAt(i);
                    continue;
                }

                float factor = _zoom.EvaluateEmitterFactor(
                    emitter.Config.zoomFadeStart,
                    emitter.Config.zoomFadeEnd);
                if (!Mathf.Approximately(emitter.CurrentScale, factor))
                {
                    emitter.CurrentScale = factor;
                    _audio.SetPlaybackScale(emitter.Handle, factor);
                }
            }
        }

        public void Dispose()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
                _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            }

            StopAll();
        }

        private void OnWorldBuilt()
        {
            StopAll();
            SpawnTileEmitters();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal evt)
        {
            if (_config?.emitters == null || string.IsNullOrEmpty(evt.BuildingId))
                return;

            foreach (var def in _config.emitters)
            {
                if (def == null || string.IsNullOrWhiteSpace(def.matchBuildingId))
                    continue;
                if (!MatchesId(evt.BuildingId, def.matchBuildingId))
                    continue;

                SpawnEmitter(def, evt.Position, fromBuilding: true);
            }
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal evt)
        {
            for (int i = _emitters.Count - 1; i >= 0; i--)
            {
                var emitter = _emitters[i];
                if (!emitter.FromBuilding || emitter.GridPosition != evt.Position)
                    continue;

                emitter.Handle.Stop();
                _emitters.RemoveAt(i);
            }
        }

        private void SpawnTileEmitters()
        {
            if (_config?.emitters == null || _grid == null || _grid.GridWidth <= 0 || _grid.GridHeight <= 0)
                return;

            foreach (var def in _config.emitters)
            {
                if (def == null || string.IsNullOrWhiteSpace(def.matchTileTypeId))
                    continue;

                // Кластеризація: один емітер на блок clusterStep×clusterStep тайлів.
                var clusters = new HashSet<Vector2Int>();
                for (int x = 0; x < _grid.GridWidth; x++)
                for (int y = 0; y < _grid.GridHeight; y++)
                {
                    if (!_grid.TryGetTileData(new Vector2Int(x, y), out string tileTypeId))
                        continue;
                    if (!MatchesId(tileTypeId, def.matchTileTypeId))
                        continue;

                    int step = Mathf.Max(1, def.clusterStep);
                    clusters.Add(new Vector2Int(x / step, y / step));
                }

                int spawned = 0;
                foreach (var cluster in clusters)
                {
                    if (spawned >= Mathf.Max(1, def.maxInstances) || _emitters.Count >= TotalCap())
                        break;

                    int step = Mathf.Max(1, def.clusterStep);
                    var center = new Vector2Int(
                        Mathf.Min(cluster.x * step + step / 2, _grid.GridWidth - 1),
                        Mathf.Min(cluster.y * step + step / 2, _grid.GridHeight - 1));

                    if (SpawnEmitter(def, center, fromBuilding: false))
                        spawned++;
                }
            }
        }

        private bool SpawnEmitter(AudioAmbienceEmitter def, Vector2Int gridPos, bool fromBuilding)
        {
            if (_audio == null || string.IsNullOrWhiteSpace(def.soundKey))
                return false;
            if (_emitters.Count >= TotalCap())
                return false;

            // Не дублювати емітер тієї ж дефініції на тій самій клітинці.
            for (int i = 0; i < _emitters.Count; i++)
                if (_emitters[i].GridPosition == gridPos
                    && ReferenceEquals(_emitters[i].Config, def))
                    return false;

            Vector3 world = _projection != null
                ? _projection.GridToWorld(gridPos)
                : new Vector3(gridPos.x, 0f, gridPos.y);

            var handle = _audio.Play(def.soundKey, new AudioPlayOptions(
                position: world,
                volumeScale: def.volume,
                loopOverride: true));
            if (!handle.IsValid || handle.Source == null)
                return false;

            var source = handle.Source;
            source.spatialBlend = 1f;
            source.minDistance = Mathf.Max(0.01f, def.minDistance);
            source.maxDistance = Mathf.Max(source.minDistance, def.maxDistance);
            source.rolloffMode = AudioRolloffMode.Logarithmic;

            _emitters.Add(new Emitter
            {
                Config = def,
                Handle = handle,
                GridPosition = gridPos,
                FromBuilding = fromBuilding,
                CurrentScale = _zoom?.EvaluateEmitterFactor(def.zoomFadeStart, def.zoomFadeEnd) ?? 1f,
            });
            _audio.SetPlaybackScale(handle, _emitters[_emitters.Count - 1].CurrentScale);
            return true;
        }

        private void StopAll()
        {
            foreach (var emitter in _emitters)
                emitter.Handle.Stop();
            _emitters.Clear();
        }

        private int TotalCap()
            => _config != null ? Mathf.Max(0, _config.maxEmittersTotal) : 24;

        private static bool MatchesId(string actual, string expected)
        {
            if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(expected))
                return false;

            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
                || actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
