using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    public sealed class MapVisualChunkRegistry : IMapVisualChunkRegistry
    {
        private readonly Dictionary<MapChunkCoord, bool> _cameraVisible = new();
        private readonly HashSet<MapChunkCoord> _fogHidden = new();
        private readonly Dictionary<Renderer, RendererEntry> _renderers = new();
        private bool _hasCameraState;

        public int CameraVisibilityVersion { get; private set; }

        public void Clear()
        {
            foreach (var entry in _renderers.Values)
                entry.Restore();

            _renderers.Clear();
        }

        public void ResetVisibilityState()
        {
            _cameraVisible.Clear();
            _fogHidden.Clear();
            _hasCameraState = false;
            CameraVisibilityVersion++;
        }

        public void Register(Renderer renderer, IReadOnlyList<MapChunkCoord> chunks)
        {
            if (renderer == null || chunks == null || chunks.Count == 0)
                return;

            if (!_renderers.TryGetValue(renderer, out var entry))
            {
                entry = new RendererEntry(renderer);
                _renderers[renderer] = entry;
            }

            entry.SetChunks(chunks);
        }

        public void SetCameraVisible(IReadOnlyCollection<MapChunkCoord> visibleChunks)
        {
            if (_hasCameraState && MatchesCameraVisibleSet(visibleChunks))
                return;

            _cameraVisible.Clear();
            _hasCameraState = true;
            if (visibleChunks != null)
            {
                foreach (var coord in visibleChunks)
                    _cameraVisible[coord] = true;
            }

            CameraVisibilityVersion++;
            ApplyVisibility();
        }

        private bool MatchesCameraVisibleSet(IReadOnlyCollection<MapChunkCoord> visibleChunks)
        {
            int incomingCount = visibleChunks?.Count ?? 0;
            if (_cameraVisible.Count != incomingCount)
                return false;

            if (visibleChunks == null)
                return true;

            foreach (MapChunkCoord coord in visibleChunks)
            {
                if (!_cameraVisible.ContainsKey(coord))
                    return false;
            }

            return true;
        }

        public bool IsCameraVisible(MapChunkCoord coord)
            => !_hasCameraState || _cameraVisible.ContainsKey(coord);

        public void SetFogFullyHidden(MapChunkCoord coord, bool hidden)
        {
            if (hidden)
                _fogHidden.Add(coord);
            else
                _fogHidden.Remove(coord);
        }

        public void ApplyVisibility()
        {
            foreach (var entry in _renderers.Values)
                entry.Apply(ShouldRenderAnyChunk(entry.Chunks));
        }

        private bool ShouldRenderAnyChunk(IReadOnlyList<MapChunkCoord> chunks)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var coord = chunks[i];
                bool cameraVisible = !_hasCameraState || _cameraVisible.ContainsKey(coord);
                if (cameraVisible && !_fogHidden.Contains(coord))
                    return true;
            }

            return false;
        }

        private sealed class RendererEntry
        {
            private readonly Renderer _renderer;
            private readonly List<MapChunkCoord> _chunks = new();
            private bool _hiddenByChunks;
            private bool _enabledBeforeChunkHide;

            public RendererEntry(Renderer renderer)
            {
                _renderer = renderer;
            }

            public IReadOnlyList<MapChunkCoord> Chunks => _chunks;

            public void SetChunks(IReadOnlyList<MapChunkCoord> chunks)
            {
                _chunks.Clear();
                for (int i = 0; i < chunks.Count; i++)
                    _chunks.Add(chunks[i]);
            }

            /*
             * Chunk visibility тільки вимикає те, що вимкнула сама.
             * Умова "visible" ніколи не вмикає renderer насильно —
             * інакше реєстр знімав би fog-hide на межі туману,
             * де чанк частково видимий, але конкретні клітини ще
             * не Visible, і дві системи перезаписували б enabled.
             */
            public void Apply(bool visible)
            {
                if (_renderer == null)
                    return;

                if (!visible)
                {
                    if (_hiddenByChunks)
                        return;

                    _enabledBeforeChunkHide = _renderer.enabled;
                    _hiddenByChunks = true;
                    _renderer.enabled = false;
                    return;
                }

                Restore();
            }

            public void Restore()
            {
                if (!_hiddenByChunks || _renderer == null)
                    return;

                _renderer.enabled = _enabledBeforeChunkHide;
                _hiddenByChunks = false;
            }
        }
    }
}
