using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    [TestFixture]
    public sealed class FogClusterMeshBuilderTests
    {
        [Test]
        public void RebuildCluster_AddsSideBetweenRenderedCellsWithDifferentHeights()
        {
            var settings = CreateSettings();
            var builder = new FogClusterMeshBuilder(settings);
            var flatMesh = new Mesh();
            var steppedMesh = new Mesh();

            builder.RebuildCluster(
                new FogClusterKey(0, 0),
                flatMesh,
                CreateContext(CreateHeightMap(0f, 0f)),
                CreateFogStateReader(FogStateType.Unexplored, FogStateType.Unexplored));
            builder.RebuildCluster(
                new FogClusterKey(0, 0),
                steppedMesh,
                CreateContext(CreateHeightMap(1f, 0f)),
                CreateFogStateReader(FogStateType.Unexplored, FogStateType.Unexplored));

            Assert.Greater(steppedMesh.vertexCount, flatMesh.vertexCount);

            Object.DestroyImmediate(flatMesh);
            Object.DestroyImmediate(steppedMesh);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void RebuildCluster_ExtendsBoundarySideToVisibleLowerNeighborSurface()
        {
            var settings = CreateSettings();
            var builder = new FogClusterMeshBuilder(settings);
            var mesh = new Mesh();

            builder.RebuildCluster(
                new FogClusterKey(0, 0),
                mesh,
                CreateContext(CreateHeightMap(2f, 0f)),
                CreateFogStateReader(FogStateType.Unexplored, FogStateType.Visible));

            Assert.LessOrEqual(mesh.bounds.min.y, 0f);

            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(settings);
        }

        private static FogOfWarSettings CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<FogOfWarSettings>();
            settings.Volume.ClusterSize = 16;
            settings.Volume.LogClusterUpdates = false;
            settings.Volume.TopClearance = 0.08f;
            settings.Volume.HeightLayerSnap = 0.01f;
            settings.Volume.EnsureDefaults();
            return settings;
        }

        private static FogWorldVisualContext CreateContext(float[,] heightMap)
        {
            return new FogWorldVisualContext(
                2,
                1,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f,
                false,
                default,
                heightMap,
                null);
        }

        private static float[,] CreateHeightMap(float left, float right)
        {
            var heightMap = new float[2, 1];
            heightMap[0, 0] = left;
            heightMap[1, 0] = right;
            return heightMap;
        }

        private static IFogStateReader CreateFogStateReader(FogStateType left, FogStateType right)
        {
            var states = new FogStateType[2, 1];
            states[0, 0] = left;
            states[1, 0] = right;
            return new FakeFogStateReader(states);
        }

        private sealed class FakeFogStateReader : IFogStateReader
        {
            private readonly FogStateType[,] _states;

            public FakeFogStateReader(FogStateType[,] states)
            {
                _states = states;
            }

            public FogStateType GetFogState(Vector2Int position)
                => _states[position.x, position.y];

            public bool IsVisible(Vector2Int position)
                => GetFogState(position) == FogStateType.Visible;

            public bool IsExplored(Vector2Int position)
                => GetFogState(position) != FogStateType.Unexplored;
        }
    }
}
