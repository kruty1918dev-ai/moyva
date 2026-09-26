using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class TwcTileMeshSourceProvider : IResolvedTileMeshSource
    {
        private const float FlatSurfaceBoundsHeightTolerance = 0.0001f;
        private const float WaterfallLipOverlap = 0.1f;

        private static Mesh _waterfallStripMesh;

        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly IAtlasTileSetCatalog _atlas;
        private readonly ITerrainPassageMap _passages;
        private readonly IRecipeHydrologyMap _hydrology;
        private readonly Dictionary<string, TilesBuildLayer> _buildLayerByGuid = new Dictionary<string, TilesBuildLayer>(System.StringComparer.Ordinal);
        private readonly Dictionary<GameObject, PrefabMeshTemplate[]> _meshTemplatesByPrefab =
            new Dictionary<GameObject, PrefabMeshTemplate[]>();

        public TwcTileMeshSourceProvider(
            ITileWorldCreatorBuildEnvironment environment,
            [InjectOptional] IAtlasTileSetCatalog atlas = null,
            [InjectOptional] ITerrainPassageMap passages = null,
            [InjectOptional] IRecipeHydrologyMap hydrology = null)
        {
            _environment = environment;
            _atlas = atlas;
            _passages = passages;
            _hydrology = hydrology;
        }

        public int CollectMeshSources(ResolvedTileComposition composition, List<TileMeshSource> results)
        {
            if (results == null)
                return 0;

            int added = 0;
            if (composition.HasPassage)
                added += CollectStairPassageSource(composition, results);
            if (!composition.HasMainTerrain)
                return added;

            var sample = composition.MainTerrain;
            TilesBuildLayer buildLayer = ResolveBuildLayer(sample);
            TilePreset preset = ResolvePreset(buildLayer, sample, composition.Cell, GlobalSeed.Current)
                                ?? ResolveAtlasPreset(sample);
            if (preset == null)
                return added;

            added += preset.gridtype == TilePreset.GridType.dual
                ? CollectDualGridSources(composition, buildLayer, preset, results)
                : CollectNormalGridSource(composition, buildLayer, preset, results);
            if (composition.HasWaterSurface
                && sample.TileGeometryMode != TileGeometryMode.SurfaceOnly)
            {
                added += CollectShoreWaterSource(composition, results);
            }
            if (sample.TileGeometryMode == TileGeometryMode.SurfaceOnly)
            {
                added += CollectWaterfallSource(composition, buildLayer, preset, results);
                added += CollectWaterBedSource(composition, sample, buildLayer, results);
            }
            return added;
        }

        /*
         * Emits the sand theme's fill tile as a solid bed column under a
         * surface-only water sheet: top lands on the hydrology bed height,
         * GeneratedClosure stretches the authored bottom ring to a floor
         * below the lowest neighbouring bed, and per-edge bottoms let open
         * skirts stop exactly at each lower neighbour's bed. Without a
         * rendered bed the transparent water would show void/backfaces
         * instead of the sandy bottom the shore fade is meant to reveal.
         * Water neighbours compare bed-to-bed (higher cell owns the skirt);
         * land neighbours occlude via their own surface height.
         */
        private int CollectWaterBedSource(
            ResolvedTileComposition composition,
            TileLayerSample waterSample,
            TilesBuildLayer buildLayer,
            List<TileMeshSource> results)
        {
            float cellSize = ResolveCellSize();
            if (cellSize <= 0.0001f
                || !IsFinite(waterSample.Height)
                || !IsFinite(waterSample.SurfaceHeight)
                || waterSample.Height >= waterSample.SurfaceHeight - 0.0001f
                || _atlas == null
                || !_atlas.IsLoaded
                || !_atlas.TryGetByTileId("sand", out AtlasTileTheme theme)
                || theme?.Preset == null)
            {
                return 0;
            }

            float bedY = waterSample.Height;
            float northH = ResolveBedEdgeHeight(
                composition.Cell, 0, 1, composition.NorthSurfaceHeight);
            float eastH = ResolveBedEdgeHeight(
                composition.Cell, 1, 0, composition.EastSurfaceHeight);
            float southH = ResolveBedEdgeHeight(
                composition.Cell, 0, -1, composition.SouthSurfaceHeight);
            float westH = ResolveBedEdgeHeight(
                composition.Cell, -1, 0, composition.WestSurfaceHeight);

            TileMeshOccludedSides occluded = TileMeshOccludedSides.None;
            if (!IsFinite(northH) || northH >= bedY - 0.0001f)
                occluded |= TileMeshOccludedSides.North;
            if (!IsFinite(eastH) || eastH >= bedY - 0.0001f)
                occluded |= TileMeshOccludedSides.East;
            if (!IsFinite(southH) || southH >= bedY - 0.0001f)
                occluded |= TileMeshOccludedSides.South;
            if (!IsFinite(westH) || westH >= bedY - 0.0001f)
                occluded |= TileMeshOccludedSides.West;

            // The column floor only needs to reach just below the deepest
            // neighbouring bed; deeper skirts would dangle past the rim.
            float floorY = bedY - 0.5f;
            if (IsFinite(northH)) floorY = Mathf.Min(floorY, northH - 0.5f);
            if (IsFinite(eastH)) floorY = Mathf.Min(floorY, eastH - 0.5f);
            if (IsFinite(southH)) floorY = Mathf.Min(floorY, southH - 0.5f);
            if (IsFinite(westH)) floorY = Mathf.Min(floorY, westH - 0.5f);

            var bedSample = new TileLayerSample(
                waterSample.LayerId,
                waterSample.LayerName,
                waterSample.BlueprintLayerGuid,
                waterSample.BuildLayerGuid,
                theme.TileTypeId,
                theme.Preset.tileId,
                LayerKind.BaseTerrain,
                waterSample.SortingOrder,
                waterSample.LayerOrder,
                waterSample.TerrainPriority,
                bedY,
                bedY,
                waterSample.SourceLayerId,
                TileGeometryMode.SolidTerrain,
                AuthoredClosurePolicy.GeneratedClosure);

            var bedComposition = new ResolvedTileComposition(
                composition.Cell,
                bedSample,
                default,
                hasMainTerrain: true,
                hasOverlay: false,
                "water bed",
                northMatches: true,
                eastMatches: true,
                southMatches: true,
                westMatches: true,
                northEastMatches: true,
                southEastMatches: true,
                southWestMatches: true,
                northWestMatches: true,
                supportHeight: floorY,
                northSurfaceHeight: northH,
                eastSurfaceHeight: eastH,
                southSurfaceHeight: southH,
                westSurfaceHeight: westH);

            var edgeBottoms = new TileMeshEdgeBottoms(
                IsFinite(northH) && northH < bedY ? northH : float.NaN,
                IsFinite(eastH) && eastH < bedY ? eastH : float.NaN,
                IsFinite(southH) && southH < bedY ? southH : float.NaN,
                IsFinite(westH) && westH < bedY ? westH : float.NaN);

            TilePreset preset = theme.Preset;
            var tileType = preset.gridtype == TilePreset.GridType.dual
                ? TilePreset.TileType.DUALGRD_fill
                : TilePreset.TileType.NRMGRD_fill;

            return TryAddMeshSources(
                bedComposition,
                buildLayer,
                preset,
                tileType,
                new Vector2(composition.Cell.x, composition.Cell.y),
                yRotation: 0,
                Vector3.one,
                occluded,
                edgeBottoms,
                results);
        }

        /*
         * Height the bed column compares against on one shared edge: a water
         * neighbour contributes its bed (its own column covers above that),
         * a land neighbour contributes its surface (its terrain covers the
         * side), and a missing neighbour sinks to the floor so the bed rim
         * stays sealed underwater.
         */
        private float ResolveBedEdgeHeight(
            Vector2Int cell, int dx, int dy, float landSurface)
        {
            var neighbor = new Vector2Int(cell.x + dx, cell.y + dy);
            if (_hydrology != null
                && _hydrology.TryGetBedHeight(neighbor, out float bed)
                && IsFinite(bed))
            {
                return bed;
            }
            return landSurface;
        }

        /*
         * Emits a vertical water strip on every shared edge between this water
         * cell and a lower neighbouring water/sink cell, so each level drop
         * reads as a waterfall wall — not only the single flow-parent edge.
         * Only the upper cell emits (the lower sees the neighbour as higher),
         * so each edge is covered exactly once. Diagonal neighbours get a
         * corner quad on the shared vertex. The strip reuses the water
         * preset's material and shares the chunk of the owning cell.
         */
        private static readonly Vector2Int[] WaterfallDirs =
        {
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(0, 1),
            new Vector2Int(-1, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(1, 1),
        };

        private int CollectWaterfallSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            List<TileMeshSource> results)
        {
            TileLayerSample sample = composition.MainTerrain;
            float cellSize = ResolveCellSize();
            // Heights must come from the rendered surfaces, not the plan's
            // WaterSurface grid: sink cells store drainage pseudo-surfaces
            // (filled - offset) that can sit metres above the rendered sheet,
            // which would lift the strip into a floating pane.
            float upperY = sample.SurfaceHeight;
            if (_hydrology == null || cellSize <= 0.0001f || !IsFinite(upperY))
                return 0;

            float minDrop = _hydrology.WaterfallMinDropMeters;
            if (minDrop <= 0.0001f)
                minDrop = 0.5f;

            Material[] materials = null;
            int added = 0;
            for (int i = 0; i < WaterfallDirs.Length; i++)
            {
                Vector2Int d = WaterfallDirs[i];
                var neighbor = new Vector2Int(composition.Cell.x + d.x, composition.Cell.y + d.y);
                // Keep the plan gate so falls only face water; the neighbour's
                // own rendered surface then sets the real drop height.
                if (!_hydrology.TryGetWaterSurface(neighbor, out _))
                    continue;
                float lowerY = ResolveNeighborSurfaceHeight(composition, d);
                if (!IsFinite(lowerY))
                    continue;
                float drop = upperY - lowerY;
                if (drop < minDrop)
                    continue;

                materials ??= ResolveWaterfallMaterials(buildLayer, preset);
                if (materials == null || materials.Length == 0)
                    return 0;

                var dir = new Vector3(d.x, 0f, d.y);
                // Sink the strip by a lip overlap: the top edge hides under the
                // upper sheet's rounded rim instead of poking through it, while
                // the submerged bottom stays hidden under the lower sheet.
                Vector3 edgeCenter = new Vector3(
                    (composition.Cell.x + d.x * 0.5f) * cellSize,
                    lowerY - WaterfallLipOverlap,
                    (composition.Cell.y + d.y * 0.5f) * cellSize);
                Quaternion rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                var localMatrix = Matrix4x4.TRS(
                    edgeCenter,
                    rotation,
                    new Vector3(cellSize, drop, 1f));

                var meshSource = new TileMeshSource(
                    GetWaterfallStripMesh(),
                    materials,
                    localMatrix,
                    sample.LayerId,
                    sample.LayerName,
                    tileCenterXZ: new Vector2(
                        composition.Cell.x * cellSize, composition.Cell.y * cellSize),
                    tileHalfExtent: cellSize * 0.5f,
                    tileGeometryMode: TileGeometryMode.SolidTerrain);
                if (!meshSource.IsValid)
                    continue;
                results.Add(meshSource);
                added++;
            }
            return added;
        }

        /// <summary>Rendered winner surface of the neighbour in direction <paramref name="d"/>.</summary>
        private static float ResolveNeighborSurfaceHeight(ResolvedTileComposition composition, Vector2Int d)
        {
            if (d.y > 0)
            {
                return d.x > 0 ? composition.NorthEastSurfaceHeight
                    : d.x < 0 ? composition.NorthWestSurfaceHeight
                    : composition.NorthSurfaceHeight;
            }
            if (d.y < 0)
            {
                return d.x > 0 ? composition.SouthEastSurfaceHeight
                    : d.x < 0 ? composition.SouthWestSurfaceHeight
                    : composition.SouthSurfaceHeight;
            }
            return d.x > 0 ? composition.EastSurfaceHeight
                : d.x < 0 ? composition.WestSurfaceHeight
                : float.NaN;
        }

        private Material[] ResolveWaterfallMaterials(TilesBuildLayer buildLayer, TilePreset preset)
        {
            Material materialOverride = preset != null ? preset.GetMaterialOverride() : null;
            if (materialOverride != null)
                return new[] { materialOverride };
            GameObject prefab = preset?.GetTile(TilePreset.TileType.NRMGRD_fill,
                out _, out _);
            if (prefab != null && TryGetMeshTemplates(prefab, out var templates)
                && templates.Length > 0)
            {
                return templates[0].ResolveMaterials(null);
            }
            return null;
        }

        /// <summary>
        /// Unit vertical strip: 1 wide (local X), 1 tall (local Y from 0 at the
        /// bottom edge to 1 at the top), facing +Z. Double-sided triangles so
        /// the fall reads from both banks.
        /// </summary>
        private static Mesh GetWaterfallStripMesh()
        {
            if (_waterfallStripMesh != null)
                return _waterfallStripMesh;
            var mesh = new Mesh
            {
                name = "WaterfallStrip",
                vertices = new[]
                {
                    new Vector3(-0.5f, 0f, 0f),
                    new Vector3(0.5f, 0f, 0f),
                    new Vector3(0.5f, 1f, 0f),
                    new Vector3(-0.5f, 1f, 0f),
                },
                uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(1f, 1f),
                    new Vector2(0f, 1f),
                },
                triangles = new[]
                {
                    0, 1, 2, 0, 2, 3,
                    2, 1, 0, 3, 2, 0,
                },
                normals = new[]
                {
                    Vector3.back, Vector3.back, Vector3.back, Vector3.back,
                },
            };
            _waterfallStripMesh = mesh;
            return mesh;
        }

        /*
         * Extends the water sheet one cell onto water-adjacent land so the
         * Stylized Water shader's tile/intersection function can wash the
         * shoreline. The fill fragment lands on the cell footprint at the
         * neighboring water surface height, using the water preset itself.
         */
        private int CollectShoreWaterSource(
            ResolvedTileComposition composition,
            List<TileMeshSource> results)
        {
            TileLayerSample waterSample = composition.WaterSurface;
            float landSurface = composition.MainTerrain.SurfaceHeight;
            // A wash sheet above the land surface reads as a floating cyan
            // plate on dry ground; it is only meant to wash the seam under
            // the bank face, never to surface on top of it.
            if (IsFinite(landSurface)
                && waterSample.SurfaceHeight > landSurface + 0.0001f)
            {
                return 0;
            }
            TilesBuildLayer buildLayer = ResolveBuildLayer(waterSample);
            TilePreset preset =
                ResolvePreset(buildLayer, waterSample, composition.Cell, GlobalSeed.Current)
                ?? ResolveAtlasPreset(waterSample);
            if (preset == null)
                return 0;

            float waterHeight = waterSample.SurfaceHeight;
            var waterComposition = new ResolvedTileComposition(
                composition.Cell,
                waterSample,
                default,
                hasMainTerrain: true,
                hasOverlay: false,
                composition.Reason,
                northMatches: true,
                eastMatches: true,
                southMatches: true,
                westMatches: true,
                northEastMatches: true,
                southEastMatches: true,
                southWestMatches: true,
                northWestMatches: true,
                supportHeight: waterHeight,
                northSurfaceHeight: waterHeight,
                eastSurfaceHeight: waterHeight,
                southSurfaceHeight: waterHeight,
                westSurfaceHeight: waterHeight,
                northEastSurfaceHeight: waterHeight,
                southEastSurfaceHeight: waterHeight,
                southWestSurfaceHeight: waterHeight,
                northWestSurfaceHeight: waterHeight);

            if (preset.gridtype == TilePreset.GridType.dual)
            {
                int before = results.Count;
                /*
                 * The wash sheet covers the owning cell's full footprint,
                 * so it is emitted at the cell centre (offset zero), not on
                 * a shared vertex like ordinary dual fragments.
                 */
                TryAddDualGridSource(
                    waterComposition,
                    buildLayer,
                    preset,
                    Vector2.zero,
                    topLeft: true,
                    topRight: true,
                    bottomLeft: true,
                    bottomRight: true,
                    waterHeight,
                    waterHeight,
                    waterHeight,
                    waterHeight,
                    ownedByCurrentCell: true,
                    results);
                return results.Count - before;
            }

            return TryAddMeshSources(
                waterComposition,
                buildLayer,
                preset,
                TilePreset.TileType.NRMGRD_fill,
                new Vector2(composition.Cell.x, composition.Cell.y),
                yRotation: 0,
                Vector3.one,
                TileMeshOccludedSides.None,
                ResolveEdgeBottoms(waterComposition),
                results);
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
            // This provider emits each physical fragment exactly once. The terrain
            // builder performs final chunk assignment from TileCenterXZ.
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
            /*
             * TWC dual-grid fragments are centered on the half-offset
             * vertices between logical cells (cell + offset, matching
             * TilesBuildLayer's own tilePosition convention). The terrain
             * builder resolves the physical cell index from the fragment's
             * TileCenterXZ and keeps the border vertices at x == width /
             * y == height, so the painted pattern stays aligned with the
             * gameplay grid instead of being shifted by half a cell.
             */
            var tileData = new BuildLayer.TileData
            {
                configuration = configuration,
                tilePosition = new Vector2(
                    composition.Cell.x + offset.x,
                    composition.Cell.y + offset.y)
            };

            float mainSurface = composition.MainTerrain.SurfaceHeight;
            TileMeshOccludedSides occludedSides = ResolveDualOccludedSides(
                mainSurface,
                topLeftSurface,
                topRightSurface,
                bottomLeftSurface,
                bottomRightSurface);
            TileMeshEdgeBottoms edgeBottoms = ResolveDualEdgeBottoms(
                composition,
                topLeftSurface,
                topRightSurface,
                bottomLeftSurface,
                bottomRightSurface);
            /*
             * The fragment spans the quad between the four surrounding cell
             * centers; those cells' surface heights are the fragment's corner
             * heights. Missing neighbours fall back to the cell surface so map
             * edges stay flat.
             */
            TileMeshCornerHeights cornerHeights = IsFinite(mainSurface)
                ? new TileMeshCornerHeights(
                    ResolveFiniteOrFallback(topLeftSurface, mainSurface),
                    ResolveFiniteOrFallback(topRightSurface, mainSurface),
                    ResolveFiniteOrFallback(bottomLeftSurface, mainSurface),
                    ResolveFiniteOrFallback(bottomRightSurface, mainSurface),
                    mainSurface)
                : default;

            int packMask = AtlasDualGridShapes.BuildMask(
                northWest: topLeft,
                northEast: topRight,
                southWest: bottomLeft,
                southEast: bottomRight);
            if (TryAddAtlasDualSource(
                    composition,
                    buildLayer,
                    preset,
                    packMask,
                    tileData.tilePosition,
                    occludedSides,
                    edgeBottoms,
                    cornerHeights,
                    results))
            {
                return;
            }

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
                occludedSides,
                edgeBottoms,
                results,
                cornerHeights);
        }

        /// <summary>
        /// Atlas-pack dual-grid dispatch: resolves the fragment's canonical form
        /// and rotation in pack space, demotes flat biome seams to fill, and
        /// picks the low (0.25 m) variant when every open drop fits it.
        /// </summary>
        private bool TryAddAtlasDualSource(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            int packMask,
            Vector2 tilePosition,
            TileMeshOccludedSides occludedSides,
            TileMeshEdgeBottoms edgeBottoms,
            TileMeshCornerHeights cornerHeights,
            List<TileMeshSource> results)
        {
            if (_atlas == null
                || !_atlas.IsLoaded
                || !_atlas.TryGetByPreset(preset, out AtlasTileTheme theme))
            {
                return false;
            }

            AtlasTileForm form;
            int yRotation;
            if (AtlasDualGridShapes.ShouldDemoteToFill(packMask, occludedSides)
                || !AtlasDualGridShapes.TryResolve(packMask, out form, out yRotation))
            {
                form = AtlasTileForm.Fill;
                yRotation = 0;
            }

            float maxOpenDrop = AtlasDualGridShapes.ResolveMaxOpenDrop(
                packMask,
                occludedSides,
                composition.MainTerrain.SurfaceHeight,
                edgeBottoms.North,
                edgeBottoms.East,
                edgeBottoms.South,
                edgeBottoms.West);
            bool lowVariant = IsFinite(maxOpenDrop)
                              && maxOpenDrop <= _atlas.LowBorderDropMaxMeters + 0.0001f;
            GameObject prefab = theme.ResolveForm(form, lowVariant);
            if (prefab == null)
                return false;

            return TryAddPrefabMeshSources(
                       composition,
                       buildLayer,
                       preset,
                       prefab,
                       xRotationOffset: 0f,
                       yRotationOffset: 0f,
                       tilePosition,
                       yRotation,
                       Vector3.one,
                       occludedSides,
                       edgeBottoms,
                       results,
                       cornerHeights) > 0;
        }

        /// <summary>
        /// Emits the generated stair module occupying the cell: the pack stair
        /// prefab rotated so it climbs toward the flight's exit direction, with
        /// its top edge flush at the module's top height.
        /// </summary>
        private int CollectStairPassageSource(
            ResolvedTileComposition composition,
            List<TileMeshSource> results)
        {
            var sample = composition.Passage;
            AtlasTileTheme theme = null;
            int directionIndex = 0;
            float topY = sample.Height;
            float rise = 0f;

            if (_passages != null
                && _passages.TryGetModule(composition.Cell, out TerrainPassageModule module))
            {
                directionIndex = module.DirectionIndex;
                topY = module.TopY;
                rise = module.RiseMeters;
                if (!string.IsNullOrWhiteSpace(module.ThemeId))
                    _atlas?.TryGetByThemeId(module.ThemeId, out theme);
            }
            if (theme == null && _atlas != null)
            {
                if (!_atlas.TryGetByThemeId(sample.PresetId, out theme))
                    _atlas.TryGetByTileId(sample.TileId, out theme);
            }

            GameObject prefab = theme?.Stair;
            if (prefab == null || !TryGetMeshTemplates(prefab, out PrefabMeshTemplate[] templates))
                return 0;

            float cellSize = ResolveCellSize();
            Quaternion rotation = Quaternion.Euler(0f, directionIndex * 90f, 0f);
            var position = new Vector3(
                composition.Cell.x * cellSize,
                topY,
                composition.Cell.y * cellSize);
            Vector3 scale = prefab.transform.localScale;
            if (rise > 0f && _atlas != null)
                scale.y *= rise / Mathf.Max(0.01f, _atlas.StairModuleRiseMeters);
            Matrix4x4 rootMatrix = Matrix4x4.TRS(position, rotation, scale);
            Material materialOverride = theme.Preset != null ? theme.Preset.GetMaterialOverride() : null;

            int added = 0;
            for (int i = 0; i < templates.Length; i++)
            {
                PrefabMeshTemplate template = templates[i];
                var meshSource = new TileMeshSource(
                    template.Mesh,
                    template.ResolveMaterials(materialOverride),
                    rootMatrix * template.ChildMatrix,
                    sample.LayerId,
                    sample.LayerName,
                    visibleBottomY: float.NaN,
                    occludedSides: TileMeshOccludedSides.None,
                    tileCenterXZ: new Vector2(position.x, position.z),
                    tileHalfExtent: cellSize * 0.5f,
                    authoredClosurePolicy: AuthoredClosurePolicy.PreserveAuthored,
                    edgeBottoms: default,
                    tileGeometryMode: TileGeometryMode.SolidTerrain);
                if (!meshSource.IsValid)
                    continue;

                results.Add(meshSource);
                added++;
            }
            return added;
        }

        private TilePreset ResolveAtlasPreset(TileLayerSample sample)
        {
            if (_atlas == null || !_atlas.IsLoaded)
                return null;

            AtlasTileTheme theme;
            if (_atlas.TryGetByPresetId(sample.PresetId, out theme)
                || _atlas.TryGetByTileId(sample.TileId, out theme))
            {
                return theme?.Preset;
            }
            return null;
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
            List<TileMeshSource> results,
            TileMeshCornerHeights cornerHeights = default)
        {
            GameObject prefab = preset.GetTile(tileType, out float xRotationOffset, out float yRotationOffset);
            if (prefab == null)
                return 0;

            return TryAddPrefabMeshSources(
                composition,
                buildLayer,
                preset,
                prefab,
                xRotationOffset,
                yRotationOffset,
                tilePosition,
                yRotation,
                scaleSign,
                occludedSides,
                edgeBottoms,
                results,
                cornerHeights);
        }

        private int TryAddPrefabMeshSources(
            ResolvedTileComposition composition,
            TilesBuildLayer buildLayer,
            TilePreset preset,
            GameObject prefab,
            float xRotationOffset,
            float yRotationOffset,
            Vector2 tilePosition,
            int yRotation,
            Vector3 scaleSign,
            TileMeshOccludedSides occludedSides,
            TileMeshEdgeBottoms edgeBottoms,
            List<TileMeshSource> results,
            TileMeshCornerHeights cornerHeights = default)
        {
            var sample = composition.MainTerrain;
            if (!TryGetMeshTemplates(prefab, out PrefabMeshTemplate[] templates))
                return 0;

            float cellSize = ResolveCellSize();
            Vector3 scale = prefab.transform.localScale;
            if (buildLayer != null && buildLayer.scaleTileToCellSize)
                scale *= cellSize;

            Vector3 scaleOffset = buildLayer != null ? buildLayer.scaleOffset : Vector3.one;
            scale = new Vector3(
                scale.x * scaleOffset.x * scaleSign.x,
                scale.y * scaleOffset.y * scaleSign.y,
                scale.z * scaleOffset.z * scaleSign.z);

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
            Material materialOverride = preset != null ? preset.GetMaterialOverride() : null;
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
                    sample.LayerId,
                    sample.LayerName,
                    visibleBottomY,
                    occludedSides,
                    new Vector2(position.x, position.z),
                    cellSize * 0.5f,
                    sample.AuthoredClosurePolicy,
                    edgeBottoms,
                    sample.TileGeometryMode,
                    generateMissingClosure: i == missingClosureOwner,
                    cornerHeights: cornerHeights);
                    
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
            TileLayerSample sample,
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

        private TilesBuildLayer ResolveBuildLayer(TileLayerSample sample)
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

        private static TilePreset ResolvePreset(TilesBuildLayer buildLayer, TileLayerSample sample, Vector2Int cell, int seed)
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
            TileLayerSample sample,
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
                sample.LayerId,
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
