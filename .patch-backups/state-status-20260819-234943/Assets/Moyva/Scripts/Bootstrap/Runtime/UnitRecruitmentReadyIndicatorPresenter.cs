using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class UnitRecruitmentReadyIndicatorPresenter :
        IInitializable,
        ITickable,
        IDisposable
    {
        private const string ContainerName = "RecruitmentReadyIndicators";
        private const string IndicatorPrefix = "ReadyIndicator_";
        private const float IndicatorWorldHeightOffset = 1.35f;
        private const float IndicatorSize = 52f;

        private readonly Dictionary<long, IndicatorHandle> _indicators = new();
        private readonly Dictionary<string, Sprite> _spriteByUnitTypeId =
            new(StringComparer.Ordinal);
        private readonly HashSet<string> _missingSpriteWarnings =
            new(StringComparer.Ordinal);
        private readonly HashSet<long> _readyQueueIds = new();
        private readonly List<long> _removeBuffer = new();

        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly IGridProjection _gridProjection;
        private readonly GameplayTurnHudView _hudView;

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private RectTransform _container;
        private UnityEngine.Camera _camera;
        private bool _createdContainer;
        private bool _warnedMissingCanvas;
        private bool _warnedMissingCamera;

        public UnitRecruitmentReadyIndicatorPresenter(
            SignalBus signalBus,
            ITurnService turns,
            IUnitRecruitmentService recruitment,
            IUnitClassConfig unitConfigs,
            IGridProjection gridProjection,
            [InjectOptional] GameplayTurnHudView hudView = null)
        {
            _signalBus = signalBus;
            _turns = turns;
            _recruitment = recruitment;
            _unitConfigs = unitConfigs;
            _gridProjection = gridProjection;
            _hudView = hudView;
        }

        public void Initialize()
        {
            _turns.StatusChanged += OnTurnStateChanged;
            _signalBus.Subscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.Subscribe<UnitRecruitmentReadySignal>(
                OnRecruitmentReady);
            _signalBus.Subscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);

            ReconcileReadyIndicators();
        }

        public void Dispose()
        {
            _turns.StatusChanged -= OnTurnStateChanged;
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
            => ReconcileReadyIndicators();

        private void OnRecruitmentQueueChanged(UnitRecruitmentQueueChangedSignal _)
            => ReconcileReadyIndicators();

        private void OnRecruitmentReady(UnitRecruitmentReadySignal _)
            => ReconcileReadyIndicators();

        private void OnRecruitmentDeployed(UnitRecruitmentDeployedSignal _)
            => ReconcileReadyIndicators();

        private void OnWorldBuilt(WorldBuiltSignal _)
            => ReconcileReadyIndicators();

        private void ReconcileReadyIndicators()
        {
            EnsureContainer();

            string ownerId = _turns.LocalOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId) || _container == null)
            {
                ClearIndicators();
                return;
            }

            string normalizedOwnerId = ownerId.Trim();
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> readyItems =
                _recruitment.GetReadyItems(normalizedOwnerId);

            _readyQueueIds.Clear();
            for (int index = 0; index < readyItems.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot ready = readyItems[index];
                if (ready.QueueId < 1
                    || !ready.IsReady
                    || !string.Equals(
                        ready.OwnerId?.Trim(),
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                _readyQueueIds.Add(ready.QueueId);
                if (_indicators.TryGetValue(
                        ready.QueueId,
                        out IndicatorHandle existing))
                {
                    existing.Snapshot = ready;
                    ApplySprite(existing, ready.UnitTypeId);
                    continue;
                }

                _indicators.Add(
                    ready.QueueId,
                    CreateIndicator(ready));
            }

            _removeBuffer.Clear();
            foreach (long queueId in _indicators.Keys)
            {
                if (!_readyQueueIds.Contains(queueId))
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
                        "[UnitRecruitmentReadyIndicator] Gameplay Canvas not found. Ready indicators are disabled.");
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

        private IndicatorHandle CreateIndicator(
            UnitRecruitmentQueueItemSnapshot snapshot)
        {
            var root = new GameObject(
                $"{IndicatorPrefix}{snapshot.QueueId}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(_container, false);
            rect.sizeDelta = new Vector2(IndicatorSize, IndicatorSize);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.1f, 0.18f, 0.14f, 0.88f);
            background.raycastTarget = true;

            Button button = root.GetComponent<Button>();
            button.targetGraphic = background;

            var iconObject = new GameObject(
                "Icon",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(rect, false);
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(6f, 6f);
            iconRect.offsetMax = new Vector2(-6f, -6f);

            Image icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var handle = new IndicatorHandle(snapshot, rect, icon, button);
            button.onClick.AddListener(() => OnIndicatorClicked(snapshot.QueueId));
            ApplySprite(handle, snapshot.UnitTypeId);
            return handle;
        }

        private void ApplySprite(IndicatorHandle handle, string unitTypeId)
        {
            Sprite sprite = ResolveUnitSprite(unitTypeId);
            handle.Icon.sprite = sprite;
            handle.Icon.enabled = sprite != null;
        }

        private Sprite ResolveUnitSprite(string unitTypeId)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return null;

            string key = unitTypeId.Trim();
            if (_spriteByUnitTypeId.TryGetValue(key, out Sprite cached))
                return cached;

            Sprite sprite = _unitConfigs.GetConfig(key)?.ResolveCustomSprite();
            _spriteByUnitTypeId[key] = sprite;

            if (sprite == null && _missingSpriteWarnings.Add(key))
            {
                Debug.LogWarning(
                    "[UnitRecruitmentReadyIndicator] Unit ready indicator has no CustomSprite. Neutral indicator will be shown.");
            }

            return sprite;
        }

        private void OnIndicatorClicked(long queueId)
        {
            if (!_indicators.TryGetValue(queueId, out IndicatorHandle handle))
                return;

            UnitRecruitmentQueueItemSnapshot snapshot = handle.Snapshot;
            _signalBus.Fire(new UnitRecruitmentReadyIndicatorClickedSignal
            {
                OwnerId = snapshot.OwnerId,
                QueueId = snapshot.QueueId,
                UnitTypeId = snapshot.UnitTypeId,
                RecruitingBuildingId = snapshot.RecruitingBuildingId,
                RecruitingBuildingPosition = snapshot.RecruitingBuildingPosition,
            });
        }

        private void UpdateIndicatorPositions()
        {
            UnityEngine.Camera camera = ResolveCamera();
            if (camera == null || _canvasRect == null || _container == null)
            {
                SetAllIndicatorsVisible(false);
                return;
            }

            foreach (IndicatorHandle handle in _indicators.Values)
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
                    "[UnitRecruitmentReadyIndicator] Main Camera not found. Ready indicators will stay hidden.");
            }

            return _camera;
        }

        private void SetAllIndicatorsVisible(bool visible)
        {
            foreach (IndicatorHandle handle in _indicators.Values)
                handle.Root.gameObject.SetActive(visible);
        }

        private void RemoveIndicator(long queueId)
        {
            if (!_indicators.TryGetValue(queueId, out IndicatorHandle handle))
                return;

            handle.Button.onClick.RemoveAllListeners();
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

            _readyQueueIds.Clear();
            _removeBuffer.Clear();
        }

        private sealed class IndicatorHandle
        {
            public IndicatorHandle(
                UnitRecruitmentQueueItemSnapshot snapshot,
                RectTransform root,
                Image icon,
                Button button)
            {
                Snapshot = snapshot;
                Root = root;
                Icon = icon;
                Button = button;
            }

            public UnitRecruitmentQueueItemSnapshot Snapshot;
            public RectTransform Root { get; }
            public Image Icon { get; }
            public Button Button { get; }
        }
    }
}
