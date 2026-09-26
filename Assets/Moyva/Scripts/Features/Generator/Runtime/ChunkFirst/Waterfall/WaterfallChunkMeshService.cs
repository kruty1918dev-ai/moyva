using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Owns the shared waterfall front field for the active world and emits
    /// the per-chunk curtain mesh: a short lipped profile on the Stylized
    /// Water 3 waterfall material. The material scrolls foam downward in
    /// world space, so any curtain shape animates like a falling sheet —
    /// replacing the stretched per-cell water strips from
    /// <see cref="TwcTileMeshSourceProvider"/>.
    /// </summary>
    internal sealed class WaterfallChunkMeshService
    {
        /*
         * Curtain cross-section (horizontal offset along the fall direction
         * as a fraction of cell size, plus height offset):
         * back lip tucks under the upper sheet's rim, the crest noses
         * outward, the fall runs straight, the base bows out slightly and
         * dips under the lower sheet.
         */
        private static readonly (float Offset, float Top, float Bottom)[] Profile =
        {
            (-0.18f, -0.02f, float.NaN),
            (+0.06f, -0.12f, float.NaN),
            (+0.06f, float.NaN, float.NaN),
            (+0.14f, float.NaN, -0.15f),
        };

        private const float MidBlend = 0.55f;

        private readonly IRecipeHydrologyMap _hydrology;
        private readonly ITileWorldCreatorBuildEnvironment _environment;

        private int _fieldVersion = -1;
        private float _fieldCellSize;
        private int _fieldWidth;
        private int _fieldHeight;
        private WaterfallFieldPlanner.Field _field;
        private RecipeWaterfallConfig _config;
        private float _minDropMeters;

        public WaterfallChunkMeshService(
            [InjectOptional] IRecipeHydrologyMap hydrology = null,
            [InjectOptional] ITileWorldCreatorBuildEnvironment environment = null)
        {
            _hydrology = hydrology;
            _environment = environment;
        }

        /// <summary>
        /// True when the recipe enables waterfalls and a curtain material is
        /// assigned. When false the legacy stretched strips keep rendering.
        /// </summary>
        public bool IsActive
            => _hydrology != null
               && _hydrology.HasHydrology
               && _config != null
               && _config.Enabled
               && _config.CurtainMaterial != null;

        public bool HasField => _field != null;

        /// <summary>Fronts detected for the active map (VFX placement, dumps).</summary>
        public IReadOnlyList<WaterfallFieldPlanner.Front> Fronts
            => (IReadOnlyList<WaterfallFieldPlanner.Front>)_field?.Fronts
               ?? System.Array.Empty<WaterfallFieldPlanner.Front>();

        /// <summary>Resolved config; null when the recipe disables waterfalls.</summary>
        public RecipeWaterfallConfig Config => _config;

        /// <summary>Effective drop threshold in meters (levels × height step).</summary>
        public float MinDropMeters => _minDropMeters;

        /// <summary>True when a curtain covers the (cell, dir) drop edge.</summary>
        public bool IsCovered(Vector2Int cell, Vector2Int dir)
            => _field != null && _field.IsCovered(cell, dir);

        /// <summary>
        /// Refreshes the cached field when the hydrology world changed. Same
        /// contract as the seabed: call once per map before chunk iteration.
        /// Rendered sheet heights come from the resolved compositions, so
        /// the field exists at chunk-build time.
        /// </summary>
        public void Prepare(
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            int mapWidth,
            int mapHeight,
            float cellSize)
        {
            _config = _hydrology?.Waterfalls;
            int version = _hydrology?.Version ?? -1;
            if (_field != null
                && _fieldVersion == version
                && _fieldWidth == mapWidth
                && _fieldHeight == mapHeight
                && Mathf.Approximately(_fieldCellSize, cellSize))
            {
                return;
            }

            _fieldVersion = version;
            _fieldWidth = mapWidth;
            _fieldHeight = mapHeight;
            _fieldCellSize = cellSize;
            _field = null;
            if (!IsActive || resolvedCells == null
                || mapWidth <= 0 || mapHeight <= 0 || cellSize <= 0.0001f)
            {
                return;
            }

            var waterSheet = new bool[mapWidth, mapHeight];
            var surfaces = new float[mapWidth, mapHeight];
            var waterTarget = new bool[mapWidth, mapHeight];
            foreach (var pair in resolvedCells)
            {
                var cell = pair.Key;
                if (cell.x < 0 || cell.y < 0 || cell.x >= mapWidth || cell.y >= mapHeight)
                    continue;
                if (pair.Value.HasMainTerrain)
                {
                    var main = pair.Value.MainTerrain;
                    waterSheet[cell.x, cell.y] =
                        main.TileGeometryMode == TileGeometryMode.SurfaceOnly;
                    surfaces[cell.x, cell.y] = main.SurfaceHeight;
                }
                else
                {
                    surfaces[cell.x, cell.y] = float.NaN;
                }
                // The pour must land on plan-water, matching the legacy
                // strip gate — a wet tile above a dry cliff is no fall.
                waterTarget[cell.x, cell.y] =
                    _hydrology.TryGetWaterSurface(cell, out _);
            }

            float step = _environment?.Options != null
                ? Mathf.Max(0.01f, _environment.Options.TerrainHeightStep)
                : 1f;
            _minDropMeters = Mathf.Max(0.01f, _config.MinDropLevels * step);
            _field = WaterfallFieldPlanner.Build(
                mapWidth, mapHeight, cellSize,
                waterSheet, surfaces, waterTarget, _minDropMeters);
        }

        /// <summary>
        /// Emits the chunk's waterfall curtains as a single mesh on the
        /// configured material; false when no front anchors inside the
        /// chunk's core rect. Chunk borders can't duplicate geometry
        /// because each front builds exactly in its anchor chunk.
        /// </summary>
        public bool TryBuildChunkMesh(
            RectInt coreRect,
            out Mesh mesh,
            out Material material)
        {
            mesh = null;
            material = null;
            if (_field == null || _config == null)
                return false;

            float cs = _field.CellSize;
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int f = 0; f < _field.Fronts.Count; f++)
            {
                var front = _field.Fronts[f];
                if (!coreRect.Contains(front.Anchor))
                    continue;
                for (int e = 0; e < front.Edges.Count; e++)
                    EmitEdge(front.Edges[e], cs, verts, normals, uvs, tris);
            }

            if (tris.Count == 0)
                return false;

            mesh = new Mesh
            {
                name = $"waterfalls_{coreRect.xMin}_{coreRect.yMin}",
                vertices = verts.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = tris.ToArray(),
            };
            mesh.RecalculateBounds();
            material = _config.CurtainMaterial;
            return true;
        }

        private static void EmitEdge(
            WaterfallFieldPlanner.Edge edge,
            float cs,
            List<Vector3> verts,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> tris)
        {
            var n = new Vector3(edge.Dir.x, 0f, edge.Dir.y).normalized;
            var t = new Vector3(-n.z, 0f, n.x);
            var center = new Vector3(
                (edge.Cell.x + edge.Dir.x * 0.5f) * cs, 0f,
                (edge.Cell.y + edge.Dir.y * 0.5f) * cs);
            Vector3 a = center - t * (cs * 0.5f);
            Vector3 b = center + t * (cs * 0.5f);

            float crestY = edge.TopY + Profile[1].Top;
            float baseY = edge.BottomY + Profile[3].Bottom;
            float midY = Mathf.Lerp(crestY, baseY, MidBlend);
            int rows = Profile.Length;

            for (int face = 0; face < 2; face++)
            {
                var faceN = face == 0 ? n : -n;
                int rowStart = verts.Count;
                for (int j = 0; j < rows; j++)
                {
                    var p = Profile[j];
                    float y = !float.IsNaN(p.Top) ? edge.TopY + p.Top
                        : !float.IsNaN(p.Bottom) ? edge.BottomY + p.Bottom
                        : midY;
                    var off = n * (p.Offset * cs);
                    verts.Add(a + off + new Vector3(0f, y, 0f));
                    verts.Add(b + off + new Vector3(0f, y, 0f));
                    normals.Add(faceN);
                    normals.Add(faceN);
                    uvs.Add(new Vector2(0f, j));
                    uvs.Add(new Vector2(1f, j));
                }

                for (int j = 0; j < rows - 1; j++)
                {
                    int v0 = rowStart + j * 2;
                    int v1 = v0 + 1;
                    int v2 = v0 + 3;
                    int v3 = v0 + 2;
                    if (face == 0)
                    {
                        tris.Add(v0); tris.Add(v1); tris.Add(v2);
                        tris.Add(v0); tris.Add(v2); tris.Add(v3);
                    }
                    else
                    {
                        tris.Add(v0); tris.Add(v2); tris.Add(v1);
                        tris.Add(v0); tris.Add(v3); tris.Add(v2);
                    }
                }
            }
        }
    }
}
