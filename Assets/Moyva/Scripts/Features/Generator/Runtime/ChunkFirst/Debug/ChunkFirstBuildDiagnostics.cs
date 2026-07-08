using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstBuildDiagnostics
    {
        private const string LogTag = "[MoyvaChunkFirst]";

        public void LogStart(TileWorldCreatorTerrainBuildMode mode, GeneratedWorldData worldData, int chunkSize)
        {
            Debug.Log($"{LogTag} START mode={mode}, map={worldData?.Width ?? 0}x{worldData?.Height ?? 0}, chunkSize={chunkSize}, hasStackMap={worldData?.LogicalTileMap != null}.");
        }

        public void LogPlan(int chunkCount, int stackSamples, int resolvedTerrain, int objectCandidates)
        {
            Debug.Log($"{LogTag} PLAN chunks={chunkCount}, stackSamples={stackSamples}, resolvedTerrain={resolvedTerrain}, objectCandidates={objectCandidates}.");
        }

        public void LogChunkMesh(string chunkName, int vertices, int indices)
        {
            Debug.Log($"{LogTag} CHUNK mesh='{chunkName}', vertices={vertices}, indices={indices}.");
        }

        public void LogLegacyAttempt(string caller)
        {
            Debug.LogError($"{LogTag} Legacy TWC visual build attempted during chunk-first mode. caller={caller}");
        }

        public void LogTwcVisualCleanup(int layerObjects, int orphanClusters)
        {
            Debug.Log($"{LogTag} TWC visual cleanup layerObjects={layerObjects}, orphanClusters={orphanClusters}.");
        }

        public void LogFailure(string reason)
        {
            Debug.LogError($"{LogTag} FAILED {reason}");
        }

        public void LogComplete(int chunksBuilt, int objectsSpawned)
        {
            Debug.Log($"{LogTag} COMPLETE chunksBuilt={chunksBuilt}, objectsSpawned={objectsSpawned}, legacyTwcVisualBuildAttempted=false.");
        }
    }
}
