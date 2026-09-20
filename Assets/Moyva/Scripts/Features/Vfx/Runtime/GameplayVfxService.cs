using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Vfx.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>Канонічний VFX-сервіс gameplay: подія-визначення-пул пайплайн, якісні ліміти та спавн-контроль.</summary>
    public sealed class GameplayVfxService
        : IVfxService, IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly VfxCatalogConfig _config;
        private readonly IVfxSpawner _spawner;
        private readonly IGridProjection _projection;
        private readonly IGridService _grid;
        private readonly IUnitService _units;
        private readonly IUnitCombatService _combat;
        private readonly IFogStateReader _fog;
        private readonly IFactionRegistry _factions;
        private readonly IGraphicsSettingsService _graphics;
        private readonly UnityEngine.Camera _camera;
        private readonly Func<float> _clock;

        private readonly VfxUnitSnapshotStore _unitSnapshots = new VfxUnitSnapshotStore();
        private readonly VfxRendererFlash _flash;
        private readonly Dictionary<string, float> _cooldowns =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private VfxDefinitionRegistry _registry;
        private VfxQualityState _quality;
        private int _spawnsThisFrame;
        private UnityEngine.Camera _resolvedCamera;

        /// <summary>Створює сервіс із реєстром правил, пулом і джерелами подій.</summary>
        public GameplayVfxService(
            SignalBus signals,
            [InjectOptional] VfxCatalogConfig config,
            [InjectOptional] IVfxSpawner spawner,
            [InjectOptional] IGridProjection projection = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitCombatService combat = null,
            [InjectOptional] IFogStateReader fog = null,
            [InjectOptional] IFactionRegistry factions = null,
            [InjectOptional] IGraphicsSettingsService graphics = null,
            [InjectOptional] UnityEngine.Camera camera = null,
            [InjectOptional] VfxPool ownedPool = null,
            Func<float> clock = null)
        {
            _signals = signals;
            _config = config;
            _projection = projection;
            _grid = grid;
            _units = units;
            _combat = combat;
            _fog = fog;
            _factions = factions;
            _graphics = graphics;
            _camera = camera;
            _clock = clock ?? (() => Time.time);
            _spawner = spawner ?? ownedPool;
            _flash = new VfxRendererFlash(_clock);
        }

        /// <summary>Поточний стан якості VFX.</summary>
        public VfxQualityState Quality => _quality;
        /// <summary>Кількість спавнів у цьому кадрі.</summary>
        public int SpawnsThisFrame => _spawnsThisFrame;

        /// <summary>Підписує сервіс на gameplay-події.</summary>
        public void Initialize()
        {
            _registry = VfxDefinitionRegistry.Build(_config);
            _quality = ResolveQuality();
            if (_graphics != null)
                _graphics.OnSettingsChanged += OnGraphicsSettingsChanged;

            if (_signals != null)
            {
                _signals.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signals.Subscribe<BuildingOperationalSignal>(OnBuildingOperational);
                _signals.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
                _signals.Subscribe<UnitCreatedSignal>(OnUnitCreated);
                _signals.Subscribe<UnitMovedSignal>(OnUnitMoved);
                _signals.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
                _signals.Subscribe<UnitRecruitmentDeployedSignal>(OnUnitDeployed);
                _signals.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
                _signals.Subscribe<SettlementCapturedSignal>(OnSettlementCaptured);
                _signals.Subscribe<WorldFocusPingRequestedSignal>(OnWorldPing);
            }

            if (_combat != null)
            {
                _combat.AttackStarted += OnAttackStarted;
                _combat.AttackResolved += OnAttackResolved;
            }
        }

        /// <summary>Прокачує пул і лічильники кадру.</summary>
        public void Tick()
        {
            _spawnsThisFrame = 0;
            (_spawner as VfxPool)?.Tick();
            _flash.Tick();
        }

        /// <summary>Відписує сервіс і звільняє пул.</summary>
        public void Dispose()
        {
            if (_graphics != null)
                _graphics.OnSettingsChanged -= OnGraphicsSettingsChanged;

            if (_signals != null)
            {
                _signals.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signals.TryUnsubscribe<BuildingOperationalSignal>(OnBuildingOperational);
                _signals.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
                _signals.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
                _signals.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
                _signals.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
                _signals.TryUnsubscribe<UnitRecruitmentDeployedSignal>(OnUnitDeployed);
                _signals.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
                _signals.TryUnsubscribe<SettlementCapturedSignal>(OnSettlementCaptured);
                _signals.TryUnsubscribe<WorldFocusPingRequestedSignal>(OnWorldPing);
            }

            if (_combat != null)
            {
                _combat.AttackStarted -= OnAttackStarted;
                _combat.AttackResolved -= OnAttackResolved;
            }

            _flash.Clear();
            _unitSnapshots.Clear();
        }

        // ---- IVfxService (tooling/test entry point) ----

        /// <summary>Відтворює ефект за назвою події у світовій позиції.</summary>
        public bool Play(string eventName, Vector3 worldPosition)
            => Play(eventName, worldPosition, null, null);

        /// <summary>Відтворює ефект із контекстом і тінтом фракції.</summary>
        public bool Play(
            string eventName,
            Vector3 worldPosition,
            string context,
            Color? factionTint)
        {
            if (_registry == null)
                _registry = VfxDefinitionRegistry.Build(_config);

            VfxEffectRule rule = _registry.Resolve(eventName, context);
            if (rule == null)
                return false;

            var request = new VfxSpawnRequest
            {
                Position = worldPosition,
                Rotation = Quaternion.identity,
                Scale = rule.scale,
                CountScale = _quality.CountScale,
            };
            if (rule.factionTint && factionTint.HasValue)
            {
                request.Tint = factionTint.Value;
                request.HasTint = true;
            }

            return TrySpawn(rule, request, null);
        }

        // ---- Signal handlers ----

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
            => Emit(VfxEventIds.BuildingPlaced, signal.Position,
                FactionTint(signal.SourceFactionId));

        private void OnBuildingOperational(BuildingOperationalSignal signal)
            => Emit(VfxEventIds.BuildingOperational, signal.Position,
                FactionTint(signal.OwnerId));

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
            => Emit(VfxEventIds.BuildingDemolished, signal.Position,
                FactionTint(signal.SourceFactionId));

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            _unitSnapshots.TrackCreated(
                signal.UnitId, signal.Position, signal.UnitTypeId, signal.OwnerId, _clock());

            if (_unitSnapshots.TryConsumeSpawnEffect(signal.UnitId, _clock()))
            {
                Emit(VfxEventIds.UnitSpawned, signal.Position,
                    FactionTint(signal.OwnerId), UnitContext(signal.UnitTypeId));
            }
        }

        private void OnUnitDeployed(UnitRecruitmentDeployedSignal signal)
        {
            _unitSnapshots.TrackCreated(
                signal.UnitId, signal.Position, signal.UnitTypeId, signal.OwnerId, _clock());

            if (_unitSnapshots.TryConsumeSpawnEffect(signal.UnitId, _clock()))
            {
                Emit(VfxEventIds.UnitSpawned, signal.Position,
                    FactionTint(signal.OwnerId), UnitContext(signal.UnitTypeId));
            }
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            string ownerId = signal.SourceFactionId;
            _unitSnapshots.TrackMoved(signal.UnitId, signal.NewPosition, ownerId);

            Emit(VfxEventIds.UnitMoveDust, signal.NewPosition,
                FactionTint(ownerId),
                TileContext(signal.NewPosition),
                cooldownKey: signal.UnitId);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (_unitSnapshots.TryGet(
                    signal.UnitId,
                    out Vector2Int position,
                    out string unitTypeId,
                    out string ownerId))
            {
                Emit(VfxEventIds.UnitDestroyed, position,
                    FactionTint(ownerId), UnitContext(unitTypeId));
            }
            else if (_units != null
                     && _units.TryGetUnitPosition(signal.UnitId, out Vector2Int live))
            {
                Emit(VfxEventIds.UnitDestroyed, live, null);
            }

            _unitSnapshots.Remove(signal.UnitId);
        }

        private void OnSettlementCreated(SettlementCreatedSignal signal)
            => Emit(VfxEventIds.SettlementCreated, signal.TownHallPosition,
                FactionTint(signal.OwnerId));

        private void OnSettlementCaptured(SettlementCapturedSignal signal)
            => Emit(VfxEventIds.SettlementCaptured, signal.CenterPosition,
                FactionTint(signal.NewOwnerId));

        private void OnWorldPing(WorldFocusPingRequestedSignal signal)
        {
            VfxEffectRule rule = _registry?.Resolve(VfxEventIds.WorldPing);
            if (rule == null)
                return;

            var request = new VfxSpawnRequest
            {
                Position = ToWorld(signal.Position),
                Rotation = Quaternion.identity,
                Scale = rule.scale,
                CountScale = _quality.CountScale,
                DurationOverride = Mathf.Max(0.2f, signal.DurationSeconds),
            };
            TrySpawn(rule, request, signal.Position);
        }

        private void OnAttackStarted(string attackerId, string targetId)
        {
            if (!_unitSnapshots.TryGet(
                    attackerId, out Vector2Int attackerPos, out string attackerType, out string ownerId))
            {
                if (_units == null || !_units.TryGetUnitPosition(attackerId, out attackerPos))
                    return;
                attackerType = _units.GetUnitTypeId(attackerId);
            }

            Vector3 origin = ToWorld(attackerPos);
            Quaternion rotation = Quaternion.identity;
            if (_unitSnapshots.TryGet(targetId, out Vector2Int targetPos, out _, out _)
                || (_units != null && _units.TryGetUnitPosition(targetId, out targetPos)))
            {
                Vector3 delta = ToWorld(targetPos) - origin;
                delta.y = 0f;
                if (delta.sqrMagnitude > 0.0001f)
                    rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            }

            Emit(VfxEventIds.CombatAttack, attackerPos, FactionTint(ownerId),
                UnitContext(attackerType), rotation);
        }

        private void OnAttackResolved(UnitAttackResult result)
        {
            if (!result.Succeeded)
                return;

            Vector2Int targetPos;
            string targetType;
            if (!_unitSnapshots.TryGet(
                    result.TargetUnitId, out targetPos, out targetType, out _))
            {
                if (_units == null || !_units.TryGetUnitPosition(result.TargetUnitId, out targetPos))
                    return;
                targetType = _units.GetUnitTypeId(result.TargetUnitId);
            }

            float severity = result.TargetHpBefore > 0
                ? Mathf.Clamp01(result.DamageApplied / (float)result.TargetHpBefore)
                : 0.5f;

            Emit(VfxEventIds.CombatImpact, targetPos, null,
                UnitContext(targetType), Quaternion.identity,
                severityScale: Mathf.Lerp(0.85f, 1.3f, severity));

            if (IsVisible(targetPos))
            {
                GameObject targetObject = _units?.GetUnitObject(result.TargetUnitId);
                if (targetObject != null)
                    _flash.Flash(targetObject);
            }
        }

        // ---- Emission pipeline ----

        private void Emit(
            string eventName,
            Vector2Int gridPosition,
            Color? factionTint,
            string context = null,
            Quaternion? rotation = null,
            float severityScale = 1f,
            string cooldownKey = null)
        {
            if (_registry == null)
                return;

            VfxEffectRule rule = _registry.Resolve(eventName, context);
            if (rule == null || !IsVisible(gridPosition))
                return;

            var request = new VfxSpawnRequest
            {
                Position = ToWorld(gridPosition),
                Rotation = rotation ?? Quaternion.identity,
                Scale = rule.scale * severityScale,
                CountScale = _quality.CountScale,
                CooldownKey = cooldownKey,
            };
            if (rule.factionTint && factionTint.HasValue)
            {
                request.Tint = factionTint.Value;
                request.HasTint = true;
            }

            TrySpawn(rule, request, gridPosition);
        }

        private bool TrySpawn(
            VfxEffectRule rule,
            in VfxSpawnRequest request,
            Vector2Int? gridPosition)
        {
            if (_spawner == null || rule.prefab == null)
                return false;

            float now = _clock();

            if (rule.cooldownPerKey > 0f && !string.IsNullOrEmpty(request.CooldownKey))
            {
                string key = rule.eventName + "|" + request.CooldownKey;
                if (_cooldowns.TryGetValue(key, out float last)
                    && now - last < rule.cooldownPerKey)
                    return false;
                _cooldowns[key] = now;
            }

            if (!PassesCameraGate(rule, request.Position))
                return false;

            if (rule.priority < 2 && _spawnsThisFrame >= _quality.MaxSpawnsPerFrame)
                return false;

            if (!PassesBudget(rule))
                return false;

            if (!_spawner.TrySpawn(rule, request))
                return false;

            _spawnsThisFrame++;
            return true;
        }

        private bool PassesCameraGate(VfxEffectRule rule, Vector3 position)
        {
            UnityEngine.Camera camera = ResolveCamera();
            if (camera == null)
                return true;

            if (rule.priority == 0
                && _quality.LowPriorityMaxOrthoSize > 0f
                && camera.orthographic
                && camera.orthographicSize > _quality.LowPriorityMaxOrthoSize)
                return false;

            if (rule.maxOrthoSize > 0f
                && camera.orthographic
                && camera.orthographicSize > rule.maxOrthoSize)
                return false;

            float cull = rule.cullDistance > 0f
                ? rule.cullDistance
                : (rule.priority < 2 ? _quality.CullDistance : 0f);
            if (cull > 0f)
            {
                float sqr = (camera.transform.position - position).sqrMagnitude;
                if (sqr > cull * cull)
                    return false;
            }

            return true;
        }

        private UnityEngine.Camera ResolveCamera()
        {
            if (_camera != null)
                return _camera;
            if (_resolvedCamera == null)
                _resolvedCamera = UnityEngine.Camera.main;
            return _resolvedCamera;
        }

        private bool PassesBudget(VfxEffectRule rule)
        {
            var pool = _spawner as VfxPool;
            if (pool == null)
                return true;

            if (pool.ActiveCount < _quality.MaxActive
                && (rule.maxConcurrent <= 0
                    || pool.ActiveCountForPrefab(rule.prefab) < rule.maxConcurrent))
                return true;

            if (rule.priority == 2)
            {
                // Critical events (kill, capture) evict the oldest effect instead of dropping.
                pool.EvictOldest();
                return rule.maxConcurrent <= 0
                    || pool.ActiveCountForPrefab(rule.prefab) < rule.maxConcurrent;
            }

            return false;
        }

        private bool IsVisible(Vector2Int gridPosition)
            => _fog == null || _fog.IsVisible(gridPosition);

        private Color? FactionTint(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId) || _factions == null)
                return null;

            return _factions.TryGet(new FactionId(ownerId), out FactionDefinition definition)
                ? definition.TeamColor
                : (Color?)null;
        }

        private Vector3 ToWorld(Vector2Int gridPosition)
            => _projection != null
                ? _projection.GridToWorld(gridPosition)
                : new Vector3(gridPosition.x, 0f, gridPosition.y);

        private string TileContext(Vector2Int gridPosition)
            => _grid != null
               && _grid.TryGetTileData(gridPosition, out string tileTypeId)
               && !string.IsNullOrWhiteSpace(tileTypeId)
                ? "tile:" + tileTypeId
                : null;

        private static string UnitContext(string unitTypeId)
            => string.IsNullOrWhiteSpace(unitTypeId) ? null : "unit:" + unitTypeId;

        private VfxQualityState ResolveQuality()
            => VfxQualityState.ForProfile(
                _graphics?.Settings.Profile ?? GraphicsQualityProfile.Balanced,
                _config?.budget);

        private void OnGraphicsSettingsChanged(GraphicsSettingsData data)
            => _quality = VfxQualityState.ForProfile(data.Profile, _config?.budget);
    }
}
