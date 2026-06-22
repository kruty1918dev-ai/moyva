using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal readonly struct FogFixedVisionAreaSnapshot
    {
        public FogFixedVisionAreaSnapshot(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
        {
            AreaId = areaId;
            Position = position;
            VisionRange = visionRange;
            Shape = shape;
        }

        public string AreaId { get; }
        public Vector2Int Position { get; }
        public int VisionRange { get; }
        public FogRevealShape Shape { get; }
    }

    internal readonly struct FogPendingRevealArea
    {
        public FogPendingRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId)
        {
            Center = center;
            Radius = radius;
            Shape = shape;
            KeepVisible = keepVisible;
            VisibleAreaId = visibleAreaId;
        }

        public Vector2Int Center { get; }
        public int Radius { get; }
        public FogRevealShape Shape { get; }
        public bool KeepVisible { get; }
        public string VisibleAreaId { get; }
    }

    /// <summary>
    /// Ядро служби "туману війни" (Fog of War).
    /// Підтримує матрицю лічильників видимості (int[,]) та масив позначок досліджених клітин (bool[,]).
    /// Обробляє реєстрацію одиниць і будівель, обчислює видимі клітини та оновлює текстуру туману.
    /// Підписується на сигнали гри через SignalBus.
    /// </summary>
    internal sealed class FogOfWarService : IFogOfWarService, IInitializable, IDisposable
    {
        /// <summary>
        /// Зведена документація полів FogOfWarService.
        /// Коротко:
        /// - Залежності: _resolver (обчислення видимих клітин), _heightVisionService (height-aware visibility),
        ///   _textureUpdater (оновлення текстури), _saveProvider (load/save explored), _signalBus (події), _settings (конфіг).
        /// - Конфіг і стан: _defaultVisionRange, _width, _height, _initialized (чи викликано Initialize).
        /// - Основні масиви: _visibilityCounters (int[,]) — лічильники видимості; _exploredTiles (bool[,]) — досліджені клітини;
        ///   _pendingExploredSnapshot — сніпшот, завантажений до ініціалізації.
        /// - Дані одиниць: _unitVisibleTiles, _unitVisionRange, _unitPositions, _unitVisionModifiers.
        /// - Фіксовані зони: _fixedVisionShapes (наприклад для будівель).
        /// - Відкладені записі: _pendingUnits (реєстрації до Initialize).
        /// - Оновлення текстури: _lastDirtyTiles; Version — внутрішня версія; IsReady — готовність.
        /// </summary>
        private const string BuildingVisionAreaPrefix = "building:";
        private const string StartupFallbackRevealAreaId = "fog-service-startup-fallback-reveal";
        private const string DebugTag = "[MoyvaFogTrace]";

        /// <summary>
        /// Сервіс, який обчислює базовий набір видимих клітин з позиції з урахуванням перешкод.
        /// Делегує обчислення прямих ліній видимості та форми відкриття.
        /// </summary>
        private readonly IFogVisibilityResolver _resolver;

        /// <summary>
        /// Сервіс, що враховує рельєф (висоти) при оцінці видимості та силуетної помітності цілей.
        /// Використовується для отримання searchRadius та факторів видимості.
        /// </summary>
        private readonly IHeightAwareVisionService _heightVisionService;

        /// <summary>
        /// Оновлювач текстури туману. Відповідає за застосування змінених клітин до візуального шару
        /// через `UpdateDirtyTiles` і за перебудову повної текстури `RebuildFullTexture`.
        /// </summary>
        private readonly IFogTextureUpdater     _textureUpdater;

        /// <summary>
        /// Провайдер даних збереження/завантаження для блоку досліджених клітин (explored).
        /// Викликається під час Initialize для потенційного відновлення сніпшоту.
        /// </summary>
        private readonly IFogSaveDataProvider   _saveProvider;

        /// <summary>
        /// `SignalBus` (Zenject) для підписки на ігрові події: створення/рух/знищення одиниць, будівлі, генерація світу тощо.
        /// </summary>
        private readonly SignalBus              _signalBus;

        /// <summary>
        /// Налаштування `FogOfWarSettings`: мін/макс дальність, пороги висотної видимості, опції відсіювання рендерерів тощо.
        /// </summary>
        private readonly FogOfWarSettings       _settings;

        private int     _defaultVisionRange = 5;
        private int     _width;
        private int     _height;
        private bool    _initialized;

        private int[,]  _visibilityCounters;
        private bool[,] _exploredTiles;
        private bool[,] _pendingExploredSnapshot;

        // unitId → список видимих клітин, коли одиницю востаннє реєстрували/переміщували
        private readonly Dictionary<string, IReadOnlyList<Vector2Int>> _unitVisibleTiles
            = new Dictionary<string, IReadOnlyList<Vector2Int>>();

        // unitId → дальність огляду (збережена під час реєстрації)
        private readonly Dictionary<string, int> _unitVisionRange
            = new Dictionary<string, int>();

        // unitId → поточна позиція
        private readonly Dictionary<string, Vector2Int> _unitPositions
            = new Dictionary<string, Vector2Int>();

        private readonly Dictionary<string, FogVisionModifiers> _unitVisionModifiers
            = new Dictionary<string, FogVisionModifiers>();

        private readonly Dictionary<string, FogRevealShape> _fixedVisionShapes
            = new Dictionary<string, FogRevealShape>();

        // unitId -> відкладені дані реєстрації, отримані до Initialize(width,height)
        private readonly Dictionary<string, (Vector2Int Position, int VisionRange, FogRevealShape? Shape, FogVisionModifiers Modifiers)> _pendingUnits
            = new Dictionary<string, (Vector2Int Position, int VisionRange, FogRevealShape? Shape, FogVisionModifiers Modifiers)>();

        private readonly List<FogPendingRevealArea> _pendingRevealAreas = new List<FogPendingRevealArea>();

        private HashSet<Vector2Int> _lastDirtyTiles = new HashSet<Vector2Int>();

        internal int Version { get; private set; }
        internal bool IsReady => _initialized;

        public FogOfWarService(
            IFogVisibilityResolver resolver,
            IHeightAwareVisionService heightVisionService,
            IFogTextureUpdater     textureUpdater,
            IFogSaveDataProvider   saveProvider,
            SignalBus              signalBus,
            [InjectOptional] FogOfWarSettings settings)
        {
            _resolver       = resolver;
            _heightVisionService = heightVisionService;
            _textureUpdater = textureUpdater;
            _saveProvider   = saveProvider;
            _signalBus      = signalBus;
            _settings       = settings;

            if (_settings != null)
                _defaultVisionRange = _settings.DefaultVisionRange;
            else
                Debug.LogWarning("[FogOfWar] FogOfWarService: FogOfWarSettings is null. Using DefaultVisionRange=5.");
        }

        // ─── Життєвий цикл Zenject ────────────────────────────────────────────

        /// <summary>
        /// Підписується на ігрові сигнали необхідні для роботи служби
        /// (створення/рух/знищення одиниць, розміщення/демонтаж будівель, генерація світу).
        /// </summary>
        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
        }

        /// <summary>
        /// Відписується від сигналів і очищує підписки.
        /// </summary>
        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
        }

        // ─── Реалізація IFogOfWarService ──────────────────────────────────────

        /// <summary>
        /// Ініціалізує службу з розмірами світу (ширина × висота).
        /// Встановлює внутрішні масиви видимості, відновлює відкладений сніпшот
        /// досліджених клітин (якщо такий є) та обробляє відкладені одиниці.
        /// </summary>
        /// <param name="width">Ширина карти в клітинах (мінімум 1).</param>
        /// <param name="height">Висота карти в клітинах (мінімум 1).</param>
        public void Initialize(int width, int height)
        {
            bool wasInitialized = _initialized;
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            Debug.Log($"{DebugTag} FogService.Initialize begin requested={width}x{height}, wasInitialized={wasInitialized}, previous={_width}x{_height}, pendingReveals={_pendingRevealAreas.Count}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}.");

            _width  = width;
            _height = height;

            _visibilityCounters = new int[width, height];
            _exploredTiles      = new bool[width, height];
            _unitVisibleTiles.Clear();

            _initialized = true;

            // Відновити стан досліджених клітин із відкладеного сніпшоту (якщо завантаження відбулося до ініціалізації карти).
            var snapshot = _pendingExploredSnapshot ?? _saveProvider?.LoadExploredData();
            bool hasLoadedSnapshot = snapshot != null;
            Debug.Log($"{DebugTag} FogService.Initialize snapshot={(snapshot != null ? $"{snapshot.GetLength(0)}x{snapshot.GetLength(1)}" : "null")}.");
            if (snapshot != null)
                LoadFromSnapshot(snapshot);
            _pendingExploredSnapshot = null;

            ApplyPendingRevealAreas("Initialize");
            ApplyStartupFallbackRevealIfNeeded(hasLoadedSnapshot);

            // Обробити одиниці, які були створені/переміщені до ініціалізації карти
            if (_pendingUnits.Count > 0)
            {
                foreach (var kvp in _pendingUnits)
                    RegisterVisionArea(kvp.Key, kvp.Value.Position, kvp.Value.VisionRange, kvp.Value.Shape, kvp.Value.Modifiers);

                _pendingUnits.Clear();
            }
            else
            {
                RecalculateAllVisibility();
            }

            // Переконатися, що текстура відображає поточний стан після обробки відкладених одиниць
            _textureUpdater?.RebuildFullTexture(this);
            BumpVersion();
            Debug.Log($"{DebugTag} FogService.Initialize end map={_width}x{_height}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, pendingReveals={_pendingRevealAreas.Count}, version={Version}.");
        }

        /// <summary>
        /// Реєструє одиницю в системі туману з вказаною позицією та дальністю огляду.
        /// </summary>
        /// <param name="unitId">Унікальний ідентифікатор одиниці.</param>
        /// <param name="position">Позиція одиниці в координатах сітки.</param>
        /// <param name="visionRange">Дальність огляду в клітинах.</param>
        public void RegisterUnit(string unitId, Vector2Int position, int visionRange)
            => RegisterVisionArea(unitId, position, visionRange, null);

        /// <summary>
        /// Оновлює дальність огляду для одиниці. Якщо служба ще не ініціалізована,
        /// значення зберігається як відкладене для застосування пізніше.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці.</param>
        /// <param name="visionRange">Нова дальність огляду.</param>
        public void UpdateUnitVisionRange(string unitId, int visionRange)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            int clampedRange = ClampVisionRange(visionRange);

            if (!_initialized)
            {
                if (_pendingUnits.TryGetValue(unitId, out var pending))
                    _pendingUnits[unitId] = (pending.Position, clampedRange, pending.Shape, pending.Modifiers);
                else
                    _unitVisionRange[unitId] = clampedRange;
                return;
            }

            if (!_unitPositions.TryGetValue(unitId, out var position))
            {
                _unitVisionRange[unitId] = clampedRange;
                return;
            }

            if (_unitVisionRange.TryGetValue(unitId, out int current) && current == clampedRange)
                return;

            _unitVisionRange[unitId] = clampedRange;

            RemoveVisibleTiles(unitId);
            var tiles = ComputeVisibleTiles(unitId, position, clampedRange);
            _unitVisibleTiles[unitId] = tiles;

            foreach (var tile in tiles)
            {
                _visibilityCounters[tile.x, tile.y]++;
                _exploredTiles[tile.x, tile.y] = true;
                _lastDirtyTiles.Add(tile);
            }

            FlushTexture();
        }

        /// <summary>
        /// Реєструє фіксовану зону огляду (наприклад, для будівлі) з вказаною формою.
        /// </summary>
        /// <param name="areaId">Ідентифікатор зони.</param>
        /// <param name="position">Позиція центру зони.</param>
        /// <param name="visionRange">Радіус зони в клітинах.</param>
        /// <param name="shape">Форма відкриття.</param>
        public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
            => RegisterVisionArea(areaId, position, visionRange, shape);

        public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null)
        {
            radius = Mathf.Max(0, radius);

            if (!_initialized)
            {
                _pendingRevealAreas.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                Debug.Log($"{DebugTag} FogService.RevealArea queued-not-initialized center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, pending={_pendingRevealAreas.Count}.");
                return;
            }

            if (!RevealTouchesCurrentMap(center, radius))
            {
                _pendingRevealAreas.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                Debug.LogWarning($"{DebugTag} FogService.RevealArea queued-outside-current-map center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, currentMap={_width}x{_height}, pending={_pendingRevealAreas.Count}. This usually means startup reveal arrived before fog resized to the generated world.");
                return;
            }

            ApplyRevealArea(center, radius, shape, keepVisible, visibleAreaId);
        }

        private void ApplyRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId)
        {
            radius = Mathf.Max(0, radius);

            string areaId = null;
            bool removedOldVisibility = false;
            if (keepVisible)
            {
                areaId = ResolveRevealVisibilityAreaId(center, radius, shape, visibleAreaId);
                removedOldVisibility = RemoveVisibleTiles(areaId);
                _unitVisionRange.Remove(areaId);
                _unitPositions.Remove(areaId);
                _fixedVisionShapes.Remove(areaId);
                _unitVisionModifiers.Remove(areaId);
            }

            var tiles = ComputeShapeTiles(center, radius, shape);
            if (tiles.Count == 0)
            {
                Debug.LogWarning($"{DebugTag} FogService.ApplyRevealArea zero-tiles center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={areaId ?? visibleAreaId ?? "<explored-only>"}, map={_width}x{_height}.");
                if (removedOldVisibility)
                    FlushTexture();
                return;
            }

            if (keepVisible)
            {
                _unitVisionRange[areaId] = radius;
                _unitPositions[areaId] = center;
                _unitVisionModifiers[areaId] = default;
                _fixedVisionShapes[areaId] = shape;
                _unitVisibleTiles[areaId] = tiles;

                foreach (var tile in tiles)
                {
                    _visibilityCounters[tile.x, tile.y]++;
                    _exploredTiles[tile.x, tile.y] = true;
                    _lastDirtyTiles.Add(tile);
                }

                FlushTexture();
                Debug.Log($"{DebugTag} FogService.ApplyRevealArea visible center={center}, radius={radius}, shape={shape}, id={areaId}, tiles={tiles.Count}, map={_width}x{_height}, centerState={GetFogState(center)}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}.");
                return;
            }

            bool changed = false;
            foreach (var tile in tiles)
            {
                if (_exploredTiles[tile.x, tile.y])
                    continue;

                _exploredTiles[tile.x, tile.y] = true;
                _lastDirtyTiles.Add(tile);
                changed = true;
            }

            if (changed)
                FlushTexture();

            Debug.Log($"{DebugTag} FogService.ApplyRevealArea explored center={center}, radius={radius}, shape={shape}, tiles={tiles.Count}, changed={changed}, map={_width}x{_height}, centerState={GetFogState(center)}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}.");
        }

        private static string ResolveRevealVisibilityAreaId(Vector2Int center, int radius, FogRevealShape shape, string visibleAreaId)
            => !string.IsNullOrWhiteSpace(visibleAreaId)
                ? visibleAreaId
                : $"fog-reveal:{center.x}:{center.y}:{radius}:{(int)shape}";

        private bool RevealTouchesCurrentMap(Vector2Int center, int radius)
        {
            radius = Mathf.Max(0, radius);
            return center.x + radius >= 0
                && center.y + radius >= 0
                && center.x - radius < _width
                && center.y - radius < _height;
        }

        private void ApplyPendingRevealAreas(string reason)
        {
            if (_pendingRevealAreas.Count == 0)
                return;

            var reveals = _pendingRevealAreas.ToArray();
            _pendingRevealAreas.Clear();
            Debug.Log($"{DebugTag} FogService.ApplyPendingRevealAreas reason={reason}, count={reveals.Length}, map={_width}x{_height}.");

            for (int index = 0; index < reveals.Length; index++)
            {
                var reveal = reveals[index];
                ApplyRevealArea(reveal.Center, reveal.Radius, reveal.Shape, reveal.KeepVisible, reveal.VisibleAreaId);
            }
        }

        private void ApplyStartupFallbackRevealIfNeeded(bool hasLoadedSnapshot)
        {
            if (_settings == null || !_settings.EnableStartupFallbackReveal)
            {
                Debug.Log($"{DebugTag} FogService.StartupFallback skipped settingsDisabled={_settings == null || !_settings.EnableStartupFallbackReveal}.");
                return;
            }

            if (hasLoadedSnapshot || GameLaunchContext.IsAutoLoadEnabled())
            {
                Debug.Log($"{DebugTag} FogService.StartupFallback skipped loadContext hasSnapshot={hasLoadedSnapshot}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, mode={GameLaunchContext.Mode}.");
                return;
            }

            if (_pendingRevealAreas.Count > 0 || _unitPositions.Count > 0 || _fixedVisionShapes.Count > 0 || CountExploredTiles() > 0)
            {
                Debug.Log($"{DebugTag} FogService.StartupFallback skipped existingState pending={_pendingRevealAreas.Count}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, explored={CountExploredTiles()}.");
                return;
            }

            int radius = Mathf.Max(1, _settings.StartupFallbackRevealRadius);
            var center = PickStartupFallbackCenter();
            var shape = _settings.StartupFallbackRevealShape;
            Debug.LogWarning($"{DebugTag} FogService.StartupFallback applying center={center}, radius={radius}, shape={shape}, map={_width}x{_height}, mode={GameLaunchContext.Mode}. Bootstrap reveal did not arrive before fog init.");
            ApplyRevealArea(center, radius, shape, true, StartupFallbackRevealAreaId);
        }

        private Vector2Int PickStartupFallbackCenter()
        {
            int minSide = Mathf.Min(_width, _height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(_settings != null ? _settings.StartupFallbackRelativeMarginFactor : 0.1667f));
            int margin = Mathf.Max(_settings != null ? _settings.StartupFallbackMinMarginFromBorder : 5, relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, Mathf.Max(0, _width - 1));
            int xMax = Mathf.Clamp(_width - margin - 1, xMin, Mathf.Max(0, _width - 1));
            int yMin = Mathf.Clamp(margin, 0, Mathf.Max(0, _height - 1));
            int yMax = Mathf.Clamp(_height - margin - 1, yMin, Mathf.Max(0, _height - 1));

            return new Vector2Int(
                UnityEngine.Random.Range(xMin, xMax + 1),
                UnityEngine.Random.Range(yMin, yMax + 1));
        }

        /// <summary>
        /// Реєструє зону огляду для заданого ідентифікатора (одиниці або фіксованої зони).
        ///
        /// Алгоритм (сумарно):
        /// 1) ігнорує порожній `unitId`;
        /// 2) якщо служба ще не ініціалізована — зберігає дані у `_pendingUnits`;
        /// 3) видаляє попередні видимі клітини для цього `unitId`;
        /// 4) зберігає дальність огляду, позицію та модифікатори;
        /// 5) обчислює початковий набір видимих клітин і збільшує лічильники;
        /// 6) позначає клітини як досліджені, додає їх до `_lastDirtyTiles`;
        /// 7) викликає `FlushTexture()` для оновлення текстури туману.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці або зони.</param>
        /// <param name="position">Центральна позиція зони.</param>
        /// <param name="visionRange">Дальність огляду (в клітинах).</param>
        /// <param name="shape">Опціональна фіксована форма відкриття.</param>
        /// <param name="modifiers">Модифікатори огляду одиниці.</param>
        private void RegisterVisionArea(string unitId, Vector2Int position, int visionRange, FogRevealShape? shape, FogVisionModifiers modifiers = default)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (!_initialized)
            {
                _pendingUnits[unitId] = (position, visionRange, shape, modifiers);
                return;
            }

            RemoveVisibleTiles(unitId);

            visionRange = ClampVisionRange(visionRange);
            _unitVisionRange[unitId] = visionRange;
            _unitPositions[unitId] = position;
            _unitVisionModifiers[unitId] = modifiers;

            if (shape.HasValue)
                _fixedVisionShapes[unitId] = shape.Value;
            else
                _fixedVisionShapes.Remove(unitId);

            var tiles = ComputeInitialVisibleTiles(unitId, position, visionRange);
            _unitVisibleTiles[unitId] = tiles;

            foreach (var t in tiles)
            {
                _visibilityCounters[t.x, t.y]++;
                _exploredTiles[t.x, t.y] = true;
                _lastDirtyTiles.Add(t);
            }

            FlushTexture();
        }

        /// <summary>
        /// Оновлює позицію одиниці та перераховує її видимі клітини.
        /// Якщо служба ще не ініціалізована — оновлення зберігається як відкладене.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці.</param>
        /// <param name="newPosition">Нова позиція в координатах сітки.</param>
        public void UpdateUnitPosition(string unitId, Vector2Int newPosition)
        {
            if (!_initialized)
            {
                int pendingRange = _unitVisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;

                FogRevealShape? shape = _fixedVisionShapes.TryGetValue(unitId, out var storedShape)
                    ? storedShape
                    : null;
                _pendingUnits[unitId] = (newPosition, pendingRange, shape, ResolveUnitVisionModifiers(unitId));
                return;
            }

            if (!_unitVisibleTiles.TryGetValue(unitId, out var oldTiles))
            {
                int fallbackRange = _unitVisionRange.TryGetValue(unitId, out int storedRange)
                    ? storedRange
                    : _defaultVisionRange;

                RegisterVisionArea(unitId, newPosition, fallbackRange, null, ResolveUnitVisionModifiers(unitId));
                return;
            }

            // Зменшення лічильників для старих клітин
            foreach (var t in oldTiles)
            {
                _visibilityCounters[t.x, t.y] = Mathf.Max(0, _visibilityCounters[t.x, t.y] - 1);
                _lastDirtyTiles.Add(t);
            }

            // Обчислення нових видимих клітин
            int range = _unitVisionRange.TryGetValue(unitId, out int r) ? r : _defaultVisionRange;
            _unitPositions[unitId] = newPosition;
            var newTiles = ComputeVisibleTiles(unitId, newPosition, range);
            _unitVisibleTiles[unitId] = newTiles;

            // Збільшення лічильників для нових клітин
            foreach (var t in newTiles)
            {
                _visibilityCounters[t.x, t.y]++;
                _exploredTiles[t.x, t.y] = true;
                _lastDirtyTiles.Add(t);
            }

            FlushTexture();
        }

        /// <summary>
        /// Видаляє одиницю з системи туману, зменшуючи відповідні лічильники видимості.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці.</param>
        public void UnregisterUnit(string unitId)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            if (!_initialized)
            {
                _pendingUnits.Remove(unitId);
                _unitVisionRange.Remove(unitId);
                _unitPositions.Remove(unitId);
                _fixedVisionShapes.Remove(unitId);
                _unitVisionModifiers.Remove(unitId);
                return;
            }

            if (!RemoveVisibleTiles(unitId))
                return;

            _unitVisionRange.Remove(unitId);
            _unitPositions.Remove(unitId);
            _fixedVisionShapes.Remove(unitId);
            _unitVisionModifiers.Remove(unitId);

            FlushTexture();
        }

        /// <summary>
        /// Повертає поточний стан туману для вказаної клітини: Visible, Explored або Unexplored.
        /// </summary>
        /// <param name="position">Координати клітини.</param>
        /// <returns>Стан туману для клітини.</returns>
        public FogStateType GetFogState(Vector2Int position)
        {
            if (!_initialized || !IsInBounds(position))
                return FogStateType.Unexplored;

            if (_visibilityCounters[position.x, position.y] >= 1)
                return FogStateType.Visible;

            if (_exploredTiles[position.x, position.y])
                return FogStateType.Explored;

            return FogStateType.Unexplored;
        }

        /// <summary>
        /// Чи видима клітина зараз (має лічильник видимості >= 1).
        /// </summary>
        /// <param name="position">Координати клітини.</param>
        /// <returns>True якщо клітина видима, інакше false.</returns>
        public bool IsVisible(Vector2Int position)
            => _initialized && IsInBounds(position) && _visibilityCounters[position.x, position.y] >= 1;

        /// <summary>
        /// Чи була клітина колись досліджена.
        /// </summary>
        /// <param name="position">Координати клітини.</param>
        /// <returns>True якщо клітина позначена як досліджена.</returns>
        public bool IsExplored(Vector2Int position)
            => _initialized && IsInBounds(position) && _exploredTiles[position.x, position.y];

        /// <summary>
        /// Повертає копію поточного масиву позначок досліджених клітин.
        /// Якщо служба не ініціалізована, повертає відкладений сніпшот (якщо є).
        /// </summary>
        /// <returns>Двовимірний булевий масив ширини `_width` × `_height`, або null якщо немає даних.</returns>
        public bool[,] GetExploredSnapshot()
        {
            if (!_initialized)
                return _pendingExploredSnapshot != null
                    ? CloneSnapshot(_pendingExploredSnapshot)
                    : null;

            var snap = new bool[_width, _height];
            System.Array.Copy(_exploredTiles, snap, _exploredTiles.Length);

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    if (_visibilityCounters[x, y] > 0)
                        snap[x, y] = true;

            return snap;
        }

        /// <summary>
        /// Завантажує стан досліджених клітин з переданого сніпшоту.
        /// Якщо служба ще не ініціалізована — зберігає сніпшот як відкладений для застосування при ініціалізації.
        /// </summary>
        /// <param name="explored">Джерельний масив досліджених клітин.</param>
        public void LoadFromSnapshot(bool[,] explored)
        {
            if (explored == null) return;

            if (!_initialized)
            {
                _pendingExploredSnapshot = CloneSnapshot(explored);
                return;
            }

            int w = explored.GetLength(0);
            int h = explored.GetLength(1);
            int copyW = Mathf.Min(w, _width);
            int copyH = Mathf.Min(h, _height);

            Array.Clear(_exploredTiles, 0, _exploredTiles.Length);

            for (int x = 0; x < copyW; x++)
                for (int y = 0; y < copyH; y++)
                    _exploredTiles[x, y] = explored[x, y];

            _textureUpdater?.RebuildFullTexture(this);
            BumpVersion();
        }

        internal IReadOnlyList<FogFixedVisionAreaSnapshot> GetFixedVisionAreasSnapshot()
        {
            var snapshot = new List<FogFixedVisionAreaSnapshot>(_fixedVisionShapes.Count);
            foreach (var shapePair in _fixedVisionShapes)
            {
                string areaId = shapePair.Key;
                if (string.IsNullOrWhiteSpace(areaId))
                    continue;

                if (!_unitPositions.TryGetValue(areaId, out Vector2Int position))
                    continue;

                if (!_unitVisionRange.TryGetValue(areaId, out int visionRange))
                    continue;

                snapshot.Add(new FogFixedVisionAreaSnapshot(areaId, position, visionRange, shapePair.Value));
            }

            return snapshot;
        }

        internal void LoadFixedVisionAreasSnapshot(IReadOnlyList<FogFixedVisionAreaSnapshot> areas)
        {
            if (areas == null || areas.Count == 0)
                return;

            for (int index = 0; index < areas.Count; index++)
            {
                var area = areas[index];
                if (string.IsNullOrWhiteSpace(area.AreaId) || area.VisionRange <= 0)
                    continue;

                RegisterFixedVisionArea(area.AreaId, area.Position, area.VisionRange, area.Shape);
            }
        }

        /// <summary>
        /// Повертає колекцію тайлів, які були змінені останніми і потребують оновлення текстури.
        /// </summary>
        /// <returns>Колекція координат змінених клітин.</returns>
        public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles()
            => _lastDirtyTiles;

        // ─── Обробники сигналів ───────────────────────────────────────────────

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            int requestedRange = signal.VisionRange > 0 ? signal.VisionRange : _defaultVisionRange;
            var modifiers = signal.HasCustomVisionModifiers
                ? new FogVisionModifiers(signal.CanSeeCrest, signal.CrestVisibilityFactor, signal.DownSlopeVisionBonus, signal.SilhouettePenalty)
                : default;
            RegisterVisionArea(signal.UnitId, signal.Position, ClampVisionRange(requestedRange), null, modifiers);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
            => UpdateUnitPosition(signal.UnitId, signal.NewPosition);

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
            => UnregisterUnit(signal.UnitId);

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            int requestedRange = _defaultVisionRange;
            RegisterFixedVisionArea(GetBuildingVisionAreaId(signal.Position), signal.Position, requestedRange, FogRevealShape.PixelCircle);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
            => UnregisterUnit(GetBuildingVisionAreaId(signal.Position));

        private void OnWorldGeneratedData(WorldGeneratedDataSignal signal)
        {
            _resolver.SetHeightMap(BuildVisibilityHeightMap(signal.TerrainLevelMap, signal.HeightMap));

            Vector2Int baseMapSize = FogWorldSignalUtility.ResolveBaseMapSize(signal);
            int signalWidth = baseMapSize.x;
            int signalHeight = baseMapSize.y;
            Debug.Log($"{DebugTag} FogService.OnWorldGeneratedData signal={signal.Width}x{signal.Height}, baseMap={signalWidth}x{signalHeight}, initialized={_initialized}, current={_width}x{_height}, pendingReveals={_pendingRevealAreas.Count}.");

            if (!_initialized)
            {
                Initialize(signalWidth, signalHeight);
                return;
            }

            if (_width != signalWidth || _height != signalHeight)
                ResizeToWorldDimensions(signalWidth, signalHeight);

            ApplyPendingRevealAreas("WorldGeneratedData");
            RecalculateAllVisibility();
            Debug.Log($"{DebugTag} FogService.OnWorldGeneratedData end map={_width}x{_height}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, pendingReveals={_pendingRevealAreas.Count}.");
        }

        private static float[,] BuildVisibilityHeightMap(int[,] terrainLevelMap, float[,] fallbackHeightMap)
        {
            if (terrainLevelMap == null)
                return fallbackHeightMap;

            int width = terrainLevelMap.GetLength(0);
            int height = terrainLevelMap.GetLength(1);
            var heightMap = new float[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                heightMap[x, y] = Mathf.Max(0, terrainLevelMap[x, y]);

            return heightMap;
        }

        // ─── Допоміжні методи ─────────────────────────────────────────────────

        /// <summary>
        /// Обмежує значення дальності огляду у межах, визначених налаштуваннями або значеннями за замовчуванням.
        /// </summary>
        private int ClampVisionRange(int range)
        {
            int min = _settings != null ? _settings.MinVisionRange : 1;
            int max = _settings != null ? _settings.MaxVisionRange : 12;
            return Mathf.Clamp(range, min, max);
        }

        /// <summary>
        /// Перевіряє, чи знаходиться позиція в межах поточних розмірів карти.
        /// </summary>
        private bool IsInBounds(Vector2Int pos)
            => pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;

        /// <summary>
        /// Видаляє запис про видимі клітини для `unitId` і зменшує відповідні лічильники видимості.
        /// Після зменшення лічильників додає клітини до `_lastDirtyTiles` для подальшого оновлення текстури.
        /// Повертає true якщо були знайдені та видалені клітини для цього `unitId`.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці або зони.</param>
        /// <returns>True якщо існували видимі клітини для цього `unitId`.</returns>
        private bool RemoveVisibleTiles(string unitId)
        {
            if (!_unitVisibleTiles.TryGetValue(unitId, out var tiles))
                return false;

            foreach (var tile in tiles)
            {
                _visibilityCounters[tile.x, tile.y] = Mathf.Max(0, _visibilityCounters[tile.x, tile.y] - 1);
                _lastDirtyTiles.Add(tile);
            }

            _unitVisibleTiles.Remove(unitId);
            return true;
        }

        /// <summary>
        /// Формує унікальний ідентифікатор фіксованої зони огляду для будівлі на вказаній позиції.
        /// Використовується для реєстрації/видалення фіксованих зон (будівель).
        /// </summary>
        private static string GetBuildingVisionAreaId(Vector2Int position)
            => $"{BuildingVisionAreaPrefix}{position.x}:{position.y}";

        /// <summary>
        /// Обчислює набір клітин, що покривають круглу область у піксельній інтерпретації.
        /// Це обгортка над <see cref="ComputeShapeTiles"/> з параметром PixelCircle.
        /// </summary>
        private IReadOnlyList<Vector2Int> ComputePixelCircleTiles(Vector2Int origin, int radius)
            => ComputeShapeTiles(origin, radius, FogRevealShape.PixelCircle);

        /// <summary>
        /// Обчислює набір клітин, що лежать всередині форми (square/diamond/circle) навколо центру.
        ///
        /// Опис логіки:
        /// - Перебирає зміщення dx/dy у межах радіусу (O(radius^2)).
        /// - Використовує <see cref="IsInsideShape"/> для перевірки приналежності точки до форми.
        /// - Додає клітину в результат тільки якщо вона в межах карти (<see cref="IsInBounds"/>).
        /// </summary>
        /// <param name="origin">Центр області в координатах сітки.</param>
        /// <param name="radius">Радіус області (в клітинах).</param>
        /// <param name="shape">Форма відкриття (квадрат, ромб, піксельне коло).</param>
        /// <returns>Список координат клітин, що належать формі.</returns>
        private IReadOnlyList<Vector2Int> ComputeShapeTiles(Vector2Int origin, int radius, FogRevealShape shape)
        {
            var result = new List<Vector2Int>();
            int safeRadius = Mathf.Max(0, radius);
            float radiusWithCellCoverage = safeRadius + 0.5f;
            float sqrRadius = radiusWithCellCoverage * radiusWithCellCoverage;

            for (int dx = -safeRadius; dx <= safeRadius; dx++)
            {
                for (int dy = -safeRadius; dy <= safeRadius; dy++)
                {
                    if (!IsInsideShape(dx, dy, safeRadius, sqrRadius, shape))
                        continue;

                    var tile = new Vector2Int(origin.x + dx, origin.y + dy);
                    if (IsInBounds(tile))
                        result.Add(tile);
                }
            }

            return result;
        }

        /// <summary>
        /// Перевіряє, чи точка (dx,dy) лежить усередині заданої форми з радіусом.
        /// Підтримувані форми: Square, Diamond, PixelCircle (за замовчуванням коло).
        /// </summary>
        private static bool IsInsideShape(int dx, int dy, int radius, float sqrRadius, FogRevealShape shape)
        {
            return shape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= radius,
                _ => dx * dx + dy * dy <= sqrRadius,
            };
        }

        /// <summary>
        /// Обчислює початковий набір видимих клітин для одиниці при її реєстрації.
        /// За поточною логікою — делегує виконання в <see cref="ComputeVisibleTiles"/>.
        /// </summary>
        private IReadOnlyList<Vector2Int> ComputeInitialVisibleTiles(string unitId, Vector2Int position, int range)
        {
            return ComputeVisibleTiles(unitId, position, range);
        }

        /// <summary>
        /// Обчислює набір клітин, які бачить спостерігач `unitId` з позиції `position` та радіусу `range`.
        /// Порядок дій:
        /// - Якщо для `unitId` задана фіксована форма — використовує <see cref="ComputeShapeTiles"/>;
        /// - Інакше отримує модифікатори (силуети тощо) та делегує обчислення в `_resolver`;
        /// - Після базового набору додає цільові тайли силуетів через <see cref="AddSilhouetteTargetTiles"/>.
        /// </summary>
        private IReadOnlyList<Vector2Int> ComputeVisibleTiles(string unitId, Vector2Int position, int range)
        {
            if (_fixedVisionShapes.TryGetValue(unitId, out var shape))
                return ComputeShapeTiles(position, range, shape);

            var modifiers = ResolveUnitVisionModifiers(unitId);
            var tiles = _resolver.ComputeVisibleTiles(position, range, _width, _height, modifiers);
            return AddSilhouetteTargetTiles(unitId, position, range, modifiers, tiles);
        }

        /// <summary>
        /// Додає до набору видимих клітин потенційні цілі-силуети інших одиниць,
        /// які можуть бути помітні завдяки рельєфу (height-aware visibility).
        ///
        /// Алгоритм:
        /// - Якщо немає <see cref="IHeightAwareVisionService"/> або немає інших одиниць — повертає вхідний набір;
        /// - Обчислює `searchRadius` через height-vision сервіс та поріг видимості з налаштувань;
        /// - Перебирає інші одиниці: ігнорує сам спостерігач, одиниці поза межами карти або з нульовим штрафом силуету;
        /// - Якщо ціль в межах `searchRadius`, оцінює фактор видимості через <see cref="IHeightAwareVisionService.GetVisibilityFactor"/>;
        /// - Якщо фактор >= порогу — додає позицію цілі в набір видимих клітин.
        ///
        /// Повертає початковий `sourceTiles`, якщо жодна додаткова ціль не була додана.
        /// </summary>
        private IReadOnlyList<Vector2Int> AddSilhouetteTargetTiles(string observerUnitId, Vector2Int observerPosition, int range, FogVisionModifiers observerModifiers, IReadOnlyList<Vector2Int> sourceTiles)
        {
            if (_heightVisionService == null || _unitPositions.Count <= 1)
                return sourceTiles;

            int maxRange = _settings != null ? _settings.MaxVisionRange : 12;
            int searchRadius = _heightVisionService.GetSearchRadius(observerPosition, range, maxRange, observerModifiers);
            float threshold = _settings != null ? Mathf.Clamp(_settings.TerrainVisibilityThreshold, 0.01f, 1f) : 0.5f;
            HashSet<Vector2Int> visible = null;

            foreach (var targetEntry in _unitPositions)
            {
                if (string.Equals(targetEntry.Key, observerUnitId, StringComparison.Ordinal))
                    continue;

                var targetModifiers = ResolveUnitVisionModifiers(targetEntry.Key);
                if (targetModifiers.EffectiveSilhouettePenalty <= 0f)
                    continue;

                Vector2Int targetPosition = targetEntry.Value;
                if (!IsInBounds(targetPosition))
                    continue;

                int distance = Mathf.Max(Mathf.Abs(targetPosition.x - observerPosition.x), Mathf.Abs(targetPosition.y - observerPosition.y));
                if (distance > searchRadius)
                    continue;

                visible ??= new HashSet<Vector2Int>(sourceTiles);
                if (visible.Contains(targetPosition))
                    continue;

                float visibility = _heightVisionService.GetVisibilityFactor(observerPosition, targetPosition, range, maxRange, observerModifiers, targetModifiers);
                if (visibility >= threshold)
                    visible.Add(targetPosition);
            }

            return visible == null || visible.Count == sourceTiles.Count
                ? sourceTiles
                : new List<Vector2Int>(visible);
        }

        /// <summary>
        /// Повертає збережені модифікатори видимості для одиниці або значення за замовчуванням.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці.</param>
        /// <returns>Модифікатори видимості для одиниці.</returns>
        /// <summary>
        /// Повертає збережені модифікатори видимості для одиниці або значення за замовчуванням.
        /// </summary>
        /// <param name="unitId">Ідентифікатор одиниці.</param>
        /// <returns>Модифікатори видимості для одиниці.</returns>
        private FogVisionModifiers ResolveUnitVisionModifiers(string unitId)
            => !string.IsNullOrWhiteSpace(unitId) && _unitVisionModifiers.TryGetValue(unitId, out var modifiers)
                ? modifiers
                : default;

        /// <summary>
        /// Робить глибоку копію двовимірного булевого масиву (сніпшоту досліджених клітин).
        /// Використовується для збереження відкладеного сніпшоту без шарінгу посилань.
        /// </summary>
        /// <summary>
        /// Робить глибоку копію двовимірного булевого масиву (сніпшоту досліджених клітин).
        /// Використовується для збереження відкладеного сніпшоту без шарінгу посилань.
        /// </summary>
        private static bool[,] CloneSnapshot(bool[,] source)
        {
            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var copy = new bool[w, h];

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    copy[x, y] = source[x, y];

            return copy;
        }

        /// <summary>
        /// Оновлює текстуру туману для накопичених змінених клітин.
        /// Викликає <see cref="IFogTextureUpdater.UpdateDirtyTiles"/>, інкрементує версію якщо були зміни
        /// та очищає буфер `_lastDirtyTiles`.
        /// </summary>
        /// <summary>
        /// Оновлює текстуру туману для накопичених змінених клітин.
        /// Викликає <see cref="IFogTextureUpdater.UpdateDirtyTiles"/>, інкрементує версію якщо були зміни
        /// та очищає буфер `_lastDirtyTiles`.
        /// </summary>
        private void FlushTexture()
        {
            int dirtyCount = _lastDirtyTiles.Count;
            if (_textureUpdater != null)
                _textureUpdater.UpdateDirtyTiles(this, _lastDirtyTiles);

            if (dirtyCount > 0)
                BumpVersion();

            _lastDirtyTiles.Clear();
        }

        /// <summary>
        /// Повністю перераховує матрицю видимості на основі поточної інформації про позиції одиниць.
        /// Використовується після масових змін або при ініціалізації/змінних розмірах карти.
        /// Кроки:
        /// - Очищає лічильники видимості;
        /// - Для кожної зареєстрованої одиниці обчислює видимі клітини і інкрементує лічильники;
        /// - Позначає ці клітини як досліджені;
        /// - Перебудовує повну текстуру та інкрементує версію.
        /// </summary>
        /// <summary>
        /// Повністю перераховує матрицю видимості на основі поточної інформації про позиції одиниць.
        /// Використовується після масових змін або при ініціалізації/змінних розмірах карти.
        /// Кроки:
        /// - Очищає лічильники видимості;
        /// - Для кожної зареєстрованої одиниці обчислює видимі клітини і інкрементує лічильники;
        /// - Позначає ці клітини як досліджені;
        /// - Перебудовує повну текстуру та інкрементує версію.
        /// </summary>
        private void RecalculateAllVisibility()
        {
            if (!_initialized)
                return;

            Array.Clear(_visibilityCounters, 0, _visibilityCounters.Length);
            _unitVisibleTiles.Clear();

            foreach (var unitEntry in _unitPositions)
            {
                if (!_unitVisionRange.TryGetValue(unitEntry.Key, out int range))
                    range = _defaultVisionRange;

                var visibleTiles = ComputeVisibleTiles(unitEntry.Key, unitEntry.Value, range);
                _unitVisibleTiles[unitEntry.Key] = visibleTiles;

                foreach (var tile in visibleTiles)
                {
                    _visibilityCounters[tile.x, tile.y]++;
                    _exploredTiles[tile.x, tile.y] = true;
                }
            }

            _textureUpdater?.RebuildFullTexture(this);
            BumpVersion();
            _lastDirtyTiles.Clear();
            Debug.Log($"{DebugTag} FogService.RecalculateAllVisibility map={_width}x{_height}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, version={Version}.");
        }

        /// <summary>
        /// Інкрементує внутрішню версію стану туману (не кидає виключень при переповненні).
        /// Використовується для повідомлення зовнішніх сервісів про зміну стану.
        /// </summary>
        /// <summary>
        /// Інкрементує внутрішню версію стану туману (не кидає виключень при переповненні).
        /// Використовується для повідомлення зовнішніх сервісів про зміну стану.
        /// </summary>
        private void BumpVersion()
        {
            unchecked
            {
                Version++;
            }
        }

        /// <summary>
        /// Змінює внутрішні розміри карти. Зберігає поточний сніпшот досліджених клітин
        /// та застосовує його після ресайзу (щоб не втратити інформацію про досліджені клітини).
        /// </summary>
        /// <param name="width">Нова ширина карти.</param>
        /// <param name="height">Нова висота карти.</param>
        /// <summary>
        /// Змінює внутрішні розміри карти. Зберігає поточний сніпшот досліджених клітин
        /// та застосовує його після ресайзу (щоб не втратити інформацію про досліджені клітини).
        /// </summary>
        /// <param name="width">Нова ширина карти.</param>
        /// <param name="height">Нова висота карти.</param>
        private void ResizeToWorldDimensions(int width, int height)
        {
            var exploredSnapshot = GetExploredSnapshot();
            Debug.Log($"{DebugTag} FogService.ResizeToWorldDimensions from={_width}x{_height} to={Mathf.Max(1, width)}x{Mathf.Max(1, height)}, snapshot={(exploredSnapshot != null ? $"{exploredSnapshot.GetLength(0)}x{exploredSnapshot.GetLength(1)}" : "null")}.");

            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _visibilityCounters = new int[_width, _height];
            _exploredTiles = new bool[_width, _height];
            _unitVisibleTiles.Clear();
            _lastDirtyTiles.Clear();

            if (exploredSnapshot != null)
                LoadFromSnapshot(exploredSnapshot);
        }

        private int CountVisibleTiles()
        {
            if (!_initialized || _visibilityCounters == null)
                return 0;

            int count = 0;
            for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                if (_visibilityCounters[x, y] > 0)
                    count++;

            return count;
        }

        private int CountExploredTiles()
        {
            if (!_initialized || _exploredTiles == null)
                return 0;

            int count = 0;
            for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                if (_exploredTiles[x, y] || _visibilityCounters[x, y] > 0)
                    count++;

            return count;
        }
    }

    /// <summary>
    /// Допоміжний оцінювач, що визначає чи потрібно рендерити об'єкт
    /// залежно від покриття клітин туманом.
    /// </summary>
    internal static class FogRendererCullingEvaluator
    {
        private const float BoundsEdgeEpsilon = 0.001f;

        /// <summary>
        /// Перевіряє, чи варто рендерити об'єкт з огляду на стан туману в області його меж.
        /// Повертає true якщо хоча б одна клітина в області не є невідкритою (Unexplored).
        /// </summary>
        public static bool ShouldRender(Bounds worldBounds, IFogOfWarService fogService, IGridService gridService, float boundsPaddingCells, IGridProjection gridProjection = null)
        {
            if (fogService == null || gridService == null)
                return true;

            if (!TryGetCoveredTileRange(worldBounds, gridService, boundsPaddingCells, out var min, out var max, gridProjection))
                return true;

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    if (fogService.GetFogState(new Vector2Int(x, y)) != FogStateType.Unexplored)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Обчислює діапазон клітин, що покриває передані світові межі, з урахуванням паддінгу.
        /// Повертає false якщо область повністю виходить за межі сітки.
        /// </summary>
        internal static bool TryGetCoveredTileRange(
            Bounds worldBounds,
            IGridService gridService,
            float boundsPaddingCells,
            out Vector2Int min,
            out Vector2Int max,
            IGridProjection gridProjection = null)
        {
            min = default;
            max = default;

            if (gridService == null || gridService.GridWidth <= 0 || gridService.GridHeight <= 0)
                return false;

            if (gridProjection != null)
                return TryGetProjectedCoveredTileRange(worldBounds, gridService, boundsPaddingCells, gridProjection, out min, out max);

            float padding = Mathf.Max(0f, boundsPaddingCells);
            int rawMinX = Mathf.FloorToInt(worldBounds.min.x + 0.5f - padding);
            int rawMinY = Mathf.FloorToInt(worldBounds.min.y + 0.5f - padding);
            int rawMaxX = Mathf.FloorToInt(worldBounds.max.x + 0.5f - BoundsEdgeEpsilon + padding);
            int rawMaxY = Mathf.FloorToInt(worldBounds.max.y + 0.5f - BoundsEdgeEpsilon + padding);

            if (rawMaxX < 0 || rawMaxY < 0 || rawMinX >= gridService.GridWidth || rawMinY >= gridService.GridHeight)
                return false;

            min = new Vector2Int(
                Mathf.Clamp(rawMinX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMinY, 0, gridService.GridHeight - 1));

            max = new Vector2Int(
                Mathf.Clamp(rawMaxX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMaxY, 0, gridService.GridHeight - 1));

            return min.x <= max.x && min.y <= max.y;
        }

        private static bool TryGetProjectedCoveredTileRange(
            Bounds worldBounds,
            IGridService gridService,
            float boundsPaddingCells,
            IGridProjection gridProjection,
            out Vector2Int min,
            out Vector2Int max)
        {
            min = default;
            max = default;

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            Vector3 boundsMin = worldBounds.min;
            Vector3 boundsMax = worldBounds.max;
            for (int xIndex = 0; xIndex < 2; xIndex++)
            for (int yIndex = 0; yIndex < 2; yIndex++)
            for (int zIndex = 0; zIndex < 2; zIndex++)
            {
                var corner = new Vector3(
                    xIndex == 0 ? boundsMin.x : boundsMax.x,
                    yIndex == 0 ? boundsMin.y : boundsMax.y,
                    zIndex == 0 ? boundsMin.z : boundsMax.z);
                Vector2Int grid = gridProjection.WorldToGrid(corner);
                minX = Mathf.Min(minX, grid.x);
                minY = Mathf.Min(minY, grid.y);
                maxX = Mathf.Max(maxX, grid.x);
                maxY = Mathf.Max(maxY, grid.y);
            }

            int padding = Mathf.CeilToInt(Mathf.Max(0f, boundsPaddingCells));
            int rawMinX = minX - padding;
            int rawMinY = minY - padding;
            int rawMaxX = maxX + padding;
            int rawMaxY = maxY + padding;

            if (rawMaxX < 0 || rawMaxY < 0 || rawMinX >= gridService.GridWidth || rawMinY >= gridService.GridHeight)
                return false;

            min = new Vector2Int(
                Mathf.Clamp(rawMinX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMinY, 0, gridService.GridHeight - 1));

            max = new Vector2Int(
                Mathf.Clamp(rawMaxX, 0, gridService.GridWidth - 1),
                Mathf.Clamp(rawMaxY, 0, gridService.GridHeight - 1));

            return min.x <= max.x && min.y <= max.y;
        }
    }

    /// <summary>
    /// Служба, яка керує відсіюванням (culling) рендерерів залежно від стану туману.
    /// Відновлює список рендерерів у сцені та поступово оцінює, чи слід їх показувати.
    /// </summary>
    internal sealed class FogRendererCullingService : IInitializable, ITickable, IDisposable
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

        private readonly List<CullableRenderer> _renderers = new List<CullableRenderer>();
        private readonly Dictionary<Renderer, CullableRenderer> _tracked = new Dictionary<Renderer, CullableRenderer>();
        private readonly Dictionary<string, GameObject> _unitObjects = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Transform> _worldRoots = new Dictionary<string, Transform>();
        private readonly HashSet<Renderer> _discoveredRenderers = new HashSet<Renderer>();
        private readonly List<Renderer> _rendererDiscoveryBuffer = new List<Renderer>(512);

        private bool _discoveryRequested = true;
        private bool _evaluationPending;
        private int _cursor;
        private int _lastFogVersion = -1;
        private float _nextDiscoveryAt;

        public FogRendererCullingService(
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

                var entry = new CullableRenderer(renderer);
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
        /// які знаходяться в батьківському `FogQuadController`.
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

            return renderer.GetComponentInParent<FogQuadController>() == null;
        }

        private sealed class CullableRenderer
        {
            public readonly Renderer Renderer;
            private bool _hiddenByFog;
            private bool _enabledBeforeFog;

            /// <summary>
            /// Обгортка для Renderer, яка зберігає попередній стан `enabled` і дозволяє тимчасово сховати рендерер.
            /// </summary>
            /// <param name="renderer">Рендерер для обгортки.</param>
            public CullableRenderer(Renderer renderer)
            {
                Renderer = renderer;
                _enabledBeforeFog = renderer != null && renderer.enabled;
            }

            /// <summary>
            /// Встановлює стан невидимості рендерера через туман.
            /// Якщо <paramref name="hidden"/> = true — вимикає рендерер і зберігає попередній стан.
            /// Інакше — відновлює попередній стан через <see cref="Restore"/>.
            /// </summary>
            /// <param name="hidden">Показувати (false) або приховувати (true) рендерер.</param>
            public void SetHiddenByFog(bool hidden)
            {
                if (Renderer == null)
                    return;

                if (hidden)
                {
                    if (!_hiddenByFog)
                    {
                        _enabledBeforeFog = Renderer.enabled;
                        _hiddenByFog = true;
                    }

                    Renderer.enabled = false;
                    return;
                }

                Restore();
            }

            /// <summary>
            /// Відновлює стан рендерера до того, що був до сховування туманом.
            /// </summary>
            public void Restore()
            {
                if (!_hiddenByFog || Renderer == null)
                    return;

                Renderer.enabled = _enabledBeforeFog;
                _hiddenByFog = false;
            }
        }
    }

    internal static class FogWorldSignalUtility
    {
        public static bool TryResolveMapWorldBounds(WorldGeneratedDataSignal signal, out Bounds bounds)
        {
            bounds = default;
            if (!signal.HasMapWorldBounds
                || !IsFinite(signal.MapWorldBoundsCenter)
                || !IsFinite(signal.MapWorldBoundsSize))
            {
                return false;
            }

            Vector3 size = new Vector3(
                Mathf.Abs(signal.MapWorldBoundsSize.x),
                Mathf.Abs(signal.MapWorldBoundsSize.y),
                Mathf.Abs(signal.MapWorldBoundsSize.z));
            if (size.x <= 0.0001f || size.z <= 0.0001f)
                return false;

            bounds = new Bounds(signal.MapWorldBoundsCenter, size);
            return true;
        }

        public static Vector2Int ResolveBaseMapSize(WorldGeneratedDataSignal signal)
        {
            int width = Mathf.Max(0, signal.Width);
            int height = Mathf.Max(0, signal.Height);

            ApplyBaseMapSize(signal.TileMap, ref width, ref height);
            ApplyBaseMapSize(signal.ObjectMap, ref width, ref height);
            ApplyBaseMapSize(signal.HeightMap, ref width, ref height);
            ApplyBaseMapSize(signal.TerrainLevelMap, ref width, ref height);

            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }

        private static void ApplyBaseMapSize<T>(T[,] map, ref int width, ref int height)
        {
            if (map == null)
                return;

            int mapWidth = map.GetLength(0);
            int mapHeight = map.GetLength(1);
            if (mapWidth <= 0 || mapHeight <= 0)
                return;

            width = width > 0 ? Mathf.Min(width, mapWidth) : mapWidth;
            height = height > 0 ? Mathf.Min(height, mapHeight) : mapHeight;
        }

        private static bool IsFinite(Vector3 value)
            => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
