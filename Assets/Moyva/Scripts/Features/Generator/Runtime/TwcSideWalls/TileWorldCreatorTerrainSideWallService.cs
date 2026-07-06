using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTerrainSideWallService : ITileWorldCreatorTerrainSideWallService
    {
        private readonly ITileWorldCreatorTerrainSideWallComponentService _components;
        private readonly ITileWorldCreatorTerrainSideWallMeshBuilder _meshBuilder;
        private readonly ITileWorldCreatorTerrainSideWallDiagnostics _diagnostics;

        public TileWorldCreatorTerrainSideWallService(
            ITileWorldCreatorTerrainSideWallComponentService components,
            ITileWorldCreatorTerrainSideWallMeshBuilder meshBuilder,
            ITileWorldCreatorTerrainSideWallDiagnostics diagnostics)
        {
            _components = components;
            _meshBuilder = meshBuilder;
            _diagnostics = diagnostics;
        }

        public void Configure(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            TileWorldCreatorTerrainSideWallConfig config)
        {
            _components.Ensure(state, owner, config);
            state.LastConfig = config;
            _diagnostics.LogConfigure(owner, state, config);
            Rebuild(state, config);
        }

        public void RebuildFromLastConfiguration(
            TileWorldCreatorTerrainSideWallState state,
            TileWorldCreatorTerrainSideWallBuilder owner,
            string reason)
        {
            _diagnostics.LogDelayedRebuild(state, reason);
            if (state.Mesh == null)
                _components.Ensure(state, owner, state.LastConfig);
            Rebuild(state, state.LastConfig);
        }

        public void ClearWalls(TileWorldCreatorTerrainSideWallState state, string reason)
        {
            if (state.Mesh != null)
                state.Mesh.Clear();

            _diagnostics.LogCleared(reason);
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

            var result = _meshBuilder.Build(state, config);
            if (result.Skipped)
            {
                _diagnostics.LogSkipped(result.SkipReason);
                return;
            }

            _diagnostics.LogBuildResult(state, result);
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
