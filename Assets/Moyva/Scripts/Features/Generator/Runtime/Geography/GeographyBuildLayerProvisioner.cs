using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Provisions one <see cref="MoyvaTerrainHeightAwareTilesBuildLayer"/> per
    /// geography tile id inside the runtime TWC configuration. Visual presets
    /// come from the canonical <see cref="TileWorldCreatorIdMappingSO"/> terrain
    /// mapping, so geography rendering never invents a second visual authority.
    /// Layers are reused across regenerations via their stable
    /// <c>geography-&lt;tileId&gt;</c> guid.
    /// </summary>
    internal sealed class GeographyBuildLayerProvisioner
    {
        private const string LayerGuidPrefix = "geography-";

        // Render order inside a cell stack and the stable compiled-layer order.
        private static readonly (string tileId, int sortOrder)[] VisualOrder =
        {
            ("water", 0),
            ("sand", 1),
            ("beach", 1),
            ("lowland", 2),
            ("grass", 3),
            ("forest-sparse", 4),
            ("forest-dense", 5),
            ("hill", 6),
            ("mountain", 7),
            ("snow", 8),
        };

        /// <summary>
        /// Ensures a height-aware build layer exists for every emitted tile id
        /// and returns the id → layer binding used by the logical map factory.
        /// </summary>
        public GeographyVisualSet Ensure(
            TileWorldCreatorManager manager,
            TileWorldCreatorIdMappingSO mapping,
            IEnumerable<string> tileIds)
        {
            var set = new GeographyVisualSet();
            if (manager?.configuration == null || tileIds == null)
                return set;

            Configuration configuration = manager.configuration;
            GraphCompilerLayerAssetUtility.EnsureBuildRootFolder(configuration);

            var wanted = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (string tileId in tileIds)
                if (!string.IsNullOrWhiteSpace(tileId))
                    wanted.Add(tileId);

            foreach (var (tileId, _) in VisualOrder)
                wanted.Add(tileId);

            foreach (string tileId in wanted)
            {
                var layer = EnsureLayer(manager, configuration, tileId);
                if (layer == null)
                    continue;

                var visual = new GeographyVisualLayer
                {
                    TileId = tileId,
                    GraphLayerId = LayerGuidPrefix + tileId,
                    BuildLayerGuid = layer.guid,
                    SortOrder = SortOrderOf(tileId),
                };

                ApplyVisual(layer, configuration, mapping, visual);
                set.Add(visual);
            }

            return set;
        }

        private static TilesBuildLayer EnsureLayer(
            TileWorldCreatorManager manager,
            Configuration configuration,
            string tileId)
        {
            string guid = LayerGuidPrefix + tileId;
            TilesBuildLayer existing = FindLayer(configuration, guid);
            if (existing is MoyvaTerrainHeightAwareTilesBuildLayer heightAware)
                return heightAware;

            if (existing != null)
            {
                return MoyvaTerrainBuildLayerUpgradeUtility.EnsureHeightAware(
                    manager, configuration, existing, LayerName(tileId));
            }

            var created = MoyvaTerrainBuildLayerUpgradeUtility.CreateHeightAware(
                manager, LayerName(tileId));
            if (created == null)
                return null;

            created.guid = guid;
            created.isEnabled = true;
            return created;
        }

        private static TilesBuildLayer FindLayer(Configuration configuration, string guid)
        {
            if (configuration?.buildLayerFolders == null)
                return null;

            foreach (var folder in configuration.buildLayerFolders)
            {
                if (folder?.buildLayers == null)
                    continue;

                foreach (var layer in folder.buildLayers)
                    if (layer is TilesBuildLayer tiles
                        && string.Equals(tiles.guid, guid, System.StringComparison.Ordinal))
                        return tiles;
            }

            return null;
        }

        private static void ApplyVisual(
            TilesBuildLayer layer,
            Configuration configuration,
            TileWorldCreatorIdMappingSO mapping,
            GeographyVisualLayer visual)
        {
            layer.configuration = configuration;
            layer.scaleOffset = Vector3.one;
            layer.layerYOffset = 0f;

            TilePreset preset = null;
            if (mapping != null
                && mapping.TryResolveTerrainLayer(visual.TileId, out var entry)
                && entry != null)
            {
                preset = entry.TilePreset;
                visual.BlueprintLayerGuid = entry.BlueprintLayerGuid;
                layer.useDualGrid = entry.UseDualGrid;
                layer.scaleTileToCellSize = entry.ScaleTileToCellSize;
            }

            if (preset != null)
            {
                layer.useDualGrid = preset.gridtype == TilePreset.GridType.dual;
                EnsurePresetSelection(layer.tilePresetsTop, preset);
                visual.PresetId = !string.IsNullOrWhiteSpace(preset.tileId)
                    ? preset.tileId
                    : preset.name;
            }
        }

        private static void EnsurePresetSelection(
            List<TilesBuildLayer.TilePresetSelection> selections,
            TilePreset preset)
        {
            if (preset == null)
                return;

            for (int i = 0; i < selections.Count; i++)
                if (selections[i]?.preset == preset)
                    return;

            selections.Add(new TilesBuildLayer.TilePresetSelection
            {
                preset = preset,
                weight = 1f,
            });
        }

        private static int SortOrderOf(string tileId)
        {
            foreach (var (id, order) in VisualOrder)
                if (string.Equals(id, tileId, System.StringComparison.Ordinal))
                    return order;
            return 9;
        }

        private static string LayerName(string tileId)
            => "Geography " + tileId;
    }
}
