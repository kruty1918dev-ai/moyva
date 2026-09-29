using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Owns rendered water drop fronts and generates curtains from the SW3
    /// source profile, with common height rows at corners. Materials use
    /// metre-scaled flow UVs and normalized lip/impact coordinates in UV2.
    /// One material per drop bucket controls downstream animation speed.
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
                Mesh mesh = BuildPrefabFrontMesh(front, cs) ?? BuildFrontMesh(front, cs);
                if (mesh == null)
                    continue;
                results.Add(new Curtain(mesh, MaterialForDrop(front.Drop)));
                added++;
            }
            return added;
        }

        // Resample the SW3 model onto a regular grid. Keeping the source's
        // irregular triangles while fitting unequal pools creates long fans
        // at corners; common world-height rows keep those seams vertical.
        private Mesh BuildPrefabFrontMesh(WaterfallFieldPlanner.Front front, float cs)
        {
            var filter = _config.CurtainPrefab != null
                ? _config.CurtainPrefab.GetComponent<MeshFilter>() : null;
            var source = filter != null ? filter.sharedMesh : null;
            if (source == null || !source.isReadable || source.bounds.size.z < 0.001f)
                return null;
            var sourceVertices = source.vertices;
            var sourceUv = source.uv;
            if (sourceUv.Length != sourceVertices.Length) return null;
            float minV = float.MaxValue, maxV = float.MinValue;
            foreach (var coordinate in sourceUv)
            {
                minV = Mathf.Min(minV, coordinate.y);
                maxV = Mathf.Max(maxV, coordinate.y);
            }
            if (maxV - minV < 0.001f) return null;

            var n = new Vector3(front.Dir.x, 0f, front.Dir.y).normalized;
            var tangent = new Vector3(-n.z, 0f, n.x);
            var center = front.Center * cs;
            float width = front.WidthCells * cs;
            var start = FindJoin(front, center - tangent * width * 0.5f, cs);
            var end = FindJoin(front, center + tangent * width * 0.5f, cs);
            float tuck = Mathf.Min(cs * 0.002f, front.Drop * 0.02f);
            float depth = Mathf.Min(cs * 0.12f, front.Drop * 0.3f);
            float approach = cs * 0.18f;
            float runout = cs * 0.32f;
            float uvTile = Mathf.Max(0.05f, _config.UvTileSizeMeters);
            int columns = Mathf.Max(8, front.WidthCells * 8) + 1;
            var distances = new List<float> { -approach, -approach * 0.6f, -approach * 0.25f,
                0f, front.Drop, front.Drop + runout * 0.25f,
                front.Drop + runout * 0.6f, front.Drop + runout };
            // This spacing is independent of either pool's bottom level.
            for (float d = cs / 12f; d < front.Drop; d += cs / 12f)
                distances.Add(d);
            // Add a small lip bend even for sub-cell drops.
            distances.Add(Mathf.Min(cs * 0.04f, front.Drop * 0.1f));
            AddJoinRows(start);
            AddJoinRows(end);
            distances.Sort();
            for (int i = distances.Count - 1; i > 0; i--)
                if (Mathf.Abs(distances[i] - distances[i - 1]) < 0.00001f)
                    distances.RemoveAt(i);

            int rows = distances.Count;
            var vertices = new Vector3[columns * rows];
            var uv = new Vector2[vertices.Length];
            var flow = new Vector2[vertices.Length];
            var triangles = new int[(columns - 1) * (rows - 1) * 6];
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                float across = column / (float)(columns - 1);
                float distance = distances[row];
                float down = Mathf.Clamp01(distance / front.Drop);
                float sourceDepth = SampleSourceDepth(sourceVertices, sourceUv, source.bounds,
                    across, down, minV, maxV);
                // A thin sheet with the authored cross-section and folds,
                // tucked under the upper surface instead of a swollen tube.
                float offset = (sourceDepth - 0.15f) * depth;
                if (distance < 0f) offset = distance - depth * 0.15f;
                if (distance > front.Drop) offset += distance - front.Drop;
                var displacement = n * offset;
                float y = front.TopY - Mathf.Clamp(distance, 0f, front.Drop) - tuck;
                // Surface foam wings sit just above the water, then blend
                // into the curved curtain and spread out at the impact.
                if (distance < 0f || distance > front.Drop) y += tuck + cs * 0.004f;
                float flowY = distance < 0f ? distance / approach * 0.3f
                    : distance > front.Drop ? 1f + (distance - front.Drop) / runout * 0.3f : down;
                FitCorner(start, Mathf.Clamp01(1f - across * width / (cs * 0.2f)));
                FitCorner(end, Mathf.Clamp01(1f - (1f - across) * width / (cs * 0.2f)));
                int index = row * columns + column;
                vertices[index] = center + tangent * ((across - 0.5f) * width)
                    + displacement + Vector3.up * y;
                uv[index] = new Vector2(across * width / uvTile, -distance / uvTile);
                flow[index] = new Vector2(across, flowY);

                void FitCorner(WaterfallFieldPlanner.Front other, float weight)
                {
                    if (other == null || weight <= 0f) return;
                    float commonDrop = Mathf.Min(front.Drop, other.Drop);
                    float commonTuck = Mathf.Min(cs * 0.002f, commonDrop * 0.02f);
                    float lipDepth = Mathf.Min(cs * 0.04f, commonDrop * 0.1f);
                    float cornerOffset = Mathf.Lerp(-commonTuck,
                        Mathf.Min(cs * 0.08f, commonDrop * 0.22f),
                        Mathf.Clamp01(distance / lipDepth));
                    if (distance < 0f) cornerOffset = distance - commonTuck;
                    if (distance > front.Drop)
                    {
                        // Different lower pools cannot share a horizontal
                        // impact skirt; each spreads over its own surface.
                        if (Mathf.Abs(other.BottomY - front.BottomY) > 0.001f) return;
                        cornerOffset += distance - front.Drop;
                    }
                    var otherNormal = new Vector3(other.Dir.x, 0f, other.Dir.y).normalized;
                    displacement = Vector3.Lerp(displacement, (n + otherNormal) * cornerOffset, weight);
                    float cornerY = front.TopY - Mathf.Clamp(distance, 0f, front.Drop) - commonTuck;
                    if (distance < 0f || distance > front.Drop) cornerY += commonTuck + cs * 0.004f;
                    y = Mathf.Lerp(y, cornerY, weight);
                }
            }
            int triangle = 0;
            for (int row = 0; row < rows - 1; row++)
            for (int column = 0; column < columns - 1; column++)
            {
                int a = row * columns + column;
                triangles[triangle++] = a;
                triangles[triangle++] = a + 1;
                triangles[triangle++] = a + columns + 1;
                triangles[triangle++] = a;
                triangles[triangle++] = a + columns + 1;
                triangles[triangle++] = a + columns;
            }
            var mesh = new Mesh { name = $"waterfall_prefab_{front.Anchor.x}_{front.Anchor.y}_{front.Dir.x}_{front.Dir.y}",
                indexFormat = vertices.Length > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16,
                vertices = vertices, uv = uv, uv2 = flow, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;

            void AddJoinRows(WaterfallFieldPlanner.Front other)
            {
                if (other == null) return;
                float common = Mathf.Min(front.Drop, other.Drop);
                distances.Add(common);
                distances.Add(Mathf.Min(cs * 0.04f, common * 0.1f));
            }
        }

        private static float SampleSourceDepth(Vector3[] vertices, Vector2[] uv,
            Bounds bounds, float across, float down, float minV, float maxV)
        {
            float v = Mathf.Lerp(maxV, minV, down);
            float lower = minV, upper = maxV;
            for (int i = 0; i < uv.Length; i++)
            {
                if (uv[i].y <= v) lower = Mathf.Max(lower, uv[i].y);
                if (uv[i].y >= v) upper = Mathf.Min(upper, uv[i].y);
            }
            float z = Mathf.Lerp(SampleRow(lower), SampleRow(upper), Mathf.InverseLerp(lower, upper, v));
            return (z - bounds.min.z) / bounds.size.z;

            float SampleRow(float row)
            {
                float x = Mathf.Lerp(bounds.min.x, bounds.max.x, across);
                float left = float.MinValue, right = float.MaxValue;
                float leftZ = 0f, rightZ = 0f;
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (Mathf.Abs(uv[i].y - row) > 0.0001f) continue;
                    var vertex = vertices[i];
                    if (vertex.x <= x && vertex.x > left) { left = vertex.x; leftZ = vertex.z; }
                    if (vertex.x >= x && vertex.x < right) { right = vertex.x; rightZ = vertex.z; }
                }
                if (left == float.MinValue) return rightZ;
                if (right == float.MaxValue) return leftZ;
                return Mathf.Lerp(leftZ, rightZ, Mathf.InverseLerp(left, right, x));
            }
        }

        // Miter both curtains to the same displaced corner. The endpoint
        // search also handles concave joins between different upper cells.
        private WaterfallFieldPlanner.Front FindJoin(WaterfallFieldPlanner.Front front, Vector3 endpoint, float cs)
        {
            foreach (var other in _field.Fronts)
            {
                if (other == front || other.Dir.x * front.Dir.x + other.Dir.y * front.Dir.y != 0
                    || Mathf.Abs(other.TopY - front.TopY) > WaterfallFieldPlanner.FrontHeightTolerance)
                    continue;
                var n = new Vector3(other.Dir.x, 0f, other.Dir.y).normalized;
                var t = new Vector3(-n.z, 0f, n.x);
                var c = other.Center * cs;
                float halfWidth = other.WidthCells * cs * 0.5f;
                if ((endpoint - (c - t * halfWidth)).sqrMagnitude < cs * cs * 0.000001f
                    || (endpoint - (c + t * halfWidth)).sqrMagnitude < cs * cs * 0.000001f)
                    return other;
            }
            return null;
        }

        private Vector3 JoinDirection(WaterfallFieldPlanner.Front front, Vector3 endpoint, float cs)
        {
            var other = FindJoin(front, endpoint, cs);
            return other != null && Mathf.Abs(other.BottomY - front.BottomY) <= WaterfallFieldPlanner.FrontHeightTolerance
                ? new Vector3(other.Dir.x, 0f, other.Dir.y).normalized : Vector3.zero;
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
            float verticalScale = Mathf.Min(1f, drop / FullProfileDropMeters);
            var startJoin = JoinDirection(front, spanA, cs);
            var endJoin = JoinDirection(front, spanB, cs);

            // Profile points in ribbon space: u = along fall dir, y = world.
            var offsets = new float[rows];
            var heights = new float[rows];
            float prevY = float.MaxValue;
            for (int j = 0; j < rows; j++)
            {
                var p = Profile[j];
                float y = !float.IsNaN(p.Top) ? topY + p.Top * verticalScale
                    : !float.IsNaN(p.Bottom) ? bottomY + p.Bottom * verticalScale
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
            var flowUvs = new List<Vector2>(cols * rows * 2);
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

                    float v = -arc[j] / uvTile;
                    for (int c = 0; c < cols; c++)
                    {
                        float s = (float)c / (cols - 1);
                        var p = Vector3.LerpUnclamped(spanA, spanB, s);
                        p += (n + Vector3.Lerp(startJoin, endJoin, s)) * offsets[j];
                        p.y = heights[j];
                        verts.Add(p);
                        normals.Add(nrm);
                        uvs.Add(new Vector2(Vector3.Dot(p, t) / uvTile, v));
                        flowUvs.Add(new Vector2(s, Mathf.InverseLerp(topY, bottomY, p.y)));
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
                uv2 = flowUvs.ToArray(),
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
