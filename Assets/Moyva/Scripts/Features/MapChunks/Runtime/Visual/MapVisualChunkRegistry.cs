using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualChunkRegistry : IMapVisualChunkRegistry
    {
        private readonly Dictionary<MapChunkCoord, bool> _cameraVisible = new();
        private readonly HashSet<MapChunkCoord> _fogHidden = new();
        private readonly Dictionary<Renderer, RendererEntry> _renderers = new();
        private bool _hasCameraState;

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
            _cameraVisible.Clear();
            _hasCameraState = true;
            if (visibleChunks != null)
            {
                foreach (var coord in visibleChunks)
                    _cameraVisible[coord] = true;
            }

            ApplyVisibility();
        }

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
            private readonly bool _originalEnabled;

            public RendererEntry(Renderer renderer)
            {
                _renderer = renderer;
                _originalEnabled = renderer.enabled;
            }

            public IReadOnlyList<MapChunkCoord> Chunks => _chunks;

            public void SetChunks(IReadOnlyList<MapChunkCoord> chunks)
            {
                _chunks.Clear();
                for (int i = 0; i < chunks.Count; i++)
                    _chunks.Add(chunks[i]);
            }

            public void Apply(bool visible)
            {
                if (_renderer != null)
                    _renderer.enabled = _originalEnabled && visible;
            }

            public void Restore()
            {
                if (_renderer != null)
                    _renderer.enabled = _originalEnabled;
            }
        }
    }
}
