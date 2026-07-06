using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionDiagnostics : ITileWorldCreatorHeightProjectionDiagnostics
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        public void LogConfigured(TileWorldCreatorHeightProjectionState state)
        {
            var root = state.TargetRoot;
            Debug.Log($"{LogTag} Projector.Configure target='{root.name}', levelMap={TileWorldCreatorHeightProjectionUtility.FormatLevelStats(state.TerrainLevelMap)}, cellSize={state.CellSize}, heightStep={state.HeightStep}, trackingSeconds={state.TrackingSecondsRemaining}, existingRenderers={root.GetComponentsInChildren<Renderer>(true).Length}, existingMeshFilters={root.GetComponentsInChildren<MeshFilter>(true).Length}.");
        }

        public void LogWorldStart(TileWorldCreatorHeightProjectionState state)
        {
            Debug.Log($"{WorldGenDiagTag} HeightProjector.START frame={Time.frameCount}, tileCount={TileWorldCreatorHeightProjectionUtility.CountLevelMapCells(state.TerrainLevelMap)}");
        }

        public void LogWorldEnd(TileWorldCreatorHeightProjectionState state)
        {
            Debug.Log($"{WorldGenDiagTag} HeightProjector.END frame={Time.frameCount}, tileCount={TileWorldCreatorHeightProjectionUtility.CountLevelMapCells(state.TerrainLevelMap)}, elapsedMs={(Time.realtimeSinceStartup - state.WorldGenDiagStartTime) * 1000f:0}");
        }

        public void LogPass(string message)
        {
            Debug.Log($"{LogTag} Projector {message}");
        }

        public string FormatSamples(System.Collections.Generic.List<string> samples)
        {
            return TileWorldCreatorHeightProjectionUtility.FormatSamples(samples);
        }
    }
}
