using System.Collections.Generic;
using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MoyvaTerrainBuildLayerCopyUtility
    {
        public static void Copy(TilesBuildLayer source, MoyvaTerrainHeightAwareTilesBuildLayer target)
        {
            if (source == null || target == null)
                return;

            target.masks = source.masks != null ? new List<BuildLayerMask>(source.masks) : target.masks;
            target.useDualGrid = source.useDualGrid;
            target.meshGenerationOverride = source.meshGenerationOverride;
            target.mergeTiles = source.mergeTiles;
            target.shadowCastingMode = source.shadowCastingMode;
            target.objectLayer = source.objectLayer;
            target.renderingLayer = source.renderingLayer;
            target.colliderType = source.colliderType;
            target.tileColliderHeight = source.tileColliderHeight;
            target.tileColliderExtrusionHeight = source.tileColliderExtrusionHeight;
            target.invertCollisionWalls = source.invertCollisionWalls;
            target.generateFlatSurface = source.generateFlatSurface;
            target.flatSurfaceMaterial = source.flatSurfaceMaterial;
            target.scaleTileToCellSize = source.scaleTileToCellSize;
            target.layerYOffset = source.layerYOffset;
            target.scaleOffset = source.scaleOffset;
            target.tilePresetsTop = CopySelections(source.tilePresetsTop);
            target.tilePresetsMiddle = CopySelections(source.tilePresetsMiddle);
            target.tilePresetsBottom = CopySelections(source.tilePresetsBottom);
            target.tileLayers = CopyTileLayers(source.tileLayers);
        }

        private static List<TilesBuildLayer.TilePresetSelection> CopySelections(List<TilesBuildLayer.TilePresetSelection> source)
        {
            var result = new List<TilesBuildLayer.TilePresetSelection>();
            if (source == null)
                return result;

            for (int i = 0; i < source.Count; i++)
            {
                var selection = source[i];
                if (selection == null)
                    continue;

                result.Add(new TilesBuildLayer.TilePresetSelection
                {
                    preset = selection.preset,
                    tileHeight = selection.tileHeight,
                    weight = selection.weight
                });
            }

            return result;
        }

        private static List<TilesBuildLayer.TileLayers> CopyTileLayers(List<TilesBuildLayer.TileLayers> source)
        {
            var result = new List<TilesBuildLayer.TileLayers>();
            if (source == null)
                return result;

            for (int i = 0; i < source.Count; i++)
            {
                var layer = source[i];
                if (layer == null)
                    continue;

                result.Add(new TilesBuildLayer.TileLayers
                {
                    name = layer.name,
                    heightOffset = layer.heightOffset,
                    ignoreFillTiles = layer.ignoreFillTiles,
                    layerOverrides = CopyOverrides(layer.layerOverrides)
                });
            }

            return result.Count > 0 ? result : new List<TilesBuildLayer.TileLayers> { new TilesBuildLayer.TileLayers() };
        }

        private static List<TilesBuildLayer.TilePresetOverride> CopyOverrides(List<TilesBuildLayer.TilePresetOverride> source)
        {
            var result = new List<TilesBuildLayer.TilePresetOverride>();
            if (source == null)
                return result;

            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (item == null)
                    continue;

                result.Add(new TilesBuildLayer.TilePresetOverride
                {
                    name = item.name,
                    blueprintOverrideLayer = item.blueprintOverrideLayer,
                    preset = item.preset,
                    requiredNeighbourCount = item.requiredNeighbourCount
                });
            }

            return result;
        }
    }
}
