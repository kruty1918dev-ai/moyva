using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class UnitRecruitmentProgressIndicatorPresenter :
        IInitializable,
        ITickable,
        IDisposable
    {
        private const string ContainerName = "RecruitmentProgressIndicators";
        private const string IndicatorPrefix = "ProgressIndicator_";
        private const float IndicatorWorldHeightOffset = 1.05f;
        private const float IndicatorWidth = 74f;
        private const float IndicatorHeight = 26f;

        private readonly Dictionary<long, ProgressHandle> _indicators = new();
        private readonly HashSet<long> _activeQueueIds = new();
        private readonly HashSet<BuildingQueueKey> _visitedQueues = new();
        private readonly List<long> _removeBuffer = new();
        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitRecruitmentStateStore _stateStore;
        private readonly IGridProjection _gridProjection;
        private readonly GameplayTurnHudView _hudView;

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private RectTransform _container;
        private UnityEngine.Camera _camera;
        private bool _createdContainer;
        private bool _warnedMissingCanvas;
        private bool _warnedMissingCamera;

        public UnitRecruitmentProgressIndicatorPresenter(
            SignalBus signalBus,
            ITurnService turns,
            IUnitRecruitmentService recruitment,
            IGridProjection gridProjection,
            [InjectOptional] IUnitRecruitmentStateStore stateStore = null,
            [InjectOptional] GameplayTurnHudView hudView = null)
        {
            _signalBus = signalBus;
            _turns = turns;
            _recruitment = recruitment;
            _gridProjection = gridProjection;
            _stateStore = stateStore;
            _hudView = hudView;
        }

        public void Initialize()
        {
            _turns.StateChanged += OnTurnStateChanged;
            _signalBus.Subscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.Subscribe<UnitRecruitmentReadySignal>(
                OnRecruitmentReady);
            _signalBus.Subscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);

            ReconcileProgressIndicators();
        }

        public void Dispose()
        {
            _turns.StateChanged -= OnTurnStateChanged;
            _signalBus.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.TryUnsubscribe<UnitRecruitmentReadySignal>(
                OnRecruitmentReady);
            _signalBus.TryUnsubscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            ClearIndicators();

            if (_createdContainer && _container != null)
                Object.Destroy(_container.gameObject);

            _container = null;
            _canvas = null;
            _canvasRect = null;
            _camera = null;
        }

        public void Tick()
        {
            if (_indicators.Count == 0)
                return;

            UpdateIndicatorPositions();
        }

        private void OnTurnStateChanged()
            => ReconcileProgressIndicators();

        private void OnRecruitmentQueueChanged(UnitRecruitmentQueueChangedSignal _)
            => ReconcileProgressIndicators();

        private void OnRecruitmentReady(UnitRecruitmentReadySignal _)
            => ReconcileProgressIndicators();

        private void OnRecruitmentDeployed(UnitRecruitmentDeployedSignal _)
            => ReconcileProgressIndicators();

        private void OnWorldBuilt(WorldBuiltSignal _)
            => ReconcileProgressIndicators();

        private void ReconcileProgressIndicators()
        {
            EnsureContainer();

            string ownerId = NormalizeId(_turns.LocalOwnerId);
            if (ownerId == null || _container == null || _stateStore == null)
            {
                ClearIndicators();
                return;
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> allItems =
                _stateStore.CaptureState();
            _activeQueueIds.Clear();
            _visitedQueues.Clear();

            for (int index = 0; index < allItems.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = allItems[index];
                string itemOwner = NormalizeId(item.OwnerId);
                if (!string.Equals(itemOwner, ownerId, StringComparison.Ordinal))
                    continue;

                var key = new BuildingQueueKey(ownerId, item.RecruitingBuildingPosition);
                if (!_visitedQueues.Add(key))
                    continue;

                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                    _recruitment.GetQueue(ownerId, item.RecruitingBuildingPosition);
                if (queue == null || queue.Count == 0)
                    continue;

                UnitRecruitmentQueueItemSnapshot head = queue[0];
                if (head.IsReady)
                    continue;

                _activeQueueIds.Add(head.QueueId);
                if (_indicators.TryGetValue(head.QueueId, out ProgressHandle existing))
                {
                    existing.Snapshot = head;
                    UpdateText(existing);
                    continue;
                }

                _indicators.Add(head.QueueId, CreateIndicator(head));
            }

            _removeBuffer.Clear();
            foreach (long queueId in _indicators.Keys)
            {
                if (!_activeQueueIds.Contains(queueId))
                    _removeBuffer.Add(queueId);
            }

            for (int index = 0; index < _removeBuffer.Count; index++)
                RemoveIndicator(_removeBuffer[index]);

            UpdateIndicatorPositions();
        }

        private void EnsureContainer()
        {
            if (_container != null)
                return;

            _canvas = ResolveCanvas();
            if (_canvas == null)
            {
                if (!_warnedMissingCanvas)
                {
                    _warnedMissingCanvas = true;
                    Debug.LogWarning(
                        "[UnitRecruitmentProgressIndicator] Gameplay Canvas not found. Training progress indicators are disabled.");
                }

                return;
            }

            _canvasRect = _canvas.transform as RectTransform;
            Transform existing = _canvas.transform.Find(ContainerName);
            if (existing != null)
            {
                _container = existing as RectTransform;
                _createdContainer = false;
                return;
            }

            var go = new GameObject(
                ContainerName,
                typeof(RectTransform));
            _container = go.GetComponent<RectTransform>();
            _container.SetParent(_canvas.transform, false);
            _container.anchorMin = Vector2.zero;
            _container.anchorMax = Vector2.one;
            _container.offsetMin = Vector2.zero;
            _container.offsetMax = Vector2.zero;
            _container.SetAsLastSibling();
            _createdContainer = true;
        }

        private Canvas ResolveCanvas()
        {
            Canvas canvas = _hudView != null
                ? _hudView.GetComponentInParent<Canvas>(true)
                : null;
            if (canvas != null)
                return canvas;

            return Object.FindFirstObjectByType<Canvas>(
                FindObjectsInactive.Include);
        }

        private ProgressHandle CreateIndicator(
            UnitRecruitmentQueueItemSnapshot snapshot)
        {
            var root = new GameObject(
                $"{IndicatorPrefix}{snapshot.QueueId}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(_container, false);
            rect.sizeDelta = new Vector2(IndicatorWidth, IndicatorHeight);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.12f, 0.14f, 0.86f);
            background.raycastTarget = false;

            var textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(rect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 14f;
            text.color = new Color(0.82f, 0.96f, 1f, 1f);
            text.raycastTarget = false;

            var handle = new ProgressHandle(snapshot, rect, text);
            UpdateText(handle);
            return handle;
        }

        private static void UpdateText(ProgressHandle handle)
            => handle.Text.text = FormatRemainingTurns(
                handle.Snapshot.RemainingTurns);

        private static string FormatRemainingTurns(int turns)
        {
            int safeTurns = Math.Max(0, turns);
            int lastTwoDigits = safeTurns % 100;
            int lastDigit = safeTurns % 10;
            string word = lastTwoDigits >= 11 && lastTwoDigits <= 14
                ? "ходів"
                : lastDigit == 1
                    ? "хід"
                    : lastDigit >= 2 && lastDigit <= 4
                        ? "ходи"
                        : "ходів";

            return $"{safeTurns} {word}";
        }

        private void UpdateIndicatorPositions()
        {
            UnityEngine.Camera camera = ResolveCamera();
            if (camera == null || _canvasRect == null || _container == null)
            {
                SetAllIndicatorsVisible(false);
                return;
            }

            foreach (ProgressHandle handle in _indicators.Values)
            {
                Vector3 world = ResolveIndicatorWorldPosition(
                    handle.Snapshot.RecruitingBuildingPosition);
                Vector3 screen = camera.WorldToScreenPoint(world);
                Vector3 viewport = camera.WorldToViewportPoint(world);
                bool visible = screen.z > 0f
                    && viewport.x >= 0f
                    && viewport.x <= 1f
                    && viewport.y >= 0f
                    && viewport.y <= 1f;

                handle.Root.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                UnityEngine.Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : (_canvas.worldCamera != null ? _canvas.worldCamera : camera);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _canvasRect,
                        screen,
                        uiCamera,
                        out Vector2 local))
                {
                    handle.Root.anchoredPosition = local;
                }
            }
        }

        private Vector3 ResolveIndicatorWorldPosition(Vector2Int buildingPosition)
            => _gridProjection.GridToWorld(buildingPosition)
               + Vector3.up * IndicatorWorldHeightOffset;

        private UnityEngine.Camera ResolveCamera()
        {
            if (_camera != null && _camera.isActiveAndEnabled)
                return _camera;

            _camera = UnityEngine.Camera.main;
            if (_camera == null && !_warnedMissingCamera)
            {
                _warnedMissingCamera = true;
                Debug.LogWarning(
                    "[UnitRecruitmentProgressIndicator] Main Camera not found. Progress indicators will stay hidden.");
            }

            return _camera;
        }

        private void SetAllIndicatorsVisible(bool visible)
        {
            foreach (ProgressHandle handle in _indicators.Values)
                handle.Root.gameObject.SetActive(visible);
        }

        private void RemoveIndicator(long queueId)
        {
            if (!_indicators.TryGetValue(queueId, out ProgressHandle handle))
                return;

            _indicators.Remove(queueId);
            Object.Destroy(handle.Root.gameObject);
        }

        private void ClearIndicators()
        {
            _removeBuffer.Clear();
            foreach (long queueId in _indicators.Keys)
                _removeBuffer.Add(queueId);

            for (int index = 0; index < _removeBuffer.Count; index++)
                RemoveIndicator(_removeBuffer[index]);

            _activeQueueIds.Clear();
            _visitedQueues.Clear();
            _removeBuffer.Clear();
        }

        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private readonly struct BuildingQueueKey : IEquatable<BuildingQueueKey>
        {
            private readonly string _ownerId;
            private readonly Vector2Int _position;

            public BuildingQueueKey(string ownerId, Vector2Int position)
            {
                _ownerId = ownerId ?? string.Empty;
                _position = position;
            }

            public bool Equals(BuildingQueueKey other)
                => string.Equals(_ownerId, other._ownerId, StringComparison.Ordinal)
                   && _position == other._position;

            public override bool Equals(object obj)
                => obj is BuildingQueueKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_ownerId != null
                        ? StringComparer.Ordinal.GetHashCode(_ownerId)
                        : 0) * 397) ^ _position.GetHashCode();
                }
            }
        }

        private sealed class ProgressHandle
        {
            public ProgressHandle(
                UnitRecruitmentQueueItemSnapshot snapshot,
                RectTransform root,
                TextMeshProUGUI text)
            {
                Snapshot = snapshot;
                Root = root;
                Text = text;
            }

            public UnitRecruitmentQueueItemSnapshot Snapshot;
            public RectTransform Root { get; }
            public TextMeshProUGUI Text { get; }
        }
    }
}
