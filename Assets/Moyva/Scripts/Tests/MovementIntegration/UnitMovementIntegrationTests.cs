using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.MovementIntegration
{
    [TestFixture]
    public class UnitMovementIntegrationTests : ZenjectUnitTestFixture
    {
        private const string PlainTile = "plain";
        private const string SwampTile = "swamp";
        private const string UnitTypeId = "warrior";
        private const string UnitId = "warrior_01";

        private readonly List<Object> _sceneObjects = new();

        private SignalBus _signalBus;
        private IGridService _gridService;
        private ITileSettingsService _tileSettingsService;
        private IUnitClassConfig _unitClassConfig;
        private IObjectsMapService _objectsMapService;
        private IUnitService _unitService;
        private IPathfinder _pathfinder;
        private IMovementAnimationService _animationService;
        private IUnitMovementService _unitMovementService;

        private object _unitServiceInstance;
        private object _unitMovementInstance;

        private System.IDisposable _objectsMapDisposable;
        private System.IDisposable _unitServiceDisposable;
        private System.IDisposable _unitMovementDisposable;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<InterruptMovementSignal>();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>();

            _signalBus = Container.Resolve<SignalBus>();

            _gridService = new TestGridService(10, 10, PlainTile);
            _tileSettingsService = new TestTileSettingsService(
                new Dictionary<string, float>
                {
                    [PlainTile] = 1f,
                    [SwampTile] = 10f
                });

            _unitClassConfig = new TestUnitClassConfig(
                UnitTypeId,
                new PathAnimationSettings
                {
                    MoveDurationPerTile = 0f,
                    DelayOnTile = 0f
                },
                baseStamina: 100f);

            _objectsMapService = CreateInternal<IObjectsMapService>(
                typeof(IObjectsMapService).Assembly,
                "Kruty1918.Moyva.ObjectsMap.Runtime.ObjectsMapService",
                _signalBus);
            _objectsMapDisposable = _objectsMapService as System.IDisposable;

            _unitService = CreateInternal<IUnitService>(
                typeof(IUnitService).Assembly,
                "Kruty1918.Moyva.Units.Runtime.UnitService",
                _signalBus,
                _gridService,
                _tileSettingsService,
                _unitClassConfig);
            _unitServiceInstance = _unitService;

            _pathfinder = CreateInternal<IPathfinder>(
                typeof(IPathfinder).Assembly,
                "Kruty1918.Moyva.Pathfinding.Runtime.Pathfinder",
                _gridService,
                _tileSettingsService,
                _objectsMapService);

            _animationService = CreateInternal<IMovementAnimationService>(
                typeof(IMovementAnimationService).Assembly,
                "Kruty1918.Moyva.Animations.Runtime.MovementAnimationService");

            _unitMovementService = CreateInternal<IUnitMovementService>(
                typeof(IUnitMovementService).Assembly,
                "Kruty1918.Moyva.Units.Runtime.UnitMovementService",
                _unitService,
                _pathfinder,
                _animationService,
                _tileSettingsService,
                _gridService,
                _signalBus,
                _unitClassConfig);
            _unitMovementInstance = _unitMovementService;

            _unitServiceDisposable = _unitService as System.IDisposable;
            _unitMovementDisposable = _unitMovementService as System.IDisposable;

            InvokeIfExists(_objectsMapService, "Initialize");
            InvokeIfExists(_unitServiceInstance, "Initialize");
            InvokeIfExists(_unitMovementInstance, "Initialize");
        }

        public override void Teardown()
        {
            _unitMovementDisposable?.Dispose();
            _unitServiceDisposable?.Dispose();
            _objectsMapDisposable?.Dispose();

            foreach (var obj in _sceneObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            _sceneObjects.Clear();

            base.Teardown();
        }

        [Test]
        public async Task Spawn_Animate_And_Move_ShouldFollowBestPath_AndKeepStateConsistent()
        {
            MarkExpensiveCorridor();

            var start = new Vector2Int(2, 5);
            var target = new Vector2Int(7, 5);
            var unitObject = CreateVisibleUnitObject(UnitId);

            var expectedPath = _pathfinder.FindPath(start, target);

            Assert.IsNotNull(expectedPath);
            Assert.Greater(expectedPath.Count, 1, "Pathfinder must produce a multi-step path.");
            Assert.AreEqual(start, expectedPath[0]);
            Assert.AreEqual(target, expectedPath[^1]);

            var expensiveTiles = new[] { new Vector2Int(3, 5), new Vector2Int(4, 5), new Vector2Int(5, 5), new Vector2Int(6, 5) };
            foreach (var expensiveTile in expensiveTiles)
            {
                Assert.IsFalse(
                    expectedPath.Skip(1).Take(expectedPath.Count - 2).Contains(expensiveTile),
                    $"Path should avoid expensive tile {expensiveTile}.");
            }

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = UnitId,
                UnitTypeId = UnitTypeId,
                Position = start,
                UnitObject = unitObject
            });

            Assert.IsTrue(_objectsMapService.IsOccupied(start), "Spawn must occupy start tile.");
            Assert.IsTrue(_objectsMapService.TryGetOccupant(start, out var spawnedId));
            Assert.AreEqual(UnitId, spawnedId);

            var actualSteps = new List<Vector2Int>();
            var unitServicePositionsOnStep = new List<Vector2Int>();
            var objectsMapOccupancyOnStep = new List<(bool IsOccupied, string OccupantId)>();
            var mapEnterEvents = new List<Vector2Int>();

            _signalBus.Subscribe<UnitMovedSignal>(signal =>
            {
                if (signal.UnitId == UnitId)
                {
                    actualSteps.Add(signal.NewPosition);

                    Assert.IsTrue(_unitService.TryGetUnitPosition(UnitId, out var currentPosition));
                    unitServicePositionsOnStep.Add(currentPosition);

                    var isOccupied = _objectsMapService.IsOccupied(signal.NewPosition);
                    _objectsMapService.TryGetOccupant(signal.NewPosition, out var occupantId);
                    objectsMapOccupancyOnStep.Add((isOccupied, occupantId));
                }
            });

            _signalBus.Subscribe<OnObjectsMapChangedSignal>(signal =>
            {
                if (signal.OccupantId == UnitId)
                {
                    mapEnterEvents.Add(signal.Position);
                }
            });

            await _unitMovementService.MoveUnitAsync(UnitId, target, CancellationToken.None);

            var expectedSteps = expectedPath.Skip(1).ToList();

            Assert.AreEqual(expectedSteps.Count, actualSteps.Count, "Every path step must emit UnitMovedSignal exactly once.");
            for (int i = 0; i < expectedSteps.Count; i++)
            {
                Assert.AreEqual(expectedSteps[i], actualSteps[i], $"Unexpected position on step {i + 1}.");

                var previous = i == 0 ? start : actualSteps[i - 1];
                var current = actualSteps[i];
                var dx = Mathf.Abs(current.x - previous.x);
                var dy = Mathf.Abs(current.y - previous.y);

                Assert.IsTrue((dx <= 1 && dy <= 1) && (dx + dy > 0),
                    $"Step {i + 1} is invalid: {previous} -> {current}");

                Assert.AreEqual(current, unitServicePositionsOnStep[i], $"UnitService position mismatch on step {i + 1}.");

                Assert.IsTrue(objectsMapOccupancyOnStep[i].IsOccupied, $"ObjectsMap must mark step {i + 1} position as occupied.");
                Assert.AreEqual(UnitId, objectsMapOccupancyOnStep[i].OccupantId, $"ObjectsMap occupant mismatch on step {i + 1}.");
            }

            Assert.AreEqual(expectedSteps.Count, mapEnterEvents.Count, "ObjectsMap must register one enter-event per movement step.");
            for (int i = 0; i < expectedSteps.Count; i++)
            {
                Assert.AreEqual(expectedSteps[i], mapEnterEvents[i], $"ObjectsMap enter-event mismatch on step {i + 1}.");
            }

            Assert.IsTrue(_unitService.TryGetUnitPosition(UnitId, out var finalPosition));
            Assert.AreEqual(target, finalPosition, "UnitService should store final position after movement.");

            Assert.IsFalse(_objectsMapService.IsOccupied(start), "Start tile must be free after movement.");
            Assert.IsTrue(_objectsMapService.IsOccupied(target), "Target tile must be occupied after movement.");
            Assert.IsTrue(_objectsMapService.TryGetOccupant(target, out var finalOccupant));
            Assert.AreEqual(UnitId, finalOccupant);

            var finalWorldPos = unitObject.transform.position;
            Assert.AreEqual(target.x, Mathf.RoundToInt(finalWorldPos.x));
            Assert.AreEqual(target.y, Mathf.RoundToInt(finalWorldPos.y));
        }

        private void MarkExpensiveCorridor()
        {
            var expensiveTiles = new[] { new Vector2Int(3, 5), new Vector2Int(4, 5), new Vector2Int(5, 5), new Vector2Int(6, 5) };
            foreach (var tile in expensiveTiles)
            {
                _gridService.SetTileData(tile, new TileData { TileTypeId = SwampTile });
            }
        }

        private GameObject CreateVisibleUnitObject(string unitId)
        {
            var go = new GameObject(unitId);
            var spriteRenderer = go.AddComponent<SpriteRenderer>();

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            spriteRenderer.sprite = sprite;

            _sceneObjects.Add(go);
            _sceneObjects.Add(sprite);
            _sceneObjects.Add(texture);

            return go;
        }

        private static TContract CreateInternal<TContract>(Assembly assembly, string typeName, params object[] args)
        {
            var concreteType = assembly.GetType(typeName);
            Assert.IsNotNull(concreteType, $"Type '{typeName}' was not found in assembly '{assembly.FullName}'.");

            var instance = System.Activator.CreateInstance(concreteType, args);
            Assert.IsNotNull(instance, $"Failed to instantiate type '{typeName}'.");

            return (TContract)instance;
        }

        private static void InvokeIfExists(object instance, string methodName)
        {
            if (instance == null) return;
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            method?.Invoke(instance, null);
        }

        private sealed class TestGridService : IGridService
        {
            private readonly TileData[,] _grid;

            public TestGridService(int width, int height, string defaultTileType)
            {
                GridWidth = width;
                GridHeight = height;

                _grid = new TileData[width, height];
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        _grid[x, y] = new TileData { TileTypeId = defaultTileType };
                    }
                }
            }

            public TileData GetTileData(Vector2Int position)
            {
                if (!IsInside(position))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(position));
                }

                return _grid[position.x, position.y];
            }

            public bool TryGetTileData(Vector2Int position, out TileData tileData)
            {
                if (IsInside(position))
                {
                    tileData = _grid[position.x, position.y];
                    return true;
                }

                tileData = default;
                return false;
            }

            public void SetTileData(Vector2Int position, TileData data)
            {
                if (!IsInside(position))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(position));
                }

                _grid[position.x, position.y] = data;
            }

            public int GridWidth { get; }
            public int GridHeight { get; }

            private bool IsInside(Vector2Int position)
            {
                return position.x >= 0 && position.x < GridWidth && position.y >= 0 && position.y < GridHeight;
            }
        }

        private sealed class TestTileSettingsService : ITileSettingsService
        {
            private readonly Dictionary<string, float> _weights;

            public TestTileSettingsService(Dictionary<string, float> weights)
            {
                _weights = weights;
            }

            public float GetTileWeight(string tileId)
            {
                return _weights.TryGetValue(tileId, out var weight) ? weight : 1f;
            }
        }

        private sealed class TestUnitClassConfig : IUnitClassConfig
        {
            private readonly string _typeId;
            private readonly UnitClassConfig _config;

            public TestUnitClassConfig(string typeId, PathAnimationSettings animationSettings, float baseStamina)
            {
                _typeId = typeId;
                _config = new UnitClassConfig
                {
                    TypeId = typeId,
                    BaseStamina = baseStamina,
                    StaminaRandomRange = Vector2.zero,
                    AnimationSettings = animationSettings
                };
            }

            public UnitClassConfig GetConfig(string typeId)
            {
                if (string.IsNullOrEmpty(typeId))
                {
                    return null;
                }

                var baseTypeId = typeId;
                var underscoreIndex = typeId.IndexOf('_');
                if (underscoreIndex >= 0)
                {
                    baseTypeId = typeId.Substring(0, underscoreIndex);
                }

                return baseTypeId == _typeId ? _config : null;
            }
        }
    }
}
