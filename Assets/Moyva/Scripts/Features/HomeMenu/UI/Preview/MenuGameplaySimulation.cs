using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Animations.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.ObjectsMap.Runtime;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Pathfinding.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // A standalone container owns all mutable state. No project/session/save/UI
    // services are inherited from the menu or from a running multiplayer room.
    internal sealed class MenuGameplaySimulation : IDisposable
    {
        private readonly DiContainer _scope = new DiContainer();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly List<IDisposable> _disposables;
        private readonly IUnitFactory _factory;
        private readonly IUnitService _units;
        private readonly IUnitMovementService _movement;
        private readonly IUnitCombatService _combat;
        private readonly IConstructionSessionCommands _construction;
        private readonly IObjectsMapService _objects;
        private readonly List<Vector2Int> _land = new List<Vector2Int>();
        private readonly List<string> _actors = new List<string>();
        private readonly Dictionary<string, int> _teams = new Dictionary<string, int>();
        private readonly UnitClassConfig[] _types;
        private readonly BuildingDefinition[] _buildings;
        private readonly MenuSimulationSettings _settings;
        private readonly Transform _root;
        private readonly Func<Vector2Int, Vector3> _position;
        private readonly SignalBus _signals;
        private readonly Action<BuildingPlacedSignal> _onBuilding;
        private readonly int _layer;
        private Task _action;
        private float _nextTurn;
        private int _turn;
        private int _buildingIndex;
        public Vector3 Focus { get; private set; }
        public int Turn => _turn;
        public int ActorCount => _actors.Count;

        public MenuGameplaySimulation(GameObject root, MenuWorldPreviewData world,
            TileRegistrySO tiles, BuildingRegistrySO buildings, MoyvaProjectSettingsSO projection,
            MenuSimulationSettings settings, int layer, Func<Vector2Int, Vector3> position,
            Action<BuildingPlacedSignal> onBuilding)
        {
            _root = root.transform;
            _settings = settings;
            _position = position;
            _layer = layer;
            _onBuilding = onBuilding;
            Install<Kruty1918.Moyva.Signals.SignalBusInstaller>(root);
            GridInstaller.InstallPreviewBindings(_scope, tiles, projection, world.Width, world.Height);
            Install<ObjectsMapInstaller>(root);
            Install<AnimationsInstaller>(root);
            _scope.Bind<IPathfinder>().To<Pathfinder>().AsSingle();
            var registry = MoyvaJsonRuntime.GetAll<UnitRegistrySO>().FirstOrDefault();
            if (registry == null) throw new InvalidOperationException("Menu simulation requires the unit registry.");
            UnitsInstaller.InstallPreviewBindings(_scope, registry);
            ConstructionPreviewBindings.Install(_scope, buildings);
            _signals = _scope.Resolve<SignalBus>();
            _signals.Subscribe<BuildingPlacedSignal>(onBuilding);
            _types = registry.Configs.Where(x => x != null && x.ResolvePrefab() != null).ToArray();
            _buildings = buildings.GetAll().Where(x => x != null && x.Prefab != null).ToArray();
            var grid = _scope.Resolve<IGridService>();
            var tileSettings = _scope.Resolve<ITileSettingsService>();
            for (int y = 0; y < world.Height; y++)
            for (int x = 0; x < world.Width; x++)
            {
                var cell = new Vector2Int(x, y);
                string id = world.BiomeMap[x, y];
                grid.SetTileData(cell, id);
                if (!string.IsNullOrEmpty(id) && id.IndexOf("water", StringComparison.OrdinalIgnoreCase) < 0 &&
                    id.IndexOf("ocean", StringComparison.OrdinalIgnoreCase) < 0 && x > 2 && y > 2 &&
                    x < world.Width - 3 && y < world.Height - 3)
                    _land.Add(cell);
            }
            if (_land.Count < 12) throw new InvalidOperationException("Generated preview has too little usable land.");
            var center = new Vector2Int(world.Width / 2, world.Height / 2);
            _land.Sort((a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude));
            Focus = _position(_land[0]);
            _objects = _scope.Resolve<IObjectsMapService>();
            _factory = _scope.Resolve<IUnitFactory>();
            _units = _scope.Resolve<IUnitService>();
            _movement = _scope.Resolve<IUnitMovementService>();
            _combat = _scope.Resolve<IUnitCombatService>();
            _construction = _scope.Resolve<IConstructionSessionCommands>();
            foreach (var initializer in _scope.ResolveAll<IInitializable>()) initializer.Initialize();
            _disposables = _scope.ResolveAll<IDisposable>();
            _nextTurn = 1f;
        }

        private void Install<T>(GameObject root) where T : MonoInstaller
        {
            var installer = _scope.InstantiateComponent<T>(root);
            installer.InstallBindings();
            UnityEngine.Object.Destroy(installer);
        }

        public void Tick(float elapsed)
        {
            if (_action != null && !_action.IsCompleted) return;
            if (_action != null && _action.IsFaulted)
            {
                Debug.LogException(_action.Exception);
                _action = null;
            }
            if (elapsed < _nextTurn) return;
            _nextTurn = elapsed + Mathf.Clamp(_settings.turnSeconds, 1f, 10f);
            _turn++;
            if (_turn <= 3 || _turn % 5 == 0) Build();
            if (_turn >= 3) Spawn();
            if (_turn >= 5) _action = PlayTurnAsync();
        }

        private void Build()
        {
            if (_buildings.Length == 0 || _buildingIndex >= 10) return;
            int team = _buildingIndex % 2;
            string owner = "menu-team-" + team;
            _construction.SetActiveOwner(owner);
            string id = _buildings[_buildingIndex % _buildings.Length].Id;
            if (_construction is IConstructionBootstrapQuery bootstrap && bootstrap.RequiresInitialCastle(owner, out string castle))
                id = castle;
            _construction.SelectBuilding(id);
            // A bounded scan delegates footprint, terrain and overlap decisions
            // to ConstructionService; only confirmed placements get a visual.
            for (int i = 0; i < Mathf.Min(128, _land.Count); i++)
            {
                Vector2Int cell = _land[(i + team * (_land.Count / 3)) % _land.Count];
                if (!_construction.TryPreviewAt(cell)) continue;
                var result = ((IConstructionConfirmationCommands)_construction).ConfirmPending();
                if (result.Succeeded)
                {
                    Focus = _position(cell);
                    _buildingIndex++;
                    return;
                }
                _construction.Cancel();
            }
        }

        private void Spawn()
        {
            _actors.RemoveAll(id => _units.GetUnitObject(id) == null);
            if (_types.Length == 0 || _actors.Count >= Mathf.Clamp(_settings.maxUnits, 2, 12)) return;
            int team = _turn % 2;
            foreach (var cell in _land)
            {
                if (_objects.IsOccupied(cell)) continue;
                string id = _factory.CreateUnit(_types[(_turn - 3) % _types.Length].TypeId, cell, "menu-team-" + team);
                if (string.IsNullOrEmpty(id)) continue;
                GameObject actor = _units.GetUnitObject(id);
                actor.transform.SetParent(_root, true);
                actor.transform.position = _position(cell);
                foreach (var child in actor.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = _layer;
                foreach (var renderer in actor.GetComponentsInChildren<Renderer>())
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                _actors.Add(id);
                _teams[id] = team;
                Focus = actor.transform.position;
                return;
            }
        }

        private async Task PlayTurnAsync()
        {
            foreach (string actor in _actors.ToArray())
            {
                _lifetime.Token.ThrowIfCancellationRequested();
                if (!_units.TryGetUnitPosition(actor, out var from)) continue;
                string enemy = null;
                int nearest = int.MaxValue;
                foreach (string other in _actors)
                {
                    if (_teams[actor] == _teams[other] || !_units.TryGetUnitPosition(other, out var at)) continue;
                    int distance = (from - at).sqrMagnitude;
                    if (distance < nearest) { nearest = distance; enemy = other; }
                }
                if (enemy == null) continue;
                Focus = _position(from);
                if (_combat.CanAttack(actor, enemy, out _))
                {
                    await ShowAttackAsync(actor, enemy);
                    _combat.TryAttack(actor, enemy, out _);
                    continue;
                }
                if (!_units.TryGetUnitPosition(enemy, out var target)) continue;
                _units.SetStamina(actor, 10f);
                // One neighbouring step per turn keeps the vignette readable.
                var step = from + new Vector2Int(Math.Sign(target.x - from.x), Math.Sign(target.y - from.y));
                if (!_objects.IsOccupied(step))
                    await _movement.MoveUnitAsync(actor, step, _lifetime.Token);
            }
        }

        private async Task ShowAttackAsync(string actor, string enemy)
        {
            var source = _units.GetUnitObject(actor);
            var target = _units.GetUnitObject(enemy);
            if (source == null || target == null) return;
            Vector3 origin = source.transform.position;
            Vector3 direction = target.transform.position - origin;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f)
                source.transform.rotation = Quaternion.LookRotation(direction);
            float elapsed = 0;
            while (elapsed < 0.4f && source != null)
            {
                _lifetime.Token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                source.transform.position = origin + direction.normalized *
                    (Mathf.Sin(Mathf.Clamp01(elapsed / 0.4f) * Mathf.PI) * 0.35f);
                await Task.Yield();
            }
            if (source != null) source.transform.position = origin;
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _signals.TryUnsubscribe<BuildingPlacedSignal>(_onBuilding);
            for (int i = _disposables.Count - 1; i >= 0; i--) _disposables[i].Dispose();
            _lifetime.Dispose();
        }
    }
}
