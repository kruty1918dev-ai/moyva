using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class MapObjectSelectionHighlightService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IMapObjectVisualRegistryService _mapObjectVisualRegistryService;
        private readonly SpriteSelectionHighlighter _highlighter = new SpriteSelectionHighlighter();

        private string _activeObjectId;
        private Vector2Int _activePosition;

        public MapObjectSelectionHighlightService(
            SignalBus signalBus,
            [InjectOptional] IMapObjectVisualRegistryService mapObjectVisualRegistryService)
        {
            _signalBus = signalBus;
            _mapObjectVisualRegistryService = mapObjectVisualRegistryService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            ClearHighlight();
            _highlighter.Dispose();
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
        {
            ClearHighlight();
        }

        private void OnSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind == WorldInfoSelectionKind.MapObject && !string.IsNullOrWhiteSpace(signal.ObjectId))
            {
                if (string.Equals(_activeObjectId, signal.ObjectId, StringComparison.Ordinal) && _activePosition == signal.Position)
                    return;

                ClearHighlight();
                ApplyHighlight(signal.ObjectId, signal.Position);
                return;
            }

            ClearHighlight();
        }

        private void ApplyHighlight(string objectId, Vector2Int position)
        {
            if (_mapObjectVisualRegistryService == null)
                return;

            if (!_mapObjectVisualRegistryService.TryGetVisual(objectId, position, out var go) || go == null)
                return;

            var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers == null || renderers.Length == 0)
                return;

            _highlighter.Apply(go);

            _activeObjectId = objectId;
            _activePosition = position;
        }

        private void ClearHighlight()
        {
            _highlighter.Clear();
            ClearHighlightStateOnly();
        }

        private void ClearHighlightStateOnly()
        {
            _activeObjectId = null;
            _activePosition = default;
        }
    }
}
