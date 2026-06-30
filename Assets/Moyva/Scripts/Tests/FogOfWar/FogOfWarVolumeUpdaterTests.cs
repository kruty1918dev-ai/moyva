using System.Collections.Generic;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    [TestFixture]
    public sealed class FogOfWarVolumeUpdaterTests
    {
        [Test]
        public void RebuildFullVisual_MapsFogStatesToUnexploredAndExploredLayers()
        {
            var settings = CreateSettings();
            var updater = new FogOfWarVolumeUpdater(settings);
            var fog = new FakeFogService(3, 2);
            fog.SetState(new Vector2Int(0, 0), FogStateType.Unexplored);
            fog.SetState(new Vector2Int(1, 0), FogStateType.Explored);
            fog.SetState(new Vector2Int(2, 0), FogStateType.Visible);

            updater.Initialize(3, 2, CreateContext(3, 2));
            updater.RebuildFullVisual(fog);
            updater.Tick();

            Assert.IsTrue(updater.DebugHasUnexploredCell(new Vector2Int(0, 0)));
            Assert.IsTrue(updater.DebugHasExploredCell(new Vector2Int(1, 0)));
            Assert.IsFalse(updater.DebugHasUnexploredCell(new Vector2Int(2, 0)));
            Assert.IsFalse(updater.DebugHasExploredCell(new Vector2Int(2, 0)));

            Object.DestroyImmediate(settings);
            updater.Dispose();
        }

        [Test]
        public void UpdateDirtyTiles_MovesCellsBetweenFogStateLayers()
        {
            var settings = CreateSettings();
            var updater = new FogOfWarVolumeUpdater(settings);
            var fog = new FakeFogService(2, 1);
            fog.SetState(new Vector2Int(0, 0), FogStateType.Unexplored);
            fog.SetState(new Vector2Int(1, 0), FogStateType.Visible);

            updater.Initialize(2, 1, CreateContext(2, 1));
            updater.RebuildFullVisual(fog);
            updater.Tick();

            fog.SetState(new Vector2Int(0, 0), FogStateType.Visible);
            fog.SetState(new Vector2Int(1, 0), FogStateType.Explored);
            updater.UpdateDirtyTiles(fog, new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) });
            updater.Tick();

            Assert.IsFalse(updater.DebugHasUnexploredCell(new Vector2Int(0, 0)));
            Assert.IsFalse(updater.DebugHasExploredCell(new Vector2Int(0, 0)));
            Assert.IsTrue(updater.DebugHasExploredCell(new Vector2Int(1, 0)));

            Object.DestroyImmediate(settings);
            updater.Dispose();
        }

        [Test]
        public void RuntimeSetup_DoesNotMutateSourceTileWorldCreatorConfiguration()
        {
            var settings = CreateSettings();
            var sourceConfiguration = ScriptableObject.CreateInstance<Configuration>();
            sourceConfiguration.blueprintLayerFolders = new List<BlueprintLayerFolder>();
            sourceConfiguration.buildLayerFolders = new List<BuildLayerFolder>();

            var managerObject = new GameObject("Fog TWC Manager");
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            manager.configuration = sourceConfiguration;
            var controller = managerObject.AddComponent<FogOfWarVolumeController>();
            SetPrivateField(controller, "_settings", settings);

            var updater = new FogOfWarVolumeUpdater(settings);
            var fog = new FakeFogService(2, 2);
            updater.AttachController(controller);
            updater.Initialize(2, 2, CreateContext(2, 2));
            updater.RebuildFullVisual(fog);

            Assert.AreNotSame(sourceConfiguration, manager.configuration);
            Assert.AreEqual(0, sourceConfiguration.blueprintLayerFolders.Count);
            Assert.AreEqual(0, sourceConfiguration.buildLayerFolders.Count);
            Assert.AreEqual(2, updater.DebugRuntimeConfiguration.blueprintLayerFolders[0].blueprintLayers.Count);
            Assert.AreEqual(2, updater.DebugRuntimeConfiguration.buildLayerFolders[0].buildLayers.Count);

            updater.Dispose();
            Assert.AreSame(sourceConfiguration, manager.configuration);
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(sourceConfiguration);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void RebuildFullVisual_BuildsInitialVolumeImmediately()
        {
            var settings = CreateSettings();
            var managerObject = new GameObject("Fog TWC Manager");
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            var controller = managerObject.AddComponent<FogOfWarVolumeController>();
            SetPrivateField(controller, "_settings", settings);

            var updater = new FogOfWarVolumeUpdater(settings);
            var fog = new FakeFogService(2, 1);
            fog.SetState(new Vector2Int(0, 0), FogStateType.Unexplored);
            fog.SetState(new Vector2Int(1, 0), FogStateType.Visible);

            updater.AttachController(controller);
            updater.Initialize(2, 1, CreateContext(2, 1));
            updater.RebuildFullVisual(fog);

            Assert.IsNotNull(manager.configuration);
            Assert.AreSame(updater.DebugRuntimeConfiguration, manager.configuration);
            Assert.AreEqual(1, updater.DebugRuntimeConfiguration.blueprintLayerFolders[0].blueprintLayers.Count);

            updater.Dispose();
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void RuntimeSetup_CreatesSeparateFogLayersForGeneratedTerrainHeights()
        {
            var settings = CreateSettings();
            var managerObject = new GameObject("Fog TWC Manager");
            managerObject.AddComponent<TileWorldCreatorManager>();
            var controller = managerObject.AddComponent<FogOfWarVolumeController>();
            SetPrivateField(controller, "_settings", settings);

            var updater = new FogOfWarVolumeUpdater(settings);
            var fog = new FakeFogService(2, 1);
            fog.SetState(new Vector2Int(0, 0), FogStateType.Unexplored);
            fog.SetState(new Vector2Int(1, 0), FogStateType.Unexplored);

            updater.AttachController(controller);
            updater.Initialize(2, 1, CreateContext(2, 1, new[,] { { 0 }, { 2 } }));
            updater.RebuildFullVisual(fog);
            updater.Tick();

            Assert.AreEqual(2, updater.DebugRuntimeConfiguration.blueprintLayerFolders[0].blueprintLayers.Count);
            Assert.AreEqual(2, updater.DebugRuntimeConfiguration.buildLayerFolders[0].buildLayers.Count);
            Assert.AreNotEqual(
                updater.DebugRuntimeConfiguration.blueprintLayerFolders[0].blueprintLayers[0].defaultLayerHeight,
                updater.DebugRuntimeConfiguration.blueprintLayerFolders[0].blueprintLayers[1].defaultLayerHeight);

            updater.Dispose();
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(settings);
        }

        private static FogOfWarSettings CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<FogOfWarSettings>();
            settings.EnableStartupFallbackReveal = false;
            settings.Volume.EnsureDefaults();
            settings.Volume.Unexplored.Enabled = true;
            settings.Volume.Explored.Enabled = true;
            return settings;
        }

        private static FogWorldVisualContext CreateContext(int width, int height)
            => CreateContext(width, height, null);

        private static FogWorldVisualContext CreateContext(int width, int height, int[,] terrainLevelMap)
        {
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f,
                false,
                default,
                null,
                terrainLevelMap);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' was not found.");
            field.SetValue(target, value);
        }

        private sealed class FakeFogService : IFogOfWarService
        {
            private readonly FogStateType[,] _states;

            public FakeFogService(int width, int height)
            {
                _states = new FogStateType[width, height];
            }

            public void SetState(Vector2Int position, FogStateType state)
            {
                _states[position.x, position.y] = state;
            }

            public void Initialize(int width, int height) { }
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position) => _states[position.x, position.y];
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
            public bool IsExplored(Vector2Int position) => GetFogState(position) != FogStateType.Unexplored;
            public bool[,] GetExploredSnapshot() => new bool[_states.GetLength(0), _states.GetLength(1)];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }
    }
}
