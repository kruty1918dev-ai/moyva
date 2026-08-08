using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Combat.Runtime;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Серверний сервіс здоров'я будівель.
    ///
    /// Реєструє <see cref="IHealth"/> для кожної будівлі при отриманні <see cref="BuildingPlacedSignal"/>
    /// та видаляє при знищенні (через <see cref="IHealth.OnDestroyed"/>).
    ///
    /// EntityId формату: "{BuildingId}@{x},{y}" — унікальний, бо на одній клітинці може стояти
    /// лише одна будівля.
    /// </summary>
    internal sealed class BuildingHealthService :
        IBuildingGarrisonService,
        IConstructionModuleStatePersistence,
        IInitializable,
        IDisposable
    {
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const double HealthPlacementPerfThresholdMs = 0.5d;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IHealthRegistry _healthRegistry;
        private readonly SignalBus _signalBus;
        private readonly IConstructionUnitGarrisonRuntime _unitGarrisonRuntime;
        private readonly ICalendarService _calendarService;
        private readonly IConstructionRuntimeAuthorityQuery
            _runtimeAuthority;
        private readonly IConstructionPlacedBuildingDestruction
            _constructionDestruction;
        private bool _loggedReplicaDefenseSuppression;

        private readonly Dictionary<Vector2Int, string> _buildingOwners = new();
        private readonly Dictionary<Vector2Int, string> _buildingIds = new();
        private readonly Dictionary<Vector2Int, List<string>>
            _garrisonedByBuilding = new();
        private readonly Dictionary<string, Vector2Int>
            _garrisonBuildingByUnit = new();
        private readonly Dictionary<string, Vector2Int>
            _pendingGarrisonRestoreByUnit =
                new Dictionary<string, Vector2Int>(
                    StringComparer.Ordinal);

        public string StateKey => "building-runtime.v1";

        private readonly Dictionary<Vector2Int, DefenseEmitter>
            _defenses = new();
        private readonly Dictionary<string, UnitTrack>
            _units = new();
        private readonly List<string> _defenseUnitIdScratch = new();

        private readonly struct DefenseEmitter
        {
            public DefenseEmitter(
                string ownerId,
                int range,
                int damage)
            {
                OwnerId = ownerId;
                Range = range;
                Damage = damage;
            }

            public string OwnerId { get; }
            public int Range { get; }
            public int Damage { get; }
        }

        private readonly struct UnitTrack
        {
            public UnitTrack(
                Vector2Int position,
                string ownerId)
            {
                Position = position;
                OwnerId = ownerId;
            }

            public Vector2Int Position { get; }
            public string OwnerId { get; }
        }

        [Inject]
        public BuildingHealthService(
            IBuildingRegistry buildingRegistry,
            IHealthRegistry healthRegistry,
            SignalBus signalBus,
            [InjectOptional]
            IConstructionUnitGarrisonRuntime unitGarrisonRuntime = null,
            [InjectOptional]
            ICalendarService calendarService = null,
            [InjectOptional]
            IConstructionRuntimeAuthorityQuery runtimeAuthority = null,
            [InjectOptional]
            IConstructionPlacedBuildingDestruction constructionDestruction = null)
        {
            _buildingRegistry = buildingRegistry ?? throw new ArgumentNullException(nameof(buildingRegistry));
            _healthRegistry = healthRegistry ?? throw new ArgumentNullException(nameof(healthRegistry));
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            _unitGarrisonRuntime = unitGarrisonRuntime;
            _calendarService = calendarService;
            _runtimeAuthority = runtimeAuthority;
            _constructionDestruction = constructionDestruction;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<UnitGarrisonStateChangedSignal>(
                OnUnitGarrisonStateChanged);
            if (_calendarService != null)
                _calendarService.OnHourChanged += OnDefenseTurn;
            BuildingDefinitionAsset.RuntimeRevisionChanged +=
                OnBuildingDefinitionRuntimeRevisionChanged;

            Debug.Log(
                $"[MoyvaConstructionModules] defense-authority " +
                $"authoritative={IsAuthoritativeRuntime}");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<UnitGarrisonStateChangedSignal>(
                OnUnitGarrisonStateChanged);
            if (_calendarService != null)
                _calendarService.OnHourChanged -= OnDefenseTurn;
            BuildingDefinitionAsset.RuntimeRevisionChanged -=
                OnBuildingDefinitionRuntimeRevisionChanged;
            _defenses.Clear();
            _units.Clear();
            _buildingOwners.Clear();
            _buildingIds.Clear();
            _garrisonedByBuilding.Clear();
            _garrisonBuildingByUnit.Clear();
            _pendingGarrisonRestoreByUnit.Clear();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            double startedAt =
                Time.realtimeSinceStartupAsDouble;

            if (signal.HasRelocationSource && signal.RelocationSourcePosition != signal.Position)
            {
                _healthRegistry.Unregister(
                    BuildingEntityId(signal.BuildingId, signal.RelocationSourcePosition));
            }

            var definition = _buildingRegistry.GetById(signal.BuildingId);
            if (definition == null)
            {
                Debug.LogWarning($"[BuildingHealthService] Визначення будівлі '{signal.BuildingId}' не знайдено; health не буде зареєстроване.");
                return;
            }

            int maxHp =
                BuildingDefinitionCapabilities.GetEffectiveMaxHp(definition);
            int armor =
                BuildingDefinitionCapabilities.GetEffectiveArmor(definition);
            string entityId =
                BuildingEntityId(signal.BuildingId, signal.Position);

            var health = new HealthComponent();
            health.Initialize(entityId, maxHp, armor);
            Vector2Int buildingPosition = signal.Position;
            health.OnDestroyed +=
                destroyedId =>
                    OnBuildingDestroyed(
                        destroyedId,
                        buildingPosition);

            _healthRegistry.Register(health);
            _buildingIds[signal.Position] = signal.BuildingId;
            _buildingOwners[signal.Position] = NormalizeOwner(
                signal.OwnerId,
                signal.SourceFactionId);

            TryRestorePendingGarrisonsForBuilding(
                signal.Position);

            if (signal.HasRelocationSource
                && signal.RelocationSourcePosition != signal.Position)
            {
                _defenses.Remove(signal.RelocationSourcePosition);
                ReleaseGarrisonForBuilding(
                    signal.RelocationSourcePosition);
                _buildingOwners.Remove(
                    signal.RelocationSourcePosition);
                _buildingIds.Remove(
                    signal.RelocationSourcePosition);
            }

            int attackRange =
                BuildingDefinitionCapabilities
                    .GetDefenseAttackRange(definition);
            int attackDamage =
                BuildingDefinitionCapabilities
                    .GetDefenseAttackDamage(definition);
            if (attackRange > 0 && attackDamage > 0)
            {
                var emitter = new DefenseEmitter(
                    NormalizeOwner(
                        signal.OwnerId,
                        signal.SourceFactionId),
                    attackRange,
                    attackDamage);
                _defenses[signal.Position] = emitter;
            }
            else
            {
                _defenses.Remove(signal.Position);
            }

            if (Debug.isDebugBuild)
            {
                double elapsedMs =
                    (Time.realtimeSinceStartupAsDouble - startedAt)
                    * 1000d;
                if (elapsedMs >= HealthPlacementPerfThresholdMs)
                {
                    Debug.Log(
                        $"{PerfLogTag} health-register " +
                        $"building={signal.BuildingId} " +
                        $"pos={signal.Position} hp={maxHp} " +
                        $"elapsedMs={elapsedMs:F3}");
                }
            }
        }

        private void OnBuildingDestroyed(
            string entityId,
            Vector2Int position)
        {
            _healthRegistry.Unregister(entityId);

            if (_constructionDestruction != null
                && _constructionDestruction.TryDestroyPlacedBuilding(
                    position,
                    "health-zero"))
            {
                // BuildingDemolishedSignal performs the normal module cleanup.
                return;
            }

            // Isolated tests/scenes can omit ConstructionService. Keep local
            // cleanup as a fail-safe rather than leaking runtime state.
            _defenses.Remove(position);
            ReleaseGarrisonForBuilding(position);
            _buildingOwners.Remove(position);
            _buildingIds.Remove(position);

            Debug.LogWarning(
                $"[MoyvaConstructionModules] building-destroyed " +
                $"position={position} cause=health-zero " +
                "constructionSink=missing-or-rejected");
        }

        private void OnBuildingDemolished(
            BuildingDemolishedSignal signal)
        {
            _healthRegistry.Unregister(
                BuildingEntityId(
                    signal.BuildingId,
                    signal.Position));
            _defenses.Remove(signal.Position);
            ReleaseGarrisonForBuilding(signal.Position);
            _buildingOwners.Remove(signal.Position);
            _buildingIds.Remove(signal.Position);
        }

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            _units[signal.UnitId] =
                new UnitTrack(
                    signal.Position,
                    NormalizeOwner(signal.OwnerId, null));
            TryRestorePendingGarrison(signal.UnitId);
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            string ownerId =
                _units.TryGetValue(
                    signal.UnitId,
                    out UnitTrack previous)
                    ? previous.OwnerId
                    : NormalizeOwner(
                        signal.SourceFactionId,
                        null);

            _units[signal.UnitId] =
                new UnitTrack(
                    signal.NewPosition,
                    ownerId);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            _units.Remove(signal.UnitId);
            RemoveUnitFromGarrisonCollections(signal.UnitId);
        }

        private void OnUnitGarrisonStateChanged(
            UnitGarrisonStateChangedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            if (signal.IsGarrisoned)
            {
                _units.Remove(signal.UnitId);
                return;
            }

            _units[signal.UnitId] = new UnitTrack(
                signal.UnitPosition,
                NormalizeOwner(signal.OwnerId, null));
        }

        public byte[] CaptureState()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(2);

            var positions =
                new List<Vector2Int>(_buildingIds.Keys);
            positions.Sort(
                (left, right) =>
                {
                    int byX =
                        left.x.CompareTo(right.x);
                    return byX != 0
                        ? byX
                        : left.y.CompareTo(right.y);
                });

            var healthEntries =
                new List<(Vector2Int Position, string BuildingId, int CurrentHp)>();

            for (int index = 0;
                 index < positions.Count;
                 index++)
            {
                Vector2Int position = positions[index];
                string buildingId = _buildingIds[position];
                string entityId =
                    BuildingEntityId(
                        buildingId,
                        position);

                if (_healthRegistry.TryGet(
                        entityId,
                        out IHealth health)
                    && health != null)
                {
                    healthEntries.Add(
                        (position,
                         buildingId,
                         health.CurrentHp));
                }
            }

            writer.Write(healthEntries.Count);
            for (int index = 0;
                 index < healthEntries.Count;
                 index++)
            {
                var entry = healthEntries[index];
                writer.Write(entry.Position.x);
                writer.Write(entry.Position.y);
                writer.Write(entry.BuildingId ?? string.Empty);
                writer.Write(entry.CurrentHp);
            }

            var unitIds =
                new List<string>(
                    _garrisonBuildingByUnit.Keys);
            unitIds.Sort(StringComparer.Ordinal);

            writer.Write(unitIds.Count);
            for (int index = 0;
                 index < unitIds.Count;
                 index++)
            {
                string unitId = unitIds[index];
                Vector2Int buildingPosition =
                    _garrisonBuildingByUnit[unitId];

                writer.Write(unitId ?? string.Empty);
                writer.Write(buildingPosition.x);
                writer.Write(buildingPosition.y);
            }

            writer.Flush();

            Debug.Log(
                $"[MoyvaConstructionModules] building-runtime-save " +
                $"health={healthEntries.Count} " +
                $"garrisonUnits={unitIds.Count}");

            return stream.ToArray();
        }

        public void RestoreState(byte[] payload)
        {
            _pendingGarrisonRestoreByUnit.Clear();

            if (payload == null || payload.Length == 0)
                return;

            using var stream =
                new MemoryStream(payload, writable: false);
            using var reader =
                new BinaryReader(stream);

            int version = reader.ReadInt32();
            if (version != 2)
            {
                Debug.LogWarning(
                    $"[MoyvaConstructionModules] building-runtime-load " +
                    $"unsupported-version={version}");
                return;
            }

            int healthCount =
                Math.Max(
                    0,
                    reader.ReadInt32());
            int restoredHealth = 0;
            int skippedHealth = 0;

            for (int index = 0;
                 index < healthCount;
                 index++)
            {
                var position =
                    new Vector2Int(
                        reader.ReadInt32(),
                        reader.ReadInt32());
                string buildingId =
                    reader.ReadString();
                int currentHp =
                    reader.ReadInt32();

                string entityId =
                    BuildingEntityId(
                        buildingId,
                        position);

                if (_healthRegistry.TryGet(
                        entityId,
                        out IHealth health)
                    && health is HealthComponent mutable)
                {
                    mutable.RestoreCurrentHp(currentHp);
                    restoredHealth++;
                }
                else
                {
                    skippedHealth++;
                }
            }

            int garrisonCount =
                Math.Max(
                    0,
                    reader.ReadInt32());
            int queued = 0;

            for (int index = 0;
                 index < garrisonCount;
                 index++)
            {
                string unitId = reader.ReadString();
                var buildingPosition =
                    new Vector2Int(
                        reader.ReadInt32(),
                        reader.ReadInt32());

                if (string.IsNullOrWhiteSpace(unitId))
                    continue;

                _pendingGarrisonRestoreByUnit[unitId] =
                    buildingPosition;
                queued++;
            }

            int restoredGarrison = 0;
            var pendingIds =
                new List<string>(
                    _pendingGarrisonRestoreByUnit.Keys);
            pendingIds.Sort(StringComparer.Ordinal);

            for (int index = 0;
                 index < pendingIds.Count;
                 index++)
            {
                int before =
                    _pendingGarrisonRestoreByUnit.Count;
                TryRestorePendingGarrison(
                    pendingIds[index]);
                if (_pendingGarrisonRestoreByUnit.Count < before)
                    restoredGarrison++;
            }

            Debug.Log(
                $"[MoyvaConstructionModules] building-runtime-load " +
                $"health={restoredHealth}/{healthCount} " +
                $"healthSkipped={skippedHealth} " +
                $"garrisonQueued={queued} " +
                $"garrisonRestored={restoredGarrison} " +
                $"garrisonPending={_pendingGarrisonRestoreByUnit.Count}");
        }

        private void TryRestorePendingGarrisonsForBuilding(
            Vector2Int buildingPosition)
        {
            if (_pendingGarrisonRestoreByUnit.Count == 0)
                return;

            var unitIds = new List<string>();
            foreach (var pair
                     in _pendingGarrisonRestoreByUnit)
            {
                if (pair.Value == buildingPosition)
                    unitIds.Add(pair.Key);
            }

            unitIds.Sort(StringComparer.Ordinal);
            for (int index = 0;
                 index < unitIds.Count;
                 index++)
            {
                TryRestorePendingGarrison(unitIds[index]);
            }
        }

        private void TryRestorePendingGarrison(string unitId)
        {
            if (_unitGarrisonRuntime == null
                || string.IsNullOrWhiteSpace(unitId)
                || !_pendingGarrisonRestoreByUnit.TryGetValue(
                    unitId,
                    out Vector2Int buildingPosition)
                || !_buildingIds.TryGetValue(
                    buildingPosition,
                    out string buildingId))
            {
                return;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);
            int capacity =
                BuildingDefinitionCapabilities
                    .GetGarrisonCapacity(definition);

            if (capacity <= 0)
            {
                _pendingGarrisonRestoreByUnit.Remove(unitId);
                Debug.LogWarning(
                    $"[MoyvaConstructionModules] garrison-load drop " +
                    $"unit={unitId} building={buildingPosition} " +
                    "reason=no-capacity");
                return;
            }

            List<string> units =
                GetOrCreateGarrisonList(buildingPosition);

            if (units.Contains(unitId))
            {
                _pendingGarrisonRestoreByUnit.Remove(unitId);
                return;
            }

            if (units.Count >= capacity)
            {
                _pendingGarrisonRestoreByUnit.Remove(unitId);
                Debug.LogWarning(
                    $"[MoyvaConstructionModules] garrison-load drop " +
                    $"unit={unitId} building={buildingPosition} " +
                    $"reason=capacity count={units.Count}/{capacity}");
                return;
            }

            string buildingOwner =
                _buildingOwners.TryGetValue(
                    buildingPosition,
                    out string owner)
                    ? owner
                    : "player_0";
            string unitOwner =
                _unitGarrisonRuntime.GetUnitOwnerId(unitId);

            if (!string.Equals(
                    buildingOwner,
                    unitOwner,
                    StringComparison.Ordinal))
            {
                _pendingGarrisonRestoreByUnit.Remove(unitId);
                Debug.LogWarning(
                    $"[MoyvaConstructionModules] garrison-load drop " +
                    $"unit={unitId} building={buildingPosition} " +
                    $"reason=owner-mismatch " +
                    $"unitOwner={unitOwner} buildingOwner={buildingOwner}");
                return;
            }

            bool entered =
                _unitGarrisonRuntime.IsGarrisoned(unitId)
                || _unitGarrisonRuntime.TryRestoreGarrison(
                    unitId,
                    buildingPosition,
                    out _);

            if (!entered)
                return;

            units.Add(unitId);
            _garrisonBuildingByUnit[unitId] =
                buildingPosition;
            _pendingGarrisonRestoreByUnit.Remove(unitId);

            Debug.Log(
                $"[MoyvaConstructionModules] garrison-load restored " +
                $"unit={unitId} building={buildingPosition} " +
                $"count={units.Count}/{capacity}");
        }

        public bool TryGarrisonUnit(
            Vector2Int buildingPosition,
            string unitId,
            out string reason)
        {
            reason = null;
            if (_unitGarrisonRuntime == null)
            {
                reason = "Unit garrison runtime не підключений.";
                return false;
            }

            if (!_buildingIds.TryGetValue(
                    buildingPosition,
                    out string buildingId))
            {
                reason = "Будівлю не знайдено в runtime.";
                return false;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);
            int capacity = BuildingDefinitionCapabilities
                .GetGarrisonCapacity(definition);
            if (capacity <= 0)
            {
                reason = "Ця будівля не має гарнізонної місткості.";
                return false;
            }

            if (_garrisonBuildingByUnit.ContainsKey(unitId))
            {
                reason = "Юніт уже закріплений за гарнізоном.";
                return false;
            }

            List<string> units = GetOrCreateGarrisonList(
                buildingPosition);
            if (units.Count >= capacity)
            {
                reason = $"Гарнізон заповнений: {units.Count}/{capacity}.";
                return false;
            }

            string buildingOwner = _buildingOwners.TryGetValue(
                    buildingPosition,
                    out string storedOwner)
                ? storedOwner
                : "player_0";
            string unitOwner = _unitGarrisonRuntime.GetUnitOwnerId(unitId);
            if (!string.Equals(
                    buildingOwner,
                    unitOwner,
                    StringComparison.Ordinal))
            {
                reason = "Не можна розмістити в гарнізоні юніт іншого власника.";
                return false;
            }

            if (!_unitGarrisonRuntime.TryEnterGarrison(
                    unitId,
                    buildingPosition,
                    out reason))
            {
                return false;
            }

            units.Add(unitId);
            _garrisonBuildingByUnit[unitId] = buildingPosition;
            Debug.Log(
                $"[MoyvaConstructionModules] garrison enter " +
                $"building={buildingId}@{buildingPosition} unit={unitId} " +
                $"count={units.Count}/{capacity}");
            return true;
        }

        public bool TryUngarrisonUnit(
            Vector2Int buildingPosition,
            string unitId,
            Vector2Int targetPosition,
            out string reason)
        {
            reason = null;
            if (_unitGarrisonRuntime == null
                || !_garrisonBuildingByUnit.TryGetValue(
                    unitId,
                    out Vector2Int registeredBuilding)
                || registeredBuilding != buildingPosition)
            {
                reason = "Юніт не належить до цього гарнізону.";
                return false;
            }

            if (!_unitGarrisonRuntime.TryExitGarrison(
                    unitId,
                    targetPosition,
                    out reason))
            {
                return false;
            }

            RemoveUnitFromGarrisonCollections(unitId);
            Debug.Log(
                $"[MoyvaConstructionModules] garrison exit " +
                $"building={buildingPosition} unit={unitId} target={targetPosition}");
            return true;
        }

        public IReadOnlyList<string> GetGarrisonedUnits(
            Vector2Int buildingPosition)
        {
            return _garrisonedByBuilding.TryGetValue(
                    buildingPosition,
                    out List<string> units)
                ? units.ToArray()
                : Array.Empty<string>();
        }

        public bool TryGetGarrisonStatus(
            Vector2Int buildingPosition,
            out int occupied,
            out int capacity)
        {
            occupied =
                _garrisonedByBuilding.TryGetValue(
                    buildingPosition,
                    out List<string> units)
                    ? units.Count
                    : 0;

            capacity = 0;
            if (!_buildingIds.TryGetValue(
                    buildingPosition,
                    out string buildingId))
            {
                return false;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);
            capacity =
                BuildingDefinitionCapabilities
                    .GetGarrisonCapacity(definition);
            return capacity > 0;
        }

        private List<string> GetOrCreateGarrisonList(
            Vector2Int buildingPosition)
        {
            if (!_garrisonedByBuilding.TryGetValue(
                    buildingPosition,
                    out List<string> units))
            {
                units = new List<string>();
                _garrisonedByBuilding[buildingPosition] = units;
            }
            return units;
        }

        private void RemoveUnitFromGarrisonCollections(string unitId)
        {
            if (!_garrisonBuildingByUnit.TryGetValue(
                    unitId,
                    out Vector2Int buildingPosition))
            {
                return;
            }

            _garrisonBuildingByUnit.Remove(unitId);
            if (_garrisonedByBuilding.TryGetValue(
                    buildingPosition,
                    out List<string> units))
            {
                units.Remove(unitId);
                if (units.Count == 0)
                    _garrisonedByBuilding.Remove(buildingPosition);
            }
        }

        private void ReleaseGarrisonForBuilding(
            Vector2Int buildingPosition)
        {
            if (!_garrisonedByBuilding.TryGetValue(
                    buildingPosition,
                    out List<string> units)
                || units.Count == 0
                || _unitGarrisonRuntime == null)
            {
                return;
            }

            string[] snapshot = units.ToArray();
            Array.Sort(
                snapshot,
                StringComparer.Ordinal);

            int released = 0;
            int failed = 0;

            for (int unitIndex = 0;
                 unitIndex < snapshot.Length;
                 unitIndex++)
            {
                string unitId = snapshot[unitIndex];

                if (_unitGarrisonRuntime.TryExitGarrisonNear(
                        unitId,
                        buildingPosition,
                        maxRadius: 8,
                        out Vector2Int target,
                        out string reason))
                {
                    RemoveUnitFromGarrisonCollections(unitId);
                    released++;

                    Debug.Log(
                        $"[MoyvaConstructionModules] garrison release " +
                        $"building={buildingPosition} unit={unitId} " +
                        $"target={target}");
                }
                else
                {
                    failed++;
                    Debug.LogError(
                        $"[MoyvaConstructionModules] garrison release failed " +
                        $"building={buildingPosition} unit={unitId} " +
                        $"reason={reason}");
                }
            }

            Debug.Log(
                $"[MoyvaConstructionModules] garrison release-summary " +
                $"building={buildingPosition} " +
                $"released={released} failed={failed}");
        }

        private void EnforceGarrisonCapacity(
            Vector2Int buildingPosition,
            BuildingDefinition definition)
        {
            if (!_garrisonedByBuilding.TryGetValue(
                    buildingPosition,
                    out List<string> units)
                || units.Count == 0
                || _unitGarrisonRuntime == null)
            {
                return;
            }

            int capacity =
                Mathf.Max(
                    0,
                    BuildingDefinitionCapabilities
                        .GetGarrisonCapacity(definition));
            if (units.Count <= capacity)
                return;

            int mustRelease =
                units.Count - capacity;
            var candidates =
                new List<string>(units);
            candidates.Sort(StringComparer.Ordinal);

            // Deterministically release the lexicographically last units.
            for (int index = candidates.Count - 1;
                 index >= 0 && mustRelease > 0;
                 index--)
            {
                string unitId = candidates[index];
                if (!_unitGarrisonRuntime.TryExitGarrisonNear(
                        unitId,
                        buildingPosition,
                        maxRadius: 8,
                        out Vector2Int target,
                        out string reason))
                {
                    Debug.LogError(
                        $"[MoyvaConstructionModules] garrison capacity-trim failed " +
                        $"building={buildingPosition} unit={unitId} " +
                        $"targetCapacity={capacity} reason={reason}");
                    continue;
                }

                RemoveUnitFromGarrisonCollections(unitId);
                mustRelease--;

                Debug.Log(
                    $"[MoyvaConstructionModules] garrison capacity-trim " +
                    $"building={buildingPosition} unit={unitId} " +
                    $"target={target} capacity={capacity}");
            }
        }

        private void OnBuildingDefinitionRuntimeRevisionChanged(
            int revision)
        {
            int refreshed = 0;

            foreach (var pair in _buildingIds)
            {
                Vector2Int position = pair.Key;
                string buildingId = pair.Value;
                BuildingDefinition definition =
                    _buildingRegistry.GetById(buildingId);
                if (definition == null)
                    continue;

                string entityId =
                    BuildingEntityId(
                        buildingId,
                        position);
                if (_healthRegistry.TryGet(
                        entityId,
                        out IHealth health)
                    && health is HealthComponent mutable)
                {
                    mutable.Reconfigure(
                        BuildingDefinitionCapabilities
                            .GetEffectiveMaxHp(definition),
                        BuildingDefinitionCapabilities
                            .GetEffectiveArmor(definition),
                        preserveHealthRatio: true);
                }

                int attackRange =
                    BuildingDefinitionCapabilities
                        .GetDefenseAttackRange(definition);
                int attackDamage =
                    BuildingDefinitionCapabilities
                        .GetDefenseAttackDamage(definition);

                if (attackRange > 0
                    && attackDamage > 0)
                {
                    string owner =
                        _buildingOwners.TryGetValue(
                            position,
                            out string storedOwner)
                            ? storedOwner
                            : "player_0";
                    _defenses[position] =
                        new DefenseEmitter(
                            owner,
                            attackRange,
                            attackDamage);
                }
                else
                {
                    _defenses.Remove(position);
                }

                EnforceGarrisonCapacity(
                    position,
                    definition);

                refreshed++;
            }

            Debug.Log(
                $"[MoyvaConstructionModules] live-refresh building-runtime " +
                $"revision={revision} buildings={refreshed}");
        }

        private bool IsAuthoritativeRuntime =>
            _runtimeAuthority == null
            || _runtimeAuthority.IsAuthoritativeRuntime;

        private void OnDefenseTurn()
        {
            if (!IsAuthoritativeRuntime)
            {
                if (!_loggedReplicaDefenseSuppression)
                {
                    _loggedReplicaDefenseSuppression = true;
                    Debug.Log(
                        "[MoyvaConstructionModules] defense-turn " +
                        "suppressed=replica");
                }
                return;
            }

            _loggedReplicaDefenseSuppression = false;

            if (_defenses.Count == 0 || _units.Count == 0)
                return;

            foreach (var defense in _defenses)
            {
                TryFireNearestTarget(
                    defense.Key,
                    defense.Value);
            }
        }

        private void TryFireNearestTarget(
            Vector2Int defensePosition,
            DefenseEmitter defense)
        {
            string targetId = null;
            int bestDistance = int.MaxValue;

            foreach (var unitPair in _units)
            {
                UnitTrack unit = unitPair.Value;
                if (string.Equals(
                        defense.OwnerId,
                        unit.OwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                int distance = Mathf.Max(
                    Mathf.Abs(
                        defensePosition.x - unit.Position.x),
                    Mathf.Abs(
                        defensePosition.y - unit.Position.y));
                if (distance > defense.Range)
                    continue;

                if (distance < bestDistance
                    || (distance == bestDistance
                        && (targetId == null
                            || string.CompareOrdinal(
                                unitPair.Key,
                                targetId) < 0)))
                {
                    targetId = unitPair.Key;
                    bestDistance = distance;
                }
            }

            if (targetId == null)
                return;

            if (_healthRegistry.TryGet(
                    targetId,
                    out IHealth health))
            {
                health.TakeDamage(defense.Damage);
            }
        }

        private static string NormalizeOwner(
            string primary,
            string fallback)
        {
            string value =
                !string.IsNullOrWhiteSpace(primary)
                    ? primary
                    : fallback;
            return string.IsNullOrWhiteSpace(value)
                ? "player_0"
                : value.Trim();
        }

        /// <summary>
        /// Генерує унікальний EntityId для будівлі за її типом і позицією на grid.
        /// Формат: "barracks@3,5"
        /// </summary>
        public static string BuildingEntityId(string buildingId, Vector2Int position)
            => $"{buildingId}@{position.x},{position.y}";
    }
}
