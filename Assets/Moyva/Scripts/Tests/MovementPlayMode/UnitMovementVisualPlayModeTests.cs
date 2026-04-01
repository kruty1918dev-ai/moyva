using System.Collections;
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
using UnityEngine.TestTools;
using Zenject;

namespace Kruty1918.Moyva.Tests.MovementPlayMode
{
    public class UnitMovementVisualPlayModeTests
    {
        private const string PlainTile = "plain";
        private const string SwampTile = "swamp";
        private const string UnitTypeId = "warrior";
        private const string UnitId = "warrior_01";

        private readonly List<Object> _sceneObjects = new();

        private DiContainer _container;
        private SignalBus _signalBus;
        private IGridService _gridService;
        private ITileSettingsService _tileSettingsService;
        private IUnitClassConfig _unitClassConfig;
        private IObjectsMapService _objectsMapService;
        private IUnitService _unitService;
        private IPathfinder _pathfinder;
        private IMovementAnimationService _animationService;
        private IUnitMovementService _unitMovementService;

        private object _objectsMapInstance;
        private object _unitServiceInstance;
        private object _unitMovementInstance;

        private System.IDisposable _objectsMapDisposable;
        private System.IDisposable _unitServiceDisposable;
        private System.IDisposable _unitMovementDisposable;

        [UnityTest]
        public IEnumerator Spawn_Animate_And_Move_PlayMode_ShouldShowBestPathMovement()
        {
            SetupServices();
            MarkExpensiveCorridor();

            var start = new Vector2Int(2, 5);
            var target = new Vector2Int(7, 5);
            CreateCamera(new Vector3(5f, 5f, -10f));
            var unitObject = CreateVisibleUnitObject(UnitId, start);

            var expectedPath = _pathfinder.FindPath(start, target);
            Assert.IsNotNull(expectedPath);
            Assert.Greater(expectedPath.Count, 1, "Pathfinder must produce a multi-step path.");
            Assert.AreEqual(start, expectedPath[0]);
            Assert.AreEqual(target, expectedPath[^1]);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId = UnitId,
                UnitTypeId = UnitTypeId,
                Position = start,
                UnitObject = unitObject
            });

            Assert.IsTrue(_unitService.TryGetUnitPosition(UnitId, out var spawnedPosition));
            Assert.AreEqual(start, spawnedPosition);

            var actualSteps = new List<Vector2Int>();
            _signalBus.Subscribe<UnitMovedSignal>(signal =>
            {
                if (signal.UnitId == UnitId)
                {
                    actualSteps.Add(signal.NewPosition);
                }
            });

            Debug.Log($"[PlayModeTest] Starting visual movement check for '{UnitId}'. Path: {string.Join(" -> ", expectedPath.Select(p => p.ToString()))}");
            yield return new WaitForSeconds(0.5f);

            var moveTask = _unitMovementService.MoveUnitAsync(UnitId, target, CancellationToken.None);

            var previousPosition = unitObject.transform.position;
            var observedFrameMovement = false;

            while (!moveTask.IsCompleted)
            {
                var currentPosition = unitObject.transform.position;
                if ((currentPosition - previousPosition).sqrMagnitude > 0.0001f)
                {
                    observedFrameMovement = true;
                }

                previousPosition = currentPosition;
                yield return null;
            }

            if (moveTask.IsFaulted)
            {
                Assert.Fail($"Movement task failed: {moveTask.Exception}");
            }

            yield return new WaitForSeconds(1.0f);

            Assert.IsTrue(observedFrameMovement, "Expected to observe transform movement across frames.");

            var expectedSteps = expectedPath.Skip(1).ToList();
            Assert.AreEqual(expectedSteps, actualSteps, "PlayMode movement should follow the best path step-by-step.");

            Assert.IsTrue(_unitService.TryGetUnitPosition(UnitId, out var finalPosition));
            Assert.AreEqual(target, finalPosition, "UnitService should keep the final grid position in sync.");

            Assert.IsTrue(_objectsMapService.IsOccupied(target), "Target tile must be occupied after movement.");
            Assert.IsTrue(_objectsMapService.TryGetOccupant(target, out var occupantId));
            Assert.AreEqual(UnitId, occupantId);

            Assert.AreEqual(target.x, Mathf.RoundToInt(unitObject.transform.position.x));
            Assert.AreEqual(target.y, Mathf.RoundToInt(unitObject.transform.position.y));

            DisposeServices();
            yield return null;
        }

        private void SetupServices()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<UnitCreatedSignal>();
            _container.DeclareSignal<UnitMovedSignal>();
            _container.DeclareSignal<UnitDestroyedSignal>();
            _container.DeclareSignal<InterruptMovementSignal>();
            _container.DeclareSignal<OnMapObjectSpawnedSignal>();
            _container.DeclareSignal<OnObjectsMapChangedSignal>();

            _signalBus = _container.Resolve<SignalBus>();
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
                    MoveDurationPerTile = 0.45f,
                    DelayOnTile = 0.1f
                },
                baseStamina: 100f);

            _objectsMapService = CreateInternal<IObjectsMapService>(
                typeof(IObjectsMapService).Assembly,
                "Kruty1918.Moyva.ObjectsMap.Runtime.ObjectsMapService",
                _signalBus);
            _objectsMapInstance = _objectsMapService;
            _objectsMapDisposable = _objectsMapService as System.IDisposable;

            _unitService = CreateInternal<IUnitService>(
                typeof(IUnitService).Assembly,
                "Kruty1918.Moyva.Units.Runtime.UnitService",
                _signalBus,
                _gridService,
                _tileSettingsService,
                _unitClassConfig);
            _unitServiceInstance = _unitService;
            _unitServiceDisposable = _unitService as System.IDisposable;

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
            _unitMovementDisposable = _unitMovementService as System.IDisposable;

            InvokeIfExists(_objectsMapInstance, "Initialize");
            InvokeIfExists(_unitServiceInstance, "Initialize");
            InvokeIfExists(_unitMovementInstance, "Initialize");
        }

        private void DisposeServices()
        {
            _unitMovementDisposable?.Dispose();
            _unitMovementDisposable = null;

            _unitServiceDisposable?.Dispose();
            _unitServiceDisposable = null;

            _objectsMapDisposable?.Dispose();
            _objectsMapDisposable = null;

            foreach (var obj in _sceneObjects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            _sceneObjects.Clear();

            _container = null;
        }

        private void MarkExpensiveCorridor()
        {
            var expensiveTiles = new[] { new Vector2Int(3, 5), new Vector2Int(4, 5), new Vector2Int(5, 5), new Vector2Int(6, 5) };
            foreach (var tile in expensiveTiles)
            {
                _gridService.SetTileData(tile, new TileData { TileTypeId = SwampTile });
            }
        }

        private Camera CreateCamera(Vector3 position)
        {
            var cameraObject = new GameObject("PlayMode Test Camera");
            cameraObject.transform.position = position;

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3f;
            camera.backgroundColor = new Color(0.15f, 0.17f, 0.2f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            _sceneObjects.Add(cameraObject);
            return camera;
        }

        private GameObject CreateVisibleUnitObject(string unitId, Vector2Int start)
        {
            var go = new GameObject(unitId);
            go.transform.position = new Vector3(start.x, start.y, 0f);

            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 10;

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(0.85f, 0.2f, 0.2f));
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

            public int GridWidth { get; }
            public int GridHeight { get; }

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