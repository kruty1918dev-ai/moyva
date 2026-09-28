using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Owns the shared waterfall front field for the active world and emits
    /// one ribbon mesh per front: a profiled curtain that tucks under the
    /// upper water sheet, noses over the lip, falls with a slight bow and
    /// curls under the lower sheet. The Stylized Water 3 river material
    /// scrolls along mesh UV, so UV is laid out in meters (constant foam
    /// density at any drop or width) and each drop bucket gets a material
    /// clone whose animation-direction magnitude scales with sqrt(drop) —
    /// free-fall speed for the pour.
    /// </summary>
    internal sealed class WaterfallChunkMeshService
    {
        /// <summary>Chunk payload: one generated ribbon plus its bucket material.</summary>
        public readonly struct Curtain
        {
            public Curtain(Mesh mesh, Material material)
            {
                Mesh = mesh;
                Material = material;
            }

            public Mesh Mesh { get; }
            public Material Material { get; }
        }

        /*
         * Curtain cross-section: horizontal offset along the fall direction
         * as a fraction of cell size, plus height offset. Rows run lip ->
         * tail; NaN rows interpolate between the upper and lower sheets.
         * The lip rows stay fixed so the tuck under the upper sheet survives
         * shallow drops; the nose/fall/curl rows compress horizontally when
         * the drop is shorter than the natural profile height.
         */
        private static readonly (float Offset, float Top, float Bottom, float Blend)[] Profile =
        {
            (-0.30f, -0.01f, float.NaN, 0f),
            (-0.08f, -0.04f, float.NaN, 0f),
            (+0.07f, -0.11f, float.NaN, 0f),
            (+0.11f, float.NaN, float.NaN, 0.30f),
            (+0.13f, float.NaN, float.NaN, 0.70f),
            (+0.19f, float.NaN, +0.06f, 0f),
            (+0.26f, float.NaN, -0.12f, 0f),
        };

        /// <summary>Profile rows from this index compress sideways on shallow drops.</summary>
        private const int CompressibleRow = 3;

        /// <summary>Drop height at which the full horizontal bow is reached.</summary>
        internal const float FullProfileDropMeters = 0.45f;

        internal const float MinBowFactor = 0.3f;
        private const float SpeedFactorMin = 0.25f;
        private const float SpeedFactorMax = 6f;
        private const float DropBucketMeters = 0.1f;
        private static readonly Vector4 ShaderDirectionId = new Vector4(0f, -1f, 0f, 0f);

        private readonly IRecipeHydrologyMap _hydrology;
        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly ResolvedGenerationProfile _profile;
        private readonly Dictionary<int, Material> _bucketMaterials =
            new Dictionary<int, Material>();

        private int _fieldVersion = -1;
        private float _fieldCellSize;
        private int _fieldWidth;
        private int _fieldHeight;
        private WaterfallFieldPlanner.Field _field;
        private RecipeWaterfallConfig _config;
        private float _minDropMeters;

        public WaterfallChunkMeshService(
            [InjectOptional] IRecipeHydrologyMap hydrology = null,
            [InjectOptional] ITileWorldCreatorBuildEnvironment environment = null,
            [InjectOptional] ResolvedGenerationProfile profile = null)
        {
            _hydrology = hydrology;
            _environment = environment;
            _profile = profile;
        }

        /// <summary>
        /// True when the recipe enables waterfalls and supplies the curtain
        /// material the generated ribbons render with.
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
        /// the field exists at chunk-build time. Bucket material clones are
        /// rebuilt alongside so they never outlive their template.
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
            ClearBucketMaterials();
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
                    if (waterSheet[cell.x, cell.y] && _profile?.SimpleWater == true)
                        surfaces[cell.x, cell.y] += _profile.WaterSurfaceOffset;
                }
                else
                {
                    surfaces[cell.x, cell.y] = float.NaN;
                }
                // The pour must land on plan-water, matching the legacy
                // strip gate — a wet tile above a dry cliff is no fall.
                waterTarget[cell.x, cell.y] = waterSheet[cell.x, cell.y];
            }

            float step = _environment?.Options != null
                ? Mathf.Max(0.01f, _environment.Options.TerrainHeightStep)
                : 1f;
            _minDropMeters = Mathf.Max(0.01f, _config.MinDropMeters > 0f
                ? _config.MinDropMeters : _config.MinDropLevels * step);
            _field = WaterfallFieldPlanner.Build(
                mapWidth, mapHeight, cellSize,
                waterSheet, surfaces, waterTarget, _minDropMeters,
                cardinalEdgesOnly: _profile?.SimpleWater == true);
        }

        /// <summary>
        /// Emits one ribbon per front anchored inside the chunk's core rect.
        /// Fronts with the same quantized drop share a material clone whose
        /// scroll speed scales with sqrt(drop / reference); chunk borders
        /// can't duplicate geometry because each front builds exactly in its
        /// anchor chunk.
        /// </summary>
        public int CollectChunkMeshes(RectInt coreRect, List<Curtain> results)
        {
            if (_field == null || _config == null || results == null)
                return 0;

            float cs = _field.CellSize;
            int added = 0;
            for (int f = 0; f < _field.Fronts.Count; f++)
            {
                var front = _field.Fronts[f];
                if (!coreRect.Contains(front.Anchor))
                    continue;
                Mesh mesh = BuildFrontMesh(front, cs);
                if (mesh == null)
                    continue;
                results.Add(new Curtain(mesh, MaterialForDrop(front.Drop)));
                added++;
            }
            return added;
        }

        /*
         * Ribbon grid: profile rows x span columns. The span runs from the
         * first edge's start to the last edge's end, so a merged front is a
         * single seamless sheet. U is world-projected on the front tangent
         * (texture phase aligns across cells and chunks); V is profile arc
         * length from the lip, so foam density never stretches with drop.
         */
        private Mesh BuildFrontMesh(
            WaterfallFieldPlanner.Front front,
            float cs)
        {
            int cols = front.Edges.Count + 1;
            int rows = Profile.Length;
            var n = new Vector3(front.Dir.x, 0f, front.Dir.y).normalized;
            var t = new Vector3(-n.z, 0f, n.x);

            var first = front.Edges[0];
            var last = front.Edges[front.Edges.Count - 1];
            var spanA = new Vector3(
                (first.Cell.x + first.Dir.x * 0.5f) * cs, 0f,
                (first.Cell.y + first.Dir.y * 0.5f) * cs) - t * (cs * 0.5f);
            var spanB = new Vector3(
                (last.Cell.x + last.Dir.x * 0.5f) * cs, 0f,
                (last.Cell.y + last.Dir.y * 0.5f) * cs) + t * (cs * 0.5f);

            float topY = front.TopY;
            float bottomY = front.BottomY;
            float drop = Mathf.Max(0.001f, front.Drop);
            float bow = BowFactor(drop);

            // Profile points in ribbon space: u = along fall dir, y = world.
            var offsets = new float[rows];
            var heights = new float[rows];
            float prevY = float.MaxValue;
            for (int j = 0; j < rows; j++)
            {
                var p = Profile[j];
                float y = !float.IsNaN(p.Top) ? topY + p.Top
                    : !float.IsNaN(p.Bottom) ? bottomY + p.Bottom
                    : Mathf.Lerp(topY, bottomY, p.Blend);
                y = Mathf.Min(y, prevY);
                prevY = y;
                heights[j] = y;
                offsets[j] = p.Offset * cs * (j >= CompressibleRow ? bow : 1f);
            }

            // Arc length per row and smoothed profile tangents.
            var arc = new float[rows];
            for (int j = 1; j < rows; j++)
            {
                float du = offsets[j] - offsets[j - 1];
                float dy = heights[j] - heights[j - 1];
                arc[j] = arc[j - 1] + Mathf.Sqrt(du * du + dy * dy);
            }

            var verts = new List<Vector3>(cols * rows * 2);
            var normals = new List<Vector3>(cols * rows * 2);
            var uvs = new List<Vector2>(cols * rows * 2);
            var tris = new List<int>((cols - 1) * (rows - 1) * 12);

            float uvTile = Mathf.Max(0.05f, _config.UvTileSizeMeters);
            for (int face = 0; face < 2; face++)
            {
                int rowStart = verts.Count;
                for (int j = 0; j < rows; j++)
                {
                    // Smoothed ribbon normal: perpendicular of the profile
                    // tangent, facing downstream (front) or upstream (back).
                    int ja = Mathf.Max(0, j - 1);
                    int jb = Mathf.Min(rows - 1, j + 1);
                    float tu = offsets[jb] - offsets[ja];
                    float tv = heights[jb] - heights[ja];
                    var nrm = new Vector3(n.x * -tv, tu, n.z * -tv);
                    if (nrm.sqrMagnitude < 0.0001f)
                        nrm = n;
                    nrm.Normalize();
                    if (face == 1)
                        nrm = -nrm;

                    float v = arc[j] / uvTile;
                    for (int c = 0; c < cols; c++)
                    {
                        float s = (float)c / (cols - 1);
                        var p = Vector3.LerpUnclamped(spanA, spanB, s);
                        p += n * offsets[j];
                        p.y = heights[j];
                        verts.Add(p);
                        normals.Add(nrm);
                        uvs.Add(new Vector2(Vector3.Dot(p, t) / uvTile, v));
                    }
                }

                for (int j = 0; j < rows - 1; j++)
                {
                    for (int c = 0; c < cols - 1; c++)
                    {
                        int v0 = rowStart + j * cols + c;
                        int v1 = v0 + 1;
                        int v2 = v0 + cols + 1;
                        int v3 = v0 + cols;
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

            if (tris.Count == 0)
                return null;

            var mesh = new Mesh
            {
                name = $"waterfall_{front.Anchor.x}_{front.Anchor.y}_{front.Dir.x}_{front.Dir.y}",
                vertices = verts.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = tris.ToArray(),
            };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>Horizontal bow compression a drop of this height gets.</summary>
        internal static float BowFactor(float drop)
            => Mathf.Max(MinBowFactor,
                Mathf.Min(1f, drop / FullProfileDropMeters));

        /*
         * Clone-per-drop-bucket materials: the river shader scrolls at
         * TIME * -_Direction, so scaling the direction vector scales every
         * animated feature (normals, foam, intersection) with one property.
         * sqrt(drop / reference) tracks free-fall velocity; buckets merge
         * near-identical drops so cloned materials stay countable.
         */
        private Material MaterialForDrop(float drop)
        {
            int key = Mathf.RoundToInt(drop / DropBucketMeters);
            if (_bucketMaterials.TryGetValue(key, out var mat) && mat != null)
                return mat;

            mat = new Material(_config.CurtainMaterial);
            mat.name = $"{_config.CurtainMaterial.name}_d{key * DropBucketMeters:0.0}";
            if (mat.HasProperty("_Direction"))
            {
                var dir = mat.GetVector("_Direction");
                float reference = Mathf.Max(0.01f, _config.SpeedReferenceDropMeters);
                float factor = Mathf.Clamp(
                    _config.ScrollSpeedScale * Mathf.Sqrt(drop / reference),
                    SpeedFactorMin, SpeedFactorMax);
                var dir2 = new Vector2(dir.x, dir.y);
                if (dir2.sqrMagnitude < 0.001f)
                    dir2 = new Vector2(ShaderDirectionId.x, ShaderDirectionId.y);
                dir2.Normalize();
                mat.SetVector("_Direction",
                    new Vector4(dir2.x * factor, dir2.y * factor, 0f, 0f));
            }
            _bucketMaterials[key] = mat;
            return mat;
        }

        private void ClearBucketMaterials()
        {
            foreach (var pair in _bucketMaterials)
            {
                if (pair.Value == null)
                    continue;
                if (Application.isPlaying)
                    Object.Destroy(pair.Value);
                else
                    Object.DestroyImmediate(pair.Value);
            }
            _bucketMaterials.Clear();
        }
    }
}
