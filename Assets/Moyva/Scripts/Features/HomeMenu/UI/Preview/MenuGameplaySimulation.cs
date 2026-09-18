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
    internal enum MenuSimulationStage
    {
        Founding,
        Growth,
        Expansion,
        Conflict,
        Battle
    }

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
        private readonly HashSet<Vector2Int> _landSet = new HashSet<Vector2Int>();
        private readonly List<Vector2Int>[] _teamLand =
        {
            new List<Vector2Int>(),
            new List<Vector2Int>()
        };
        private readonly List<Vector2Int> _frontLineLand = new List<Vector2Int>();
        private readonly Vector2Int[] _teamAnchors = new Vector2Int[2];
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
        public MenuSimulationStage Stage { get; private set; } = MenuSimulationStage.Founding;

        public MenuGameplaySimulation(
            GameObject root,
            MenuWorldPreviewData world,
            TileRegistrySO tiles,
            BuildingRegistrySO buildings,
            MoyvaProjectSettingsSO projection,
            MenuSimulationSettings settings,
            int layer,
            Func<Vector2Int, Vector3> position,
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
            if (registry == null)
                throw new InvalidOperationException("Menu simulation requires the unit registry.");

            UnitsInstaller.InstallPreviewBindings(_scope, registry);
            ConstructionPreviewBindings.Install(_scope, buildings);

            _signals = _scope.Resolve<SignalBus>();
            _signals.Subscribe<BuildingPlacedSignal>(onBuilding);

            _types = registry.Configs
                .Where(x => x != null && x.ResolvePrefab() != null)
                .ToArray();

            _buildings = buildings.GetAll()
                .Where(x => x != null && x.Prefab != null)
                .ToArray();

            var grid = _scope.Resolve<IGridService>();

            for (int y = 0; y < world.Height; y++)
            for (int x = 0; x < world.Width; x++)
            {
                var cell = new Vector2Int(x, y);
                string id = world.BiomeMap[x, y];
                grid.SetTileData(cell, id);

                if (IsUsableLand(id, x, y, world.Width, world.Height))
                {
                    _land.Add(cell);
                    _landSet.Add(cell);
                }
            }

            if (_land.Count < 12)
                throw new InvalidOperationException("Generated preview has too little usable land.");

            BuildTerritories(world.Width, world.Height);

            Focus = Vector3.Lerp(
                _position(_teamAnchors[0]),
                _position(_teamAnchors[1]),
                0.5f);

            _objects = _scope.Resolve<IObjectsMapService>();
            _factory = _scope.Resolve<IUnitFactory>();
            _units = _scope.Resolve<IUnitService>();
            _movement = _scope.Resolve<IUnitMovementService>();
            _combat = _scope.Resolve<IUnitCombatService>();
            _construction = _scope.Resolve<IConstructionSessionCommands>();

            foreach (var initializer in _scope.ResolveAll<IInitializable>())
                initializer.Initialize();

            _disposables = _scope.ResolveAll<IDisposable>();
            _nextTurn = 0.75f;
        }

        private static bool IsUsableLand(string id, int x, int y, int width, int height)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (id.IndexOf("water", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            if (id.IndexOf("ocean", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return x > 2 && y > 2 && x < width - 3 && y < height - 3;
        }

        private void BuildTerritories(int width, int height)
        {
            var mapCenter = new Vector2Int(width / 2, height / 2);
            Vector2Int seed = _land
                .OrderBy(cell => (cell - mapCenter).sqrMagnitude)
                .First();

            Vector2Int first = FindFarthest(seed);
            Vector2Int second = FindFarthest(first);
            first = FindFarthest(second);

            _teamAnchors[0] = first;
            _teamAnchors[1] = second;

            foreach (var cell in _land)
            {
                int d0 = (cell - first).sqrMagnitude;
                int d1 = (cell - second).sqrMagnitude;
                _teamLand[d0 <= d1 ? 0 : 1].Add(cell);
            }

            for (int team = 0; team < 2; team++)
            {
                Vector2Int anchor = _teamAnchors[team];
                _teamLand[team].Sort((a, b) =>
                    (a - anchor).sqrMagnitude.CompareTo((b - anchor).sqrMagnitude));
            }

            Vector2 center = ((Vector2)first + (Vector2)second) * 0.5f;
            _frontLineLand.AddRange(_land);
            _frontLineLand.Sort((a, b) =>
                ((Vector2)a - center).sqrMagnitude.CompareTo(((Vector2)b - center).sqrMagnitude));
        }

        private Vector2Int FindFarthest(Vector2Int origin)
        {
            Vector2Int best = _land[0];
            int bestDistance = -1;

            foreach (var cell in _land)
            {
                int distance = (cell - origin).sqrMagnitude;
                if (distance <= bestDistance) continue;
                bestDistance = distance;
                best = cell;
            }

            return best;
        }

        private void Install<T>(GameObject root) where T : MonoInstaller
        {
            var installer = _scope.InstantiateComponent<T>(root);
            installer.InstallBindings();
            UnityEngine.Object.Destroy(installer);
        }

        public void Tick(float elapsed)
        {
            if (_action != null && !_action.IsCompleted)
                return;

            if (_action != null && _action.IsFaulted)
            {
                Debug.LogException(_action.Exception);
                _action = null;
            }

            if (elapsed < _nextTurn)
                return;

            _nextTurn = elapsed + Mathf.Clamp(_settings.turnSeconds, 1f, 10f);
            _turn++;
            Stage = ResolveStage(_turn);

            if (ShouldBuildThisTurn())
                Build();

            if (_turn >= 4)
                Spawn();

            if (_turn >= 7)
                _action = PlayTurnAsync();
        }

        private static MenuSimulationStage ResolveStage(int turn)
        {
            if (turn <= 3) return MenuSimulationStage.Founding;
            if (turn <= 6) return MenuSimulationStage.Growth;
            if (turn <= 9) return MenuSimulationStage.Expansion;
            if (turn <= 13) return MenuSimulationStage.Conflict;
            return MenuSimulationStage.Battle;
        }

        private bool ShouldBuildThisTurn()
        {
            return Stage switch
            {
                MenuSimulationStage.Founding => true,
                MenuSimulationStage.Growth => _turn % 2 == 0,
                MenuSimulationStage.Expansion => _turn % 2 == 1,
                MenuSimulationStage.Conflict => _turn % 3 == 0,
                _ => _turn % 4 == 0
            };
        }

        private void Build()
        {
            if (_buildings.Length == 0 || _buildingIndex >= 14)
                return;

            int team = _buildingIndex % 2;
            string owner = "menu-team-" + team;
            _construction.SetActiveOwner(owner);

            string id = _buildings[_buildingIndex % _buildings.Length].Id;
            if (_construction is IConstructionBootstrapQuery bootstrap &&
                bootstrap.RequiresInitialCastle(owner, out string castle))
            {
                id = castle;
            }

            _construction.SelectBuilding(id);

            var candidates = _teamLand[team];
            int scan = Mathf.Min(180, candidates.Count);

            for (int i = 0; i < scan; i++)
            {
                Vector2Int cell = candidates[i];
                if (!_construction.TryPreviewAt(cell))
                    continue;

                var result = ((IConstructionConfirmationCommands)_construction).ConfirmPending();
                if (!result.Succeeded)
                {
                    _construction.Cancel();
                    continue;
                }

                Focus = _position(cell);
                _buildingIndex++;
                return;
            }

            _construction.Cancel();
        }

        private void Spawn()
        {
            _actors.RemoveAll(id =>
            {
                bool dead = _units.GetUnitObject(id) == null;
                if (dead) _teams.Remove(id);
                return dead;
            });

            if (_types.Length == 0 ||
                _actors.Count >= Mathf.Clamp(_settings.maxUnits, 2, 12))
            {
                return;
            }

            int team = _turn % 2;
            IReadOnlyList<Vector2Int> candidates =
                Stage >= MenuSimulationStage.Conflict ? _frontLineLand : _teamLand[team];

            foreach (var cell in candidates)
            {
                if (_objects.IsOccupied(cell))
                    continue;

                string typeId = _types[Mathf.Abs(_turn + team) % _types.Length].TypeId;
                string id = _factory.CreateUnit(typeId, cell, "menu-team-" + team);

                if (string.IsNullOrEmpty(id))
                    continue;

                GameObject actor = _units.GetUnitObject(id);
                if (actor == null)
                    continue;

                actor.transform.SetParent(_root, true);
                actor.transform.position = _position(cell);

                foreach (var child in actor.GetComponentsInChildren<Transform>(true))
                    child.gameObject.layer = _layer;

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

                if (!_units.TryGetUnitPosition(actor, out var from))
                    continue;

                string enemy = FindNearestEnemy(actor, from);
                if (enemy == null)
                    continue;

                if (!_units.TryGetUnitPosition(enemy, out var target))
                    continue;

                Focus = Vector3.Lerp(_position(from), _position(target), 0.5f);

                if (_combat.CanAttack(actor, enemy, out _))
                {
                    await ShowAttackAsync(actor, enemy);
                    _combat.TryAttack(actor, enemy, out _);
                    continue;
                }

                _units.SetStamina(actor, 10f);

                if (TryChooseStep(from, target, out var step))
                    await _movement.MoveUnitAsync(actor, step, _lifetime.Token);
            }
        }

        private string FindNearestEnemy(string actor, Vector2Int from)
        {
            if (!_teams.TryGetValue(actor, out int actorTeam))
                return null;

            string enemy = null;
            int nearest = int.MaxValue;

            foreach (string other in _actors)
            {
                if (other == actor ||
                    !_teams.TryGetValue(other, out int otherTeam) ||
                    actorTeam == otherTeam ||
                    !_units.TryGetUnitPosition(other, out var at))
                {
                    continue;
                }

                int distance = (from - at).sqrMagnitude;
                if (distance >= nearest)
                    continue;

                nearest = distance;
                enemy = other;
            }

            return enemy;
        }

        private bool TryChooseStep(Vector2Int from, Vector2Int target, out Vector2Int step)
        {
            int dx = Math.Sign(target.x - from.x);
            int dy = Math.Sign(target.y - from.y);

            Vector2Int[] candidates =
            {
                new Vector2Int(from.x + dx, from.y),
                new Vector2Int(from.x, from.y + dy),
                new Vector2Int(from.x - dx, from.y),
                new Vector2Int(from.x, from.y - dy)
            };

            int bestDistance = int.MaxValue;
            step = from;
            bool found = false;

            foreach (var candidate in candidates)
            {
                if (candidate == from ||
                    !_landSet.Contains(candidate) ||
                    _objects.IsOccupied(candidate))
                {
                    continue;
                }

                int distance = (candidate - target).sqrMagnitude;
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                step = candidate;
                found = true;
            }

            return found;
        }

        private async Task ShowAttackAsync(string actor, string enemy)
        {
            var source = _units.GetUnitObject(actor);
            var target = _units.GetUnitObject(enemy);

            if (source == null || target == null)
                return;

            Vector3 origin = source.transform.position;
            Vector3 targetPosition = target.transform.position;
            Vector3 direction = targetPosition - origin;
            direction.y = 0;

            Focus = Vector3.Lerp(origin, targetPosition, 0.5f);

            if (direction.sqrMagnitude > 0.001f)
                source.transform.rotation = Quaternion.LookRotation(direction);

            float elapsed = 0f;
            const float duration = 0.42f;

            while (elapsed < duration && source != null)
            {
                _lifetime.Token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;

                float normalized = Mathf.Clamp01(elapsed / duration);
                float lunge = Mathf.Sin(normalized * Mathf.PI) * 0.35f;
                source.transform.position = origin + direction.normalized * lunge;

                await Task.Yield();
            }

            if (source != null)
                source.transform.position = origin;
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _signals.TryUnsubscribe<BuildingPlacedSignal>(_onBuilding);

            for (int i = _disposables.Count - 1; i >= 0; i--)
                _disposables[i].Dispose();

            _lifetime.Dispose();
        }
    }
}
