using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class TwcTileMeshSourceProvider : IResolvedTileMeshSource
    {
        private const string HeightDiagnosticsTag = "[MoyvaTileHeightDiag]";
        private const float FlatSurfaceBoundsHeightTolerance = 0.0001f;

        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly Dictionary<string, TilesBuildLayer> _buildLayerByGuid = new Dictionary<string, TilesBuildLayer>(System.StringComparer.Ordinal);
        private readonly Dictionary<GameObject, PrefabMeshTemplate[]> _meshTemplatesByPrefab =
            new Dictionary<GameObject, PrefabMeshTemplate[]>();
        private readonly HashSet<string> _heightDiagnosticKeys = new HashSet<string>(System.StringComparer.Ordinal);

        public TwcTileMeshSourceProvider(ITileWorldCreatorBuildEnvironment environment)
        {
            _environment = environment;
        }

        public int CollectMeshSources(ResolvedTileComposition composition, List<TileMeshSource> results)
        {
            if (!composition.HasMainTerrain || results == null)
                return 0;

            var sample = composition.MainTerrain;
            TilesBuildLayer buildLayer = ResolveBuildLayer(sample);
            TilePreset preset = ResolvePreset(buildLayer, sample, composition.Cell, GlobalSeed.Current);
            if (buildLayer == null || preset == null)
                return 0;

            return preset.gridtype == TilePreset.GridType.dual
                ? CollectDualGridSources(composition, buildLayer, preset, results)
                : CollectNormalGridSource(composition, buildLayer, preset, results);
        }

        private int CollectNormalGridSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            List<TileMeshSource> results)
        {
            int configuration = BuildNormalConfiguration(composition);
            var tileType = ResolveTileType(preset.gridtype, configuration, out int yRotation);
            if (tileType == TilePreset.TileType.none)
                tileType = TilePreset.TileType.NRMGRD_fill;

            var scaleSign = TileConfigurations.NRMGRD_minusXScale_configurations.Contains(configuration)
                ? new Vector3(-1f, 1f, 1f)
                : Vector3.one;
            return TryAddMeshSources(
                composition,
                buildLayer,
                preset,
                tileType,
                new Vector2(composition.Cell.x, composition.Cell.y),
                yRotation,
                scaleSign,
                ResolveOccludedSides(composition),
                ResolveEdgeBottoms(composition),
                results);
        }

        private int CollectDualGridSources(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            List<TileMeshSource> results)
        {
            int before = results.Count;
            // TWC dual grid creates four half-offset fragments around a source cell.
            // We keep that shape selection, but assign each fragment to one source cell
            // so neighboring chunks do not duplicate border geometry.
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(-0.5f, -0.5f),
                composition.WestMatches,
                true,
                composition.SouthWestMatches,
                composition.SouthMatches,
                composition.WestSurfaceHeight,
                composition.MainTerrain.SurfaceHeight,
                composition.SouthWestSurfaceHeight,
                composition.SouthSurfaceHeight,
                ShouldCurrentOwnDualFragment(
                    composition.WestMatches,
                    composition.SouthMatches,
                    composition.SouthWestMatches),
                results);
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(0.5f, -0.5f),
                true,
                composition.EastMatches,
                composition.SouthMatches,
                composition.SouthEastMatches,
                composition.MainTerrain.SurfaceHeight,
                composition.EastSurfaceHeight,
                composition.SouthSurfaceHeight,
                composition.SouthEastSurfaceHeight,
                ShouldCurrentOwnDualFragment(
                    false,
                    composition.SouthMatches,
                    composition.SouthEastMatches),
                results);
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(-0.5f, 0.5f),
                composition.NorthWestMatches,
                composition.NorthMatches,
                composition.WestMatches,
                true,
                composition.NorthWestSurfaceHeight,
                composition.NorthSurfaceHeight,
                composition.WestSurfaceHeight,
                composition.MainTerrain.SurfaceHeight,
                !composition.WestMatches,
                results);
            TryAddDualGridSource(
                composition,
                buildLayer,
                preset,
                new Vector2(0.5f, 0.5f),
                composition.NorthMatches,
                composition.NorthEastMatches,
                true,
                composition.EastMatches,
                composition.NorthSurfaceHeight,
                composition.NorthEastSurfaceHeight,
                composition.MainTerrain.SurfaceHeight,
                composition.EastSurfaceHeight,
                true,
                results);
            return results.Count - before;
        }

        private void TryAddDualGridSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            Vector2 offset,
            bool topLeft,
            bool topRight,
            bool bottomLeft,
            bool bottomRight,
            float topLeftSurface,
            float topRightSurface,
            float bottomLeftSurface,
            float bottomRightSurface,
            bool ownedByCurrentCell,
            List<TileMeshSource> results)
        {
            if (!ownedByCurrentCell)
                return;

            int configuration = BuildDualConfiguration(topLeft, topRight, bottomLeft, bottomRight);
            var tileData = new BuildLayer.TileData
            {
                configuration = configuration,
                tilePosition = new Vector2(composition.Cell.x + offset.x, composition.Cell.y + offset.y)
            };
            var tileType = ResolveTileType(preset.gridtype, configuration, out int yRotation);
            if (tileType == TilePreset.TileType.none)
                return;

            TryAddMeshSources(
                composition,
                buildLayer,
                preset,
                tileType,
                tileData.tilePosition,
                yRotation,
                Vector3.one,
                ResolveDualOccludedSides(
                    composition.MainTerrain.SurfaceHeight,
                    topLeftSurface,
                    topRightSurface,
                    bottomLeftSurface,
                    bottomRightSurface),
                ResolveDualEdgeBottoms(
                    composition,
                    topLeftSurface,
                    topRightSurface,
                    bottomLeftSurface,
                    bottomRightSurface),
                results);
        }

        private int TryAddMeshSources(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            TilePreset.TileType tileType,
            Vector2 tilePosition,
            int yRotation,
            Vector3 scaleSign,
            TileMeshOccludedSides occludedSides,
            TileMeshEdgeBottoms edgeBottoms,
            List<TileMeshSource> results)
        {
            var sample = composition.MainTerrain;
            GameObject prefab = preset.GetTile(tileType, out float xRotationOffset, out float yRotationOffset);
            if (prefab == null)
                return 0;

            if (!TryGetMeshTemplates(prefab, out PrefabMeshTemplate[] templates))
                return 0;

            float cellSize = ResolveCellSize();
            Vector3 scale = prefab.transform.localScale;
            if (buildLayer.scaleTileToCellSize)
                scale *= cellSize;

            scale = new Vector3(
                scale.x * buildLayer.scaleOffset.x * scaleSign.x,
                scale.y * buildLayer.scaleOffset.y * scaleSign.y,
                scale.z * buildLayer.scaleOffset.z * scaleSign.z);

            Quaternion rotation = Quaternion.Euler(xRotationOffset, yRotation + yRotationOffset, 0f);
            float fallbackPlacementHeight = ResolvePlacementHeight(sample, buildLayer);
            Matrix4x4 unplacedRootMatrix = Matrix4x4.TRS(Vector3.zero, rotation, scale);
            bool preferFlatSurfaceTemplates =
                sample.TileGeometryMode == TileGeometryMode.SurfaceOnly
                && HasFlatSurfaceTemplate(templates, unplacedRootMatrix);
            float prefabTopOffset = ResolveAggregateTransformedBoundsTop(
                templates,
                unplacedRootMatrix,
                preferFlatSurfaceTemplates);
            float prefabBottomOffset = ResolveAggregateTransformedBoundsBottom(
                templates,
                unplacedRootMatrix,
                preferFlatSurfaceTemplates);
            float placementHeight = ResolveSurfaceAlignedPlacementHeight(
                sample.SurfaceHeight,
                fallbackPlacementHeight,
                prefabTopOffset);
            Vector3 position = new Vector3(
                tilePosition.x * cellSize,
                placementHeight,
                tilePosition.y * cellSize);
            Matrix4x4 rootMatrix = Matrix4x4.TRS(position, rotation, scale);
            float visibleBottomY = sample.TileGeometryMode == TileGeometryMode.SolidTerrain
                ? ResolveVisibleBottomY(composition)
                : float.NaN;

            LogHeightPlacementOnce(
                composition,
                buildLayer,
                preset,
                tileType,
                prefab,
                fallbackPlacementHeight,
                prefabTopOffset,
                prefabBottomOffset,
                placementHeight,
                visibleBottomY);

            int missingClosureOwner =
                sample.TileGeometryMode == TileGeometryMode.SolidTerrain
                && sample.AuthoredClosurePolicy
                    == AuthoredClosurePolicy.PreserveAuthored
                    ? ResolveMissingClosureOwnerIndex(
                        templates,
                        unplacedRootMatrix,
                        preferFlatSurfaceTemplates,
                        prefabTopOffset)
                    : -1;
            int added = 0;
            Material materialOverride = preset.GetMaterialOverride();
            for (int i = 0; i < templates.Length; i++)
            {
                PrefabMeshTemplate template = templates[i];
                if (!ShouldIncludeMeshTemplate(
                        template,
                        unplacedRootMatrix,
                        preferFlatSurfaceTemplates,
                        prefabTopOffset))
                {
                    continue;
                }

                var meshSource = new TileMeshSource(
                    template.Mesh,
                    template.ResolveMaterials(materialOverride),
                    rootMatrix * template.ChildMatrix,
                    visibleBottomY,
                    occludedSides,
                    new Vector2(position.x, position.z),
                    cellSize * 0.5f,
                    sample.AuthoredClosurePolicy,
                    edgeBottoms,
                    sample.TileGeometryMode,
                    generateMissingClosure: i == missingClosureOwner);
                if (!meshSource.IsValid)
                    continue;

                results.Add(meshSource);
                added++;
            }

            return added;
        }

        internal static TileMeshOccludedSides ResolveOccludedSides(
            ResolvedTileComposition composition)
        {
            TileMeshOccludedSides sides = TileMeshOccludedSides.None;
            float surface = composition.MainTerrain.SurfaceHeight;
            if (ShouldOccludeSide(surface, composition.NorthSurfaceHeight, composition.NorthMatches))
                sides |= TileMeshOccludedSides.North;
            if (ShouldOccludeSide(surface, composition.EastSurfaceHeight, composition.EastMatches))
                sides |= TileMeshOccludedSides.East;
            if (ShouldOccludeSide(surface, composition.SouthSurfaceHeight, composition.SouthMatches))
                sides |= TileMeshOccludedSides.South;
            if (ShouldOccludeSide(surface, composition.WestSurfaceHeight, composition.WestMatches))
                sides |= TileMeshOccludedSides.West;

            return sides;
        }

        internal static TileMeshEdgeBottoms ResolveEdgeBottoms(
            ResolvedTileComposition composition)
        {
            float fallback = ResolveVisibleBottomY(composition);
            return new TileMeshEdgeBottoms(
                ResolveFiniteOrFallback(composition.NorthSurfaceHeight, fallback),
                ResolveFiniteOrFallback(composition.EastSurfaceHeight, fallback),
                ResolveFiniteOrFallback(composition.SouthSurfaceHeight, fallback),
                ResolveFiniteOrFallback(composition.WestSurfaceHeight, fallback));
        }

        internal static TileMeshOccludedSides ResolveDualOccludedSides(
            bool topLeft,
            bool topRight,
            bool bottomLeft,
            bool bottomRight)
        {
            TileMeshOccludedSides sides = TileMeshOccludedSides.None;
            if (topLeft && topRight)
                sides |= TileMeshOccludedSides.North;
            if (topRight && bottomRight)
                sides |= TileMeshOccludedSides.East;
            if (bottomLeft && bottomRight)
                sides |= TileMeshOccludedSides.South;
            if (topLeft && bottomLeft)
                sides |= TileMeshOccludedSides.West;
            return sides;
        }

        internal static TileMeshOccludedSides ResolveDualOccludedSides(
            float surfaceHeight,
            float topLeftSurface,
            float topRightSurface,
            float bottomLeftSurface,
            float bottomRightSurface)
        {
            TileMeshOccludedSides sides = TileMeshOccludedSides.None;
            if (BothCover(surfaceHeight, topLeftSurface, topRightSurface))
                sides |= TileMeshOccludedSides.North;
            if (BothCover(surfaceHeight, topRightSurface, bottomRightSurface))
                sides |= TileMeshOccludedSides.East;
            if (BothCover(surfaceHeight, bottomLeftSurface, bottomRightSurface))
                sides |= TileMeshOccludedSides.South;
            if (BothCover(surfaceHeight, topLeftSurface, bottomLeftSurface))
                sides |= TileMeshOccludedSides.West;
            return sides;
        }

        private static TileMeshEdgeBottoms ResolveDualEdgeBottoms(
            ResolvedTileComposition composition,
            float topLeftSurface,
            float topRightSurface,
            float bottomLeftSurface,
            float bottomRightSurface)
        {
            float fallback = ResolveVisibleBottomY(composition);
            return new TileMeshEdgeBottoms(
                ResolveLowestFinite(fallback, topLeftSurface, topRightSurface),
                ResolveLowestFinite(fallback, topRightSurface, bottomRightSurface),
                ResolveLowestFinite(fallback, bottomLeftSurface, bottomRightSurface),
                ResolveLowestFinite(fallback, topLeftSurface, bottomLeftSurface));
        }

        private static bool BothCover(float surfaceHeight, float first, float second)
            => IsFinite(surfaceHeight)
               && IsFinite(first)
               && IsFinite(second)
               && first >= surfaceHeight - 0.0001f
               && second >= surfaceHeight - 0.0001f;

        private static bool ShouldOccludeSide(
            float surfaceHeight,
            float neighborSurfaceHeight,
            bool identityFallback)
        {
            if (!IsFinite(surfaceHeight) || !IsFinite(neighborSurfaceHeight))
                return identityFallback;

            // Equal surfaces suppress both faces. At a height transition only
            // the higher tile emits the delta wall; the lower tile is covered by
            // its neighbor and therefore emits nothing.
            return neighborSurfaceHeight >= surfaceHeight - 0.0001f;
        }

        private static float ResolveFiniteOrFallback(float value, float fallback)
            => IsFinite(value) ? value : fallback;

        private static float ResolveLowestFinite(float fallback, float first, float second)
        {
            bool hasFirst = IsFinite(first);
            bool hasSecond = IsFinite(second);
            if (hasFirst && hasSecond)
                return Mathf.Min(first, second);
            if (hasFirst)
                return first;
            return hasSecond ? second : fallback;
        }

        internal static float ResolveVisibleBottomY(
            ResolvedTileComposition composition)
        {
            if (composition.HasSupportHeight
                && IsFinite(composition.SupportHeight))
            {
                return composition.SupportHeight;
            }

            return 0f;
        }

        private static float ResolvePlacementHeight(
            GraphTileLayerSample sample,
            TilesBuildLayer buildLayer)
        {
            float height = sample.Height;
            if (buildLayer == null)
                return height;

            height += buildLayer.layerYOffset;
            if (buildLayer.tileLayers != null
                && buildLayer.tileLayers.Count > 0
                && buildLayer.tileLayers[0] != null)
            {
                height += buildLayer.tileLayers[0].heightOffset;
            }

            return height;
        }

        internal static float ResolveSurfaceAlignedPlacementHeight(
            float expectedSurfaceHeight,
            float fallbackPlacementHeight,
            float prefabTopOffset)
        {
            if (!IsFinite(expectedSurfaceHeight) || !IsFinite(prefabTopOffset))
                return fallbackPlacementHeight;

            return expectedSurfaceHeight - prefabTopOffset;
        }

        internal static float ResolveTransformedBoundsTop(Bounds bounds, Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
            float extentY = ResolveTransformedBoundsExtentY(bounds, matrix);
            return center.y + extentY;
        }

        internal static float ResolveTransformedBoundsBottom(
            Bounds bounds,
            Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
            float extentY = ResolveTransformedBoundsExtentY(bounds, matrix);
            return center.y - extentY;
        }

        private static float ResolveTransformedBoundsExtentY(
            Bounds bounds,
            Matrix4x4 matrix)
        {
            Vector3 extents = bounds.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            return Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        }

        private void LogHeightPlacementOnce(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            TilePreset.TileType tileType,
            GameObject prefab,
            float fallbackPlacementHeight,
            float prefabTopOffset,
            float prefabBottomOffset,
            float placementHeight,
            float visibleBottomY)
        {
            GraphTileLayerSample sample = composition.MainTerrain;
            string key =
                $"{sample.GraphLayerId}|{preset.GetInstanceID()}|{tileType}|{prefab.GetInstanceID()}";
            if (!_heightDiagnosticKeys.Add(key))
                return;

            float tileLayerOffset = buildLayer.tileLayers != null
                                    && buildLayer.tileLayers.Count > 0
                                    && buildLayer.tileLayers[0] != null
                ? buildLayer.tileLayers[0].heightOffset
                : 0f;
            float actualSurfaceHeight = placementHeight + prefabTopOffset;
            float authoredBottomHeight = placementHeight + prefabBottomOffset;
            float unclosedGap = IsFinite(visibleBottomY)
                ? Mathf.Max(0f, authoredBottomHeight - visibleBottomY)
                : 0f;
            string mode = IsFinite(sample.SurfaceHeight) && IsFinite(prefabTopOffset)
                ? "surface-aligned"
                : "fallback";

            Debug.Log(
                $"{HeightDiagnosticsTag} Placement mode={mode} layer='{sample.GraphLayerName}' " +
                $"layerId='{sample.GraphLayerId}' cell={composition.Cell} tileType={tileType} " +
                $"preset='{preset.name}' prefab='{prefab.name}' layerHeight={sample.Height:0.###} " +
                $"expectedSurface={sample.SurfaceHeight:0.###} buildYOffset={buildLayer.layerYOffset:0.###} " +
                $"tileLayerOffset={tileLayerOffset:0.###} fallbackRootY={fallbackPlacementHeight:0.###} " +
                $"prefabTopOffset={prefabTopOffset:0.###} prefabBottomOffset={prefabBottomOffset:0.###} " +
                $"correctedRootY={placementHeight:0.###} " +
                $"actualSurface={actualSurfaceHeight:0.###} authoredBottom={authoredBottomHeight:0.###} " +
                $"support={visibleBottomY:0.###} unclosedGap={unclosedGap:0.###} " +
                $"surfaceDelta={(actualSurfaceHeight - sample.SurfaceHeight):0.#####}");
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private bool TryGetMeshTemplates(GameObject prefab, out PrefabMeshTemplate[] templates)
        {
            if (_meshTemplatesByPrefab.TryGetValue(prefab, out templates)
                && templates != null
                && templates.Length > 0)
            {
                return true;
            }

            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters == null || meshFilters.Length == 0)
            {
                templates = null;
                return false;
            }

            var collected = new List<PrefabMeshTemplate>(meshFilters.Length);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                MeshRenderer renderer = meshFilter.GetComponent<MeshRenderer>()
                    ?? meshFilter.GetComponentInParent<MeshRenderer>(true);
                if (renderer == null)
                    continue;

                collected.Add(new PrefabMeshTemplate(
                    meshFilter.sharedMesh,
                    prefab.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix,
                    renderer.sharedMaterials));
            }

            templates = collected.ToArray();
            if (templates.Length == 0)
                return false;

            _meshTemplatesByPrefab[prefab] = templates;
            return true;
        }

        internal static float ResolveAggregateTransformedBoundsTop(
            IReadOnlyList<PrefabMeshTemplate> templates,
            Matrix4x4 rootMatrix)
            => ResolveAggregateTransformedBoundsTop(
                templates,
                rootMatrix,
                flatSurfaceTemplatesOnly: false);

        private static float ResolveAggregateTransformedBoundsTop(
            IReadOnlyList<PrefabMeshTemplate> templates,
            Matrix4x4 rootMatrix,
            bool flatSurfaceTemplatesOnly)
        {
            float top = float.NegativeInfinity;
            if (templates == null)
                return top;

            for (int i = 0; i < templates.Count; i++)
            {
                PrefabMeshTemplate template = templates[i];
                if (template?.Mesh == null)
                    continue;
                if (flatSurfaceTemplatesOnly
                    && !IsFlatSurfaceTemplate(template, rootMatrix))
                {
                    continue;
                }

                top = Mathf.Max(
                    top,
                    ResolveTransformedBoundsTop(
                        template.Mesh.bounds,
                        rootMatrix * template.ChildMatrix));
            }

            return top;
        }

        internal static float ResolveAggregateTransformedBoundsBottom(
            IReadOnlyList<PrefabMeshTemplate> templates,
            Matrix4x4 rootMatrix)
            => ResolveAggregateTransformedBoundsBottom(
                templates,
                rootMatrix,
                flatSurfaceTemplatesOnly: false);

        private static float ResolveAggregateTransformedBoundsBottom(
            IReadOnlyList<PrefabMeshTemplate> templates,
            Matrix4x4 rootMatrix,
            bool flatSurfaceTemplatesOnly)
        {
            float bottom = float.PositiveInfinity;
            if (templates == null)
                return bottom;

            for (int i = 0; i < templates.Count; i++)
            {
                PrefabMeshTemplate template = templates[i];
                if (template?.Mesh == null)
                    continue;
                if (flatSurfaceTemplatesOnly
                    && !IsFlatSurfaceTemplate(template, rootMatrix))
                {
                    continue;
                }

                bottom = Mathf.Min(
                    bottom,
                    ResolveTransformedBoundsBottom(
                        template.Mesh.bounds,
                        rootMatrix * template.ChildMatrix));
            }

            return bottom;
        }

        private static int ResolveMissingClosureOwnerIndex(
            IReadOnlyList<PrefabMeshTemplate> templates,
            Matrix4x4 rootMatrix,
            bool flatSurfaceTemplatesOnly,
            float selectedSurfaceTop)
        {
            int owner = -1;
            float lowestBottom = float.PositiveInfinity;
            if (templates == null)
                return owner;

            for (int i = 0; i < templates.Count; i++)
            {
                PrefabMeshTemplate template = templates[i];
                if (!ShouldIncludeMeshTemplate(
                        template,
                        rootMatrix,
                        flatSurfaceTemplatesOnly,
                        selectedSurfaceTop))
                {
                    continue;
                }

                float bottom = ResolveTransformedBoundsBottom(
                    template.Mesh.bounds,
                    rootMatrix * template.ChildMatrix);
                if (!IsFinite(bottom) || bottom >= lowestBottom)
                    continue;

                lowestBottom = bottom;
                owner = i;
            }

            return owner;
        }

        internal static bool HasFlatSurfaceTemplate(
            IReadOnlyList<PrefabMeshTemplate> templates,
            Matrix4x4 rootMatrix)
        {
            if (templates == null)
                return false;

            for (int i = 0; i < templates.Count; i++)
            {
                if (IsFlatSurfaceTemplate(templates[i], rootMatrix))
                    return true;
            }

            return false;
        }

        internal static bool IsFlatSurfaceTemplate(
            PrefabMeshTemplate template,
            Matrix4x4 rootMatrix)
        {
            if (template?.Mesh == null)
                return false;

            float height = ResolveTransformedBoundsExtentY(
                               template.Mesh.bounds,
                               rootMatrix * template.ChildMatrix)
                           * 2f;
            return IsFinite(height)
                   && height <= FlatSurfaceBoundsHeightTolerance;
        }

        internal static bool ShouldIncludeMeshTemplate(
            PrefabMeshTemplate template,
            Matrix4x4 rootMatrix,
            bool flatSurfaceTemplatesOnly,
            float selectedSurfaceTop)
        {
            if (template?.Mesh == null)
                return false;
            if (!flatSurfaceTemplatesOnly)
                return true;
            if (!IsFlatSurfaceTemplate(template, rootMatrix)
                || !IsFinite(selectedSurfaceTop))
            {
                return false;
            }

            float templateTop = ResolveTransformedBoundsTop(
                template.Mesh.bounds,
                rootMatrix * template.ChildMatrix);
            return IsFinite(templateTop)
                   && Mathf.Abs(templateTop - selectedSurfaceTop)
                   <= FlatSurfaceBoundsHeightTolerance;
        }

        internal static int BuildNormalConfiguration(ResolvedTileComposition composition)
        {
            return (composition.NorthWestMatches ? 1 << 0 : 0)
                   | (composition.NorthMatches ? 1 << 1 : 0)
                   | (composition.NorthEastMatches ? 1 << 2 : 0)
                   | (composition.WestMatches ? 1 << 3 : 0)
                   | 1 << 4
                   | (composition.EastMatches ? 1 << 5 : 0)
                   | (composition.SouthWestMatches ? 1 << 6 : 0)
                   | (composition.SouthMatches ? 1 << 7 : 0)
                   | (composition.SouthEastMatches ? 1 << 8 : 0);
        }

        internal static int BuildDualConfiguration(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
            => (topLeft ? 1 << 0 : 0)
               | (topRight ? 1 << 1 : 0)
               | (bottomLeft ? 1 << 2 : 0)
               | (bottomRight ? 1 << 3 : 0);

        internal static TilePreset.TileType ResolveTileType(
            TilePreset.GridType gridType,
            int configuration,
            out int rotation)
        {
            rotation = ResolveRotation(gridType, configuration);

            return gridType == TilePreset.GridType.dual
                ? ResolveDualTileType(configuration)
                : ResolveNormalTileType(configuration);
        }

        private static TilePreset.TileType ResolveNormalTileType(int configuration)
        {
            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_cornerWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_cornerWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_cornerFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_cornerFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_edgeWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_edgeWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_edgeFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_edgeFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_fill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_fill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_single_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_single;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_threeWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_threeWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_threeWayFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_threeWayFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_threeCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_threeCorner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_deadEndWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_deadEnd;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_fourWay_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_fourWay;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_edgeCornerFill_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_edgeCornerFill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_doubleCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_doubleCorner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.NRMGRD_interiorCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.NRMGRD_interiorCorner;
            }

            return TilePreset.TileType.none;
        }

        private static TilePreset.TileType ResolveDualTileType(int configuration)
        {
            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_corner_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_corner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_edge_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_edge;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_fill_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_fill;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_interiorCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_interiorCorner;
            }

            if (ContainsConfiguration(
                    TileConfigurations.DUALGRD_doubleInteriorCorner_configurations,
                    configuration))
            {
                return TilePreset.TileType.DUALGRD_doubleInteriorCorner;
            }

            return TilePreset.TileType.none;
        }

        private static int ResolveRotation(
            TilePreset.GridType gridType,
            int configuration)
        {
            if (gridType == TilePreset.GridType.dual)
            {
                if (ContainsConfiguration(TileConfigurations.rotation90Configurations, configuration))
                    return 90;
                if (ContainsConfiguration(TileConfigurations.rotation180Configurations, configuration))
                    return 180;
                if (ContainsConfiguration(TileConfigurations.rotation270Configurations, configuration))
                    return 270;

                return 0;
            }

            if (ContainsConfiguration(TileConfigurations.NRMGRD_rotation90_configurations, configuration))
                return 90;
            if (ContainsConfiguration(TileConfigurations.NRMGRD_rotation180_configurations, configuration))
                return 180;
            if (ContainsConfiguration(TileConfigurations.NRMGRD_rotation270_configurations, configuration))
                return 270;

            return 0;
        }

        private static bool ContainsConfiguration(
            IEnumerable<int> configurations,
            int configuration)
        {
            if (configurations == null)
                return false;

            foreach (int candidate in configurations)
            {
                if (candidate == configuration)
                    return true;
            }

            return false;
        }

        internal static bool ShouldCurrentOwnDualFragment(
            bool westMatchesIdentity,
            bool southMatchesIdentity,
            bool southWestMatchesIdentity)
            => !westMatchesIdentity
               && !southMatchesIdentity
               && !southWestMatchesIdentity;

        private TilesBuildLayer ResolveBuildLayer(GraphTileLayerSample sample)
        {
            if (!string.IsNullOrWhiteSpace(sample.BuildLayerGuid)
                && _buildLayerByGuid.TryGetValue(sample.BuildLayerGuid, out var cached))
            {
                return cached;
            }

            var configuration = _environment?.Manager?.configuration;
            if (configuration?.buildLayerFolders == null)
                return null;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer layer)
                        continue;

                    bool matchesBuild = !string.IsNullOrWhiteSpace(sample.BuildLayerGuid)
                        && string.Equals(layer.guid, sample.BuildLayerGuid, System.StringComparison.Ordinal);
                    bool matchesBlueprint = !string.IsNullOrWhiteSpace(sample.BlueprintLayerGuid)
                        && (string.Equals(layer.assignedBlueprintLayerGuid, sample.BlueprintLayerGuid, System.StringComparison.Ordinal)
                            || string.Equals(layer.currentBlueprintLayer?.guid, sample.BlueprintLayerGuid, System.StringComparison.Ordinal));

                    if (!matchesBuild && !matchesBlueprint)
                        continue;

                    if (!string.IsNullOrWhiteSpace(layer.guid))
                        _buildLayerByGuid[layer.guid] = layer;
                    return layer;
                }
            }

            return null;
        }

        private static TilePreset ResolvePreset(TilesBuildLayer buildLayer, GraphTileLayerSample sample, Vector2Int cell, int seed)
        {
            if (buildLayer == null)
                return null;

            TilePreset preset = FindPreset(buildLayer.tilePresetsTop, sample.PresetId)
                                ?? FindPreset(buildLayer.tilePresetsMiddle, sample.PresetId)
                                ?? FindPreset(buildLayer.tilePresetsBottom, sample.PresetId);
            if (preset != null)
                return preset;

            return ChooseWeightedPreset(buildLayer.tilePresetsTop, sample, cell, seed, 0)
                   ?? ChooseWeightedPreset(buildLayer.tilePresetsMiddle, sample, cell, seed, 1)
                   ?? ChooseWeightedPreset(buildLayer.tilePresetsBottom, sample, cell, seed, 2);
        }

        private static TilePreset FindPreset(List<TilesBuildLayer.TilePresetSelection> selections, string presetId)
        {
            if (selections == null || string.IsNullOrWhiteSpace(presetId))
                return null;

            for (int i = 0; i < selections.Count; i++)
            {
                var preset = selections[i]?.preset;
                if (preset == null)
                    continue;

                if (string.Equals(preset.name, presetId, System.StringComparison.Ordinal)
                    || string.Equals(preset.tileId, presetId, System.StringComparison.Ordinal))
                    return preset;
            }

            return null;
        }

        private static TilePreset ChooseWeightedPreset(
            List<TilesBuildLayer.TilePresetSelection> selections,
            GraphTileLayerSample sample,
            Vector2Int cell,
            int seed,
            int tileLayerIndex)
        {
            if (selections == null)
                return null;

            float totalWeight = 0f;
            for (int i = 0; i < selections.Count; i++)
            {
                if (selections[i]?.preset != null)
                    totalWeight += Mathf.Max(0f, selections[i].weight);
            }

            if (totalWeight <= 0.0001f)
                return FirstPreset(selections);

            uint hash = ChunkFirstStableHash.TileVariant(
                seed,
                cell,
                sample.GraphLayerId,
                !string.IsNullOrWhiteSpace(sample.PresetId) ? sample.PresetId : sample.TileId,
                tileLayerIndex,
                "tile-preset");
            float roll = (hash / (float)uint.MaxValue) * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < selections.Count; i++)
            {
                var selection = selections[i];
                if (selection?.preset == null)
                    continue;

                cumulative += Mathf.Max(0f, selection.weight);
                if (roll <= cumulative)
                    return selection.preset;
            }

            return FirstPreset(selections);
        }

        private static TilePreset FirstPreset(List<TilesBuildLayer.TilePresetSelection> selections)
        {
            if (selections == null)
                return null;

            for (int i = 0; i < selections.Count; i++)
            {
                if (selections[i]?.preset != null)
                    return selections[i].preset;
            }

            return null;
        }

        private float ResolveCellSize()
        {
            var configuration = _environment?.Manager?.configuration;
            return configuration != null && configuration.cellSize > 0.0001f ? configuration.cellSize : 1f;
        }

        internal sealed class PrefabMeshTemplate
        {
            private readonly Material[] _baseMaterials;
            private readonly Dictionary<Material, Material[]> _overrideMaterials = new Dictionary<Material, Material[]>();

            public PrefabMeshTemplate(
                Mesh mesh,
                Matrix4x4 childMatrix,
                Material[] baseMaterials)
            {
                Mesh = mesh;
                ChildMatrix = childMatrix;
                _baseMaterials = baseMaterials;
            }

            public Mesh Mesh { get; }
            public Matrix4x4 ChildMatrix { get; }

            public Material[] ResolveMaterials(Material overrideMaterial)
            {
                if (overrideMaterial == null)
                    return _baseMaterials;

                if (_overrideMaterials.TryGetValue(overrideMaterial, out Material[] materials))
                    return materials;

                int count = Mathf.Max(1, _baseMaterials?.Length ?? 0);
                materials = new Material[count];
                for (int i = 0; i < count; i++)
                    materials[i] = overrideMaterial;

                _overrideMaterials[overrideMaterial] = materials;
                return materials;
            }
        }
    }
}
