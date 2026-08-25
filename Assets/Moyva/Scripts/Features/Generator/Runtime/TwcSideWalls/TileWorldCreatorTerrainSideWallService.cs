using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallService : ITileWorldCreatorTerrainSideWallService
    {
        private readonly ITileWorldCreatorTerrainSideWallComponentService _components;
        private readonly ITileWorldCreatorTerrainSideWallMeshBuilder _meshBuilder;

        public TileWorldCreatorTerrainSideWallService(
            ITileWorldCreatorTerrainSideWallComponentService components,
            ITileWorldCreatorTerrainSideWallMeshBuilder meshBuilder)
        {
            _components = components;
            _meshBuilder = meshBuilder;
        }

        public void Configure(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            TileWorldCreatorTerrainSideWallConfig config)
        {
            _components.Ensure(state, owner, config);
            state.LastConfig = config;
            Rebuild(state, config);
        }

        public void RebuildFromLastConfiguration(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            string reason)
        {
            if (state.Mesh == null)
                _components.Ensure(state, owner, state.LastConfig);
            Rebuild(state, state.LastConfig);
        }

        public void ClearWalls(TileWorldCreatorTerrainSideWallState state, string reason)
        {
            if (state.Mesh != null)
                state.Mesh.Clear();

        }

        public void Dispose(TileWorldCreatorTerrainSideWallState state)
        {
            DestroyRuntimeObject(state.Mesh);
            DestroyRuntimeObject(state.RuntimeMaterial);
            state.Mesh = null;
            state.RuntimeMaterial = null;
        }

        private void Rebuild(TileWorldCreatorTerrainSideWallState state, TileWorldCreatorTerrainSideWallConfig config)
        {
            if (state.Mesh == null)
                return;

            _meshBuilder.Build(state, config);
        }

        private static void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(instance);
            else
                Object.DestroyImmediate(instance);
        }
    }
}
