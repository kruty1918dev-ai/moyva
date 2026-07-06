using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionStableActionService : ITileWorldCreatorHeightProjectionStableActionService
    {
        public void Execute(TileWorldCreatorHeightProjectionState state, TileWorldCreatorHeightProjectionPassResult result)
        {
            RefreshSideWalls(state, result);
            OptimizeMeshes(state, result);
        }

        private static void RefreshSideWalls(TileWorldCreatorHeightProjectionState state, TileWorldCreatorHeightProjectionPassResult result)
        {
            if (state.SideWallsRefreshedAfterStable || result.Root == null)
                return;

            var builder = result.Root.GetComponentInChildren<TileWorldCreatorTerrainSideWallBuilder>(true);
            if (builder == null)
                return;

            state.SideWallsRefreshedAfterStable = true;
            builder.RebuildFromLastConfiguration(BuildReason(state, result));
        }

        private static void OptimizeMeshes(TileWorldCreatorHeightProjectionState state, TileWorldCreatorHeightProjectionPassResult result)
        {
            if (state.MeshOptimizationRequestedAfterStable || result.Root == null)
                return;

            var optimizer = result.Root.GetComponentInChildren<TileWorldCreatorRuntimeMeshOptimizer>(true);
            if (optimizer == null)
                return;

            state.MeshOptimizationRequestedAfterStable = true;
            optimizer.RequestOptimizeAfterStable(BuildReason(state, result));
        }

        private static string BuildReason(TileWorldCreatorHeightProjectionState state, TileWorldCreatorHeightProjectionPassResult result)
        {
            return $"height projector stable pass={state.ApplyPassIndex}, renderers={result.RendererCount}, tileTransforms={result.TileTransformCount}, usedCells={result.UsedCellCount}/{result.TotalCellCount}";
        }
    }
}
