using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    [TestFixture]
    public class WorldLoadPipelineTests : ZenjectUnitTestFixture
    {
        private sealed class FakeMapObjectRegistry : IMapObjectRegistryService
        {
            public bool TryGetDefinition(string id, out MapObjectDefinition definition)
            {
                definition = null;
                return false;
            }
        }
        private sealed class FakeGridService : IGridService
        {
            private readonly string[,] _grid;

            public FakeGridService(int width, int height)
            {
                GridWidth = width;
                GridHeight = height;
                _grid = new string[width, height];
            }

            public string GetTileData(Vector2Int position) => _grid[position.x, position.y];

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                tileTypeId = _grid[position.x, position.y];
                return !string.IsNullOrEmpty(tileTypeId);
            }

            public void SetTileData(Vector2Int position, string tileTypeId)
            {
                _grid[position.x, position.y] = tileTypeId;
            }

            public int GridWidth { get; }
            public int GridHeight { get; }
        }

        private sealed class FakeMapDataGenerator : IMapDataGenerator
        {
            public bool WasGenerateCalled;

            public void GenerateMapData(int width, int height, System.Action<string[,], string[,], float[,], string[,]> onComplete)
            {
                WasGenerateCalled = true;

                var biomes = new string[width, height];
                var objects = new string[width, height];
                var heights = new float[width, height];
                var buildings = new string[width, height];
                onComplete?.Invoke(biomes, objects, heights, buildings);
            }
        }

        private sealed class FakeUnitService : IUnitService
        {
            private readonly Dictionary<string, float> _stamina = new();
            private readonly Dictionary<string, Vector2Int> _positions = new();
            private readonly Dictionary<string, string> _typeIds = new();
            private readonly Dictionary<string, GameObject> _objects = new();

            public void Seed(string unitId, string typeId, Vector2Int position, float stamina)
            {
                _typeIds[unitId] = typeId;
                _positions[unitId] = position;
                _stamina[unitId] = stamina;
                _objects[unitId] = new GameObject(unitId);
            }

            public string RegisterCreated(string typeId, Vector2Int position)
            {
                string unitId = $"{typeId}_{_typeIds.Count + 1:D2}";
                _typeIds[unitId] = typeId;
                _positions[unitId] = position;
                _stamina[unitId] = 0f;
                _objects[unitId] = new GameObject(unitId);
                return unitId;
            }

            public float GetStamina(string unitId) => _stamina.GetValueOrDefault(unitId, 0f);
            public void SetStamina(string unitId, float stamina)
            {
                if (_stamina.ContainsKey(unitId))
                    _stamina[unitId] = stamina;
            }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position) => _positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => _objects.TryGetValue(unitId, out var gameObject) ? gameObject : null;
            public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys;
            public string GetUnitTypeId(string unitId) => _typeIds.TryGetValue(unitId, out var typeId) ? typeId : null;
        }

        private sealed class FakeUnitFactory : IUnitFactory
        {
            private readonly FakeUnitService _unitService;
            public readonly List<(string typeId, Vector2Int position, string unitId)> Created = new();

            public FakeUnitFactory(FakeUnitService unitService)
            {
                _unitService = unitService;
            }

            public string CreateUnit(string typeId, Vector2Int gridPosition)
            {
                string unitId = _unitService.RegisterCreated(typeId, gridPosition);
                Created.Add((typeId, gridPosition, unitId));
                return unitId;
            }

            public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId)
            {
                return CreateUnit(typeId, gridPosition);
            }
        }

        private SignalBus _signalBus;
        private List<Object> _createdUnityObjects;

        public override void Setup()
        {
            base.Setup();

            _createdUnityObjects = new List<Object>();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<WorldBuiltSignal>();
            Container.DeclareSignal<WorldGeneratedDataSignal>();
        }

        public override void Teardown()
        {
            foreach (var unityObject in _createdUnityObjects)
            {
                if (unityObject != null)
                    Object.DestroyImmediate(unityObject);
            }

            base.Teardown();
        }

        [Test]
        public void LoadSavedWorld_BuildsWorld_BeforeUnitsAreRestored()
        {
            _signalBus = Container.Resolve<SignalBus>();

            var tileRegistry = ScriptableObject.CreateInstance<TileRegistrySO>();
            _createdUnityObjects.Add(tileRegistry);
            SetTileDefinitions(tileRegistry, "grass", "river");

            var prefab = new GameObject("TilePrefab");
            _createdUnityObjects.Add(prefab);

            var grid = new FakeGridService(2, 2);
            var dataGenerator = new FakeMapDataGenerator();
            var container = new DiContainer();
            container.Inject(tileRegistry);

            var instantiator = new MapVisualInstantiator(tileRegistry, new FakeMapObjectRegistry(), null, null, null, grid, dataGenerator, container, _signalBus);
            instantiator.Initialize();

            var sourceWorld = new GeneratedWorldData
            {
                Width = 2,
                Height = 2,
                BiomeMap = new[,] { { "grass", "grass" }, { "grass", "grass" } },
                ObjectMap = new[,] { { string.Empty, "river" }, { string.Empty, string.Empty } },
                HeightMap = new[,] { { 0.1f, 0.2f }, { 0.3f, 0.4f } },
            };

            instantiator.SetPendingWorldData(sourceWorld);
            instantiator.BuildWorld();

            Assert.IsTrue(instantiator.TryGetCurrentWorldData(out var builtWorld));
            Assert.AreEqual("river", builtWorld.ObjectMap[0, 1]);

            byte[] worldPayload = SaveModulePayload(new GeneratedWorldSaveModule(instantiator));

            var sourceUnitService = new FakeUnitService();
            sourceUnitService.Seed("warrior_seed", "warrior", new Vector2Int(1, 1), 7.5f);
            var sourceUnitsModule = new UnitsSaveModule(sourceUnitService, new FakeUnitFactory(sourceUnitService), _signalBus);
            sourceUnitsModule.Initialize();
            byte[] unitPayload = SaveModulePayload(sourceUnitsModule);
            sourceUnitsModule.Dispose();

            var freshGrid = new FakeGridService(2, 2);
            var freshGenerator = new FakeMapDataGenerator();
            var freshContainer = new DiContainer();
            var freshInstantiator = new MapVisualInstantiator(tileRegistry, new FakeMapObjectRegistry(), null, null, null, freshGrid, freshGenerator, freshContainer, _signalBus);
            freshInstantiator.Initialize();

            var loadWorldModule = new GeneratedWorldSaveModule(freshInstantiator);
            var targetUnitService = new FakeUnitService();
            var targetUnitFactory = new FakeUnitFactory(targetUnitService);
            var loadUnitsModule = new UnitsSaveModule(targetUnitService, targetUnitFactory, _signalBus);
            loadUnitsModule.Initialize();

            LoadModulePayload(loadWorldModule, worldPayload);
            LoadModulePayload(loadUnitsModule, unitPayload);

            Assert.AreEqual(0, targetUnitFactory.Created.Count, "Units should wait until world is built.");

            freshInstantiator.BuildWorld();

            Assert.IsFalse(freshGenerator.WasGenerateCalled, "Saved world must be used instead of generating new map.");
            Assert.AreEqual(1, targetUnitFactory.Created.Count);
            Assert.AreEqual(new Vector2Int(1, 1), targetUnitFactory.Created[0].position);
            Assert.AreEqual("warrior", targetUnitFactory.Created[0].typeId);
            Assert.AreEqual(7.5f, targetUnitService.GetStamina(targetUnitFactory.Created[0].unitId), 0.001f);
            Assert.AreEqual("river", freshGrid.GetTileData(new Vector2Int(0, 1)));

            loadUnitsModule.Dispose();
        }

        private static byte[] SaveModulePayload(ISaveModule module)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            module.OnSave(new SaveContext(writer, null));
            writer.Flush();
            return stream.ToArray();
        }

        private static void LoadModulePayload(ISaveModule module, byte[] payload)
        {
            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream);
            module.OnLoad(new SaveContext(null, reader));
        }

        private static void SetTileDefinitions(TileRegistrySO registry, string biomeId, string objectId)
        {
            var prefab = new GameObject("TileViewPrefab");
            var definitions = new[]
            {
                CreateDefinition(biomeId, prefab),
                CreateDefinition(objectId, prefab),
            };

            var field = typeof(TileRegistrySO).GetField("_definitions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(registry, definitions);
        }

        private static TileTypeDefinition CreateDefinition(string id, GameObject prefab)
        {
            var definition = new TileTypeDefinition();
            SetPrivateField(definition, "_id", id);
            SetPrivateField(definition, "_movementCost", 1f);
            SetPrivateField(definition, "_visualPrefab", prefab);
            return definition;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(instance, value);
        }
    }
}