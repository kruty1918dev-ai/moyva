using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainVisualPostProcessor
    {
        void Apply(GeneratedWorldData worldData, Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy);
    }

    internal sealed class TileWorldCreatorTerrainVisualPostProcessor : ITileWorldCreatorTerrainVisualPostProcessor
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private const string SideWallLogTag = "[MoyvaTWCHeight:SideWalls]";
        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorBuildOptions _options;
        private readonly ITileWorldCreatorTerrainBaseHeightResolver _baseHeightResolver;

        public TileWorldCreatorTerrainVisualPostProcessor(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorTerrainBaseHeightResolver baseHeightResolver)
        {
            _manager = environment.Manager;
            _options = environment.Options;
            _baseHeightResolver = baseHeightResolver;
        }

        public void Apply(GeneratedWorldData worldData, Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            if (terrainPolicy.UsesLegacyHeightProjection)
                ApplyLegacyProjection(worldData, configuration);
            else if (terrainPolicy.UsesPrecomputedHeights)
                ApplyMergedVisuals(worldData, configuration);
        }

        private void ApplyLegacyProjection(GeneratedWorldData worldData, Configuration configuration)
        {
            int[,] levelMap = worldData?.TerrainLevelMap;
            if (levelMap == null)
            {
                Debug.LogWarning($"{LogTag} ApplyIntegerTerrainHeights skipped: TerrainLevelMap is null after fallback attempt.");
                return;
            }

            var managerGo = _manager.gameObject;
            var projector = managerGo.GetComponentInChildren<TileWorldCreatorHeightProjector>(true);
            if (projector == null)
            {
                var projectorGo = new GameObject("Moyva TWC Height Projector")
                {
                    hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
                projectorGo.transform.SetParent(managerGo.transform, false);
                projector = projectorGo.AddComponent<TileWorldCreatorHeightProjector>();
                projector.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            float cellSize = ResolveCellSize(configuration);
            projector.Configure(managerGo.transform, levelMap, cellSize, _options.TerrainHeightStep, _options.TerrainHeightTrackingSeconds);
            ApplySideWallsOrClear(managerGo.transform, levelMap, configuration, cellSize, "GenerateTerrainSideWalls option is disabled");
            ApplyTerrainMeshOptimizer(managerGo.transform);
        }

        private void ApplyMergedVisuals(GeneratedWorldData worldData, Configuration configuration)
        {
            int[,] levelMap = worldData?.TerrainLevelMap;
            if (levelMap == null)
                return;

            ApplySideWallsOrClear(
                _manager.transform,
                levelMap,
                configuration,
                ResolveCellSize(configuration),
                "GenerateTerrainSideWalls option is disabled in merged terrain mode");
        }

        private void ApplySideWallsOrClear(Transform managerRoot, int[,] levelMap, Configuration configuration, float cellSize, string clearReason)
        {
            if (!_options.GenerateTerrainSideWalls)
            {
                ClearSideWalls(managerRoot, clearReason);
                return;
            }

            var wallBuilder = managerRoot.GetComponentInChildren<TileWorldCreatorTerrainSideWallBuilder>(true);
            if (wallBuilder == null)
            {
                var wallGo = new GameObject("Moyva TWC Terrain Side Walls")
                {
                    hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
                wallGo.transform.SetParent(managerRoot, false);
                wallBuilder = wallGo.AddComponent<TileWorldCreatorTerrainSideWallBuilder>();
                wallBuilder.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            float baseY = _baseHeightResolver.ResolveTerrainBaseHeight(configuration);
            Debug.Log($"{SideWallLogTag} Configure side walls from post processor: manager='{managerRoot.name}', cellSize={cellSize}, heightStep={_options.TerrainHeightStep}, baseY={baseY:0.###}, material='{(_options.TerrainSideWallMaterial != null ? _options.TerrainSideWallMaterial.name : "<runtime>")}', color={TileWorldCreatorMapFormatUtility.FormatColor(_options.TerrainSideWallColor)}, levelStats={TileWorldCreatorMapFormatUtility.FormatLevelStats(levelMap)}.");
            wallBuilder.Configure(managerRoot, levelMap, cellSize, _options.TerrainHeightStep, baseY, _options.TerrainSideWallMaterial, _options.TerrainSideWallColor, _options.GenerateTerrainSideWallsAtMapBorder);
        }

        private void ApplyTerrainMeshOptimizer(Transform managerRoot)
        {
            var optimizer = managerRoot.GetComponentInChildren<TileWorldCreatorRuntimeMeshOptimizer>(true);
            if (!_options.CombineTerrainMeshesAfterHeightProjection)
            {
                optimizer?.ClearConfiguration("CombineTerrainMeshesAfterHeightProjection option is disabled");
                return;
            }

            if (optimizer == null)
            {
                var optimizerGo = new GameObject("Moyva TWC Runtime Mesh Optimizer") { hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild };
                optimizerGo.transform.SetParent(managerRoot, false);
                optimizer = optimizerGo.AddComponent<TileWorldCreatorRuntimeMeshOptimizer>();
                optimizer.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            optimizer.Configure(managerRoot, _options.TerrainMeshCombineClustersPerFrame, _options.TerrainMeshCombineDeactivateSourceObjects);
        }

        private static void ClearSideWalls(Transform managerRoot, string reason)
            => managerRoot.GetComponentInChildren<TileWorldCreatorTerrainSideWallBuilder>(true)?.ClearWalls(reason);

        private static float ResolveCellSize(Configuration configuration)
            => configuration != null && configuration.cellSize > 0.0001f ? configuration.cellSize : 1f;
    }
}
