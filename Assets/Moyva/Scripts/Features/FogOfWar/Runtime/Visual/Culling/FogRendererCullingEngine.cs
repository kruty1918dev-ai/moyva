using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Heavyweight runtime engine for fog-based renderer culling.
    /// Owns renderer discovery, tracking state and batch evaluation.
    /// </summary>
    internal sealed class FogRendererCullingEngine : IInitializable, ITickable, IDisposable
    {
        private const int DefaultMaxRenderersPerFrame = 384;
        private const float DefaultDiscoveryInterval = 0.75f;
        private const float DefaultBoundsPaddingCells = 0f;

        private static readonly string[] WorldRootNames =
        {
            "TilesRoot",
            "ObjectsRoot",
            "BuildingsRoot",
            "PlayerBuildingsRoot",
            "Clouds",
            "CloudsRoot",
        };

        private readonly FogOfWarService _fogService;
        private readonly IGridService _gridService;
        private readonly SignalBus _signalBus;
        private readonly FogOfWarSettings _settings;
        private readonly IGridProjection _gridProjection;

        private readonly List<FogCullableRenderer> _renderers = new List<FogCullableRenderer>();
        private readonly Dictionary<Renderer, FogCullableRenderer> _tracked = new Dictionary<Renderer, FogCullableRenderer>();
        private readonly Dictionary<string, GameObject> _unitObjects = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Transform> _worldRoots = new Dictionary<string, Transform>();
        private readonly HashSet<Renderer> _discoveredRenderers = new HashSet<Renderer>();
        private readonly List<Renderer> _rendererDiscoveryBuffer = new List<Renderer>(512);

        private bool _discoveryRequested = true;
        private bool _evaluationPending;
        private int _cursor;
        private int _lastFogVersion = -1;
        private float _nextDiscoveryAt;

        /// <summary>
        /// Створює runtime culling service для приховування renderer-ів під unexplored fog.
        /// </summary>
        /// <param name="fogService">Gameplay fog service, який дає актуальний fog state.</param>
        /// <param name="gridService">Grid service для розрахунку покритих клітин.</param>
        /// <param name="signalBus">SignalBus для реакції на зміни світу та об'єктів.</param>
        /// <param name="gridProjection">Необов'язковий projected grid adapter.</param>
        /// <param name="settings">Fog settings із culling tuning-ом.</param>
        public FogRendererCullingEngine(
            FogOfWarService fogService,
            IGridService gridService,
            SignalBus signalBus,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] FogOfWarSettings settings = null)
        {
            _fogService = fogService;
            _gridService = gridService;
            _signalBus = signalBus;
            _gridProjection = gridProjection;
            _settings = settings;
        }

        /// <summary>
        /// Ініціалізує службу: підписується на події світу та запитує першочергове сканування рендерерів.
        /// </summary>
        public void Initialize()
        {
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            RequestDiscovery();
        }

        /// <summary>
        /// Зупиняє службу: відписується від подій і відновлює усі рендерери.
        /// </summary>
        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

            RestoreAllRenderers();
            _unitObjects.Clear();
        }

        /// <summary>
        /// Виконує щокадрову обробку: при необхідності сканує сцену, оцінює порцію рендерерів
        /// і приховує/показує їх залежно від туману.
        /// </summary>
        public void Tick()
        {
            if (!IsCullingEnabled())
            {
                RestoreAllRenderers();
                return;
            }

            if (_fogService == null || !_fogService.IsReady)
                return;

            float now = Time.unscaledTime;
            if (now >= _nextDiscoveryAt)
                RequestDiscovery();

            if (_discoveryRequested)
            {
                RebuildTrackedRenderers();
                _discoveryRequested = false;
                _nextDiscoveryAt = now + ResolveDiscoveryInterval();
                RequestEvaluation(resetCursor: true);
            }

            if (_lastFogVersion != _fogService.Version)
            {
                _lastFogVersion = _fogService.Version;
                RequestEvaluation(resetCursor: true);
            }

            if (!_evaluationPending)
                return;

            EvaluateBatch(ResolveMaxRenderersPerFrame());
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
            => RequestDiscovery();

        private void OnWorldGeneratedData(WorldGeneratedDataSignal _)
            => RequestDiscovery();

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(signal.UnitId) && signal.UnitObject != null)
                _unitObjects[signal.UnitId] = signal.UnitObject;

            RequestDiscovery();
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(signal.UnitId))
                _unitObjects.Remove(signal.UnitId);

            RequestDiscovery();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal _)
            => RequestDiscovery();

        private void OnBuildingDemolished(BuildingDemolishedSignal _)
            => RequestDiscovery();

        /// <summary>
        /// Позначає необхідність повторного сканування сцени для виявлення рендерерів.
        /// Метод лише встановлює прапорець; фактичне сканування виконається в Tick().
        /// </summary>
        private void RequestDiscovery()
        {
            _discoveryRequested = true;
        }

        /// <summary>
        /// Просить виконати оцінку видимості для поточного набору рендерерів.
        /// Якщо <paramref name="resetCursor"/> = true — скидає курсор і починає з початку списку.
        /// </summary>
        /// <param name="resetCursor">Скинути позицію обробки до початку списку.</param>
        private void RequestEvaluation(bool resetCursor)
        {
            if (resetCursor)
                _cursor = 0;

            _evaluationPending = true;
        }

        /// <summary>
        /// Перебирає корені світу та об'єкти одиниць, збирає поточні рендерери
        /// і оновлює внутрішні списки `_renderers` та `_tracked`.
        /// Коротко: виявляє нові рендерери, додає їх до відстеження, та видаляє застарілі.
        /// </summary>
        private void RebuildTrackedRenderers()
        {
            _discoveredRenderers.Clear();

            for (int i = 0; i < WorldRootNames.Length; i++)
            {
                var root = ResolveWorldRoot(WorldRootNames[i]);
                if (root != null)
                    AddRenderersFrom(root, _discoveredRenderers);
            }

            foreach (var unitObject in _unitObjects.Values)
            {
                if (unitObject != null)
                    AddRenderersFrom(unitObject.transform, _discoveredRenderers);
            }

            for (int i = _renderers.Count - 1; i >= 0; i--)
            {
                var entry = _renderers[i];
                if (entry.Renderer != null && _discoveredRenderers.Contains(entry.Renderer))
                    continue;

                entry.Restore();
                if (entry.Renderer != null)
                    _tracked.Remove(entry.Renderer);

                _renderers.RemoveAt(i);
            }

            _discoveredRenderers.Clear();
        }

        /// <summary>
        /// Повертає трансформ кореневого об'єкта світу з кешуванням.
        /// Якщо об'єкт не знайдено — кешується null.
        /// </summary>
        private Transform ResolveWorldRoot(string rootName)
        {
            if (_worldRoots.TryGetValue(rootName, out var cachedRoot) && cachedRoot != null)
                return cachedRoot;

            var rootObject = GameObject.Find(rootName);
            var root = rootObject != null ? rootObject.transform : null;
            _worldRoots[rootName] = root;
            return root;
        }

        /// <summary>
        /// Збирає рендерери з ієрархії <paramref name="root"/>, фільтрує їх і додає
        /// до множини <paramref name="discovered"/>. Нові рендерери обгортаються в <see cref="CullableRenderer"/>.
        /// </summary>
        private void AddRenderersFrom(Transform root, HashSet<Renderer> discovered)
        {
            if (root == null || !root.gameObject.activeInHierarchy)
                return;

            _rendererDiscoveryBuffer.Clear();
            root.GetComponentsInChildren(true, _rendererDiscoveryBuffer);
            for (int i = 0; i < _rendererDiscoveryBuffer.Count; i++)
            {
                var renderer = _rendererDiscoveryBuffer[i];
                if (!IsSupportedRenderer(renderer))
                    continue;

                discovered.Add(renderer);

                if (_tracked.ContainsKey(renderer))
                    continue;

                var entry = new FogCullableRenderer(renderer);
                _tracked.Add(renderer, entry);
                _renderers.Add(entry);
            }

            _rendererDiscoveryBuffer.Clear();
        }

        /// <summary>
        /// Оцінює порцію рендерерів (до <paramref name="maxRenderers"/>)
        /// і встановлює їх видимість залежно від туману.
        /// Для кожного рендерера викликає <see cref="FogRendererCullingEvaluator.ShouldRender"/>.
        /// </summary>
        /// <param name="maxRenderers">Максимальна кількість рендерерів для обробки за один раз.</param>
        private void EvaluateBatch(int maxRenderers)
        {
            if (_renderers.Count == 0)
            {
                _cursor = 0;
                _evaluationPending = false;
                return;
            }

            int processed = 0;
            float paddingCells = ResolveBoundsPaddingCells();

            while (processed < maxRenderers && _cursor < _renderers.Count)
            {
                var entry = _renderers[_cursor++];
                var renderer = entry.Renderer;
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                {
                    processed++;
                    continue;
                }

                bool shouldRender = FogRendererCullingEvaluator.ShouldRender(renderer.bounds, _fogService, _gridService, paddingCells, _gridProjection);
                entry.SetHiddenByFog(!shouldRender);
                processed++;
            }

            if (_cursor < _renderers.Count)
                return;

            _cursor = 0;
            _evaluationPending = false;
        }

        /// <summary>
        /// Відновлює початковий стан усіх відстежених рендерерів і очищає внутрішні списки.
        /// </summary>
        private void RestoreAllRenderers()
        {
            for (int i = 0; i < _renderers.Count; i++)
                _renderers[i].Restore();

            _tracked.Clear();
            _renderers.Clear();
            _cursor = 0;
            _evaluationPending = false;
            _lastFogVersion = -1;
        }

        /// <summary>
        /// Перевіряє конфігурацію, чи має працювати механізм відсіювання рендерерів.
        /// Бере до уваги налаштування `FogOfWarSettings`.
        /// </summary>
        private bool IsCullingEnabled()
        {
            if (_settings == null)
                return true;

            if (!_settings.EnableRendererCulling)
                return false;

            if (_settings.RequireOpaqueUnexploredForCulling && _settings.UnexploredAlpha < 0.99f)
                return false;

            return true;
        }

        /// <summary>
        /// Повертає максимальну кількість рендерерів, яку можна обробити за один Tick,
        /// використовуючи налаштування або значення за замовчуванням.
        /// </summary>
        private int ResolveMaxRenderersPerFrame()
            => _settings != null
                ? Mathf.Max(1, _settings.RendererCullingMaxRenderersPerFrame)
                : DefaultMaxRenderersPerFrame;

        /// <summary>
        /// Повертає інтервал (в секундах) між автоматичними скануваннями рендерерів.
        /// </summary>
        private float ResolveDiscoveryInterval()
            => _settings != null
                ? Mathf.Max(0.05f, _settings.RendererCullingDiscoveryInterval)
                : DefaultDiscoveryInterval;

        /// <summary>
        /// Повертає відступ (padding) в клітинах для розрахунку покриття об'єктів туманом.
        /// </summary>
        private float ResolveBoundsPaddingCells()
            => _settings != null
                ? Mathf.Max(0f, _settings.RendererCullingBoundsPaddingCells)
                : DefaultBoundsPaddingCells;

        /// <summary>
        /// Перевіряє, чи підтримується даний рендерер для цілей відсіювання.
        /// Підтримуються `SpriteRenderer`, `MeshRenderer` і `TilemapRenderer`,
        /// рендерер має бути активним та належати дозволеному шару. Ігнорує рендерери
        /// які знаходяться в батьківському `FogOfWarVolumeController`.
        /// </summary>
        private bool IsSupportedRenderer(Renderer renderer)
        {
            if (renderer == null || !renderer.gameObject.activeInHierarchy)
                return false;

            if (!(renderer is SpriteRenderer) && !(renderer is MeshRenderer) && !(renderer is TilemapRenderer))
                return false;

            if (_settings != null)
            {
                int bit = 1 << renderer.gameObject.layer;
                if ((_settings.RendererCullingLayerMask.value & bit) == 0)
                    return false;
            }

            return renderer.GetComponentInParent<FogOfWarVolumeController>() == null;
        }
    }
}
