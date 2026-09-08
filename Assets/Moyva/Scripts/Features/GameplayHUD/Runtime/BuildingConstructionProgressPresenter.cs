using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Turns.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class BuildingConstructionProgressPresenter : ITickable, IDisposable
    {
        private readonly IConstructionPortfolioQuery _portfolio;
        private readonly IConstructionLifecycle _lifecycle;
        private readonly IGridProjection _projection;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _clock;
        private readonly GameplayHtmlAnchor[] _anchors;
        private readonly Dictionary<Vector2Int, Indicator> _indicators = new();
        private readonly HashSet<Vector2Int> _active = new();
        private readonly List<Vector2Int> _removed = new();
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private RectTransform _container;
        private UnityEngine.Camera _camera;
        private bool _createdContainer;
        private bool _warnedMissingCanvas;
        private bool _warnedMissingCamera;
        private float _nextRefresh;

        public BuildingConstructionProgressPresenter(
            IConstructionPortfolioQuery portfolio,
            IConstructionLifecycle lifecycle,
            IGridProjection projection,
            ITurnService turns,
            [InjectOptional] IGameplayProgressClock clock = null,
            [InjectOptional] GameplayHtmlAnchor[] anchors = null)
        {
            _portfolio = portfolio;
            _lifecycle = lifecycle;
            _projection = projection;
            _turns = turns;
            _clock = clock;
            _anchors = anchors ?? Array.Empty<GameplayHtmlAnchor>();
        }

        public void Tick()
        {
            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + 0.25f;
                Reconcile();
            }

            if (_indicators.Count == 0)
                return;

            UnityEngine.Camera camera = RecruitmentIndicatorScreenProjection.ResolveMainCamera(
                ref _camera, ref _warnedMissingCamera, string.Empty);
            if (camera == null || _canvasRect == null)
            {
                foreach (Indicator indicator in _indicators.Values)
                    indicator.Root.gameObject.SetActive(false);
                return;
            }

            RecruitmentIndicatorScreenProjection.UpdatePositions(
                _indicators.Values,
                static indicator => indicator.Position,
                static indicator => indicator.Root,
                _projection, 1.6f, _canvas, _canvasRect, camera);
        }

        private void Reconcile()
        {
            string ownerId = _turns.LocalOwnerId;
            _active.Clear();
            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                IReadOnlyList<ConstructionSavedPlacement> placements = _portfolio.GetOwnerPlacements(ownerId);
                for (int index = 0; index < placements.Count; index++)
                {
                    Vector2Int position = placements[index].Position;
                    if (!_lifecycle.TryGetProgress(position, out int completed, out int required)
                        || completed >= required)
                        continue;

                    if (_container == null)
                        EnsureContainer();
                    if (_container == null)
                        break;

                    _active.Add(position);
                    if (!_indicators.TryGetValue(position, out Indicator indicator))
                    {
                        indicator = CreateIndicator(position);
                        _indicators.Add(position, indicator);
                    }
                    string text = "Build " + GameplayProgressTimeText.Remaining(required - completed, _clock);
                    if (!string.Equals(indicator.Text.text, text, StringComparison.Ordinal))
                        indicator.Text.text = text;
                }
            }

            _removed.Clear();
            foreach (Vector2Int position in _indicators.Keys)
                if (!_active.Contains(position))
                    _removed.Add(position);
            foreach (Vector2Int position in _removed)
            {
                Object.Destroy(_indicators[position].Root.gameObject);
                _indicators.Remove(position);
            }
        }

        private void EnsureContainer()
        {
            Component anchor = null;
            foreach (GameplayHtmlAnchor candidate in _anchors)
                if (candidate != null)
                {
                    anchor = candidate;
                    break;
                }
            RecruitmentIndicatorCanvasContainer.TryResolve(
                anchor, "BuildingConstructionIndicators", string.Empty,
                ref _warnedMissingCanvas, out _canvas, out _canvasRect,
                out _container, out _createdContainer);
            // Keep map labels beneath the interactive HTML panels.
            if (_createdContainer && _container != null)
                _container.SetAsFirstSibling();
        }

        private Indicator CreateIndicator(Vector2Int position)
        {
            var root = new GameObject($"Construction_{position.x}_{position.y}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(_container, false);
            rect.sizeDelta = new Vector2(126f, 28f);
            Image background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.13f, 0.9f);
            background.raycastTarget = false;

            var textObject = new GameObject("Countdown",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(rect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 14f;
            text.color = new Color(0.95f, 0.82f, 0.48f);
            text.raycastTarget = false;
            return new Indicator(position, rect, text);
        }

        public void Dispose()
        {
            foreach (Indicator indicator in _indicators.Values)
                if (indicator.Root != null)
                    Object.Destroy(indicator.Root.gameObject);
            _indicators.Clear();
            if (_createdContainer && _container != null)
                Object.Destroy(_container.gameObject);
        }

        private sealed class Indicator
        {
            public Indicator(Vector2Int position, RectTransform root, TextMeshProUGUI text)
            {
                Position = position;
                Root = root;
                Text = text;
            }

            public Vector2Int Position { get; }
            public RectTransform Root { get; }
            public TextMeshProUGUI Text { get; }
        }
    }
}
