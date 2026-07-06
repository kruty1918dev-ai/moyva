using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public readonly struct TileWorldCreatorTerrainBuildPolicyResult
    {
        public TileWorldCreatorTerrainBuildPolicyResult(TileWorldCreatorTerrainBuildMode mode, int chunkSizeTiles, bool applyHeights)
        {
            Mode = mode;
            ChunkSizeTiles = Mathf.Max(0, chunkSizeTiles);
            UsesPrecomputedHeights = applyHeights && mode == TileWorldCreatorTerrainBuildMode.MergedChunksWithPrecomputedHeights;
            UsesLegacyHeightProjection = applyHeights && mode == TileWorldCreatorTerrainBuildMode.LegacyPostBuildHeightProjection;
        }

        public TileWorldCreatorTerrainBuildMode Mode { get; }
        public int ChunkSizeTiles { get; }
        public bool UsesPrecomputedHeights { get; }
        public bool UsesLegacyHeightProjection { get; }
        public bool ForceMergeTiles => !UsesLegacyHeightProjection;
    }

    public static class TileWorldCreatorTerrainBuildPolicy
    {
        private static readonly ITileWorldCreatorTerrainBuildPolicyService DefaultService = new TileWorldCreatorTerrainBuildPolicyService();

        public static TileWorldCreatorTerrainBuildPolicyResult Resolve(TileWorldCreatorBuildOptions options, int chunkSizeTiles)
            => DefaultService.Resolve(options, chunkSizeTiles);

        public static void Apply(Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult policy, string source)
            => DefaultService.Apply(configuration, policy, source);
    }

    public interface ITileWorldCreatorTerrainBuildPolicyService
    {
        TileWorldCreatorTerrainBuildPolicyResult Resolve(TileWorldCreatorBuildOptions options, int chunkSizeTiles);
        void Apply(Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult policy, string source);
    }

    public sealed class TileWorldCreatorTerrainBuildPolicyService : ITileWorldCreatorTerrainBuildPolicyService
    {
        private const string LogTag = "[MoyvaTWCChunks]";

        public TileWorldCreatorTerrainBuildPolicyResult Resolve(TileWorldCreatorBuildOptions options, int chunkSizeTiles)
            => new TileWorldCreatorTerrainBuildPolicyResult(
                options?.TerrainBuildMode ?? TileWorldCreatorTerrainBuildMode.MergedChunksWithPrecomputedHeights,
                chunkSizeTiles,
                options?.ApplyIntegerTerrainHeights ?? false);

        public void Apply(Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult policy, string source)
        {
            if (configuration == null)
                return;

            Debug.Log(
                $"{LogTag} Terrain build policy apply: source={source}, mode={policy.Mode}, chunk={policy.ChunkSizeTiles}, " +
                $"forceMergeTiles={policy.ForceMergeTiles}, precomputedHeights={policy.UsesPrecomputedHeights}, legacyProjector={policy.UsesLegacyHeightProjection}.");
            TileWorldCreatorChunkBatchingUtility.Apply(configuration, policy.ChunkSizeTiles, policy.ForceMergeTiles, source);
        }
    }
}
