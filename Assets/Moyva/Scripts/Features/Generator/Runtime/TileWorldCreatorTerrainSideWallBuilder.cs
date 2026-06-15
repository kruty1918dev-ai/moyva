using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [DisallowMultipleComponent]
    public sealed class TileWorldCreatorTerrainSideWallBuilder : MonoBehaviour
    {
        private const string LogTag = "[MoyvaTWCHeight:SideWalls]";
        private const string ArtifactLogTag = "[MoyvaTWCHeight:SideWallArtifact]";
        private const float DefaultCellSize = 1f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly List<Vector3> _vertices = new List<Vector3>(4096);
        private readonly List<int> _triangles = new List<int>(6144);
        private readonly List<Vector2> _uvs = new List<Vector2>(4096);
        private readonly List<string> _samples = new List<string>(16);
        private readonly List<string> _artifactSamples = new List<string>(24);

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Material _runtimeMaterial;
        private int[,] _lastTerrainLevelMap;
        private float _lastCellSize = DefaultCellSize;
        private int _lastHeightStep = 1;
        private float _lastBaseY;
        private bool _lastIncludeMapBoundaryWalls;

        public void Configure(
            Transform targetRoot,
            int[,] terrainLevelMap,
            float cellSize,
            int heightStep,
            float baseY,
            Material materialOverride,
            Color wallColor,
            bool includeMapBoundaryWalls)
        {
            Transform root = targetRoot != null ? targetRoot : transform.parent;
            if (root != null && transform.parent != root)
                transform.SetParent(root, false);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            EnsureComponents(materialOverride, wallColor);

            float resolvedCellSize = cellSize > 0.0001f ? cellSize : DefaultCellSize;
            int resolvedHeightStep = Mathf.Max(1, heightStep);
            _lastTerrainLevelMap = terrainLevelMap;
            _lastCellSize = resolvedCellSize;
            _lastHeightStep = resolvedHeightStep;
            _lastBaseY = baseY;
            _lastIncludeMapBoundaryWalls = includeMapBoundaryWalls;
            Debug.Log($"{LogTag} Configure root='{(root != null ? root.name : "<null>")}', map={FormatLevelStats(terrainLevelMap)}, cellSize={FormatNumber(resolvedCellSize)}, heightStep={resolvedHeightStep}, baseY={FormatNumber(baseY)}, includeMapBoundaryWalls={includeMapBoundaryWalls}, material='{(_meshRenderer.sharedMaterial != null ? _meshRenderer.sharedMaterial.name : "<null>")}', color={FormatColor(wallColor)}.");
            Debug.Log($"{ArtifactLogTag} Configure artifact diagnostics. builder='{name}', root='{(root != null ? root.name : "<null>")}', rootTransform={FormatTransform(root)}, builderTransform={FormatTransform(transform)}, map={FormatLevelStats(terrainLevelMap)}, cellSize={FormatNumber(resolvedCellSize)}, heightStep={resolvedHeightStep}, baseY={FormatNumber(baseY)}, includeMapBoundaryWalls={includeMapBoundaryWalls}, material='{(_meshRenderer.sharedMaterial != null ? _meshRenderer.sharedMaterial.name : "<null>")}', materialCull={FormatMaterialCull(_meshRenderer.sharedMaterial)}.");

            RebuildMesh(terrainLevelMap, resolvedCellSize, resolvedHeightStep, baseY, includeMapBoundaryWalls);
        }

        public void RebuildFromLastConfiguration(string reason)
        {
            Debug.Log($"{LogTag} Delayed rebuild requested. reason='{reason}', map={FormatLevelStats(_lastTerrainLevelMap)}, cellSize={FormatNumber(_lastCellSize)}, heightStep={_lastHeightStep}, baseY={FormatNumber(_lastBaseY)}, includeMapBoundaryWalls={_lastIncludeMapBoundaryWalls}.");
            Debug.Log($"{ArtifactLogTag} Delayed artifact-diagnostics rebuild. reason='{reason}', builder='{name}', root='{(transform.parent != null ? transform.parent.name : "<null>")}', builderTransform={FormatTransform(transform)}, map={FormatLevelStats(_lastTerrainLevelMap)}, cellSize={FormatNumber(_lastCellSize)}, heightStep={_lastHeightStep}, baseY={FormatNumber(_lastBaseY)}, includeMapBoundaryWalls={_lastIncludeMapBoundaryWalls}.");
            RebuildMesh(_lastTerrainLevelMap, _lastCellSize, _lastHeightStep, _lastBaseY, _lastIncludeMapBoundaryWalls);
        }

        public void ClearWalls(string reason)
        {
            if (_mesh != null)
                _mesh.Clear();

            Debug.Log($"{LogTag} Cleared generated side walls. reason='{reason}'.");
        }

        private void EnsureComponents(Material materialOverride, Color wallColor)
        {
            if (_meshFilter == null && !TryGetComponent(out _meshFilter))
                _meshFilter = gameObject.AddComponent<MeshFilter>();

            if (_meshRenderer == null && !TryGetComponent(out _meshRenderer))
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (_mesh == null)
            {
                _mesh = new Mesh
                {
                    name = "Moyva TWC Terrain Side Walls",
                    hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
                _meshFilter.sharedMesh = _mesh;
            }

            _meshRenderer.sharedMaterial = ResolveMaterial(materialOverride, wallColor);
            _meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            _meshRenderer.receiveShadows = true;
        }

        private Material ResolveMaterial(Material materialOverride, Color wallColor)
        {
            if (materialOverride != null)
                return materialOverride;

            if (_runtimeMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Unlit/Color")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Hidden/InternalErrorShader");

                if (shader == null)
                {
                    Debug.LogWarning($"{LogTag} Could not find a shader for generated side-wall material.");
                    return null;
                }

                _runtimeMaterial = new Material(shader)
                {
                    name = "Moyva TWC Terrain Side Wall Runtime Material",
                    hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
            }

            ApplyMaterialColor(_runtimeMaterial, wallColor);
            if (_runtimeMaterial.HasProperty("_Cull"))
            {
                _runtimeMaterial.SetFloat("_Cull", (float)CullMode.Back);
                Debug.Log($"{ArtifactLogTag} Runtime side-wall material uses back-face culling for one-sided wall quads. material='{_runtimeMaterial.name}', materialCull={FormatMaterialCull(_runtimeMaterial)}.");
            }

            return _runtimeMaterial;
        }

        private static void ApplyMaterialColor(Material material, Color color)
        {
            if (material == null)
                return;

            if (color.a <= 0.001f)
                color.a = 1f;

            if (material.HasProperty(BaseColorId))
                material.SetColor(BaseColorId, color);
            if (material.HasProperty(ColorId))
                material.SetColor(ColorId, color);
        }

        private void RebuildMesh(int[,] terrainLevelMap, float cellSize, int heightStep, float baseY, bool includeMapBoundaryWalls)
        {
            if (_mesh == null)
                return;

            _vertices.Clear();
            _triangles.Clear();
            _uvs.Clear();
            _samples.Clear();
            _artifactSamples.Clear();
            _mesh.Clear();

            if (terrainLevelMap == null)
            {
                Debug.LogWarning($"{LogTag} Rebuild skipped: TerrainLevelMap is null.");
                return;
            }

            int width = terrainLevelMap.GetLength(0);
            int height = terrainLevelMap.GetLength(1);
            if (width <= 0 || height <= 0)
            {
                Debug.LogWarning($"{LogTag} Rebuild skipped: TerrainLevelMap size is {width}x{height}.");
                return;
            }

            FindLevelRange(terrainLevelMap, width, height, out int minLevel, out int maxLevel);
            int edgeLevel = Mathf.Min(0, minLevel);
            int eastWalls = 0;
            int westWalls = 0;
            int northWalls = 0;
            int southWalls = 0;
            int skippedBoundaryWalls = 0;
            int totalLevelDifference = 0;
            int maxLevelDifference = 0;
            var differenceHistogram = new SortedDictionary<int, int>();
            var diagnostics = new SideWallArtifactDiagnostics(cellSize, heightStep);

            for (int cellX = 0; cellX < width; cellX++)
            {
                for (int cellY = 0; cellY < height; cellY++)
                {
                    int currentLevel = terrainLevelMap[cellX, cellY];
                    float minX = (cellX * cellSize) - (cellSize * 0.5f);
                    float maxX = minX + cellSize;
                    float minZ = (cellY * cellSize) - (cellSize * 0.5f);
                    float maxZ = minZ + cellSize;

                    TryAppendWall(
                        terrainLevelMap,
                        width,
                        height,
                        cellX,
                        cellY,
                        currentLevel,
                        cellX + 1,
                        cellY,
                        edgeLevel,
                        "East",
                        includeMapBoundaryWalls,
                        new Vector3(maxX, 0f, minZ),
                        new Vector3(maxX, 0f, maxZ),
                        baseY,
                        heightStep,
                        ref eastWalls,
                        ref skippedBoundaryWalls,
                        ref totalLevelDifference,
                        ref maxLevelDifference,
                        differenceHistogram,
                        ref diagnostics);

                    TryAppendWall(
                        terrainLevelMap,
                        width,
                        height,
                        cellX,
                        cellY,
                        currentLevel,
                        cellX - 1,
                        cellY,
                        edgeLevel,
                        "West",
                        includeMapBoundaryWalls,
                        new Vector3(minX, 0f, maxZ),
                        new Vector3(minX, 0f, minZ),
                        baseY,
                        heightStep,
                        ref westWalls,
                        ref skippedBoundaryWalls,
                        ref totalLevelDifference,
                        ref maxLevelDifference,
                        differenceHistogram,
                        ref diagnostics);

                    TryAppendWall(
                        terrainLevelMap,
                        width,
                        height,
                        cellX,
                        cellY,
                        currentLevel,
                        cellX,
                        cellY + 1,
                        edgeLevel,
                        "North",
                        includeMapBoundaryWalls,
                        new Vector3(maxX, 0f, maxZ),
                        new Vector3(minX, 0f, maxZ),
                        baseY,
                        heightStep,
                        ref northWalls,
                        ref skippedBoundaryWalls,
                        ref totalLevelDifference,
                        ref maxLevelDifference,
                        differenceHistogram,
                        ref diagnostics);

                    TryAppendWall(
                        terrainLevelMap,
                        width,
                        height,
                        cellX,
                        cellY,
                        currentLevel,
                        cellX,
                        cellY - 1,
                        edgeLevel,
                        "South",
                        includeMapBoundaryWalls,
                        new Vector3(minX, 0f, minZ),
                        new Vector3(maxX, 0f, minZ),
                        baseY,
                        heightStep,
                        ref southWalls,
                        ref skippedBoundaryWalls,
                        ref totalLevelDifference,
                        ref maxLevelDifference,
                        differenceHistogram,
                        ref diagnostics);
                }
            }

            int wallCount = eastWalls + westWalls + northWalls + southWalls;
            if (wallCount == 0)
            {
                Debug.LogWarning($"{LogTag} Rebuild produced no side walls. map={width}x{height}, levelRange={minLevel}..{maxLevel}, edgeLevel={edgeLevel}, cellSize={cellSize}, heightStep={heightStep}, includeMapBoundaryWalls={includeMapBoundaryWalls}, skippedBoundaryWalls={skippedBoundaryWalls}.");
                Debug.LogWarning($"{ArtifactLogTag} Rebuild produced no side-wall artifact mesh. map={width}x{height}, expectedLocalBounds={FormatExpectedGridBounds(width, height, cellSize, baseY, minLevel, maxLevel, heightStep)}, includeMapBoundaryWalls={includeMapBoundaryWalls}, diagnostics={diagnostics.Format()}, samples={FormatSamples(_artifactSamples)}.");
                return;
            }

            _mesh.indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            _mesh.SetVertices(_vertices);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(0, _uvs);
            _mesh.RecalculateNormals();
            _mesh.RecalculateTangents();  // Critical for water shader with normal maps
            _mesh.RecalculateBounds();

            Debug.Log($"{LogTag} Rebuild complete. map={width}x{height}, levelRange={minLevel}..{maxLevel}, edgeLevel={edgeLevel}, cellSize={FormatNumber(cellSize)}, heightStep={heightStep}, baseY={FormatNumber(baseY)}, includeMapBoundaryWalls={includeMapBoundaryWalls}, skippedBoundaryWalls={skippedBoundaryWalls}, walls={wallCount} (E={eastWalls}, W={westWalls}, N={northWalls}, S={southWalls}), totalLevelDiff={totalLevelDifference}, maxLevelDiff={maxLevelDifference}, diffHistogram={FormatHistogram(differenceHistogram)}, vertices={_vertices.Count}, triangles={_triangles.Count / 3}, indexFormat={_mesh.indexFormat}, bounds={FormatBounds(_mesh.bounds)}, samples={FormatSamples(_samples)}.");
            Debug.Log($"{ArtifactLogTag} Rebuild diagnostics. map={width}x{height}, expectedLocalBounds={FormatExpectedGridBounds(width, height, cellSize, baseY, minLevel, maxLevel, heightStep)}, meshLocalBounds={FormatBounds(_mesh.bounds)}, rendererWorldBounds={(_meshRenderer != null ? FormatBounds(_meshRenderer.bounds) : "<no renderer>")}, builderTransform={FormatTransform(transform)}, includeMapBoundaryWalls={includeMapBoundaryWalls}, walls={wallCount}, vertices={_vertices.Count}, triangles={_triangles.Count / 3}, diagnostics={diagnostics.Format()}, samples={FormatSamples(_artifactSamples)}.");
        }

        private void TryAppendWall(
            int[,] terrainLevelMap,
            int width,
            int height,
            int cellX,
            int cellY,
            int currentLevel,
            int neighbourX,
            int neighbourY,
            int edgeLevel,
            string direction,
            bool includeMapBoundaryWalls,
            Vector3 edgeStart,
            Vector3 edgeEnd,
            float baseY,
            int heightStep,
            ref int directionWallCount,
            ref int skippedBoundaryWalls,
            ref int totalLevelDifference,
            ref int maxLevelDifference,
            SortedDictionary<int, int> differenceHistogram,
            ref SideWallArtifactDiagnostics diagnostics)
        {
            bool hasNeighbour = IsInside(neighbourX, neighbourY, width, height);
            if (!hasNeighbour)
            {
                diagnostics.BoundaryEdgeChecks++;
                if (currentLevel > edgeLevel)
                    diagnostics.BoundaryWallCandidates++;
            }

            if (!hasNeighbour && !includeMapBoundaryWalls)
            {
                if (currentLevel > edgeLevel)
                {
                    skippedBoundaryWalls++;
                    diagnostics.SkippedBoundaryWallCandidates++;
                    AddArtifactSample($"SKIP boundary cell=({cellX},{cellY}) dir={direction} outside=({neighbourX},{neighbourY}) levels={edgeLevel}->{currentLevel} edge=({FormatVector3(edgeStart)})->({FormatVector3(edgeEnd)})");
                }
                return;
            }

            int neighbourLevel = hasNeighbour ? terrainLevelMap[neighbourX, neighbourY] : edgeLevel;

            if (currentLevel <= neighbourLevel)
                return;

            int levelDifference = currentLevel - neighbourLevel;
            float bottomY = baseY + (neighbourLevel * heightStep);
            float topY = baseY + (currentLevel * heightStep);
            float wallLength = Vector3.Distance(edgeStart, edgeEnd);
            float wallHeight = topY - bottomY;
            AppendQuad(
                new Vector3(edgeStart.x, bottomY, edgeStart.z),
                new Vector3(edgeStart.x, topY, edgeStart.z),
                new Vector3(edgeEnd.x, topY, edgeEnd.z),
                new Vector3(edgeEnd.x, bottomY, edgeEnd.z),
                cellSize: wallLength,
                wallHeight: wallHeight);

            if (hasNeighbour)
                diagnostics.GeneratedInteriorWalls++;
            else
                diagnostics.GeneratedBoundaryWalls++;

            diagnostics.ObserveGeneratedWall(
                cellX,
                cellY,
                neighbourX,
                neighbourY,
                direction,
                currentLevel,
                neighbourLevel,
                hasNeighbour,
                edgeStart,
                edgeEnd,
                wallLength,
                wallHeight);

            if (!hasNeighbour || diagnostics.IsSuspicious(wallLength, wallHeight))
            {
                AddArtifactSample($"GEN {(hasNeighbour ? "interior" : "boundary")} cell=({cellX},{cellY}) dir={direction} neighbour=({neighbourX},{neighbourY}) levels={neighbourLevel}->{currentLevel} length={FormatNumber(wallLength)} height={FormatNumber(wallHeight)} edge=({FormatVector3(edgeStart)})->({FormatVector3(edgeEnd)})");
            }

            directionWallCount++;
            totalLevelDifference += levelDifference;
            maxLevelDifference = Mathf.Max(maxLevelDifference, levelDifference);
            differenceHistogram.TryGetValue(levelDifference, out int histogramCount);
            differenceHistogram[levelDifference] = histogramCount + 1;

            if (_samples.Count < 16)
            {
                _samples.Add($"cell=({cellX},{cellY}) dir={direction} levels={neighbourLevel}->{currentLevel} y={FormatNumber(bottomY)}->{FormatNumber(topY)} boundary={!hasNeighbour}");
            }
        }

        private void AddArtifactSample(string sample)
        {
            if (_artifactSamples.Count < 24)
                _artifactSamples.Add(sample);
        }

        private void AppendQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, float cellSize, float wallHeight)
        {
            int startIndex = _vertices.Count;
            _vertices.Add(bottomLeft);
            _vertices.Add(topLeft);
            _vertices.Add(topRight);
            _vertices.Add(bottomRight);

            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 1);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex);
            _triangles.Add(startIndex + 2);
            _triangles.Add(startIndex + 3);

            _uvs.Add(new Vector2(0f, 0f));
            _uvs.Add(new Vector2(0f, wallHeight));
            _uvs.Add(new Vector2(cellSize, wallHeight));
            _uvs.Add(new Vector2(cellSize, 0f));
        }

        private void OnDestroy()
        {
            DestroyRuntimeObject(_mesh);
            DestroyRuntimeObject(_runtimeMaterial);
        }

        private static void FindLevelRange(int[,] terrainLevelMap, int width, int height, out int minLevel, out int maxLevel)
        {
            minLevel = int.MaxValue;
            maxLevel = int.MinValue;
            for (int cellX = 0; cellX < width; cellX++)
            {
                for (int cellY = 0; cellY < height; cellY++)
                {
                    int level = terrainLevelMap[cellX, cellY];
                    minLevel = Mathf.Min(minLevel, level);
                    maxLevel = Mathf.Max(maxLevel, level);
                }
            }
        }

        private static bool IsInside(int cellX, int cellY, int width, int height)
            => cellX >= 0 && cellY >= 0 && cellX < width && cellY < height;

        private static string FormatLevelStats(int[,] levelMap)
        {
            if (levelMap == null)
                return "null";

            int width = levelMap.GetLength(0);
            int height = levelMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            FindLevelRange(levelMap, width, height, out int minLevel, out int maxLevel);
            return $"{width}x{height}, min={minLevel}, max={maxLevel}, samples=(0,0:{levelMap[0, 0]}), (mid:{levelMap[width / 2, height / 2]}), (last:{levelMap[width - 1, height - 1]})";
        }

        private static string FormatHistogram(SortedDictionary<int, int> histogram)
        {
            if (histogram == null || histogram.Count == 0)
                return "{}";

            var builder = new StringBuilder();
            builder.Append('{');
            int entryIndex = 0;
            foreach (var pair in histogram)
            {
                if (entryIndex > 0)
                    builder.Append(", ");
                builder.Append(pair.Key).Append(':').Append(pair.Value);
                entryIndex++;
            }
            builder.Append('}');
            return builder.ToString();
        }

        private static string FormatSamples(List<string> samples)
        {
            if (samples == null || samples.Count == 0)
                return "[]";

            var builder = new StringBuilder();
            builder.Append('[');
            for (int sampleIndex = 0; sampleIndex < samples.Count; sampleIndex++)
            {
                if (sampleIndex > 0)
                    builder.Append(" | ");
                builder.Append(samples[sampleIndex]);
            }
            builder.Append(']');
            return builder.ToString();
        }

        private static string FormatBounds(Bounds bounds)
            => $"center=(x={FormatNumber(bounds.center.x)}, y={FormatNumber(bounds.center.y)}, z={FormatNumber(bounds.center.z)}), size=(x={FormatNumber(bounds.size.x)}, y={FormatNumber(bounds.size.y)}, z={FormatNumber(bounds.size.z)})";

        private static string FormatExpectedGridBounds(int width, int height, float cellSize, float baseY, int minLevel, int maxLevel, int heightStep)
        {
            var bounds = new Bounds();
            float minX = -cellSize * 0.5f;
            float maxX = ((width - 1) * cellSize) + (cellSize * 0.5f);
            float minZ = -cellSize * 0.5f;
            float maxZ = ((height - 1) * cellSize) + (cellSize * 0.5f);
            float minY = baseY + (minLevel * heightStep);
            float maxY = baseY + (maxLevel * heightStep);
            bounds.SetMinMax(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
            return FormatBounds(bounds);
        }

        private static string FormatTransform(Transform target)
        {
            if (target == null)
                return "<null>";

            return $"localPos=({FormatVector3(target.localPosition)}), worldPos=({FormatVector3(target.position)}), localScale=({FormatVector3(target.localScale)}), lossyScale=({FormatVector3(target.lossyScale)})";
        }

        private static string FormatMaterialCull(Material material)
        {
            if (material == null)
                return "<null>";

            return material.HasProperty("_Cull") ? FormatNumber(material.GetFloat("_Cull")) : "<no _Cull>";
        }

        private static string FormatVector3(Vector3 value)
            => $"x={FormatNumber(value.x)}, y={FormatNumber(value.y)}, z={FormatNumber(value.z)}";

        private static string FormatColor(Color color)
            => $"r={FormatNumber(color.r)}, g={FormatNumber(color.g)}, b={FormatNumber(color.b)}, a={FormatNumber(color.a)}";

        private static string FormatNumber(float value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static void DestroyRuntimeObject(Object instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                Destroy(instance);
            else
                DestroyImmediate(instance);
        }

        private struct SideWallArtifactDiagnostics
        {
            private readonly float _expectedWallLength;
            private readonly int _heightStep;
            private float _largestArea;
            private string _largestAreaSample;

            public SideWallArtifactDiagnostics(float expectedWallLength, int heightStep)
            {
                _expectedWallLength = Mathf.Max(0.0001f, expectedWallLength);
                _heightStep = Mathf.Max(1, heightStep);
                _largestArea = 0f;
                _largestAreaSample = "<none>";
                BoundaryEdgeChecks = 0;
                BoundaryWallCandidates = 0;
                SkippedBoundaryWallCandidates = 0;
                GeneratedBoundaryWalls = 0;
                GeneratedInteriorWalls = 0;
                SuspiciousLengthWalls = 0;
                TallWalls = 0;
            }

            public int BoundaryEdgeChecks { get; set; }
            public int BoundaryWallCandidates { get; set; }
            public int SkippedBoundaryWallCandidates { get; set; }
            public int GeneratedBoundaryWalls { get; set; }
            public int GeneratedInteriorWalls { get; set; }
            public int SuspiciousLengthWalls { get; set; }
            public int TallWalls { get; set; }

            public bool IsSuspicious(float wallLength, float wallHeight)
                => wallLength > _expectedWallLength * 1.05f || wallHeight > _heightStep * 3.05f;

            public void ObserveGeneratedWall(
                int cellX,
                int cellY,
                int neighbourX,
                int neighbourY,
                string direction,
                int currentLevel,
                int neighbourLevel,
                bool hasNeighbour,
                Vector3 edgeStart,
                Vector3 edgeEnd,
                float wallLength,
                float wallHeight)
            {
                if (wallLength > _expectedWallLength * 1.05f)
                    SuspiciousLengthWalls++;
                if (wallHeight > _heightStep * 3.05f)
                    TallWalls++;

                float area = wallLength * wallHeight;
                if (area <= _largestArea)
                    return;

                _largestArea = area;
                _largestAreaSample = $"cell=({cellX},{cellY}) dir={direction} neighbour=({neighbourX},{neighbourY}) hasNeighbour={hasNeighbour} levels={neighbourLevel}->{currentLevel} length={FormatNumber(wallLength)} height={FormatNumber(wallHeight)} area={FormatNumber(area)} edge=({FormatVector3(edgeStart)})->({FormatVector3(edgeEnd)})";
            }

            public string Format()
                => $"boundaryEdgeChecks={BoundaryEdgeChecks}, boundaryWallCandidates={BoundaryWallCandidates}, skippedBoundaryWallCandidates={SkippedBoundaryWallCandidates}, generatedBoundaryWalls={GeneratedBoundaryWalls}, generatedInteriorWalls={GeneratedInteriorWalls}, suspiciousLengthWalls={SuspiciousLengthWalls}, tallWalls={TallWalls}, largestWall={_largestAreaSample}";
        }
    }
}