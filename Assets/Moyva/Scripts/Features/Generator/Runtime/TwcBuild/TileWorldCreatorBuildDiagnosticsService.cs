using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorBuildDiagnosticsService
    {
        void LogBuildStart(GeneratedWorldData worldData, Configuration configuration);
        void LogLevelMap(string label, int[,] levelMap);
        void LogMappedLayerSummary(string label, Dictionary<string, HashSet<Vector2>> positions, HashSet<string> mappedIds, Configuration configuration);
        int CountComponentsInManager<T>() where T : Component;
        int CountManagerChildren();
    }

    internal sealed class TileWorldCreatorBuildDiagnosticsService : ITileWorldCreatorBuildDiagnosticsService
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorIdMappingSO _mapping;
        private readonly TileWorldCreatorBuildOptions _options;

        public TileWorldCreatorBuildDiagnosticsService(ITileWorldCreatorBuildEnvironment environment)
        {
            _manager = environment.Manager;
            _mapping = environment.Mapping;
            _options = environment.Options;
        }

        public void LogBuildStart(GeneratedWorldData worldData, Configuration configuration)
        {
            LogConfigurationFolders(configuration);
            LogLevelMap("initial", worldData.TerrainLevelMap);
        }

        public void LogLevelMap(string label, int[,] levelMap)
        {
        }

        public void LogMappedLayerSummary(string label, Dictionary<string, HashSet<Vector2>> positions, HashSet<string> mappedIds, Configuration configuration)
        {
            var builder = new StringBuilder();
            builder.Append(LogTag).Append(' ')
                .Append("Mapped ").Append(label)
                .Append(": layers=").Append(positions.Count)
                .Append(", uniqueIds=").Append(mappedIds.Count)
                .Append(", ids=").Append(FormatStringSet(mappedIds, 24));

            foreach (var pair in positions)
            {
                string layerName = configuration.GetBlueprintLayerByGuid(pair.Key)?.layerName ?? "<missing blueprint>";
                builder.Append(" | ").Append(layerName)
                    .Append("(").Append(pair.Key).Append(")")
                    .Append(" count=").Append(pair.Value?.Count ?? 0)
                    .Append(" bounds=").Append(TileWorldCreatorMapFormatUtility.FormatPositionBounds(pair.Value));
            }
        }

        public int CountComponentsInManager<T>() where T : Component
            => _manager != null ? _manager.GetComponentsInChildren<T>(true).Length : 0;

        public int CountManagerChildren()
            => _manager != null ? _manager.GetComponentsInChildren<Transform>(true).Length - 1 : 0;

        private string FormatOptions()
            => $"replaceTerrain={_options.ReplaceMappedTerrainVisuals}, suppressMoyvaLayers={_options.SuppressMoyvaLayerDataWhenTerrainMapped}, resetConfig={_options.ResetConfigurationBeforeBuild}, syncSize={_options.SyncConfigurationSize}, useWorldSeed={_options.UseWorldSeed}, cellSizeOverride={_options.ConfigurationCellSizeOverride}, applyIntegerHeights={_options.ApplyIntegerTerrainHeights}, heightStep={_options.TerrainHeightStep}, trackingSeconds={_options.TerrainHeightTrackingSeconds}, normalizeLevels={_options.NormalizeTerrainLevelsForTileWorldCreator}, water={_options.WaterTerrainLevel}, shore={_options.ShoreTerrainLevel}, land={_options.LandTerrainLevel}, hill={_options.HillTerrainLevel}, max={_options.MaxTerrainLevel}, sideWalls={_options.GenerateTerrainSideWalls}, meshOptimize={_options.CombineTerrainMeshesAfterHeightProjection}, expandShore={_options.ExpandSandShoreBand}, shoreId='{_options.ShoreBandTileId}'";

        private static string FormatStringSet(HashSet<string> values, int maxEntries)
        {
            if (values == null || values.Count == 0)
                return "[]";

            var builder = new StringBuilder();
            builder.Append('[');
            int index = 0;
            foreach (string value in values)
            {
                if (index > 0)
                    builder.Append(", ");
                if (index >= maxEntries)
                {
                    builder.Append("...");
                    break;
                }

                builder.Append(value);
                index++;
            }
            builder.Append(']');
            return builder.ToString();
        }

        private static void LogConfigurationFolders(Configuration configuration)
        {
            int blueprintLayerCount = 0;
            int buildLayerCount = 0;
            for (int i = 0; i < (configuration.blueprintLayerFolders?.Count ?? 0); i++)
                blueprintLayerCount += configuration.blueprintLayerFolders[i]?.blueprintLayers?.Count ?? 0;
            for (int i = 0; i < (configuration.buildLayerFolders?.Count ?? 0); i++)
                buildLayerCount += configuration.buildLayerFolders[i]?.buildLayers?.Count ?? 0;
        }
    }
}
