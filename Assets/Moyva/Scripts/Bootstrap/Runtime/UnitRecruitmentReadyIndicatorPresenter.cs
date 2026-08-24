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
            _turns.StateChanged += OnTurnStateChanged;
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

            RecruitmentIndicatorCanvasContainer.TryResolve(
                _hudView,
                ContainerName,
                "[UnitRecruitmentReadyIndicator] Gameplay Canvas not found. Ready indicators are disabled.",
                ref _warnedMissingCanvas,
                out _canvas,
                out _canvasRect,
                out _container,
                out _createdContainer);
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
            UnityEngine.Camera camera =
                RecruitmentIndicatorScreenProjection.ResolveMainCamera(
                    ref _camera,
                    ref _warnedMissingCamera,
                    "[UnitRecruitmentReadyIndicator] Main Camera not found. Ready indicators will stay hidden.");
            if (camera == null || _canvasRect == null || _container == null)
            {
                SetAllIndicatorsVisible(false);
                return;
            }

            RecruitmentIndicatorScreenProjection.UpdatePositions(
                _indicators.Values,
                static handle =>
                    handle.Snapshot.RecruitingBuildingPosition,
                static handle => handle.Root,
                _gridProjection,
                IndicatorWorldHeightOffset,
                _canvas,
                _canvasRect,
                camera);
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
