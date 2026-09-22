using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Motion;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>Presentation-сервіс бойових реакцій юнітів: відкат при влучанні та ривок атаки через EntityMotion.</summary>
    internal sealed class UnitCombatPresentationService : IInitializable, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly IUnitCombatService _combat;
        private readonly IUnitService _units;
        private readonly IUnitClassConfig _configs;
        private readonly IGridProjection _projection;
        private readonly IGameplayMotionSettingsProvider _motion;
        private readonly HashSet<Vector2Int> _targetPositions = new();

        private GameObject _root;
        private Material _rangeMaterial;
        private Material _targetMaterial;
        private string _selectedUnitId;
        private GameModeType _mode = GameModeType.Normal;

        /// <summary>Створює сервіс із провайдером налаштувань руху.</summary>
        public UnitCombatPresentationService(
            SignalBus signals,
            IUnitCombatService combat,
            IUnitService units,
            [InjectOptional] IUnitClassConfig configs = null,
            [InjectOptional] IGridProjection projection = null,
            [InjectOptional] IGameplayMotionSettingsProvider motion = null)
        {
            _signals = signals;
            _combat = combat;
            _units = units;
            _configs = configs;
            _projection = projection;
            _motion = motion;
        }

        /// <summary>Підписує сервіс на бойові події.</summary>
        public void Initialize()
        {
            _signals.Subscribe<LocalUnitSelectionChangedSignal>(OnSelectionChanged);
            _signals.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signals.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signals.Subscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
            _signals.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _combat.AttackStarted += OnAttackStarted;
            _combat.AttackResolved += OnAttackResolved;
        }

        /// <summary>Відписує сервіс.</summary>
        public void Dispose()
        {
            _signals.TryUnsubscribe<LocalUnitSelectionChangedSignal>(OnSelectionChanged);
            _signals.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signals.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signals.TryUnsubscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
            _signals.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _combat.AttackStarted -= OnAttackStarted;
            _combat.AttackResolved -= OnAttackResolved;
            Clear();
            DestroyMaterial(ref _rangeMaterial);
            DestroyMaterial(ref _targetMaterial);
        }

        private void OnSelectionChanged(LocalUnitSelectionChangedSignal signal)
        {
            if (!signal.IsSelected || string.IsNullOrWhiteSpace(signal.UnitId))
            {
                if (string.IsNullOrWhiteSpace(signal.UnitId)
                    || string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal))
                {
                    _selectedUnitId = null;
                    Clear();
                }
                return;
            }
            _selectedUnitId = signal.UnitId.Trim();
            Refresh();
        }

        private void OnUnitMoved(UnitMovedSignal _) { if (_selectedUnitId != null) Refresh(); }
        private void OnObjectsMapChanged(OnObjectsMapChangedSignal _) { if (_selectedUnitId != null) Refresh(); }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal))
            {
                _selectedUnitId = null;
                Clear();
            }
            else if (_selectedUnitId != null) Refresh();
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _mode = signal.NewMode;
            if (_mode == GameModeType.Normal) Refresh(); else Clear();
        }

        private void OnAttackStarted(string attacker, string target)
        {
            PlayConfiguredAnimation(attacker, AnimationType.Attack);
            PlayAttackLunge(attacker, target);
        }

        private void OnAttackResolved(UnitAttackResult result)
        {
            if (result.Succeeded && !result.TargetDied)
            {
                PlayConfiguredAnimation(result.TargetUnitId, AnimationType.TakeDamage);
                PlayHitRecoil(result.AttackerUnitId, result.TargetUnitId);
            }
            if (_selectedUnitId != null) Refresh();
        }

        /// <summary>Tiny forward lunge toward the target — never leaves the attacker's tile.</summary>
        private void PlayAttackLunge(string attackerId, string targetId)
        {
            GameObject attacker = _units.GetUnitObject(attackerId);
            GameObject target = _units.GetUnitObject(targetId);
            if (attacker == null || target == null)
                return;

            UnitTransitionMotionProfile p = _motion?.UnitTransitions ?? new UnitTransitionMotionProfile();
            float amplitude = _motion?.ScaleSecondaryAmplitude(p.attackLungeDistance) ?? p.attackLungeDistance;
            float duration = _motion?.ScaleDuration(p.attackLungeDuration) ?? p.attackLungeDuration;
            if (amplitude <= 0f || duration <= 0f)
                return;

            Vector3 dir = target.transform.position - attacker.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            EntityMotion.AttachOrUpdate(attacker)
                ?.PunchPosition(dir.normalized * amplitude, duration, p.attackLungeEase);
        }

        /// <summary>Short directional recoil on the surviving target.</summary>
        private void PlayHitRecoil(string attackerId, string targetId)
        {
            GameObject attacker = _units.GetUnitObject(attackerId);
            GameObject target = _units.GetUnitObject(targetId);
            if (attacker == null || target == null)
                return;

            UnitTransitionMotionProfile p = _motion?.UnitTransitions ?? new UnitTransitionMotionProfile();
            float amplitude = _motion?.ScaleSecondaryAmplitude(p.hitRecoilDistance) ?? p.hitRecoilDistance;
            float duration = _motion?.ScaleDuration(p.hitRecoilDuration) ?? p.hitRecoilDuration;
            if (amplitude <= 0f || duration <= 0f)
                return;

            Vector3 dir = target.transform.position - attacker.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            EntityMotion.AttachOrUpdate(target)
                ?.PunchPosition(dir.normalized * amplitude, duration, p.hitRecoilEase);
        }

        private void Refresh()
        {
            Clear();
            if (_mode != GameModeType.Normal || string.IsNullOrWhiteSpace(_selectedUnitId)
                || _projection == null || _combat == null) return;

            EnsureMaterials();
            _targetPositions.Clear();
            foreach (string target in _combat.GetAttackableTargets(_selectedUnitId))
            {
                Vector2Int pos;
                if (_units.TryGetUnitPosition(target, out pos)) _targetPositions.Add(pos);
            }

            IReadOnlyList<Vector2Int> tiles = _combat.GetAttackableTiles(_selectedUnitId);
            if (tiles.Count == 0) return;

            _root = new GameObject("UnitCombatRangeOverlay");
            foreach (Vector2Int pos in tiles)
            {
                bool target = _targetPositions.Contains(pos);
                CreateCell(pos, target, target ? _targetMaterial : _rangeMaterial);
            }
        }

        private void CreateCell(Vector2Int pos, bool target, Material material)
        {
            var go = new GameObject(target ? "AttackTarget" : "AttackRange");
            go.transform.SetParent(_root.transform, false);
            go.transform.position = _projection.GridToWorld(pos) + Vector3.up * 0.055f;
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 4;
            line.startWidth = line.endWidth = target ? 0.085f : 0.045f;
            line.sharedMaterial = material;
            Color c = target ? new Color(1f, .22f, .12f, .98f) : new Color(1f, .38f, .18f, .62f);
            line.startColor = line.endColor = c;
            const float h = .43f;
            line.SetPosition(0,new Vector3(-h,0,-h));
            line.SetPosition(1,new Vector3(-h,0,h));
            line.SetPosition(2,new Vector3(h,0,h));
            line.SetPosition(3,new Vector3(h,0,-h));
        }

        private void PlayConfiguredAnimation(string unitId, AnimationType type)
        {
            if (_configs == null || string.IsNullOrWhiteSpace(unitId)) return;
            GameObject obj = _units.GetUnitObject(unitId);
            if (obj == null) return;
            string typeId = _units.GetUnitTypeId(unitId);
            UnitClassConfig config = string.IsNullOrWhiteSpace(typeId) ? null : _configs.GetConfig(typeId);
            UnitAnimationTrigger.Play(obj, config, type);
        }

        private void EnsureMaterials()
        {
            if (_rangeMaterial != null && _targetMaterial != null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (shader == null) return;
            _rangeMaterial = new Material(shader) { hideFlags = HideFlags.DontSave };
            _targetMaterial = new Material(shader) { hideFlags = HideFlags.DontSave };
        }

        private void Clear()
        {
            _targetPositions.Clear();
            if (_root != null) UnityEngine.Object.Destroy(_root);
            _root = null;
        }

        private static void DestroyMaterial(ref Material material)
        {
            if (material != null) UnityEngine.Object.Destroy(material);
            material = null;
        }
    }
}
