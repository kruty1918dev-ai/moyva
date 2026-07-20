using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorFillTileSurfaceHeightUtility
    {
        public static float ResolveTilesBuildLayerTopHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer)
        {
            float baseHeight = blueprint != null ? blueprint.defaultLayerHeight : 0f;
            if (buildLayer == null)
                return baseHeight;

            float layerBaseHeight = baseHeight + buildLayer.layerYOffset;
            int layerCount = buildLayer.tileLayers?.Count ?? 0;
            if (layerCount <= 0)
                return layerBaseHeight + ResolveMaxFillPrefabTopOffset(buildLayer, ResolvePresetSelections(buildLayer, 0, 1));

            bool hasLayer = false;
            float topHeight = layerBaseHeight;
            for (int i = 0; i < layerCount; i++)
            {
                var tileLayer = buildLayer.tileLayers[i];
                if (tileLayer == null)
                    continue;

                float prefabTopOffset = ResolveMaxFillPrefabTopOffset(buildLayer, ResolvePresetSelections(buildLayer, i, layerCount));
                float candidate = layerBaseHeight + tileLayer.heightOffset + prefabTopOffset;
                topHeight = hasLayer ? Mathf.Max(topHeight, candidate) : candidate;
                hasLayer = true;
            }

            return hasLayer ? topHeight : layerBaseHeight;
        }

        private static List<TilesBuildLayer.TilePresetSelection> ResolvePresetSelections(
            TilesBuildLayer buildLayer,
            int tileLayerIndex,
            int tileLayerCount)
        {
            if (buildLayer == null)
                return null;

            var presets = buildLayer.tilePresetsTop;
            if (tileLayerIndex == 0)
            {
                if (tileLayerCount == 1)
                    presets = buildLayer.tilePresetsTop;
                else if (HasSelections(buildLayer.tilePresetsBottom))
                    presets = buildLayer.tilePresetsBottom;
                else if (HasSelections(buildLayer.tilePresetsMiddle))
                    presets = buildLayer.tilePresetsMiddle;
            }
            else if (tileLayerIndex == tileLayerCount - 1 && HasSelections(buildLayer.tilePresetsTop))
            {
                presets = buildLayer.tilePresetsTop;
            }
            else if (tileLayerIndex > 0 && HasSelections(buildLayer.tilePresetsMiddle))
            {
                presets = buildLayer.tilePresetsMiddle;
            }

            return presets;
        }

        private static float ResolveMaxFillPrefabTopOffset(
            TilesBuildLayer buildLayer,
            IReadOnlyList<TilesBuildLayer.TilePresetSelection> selections)
        {
            if (buildLayer == null || selections == null || selections.Count == 0)
                return 0f;

            bool hasTop = false;
            float top = 0f;
            for (int i = 0; i < selections.Count; i++)
            {
                TilePreset preset = selections[i]?.preset;
                if (preset == null || !TryResolveFillPrefabTopOffset(buildLayer, preset, out float candidate))
                    continue;

                top = hasTop ? Mathf.Max(top, candidate) : candidate;
                hasTop = true;
            }

            return hasTop ? top : 0f;
        }

        private static bool TryResolveFillPrefabTopOffset(
            TilesBuildLayer buildLayer,
            TilePreset preset,
            out float topOffset)
        {
            topOffset = 0f;
            TilePreset.TileType fillType = ResolveFillTileType(preset);
            GameObject prefab = preset.GetTile(fillType, out float xRotationOffset, out float yRotationOffset);
            if (prefab == null)
                return false;

            Quaternion rotation = Quaternion.Euler(xRotationOffset, yRotationOffset, 0f);
            Vector3 scale = ResolvePlacedScale(buildLayer, prefab);
            return TryResolveMeshTopOffset(prefab, rotation, scale, out topOffset);
        }

        internal static TilePreset.TileType ResolveFillTileType(TilePreset preset)
            => preset != null && preset.gridtype == TilePreset.GridType.dual
                ? TilePreset.TileType.DUALGRD_fill
                : TilePreset.TileType.NRMGRD_fill;

        private static Vector3 ResolvePlacedScale(TilesBuildLayer buildLayer, GameObject prefab)
        {
            Vector3 scale = prefab != null ? prefab.transform.localScale : Vector3.one;
            if (buildLayer == null)
                return scale;

            float cellSize = buildLayer.scaleTileToCellSize ? ResolveCellSize(buildLayer) : 1f;
            return new Vector3(
                scale.x * buildLayer.scaleOffset.x * cellSize,
                scale.y * buildLayer.scaleOffset.y * cellSize,
                scale.z * buildLayer.scaleOffset.z * cellSize);
        }

        private static bool TryResolveMeshTopOffset(
            GameObject prefab,
            Quaternion rotation,
            Vector3 scale,
            out float topOffset)
        {
            topOffset = 0f;
            if (prefab == null)
                return false;

            MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            if (filters == null || filters.Length == 0)
                return false;

            Matrix4x4 rootMatrix = Matrix4x4.TRS(Vector3.zero, rotation, scale);
            bool hasBounds = false;
            Bounds bounds = default;
            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter == null || filter.sharedMesh == null)
                    continue;

                Matrix4x4 childMatrix = prefab.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                Bounds transformed = TransformBounds(filter.sharedMesh.bounds, rootMatrix * childMatrix);
                if (!hasBounds)
                {
                    bounds = transformed;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(transformed);
            }

            if (!hasBounds)
                return false;

            topOffset = bounds.max.y;
            return true;
        }

        private static Bounds TransformBounds(Bounds bounds, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
            Vector3 extents = bounds.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, extents * 2f);
        }

        private static float ResolveCellSize(TilesBuildLayer buildLayer)
            => buildLayer?.configuration != null && buildLayer.configuration.cellSize > 0.0001f
                ? buildLayer.configuration.cellSize
                : 1f;

        private static bool HasSelections(IReadOnlyList<TilesBuildLayer.TilePresetSelection> selections)
            => selections != null && selections.Count > 0;
    }
}
