using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstBuildDiagnostics
    {
        private const string LogTag = "[MoyvaChunkFirst]";

        public void LogStart(TileWorldCreatorTerrainBuildMode mode, GeneratedWorldData worldData, int chunkSize)
        {
        }

        public void LogPlan(int chunkCount, int stackSamples, int resolvedTerrain, int objectCandidates)
        {
        }

        public void LogChunkMesh(
            string chunkName,
            int sourceVertices,
            int sourceIndices,
            int sourceTriangles,
            int processedVertices,
            int processedIndices,
            int processedTriangles,
            int emittedVertices,
            int emittedIndices,
            int emittedTriangles,
            int unreferencedVerticesRemoved,
            int exactDuplicateVerticesRemoved)
        {
            int culledFaces = Mathf.Max(
                0,
                sourceTriangles - processedTriangles);
        }

        public void LogLegacyAttempt(string caller)
        {
            Debug.LogError($"{LogTag} Legacy TWC visual build attempted during chunk-first mode. caller={caller}");
        }

        public void LogTwcVisualCleanup(int layerObjects, int orphanClusters)
        {
        }

        public void LogFailure(string reason)
        {
            Debug.LogError($"{LogTag} FAILED {reason}");
        }

        public void LogComplete(int chunksBuilt, int objectsSpawned)
        {
        }
    }
}
