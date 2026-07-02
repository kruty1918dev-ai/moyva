using System.Collections;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator.Components;
using GiantGrey.TileWorldCreator.Utilities;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Поточний TileWorldCreator API не підтримує per-cell Y-зсув.
    /// Цей компонент пост-обробляє ієрархію тайлів TWC: проходить по нащадках
    /// <see cref="Transform"/> і за <see cref="TerrainLevelMap"/> зсуває кожен тайл
    /// по Y на цілу кількість одиниць (height step).
    /// TWC спавнить тайли через корутини після GenerateCompleteMap, тому компонент
    /// працює декілька секунд після старту, поки всі тайли не зʼявляться.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TileWorldCreatorHeightProjector : MonoBehaviour
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const float DefaultCellSize = 1f;

        private Transform _targetRoot;
        private int[,] _terrainLevelMap;
        private float _cellSize = DefaultCellSize;
        private int _heightStep = 1;
        private float _trackingSecondsRemaining;
        private readonly Dictionary<int, float> _appliedYOffsetByTransformId = new Dictionary<int, float>();
        private readonly List<TileTransformSample> _scratchBuffer = new List<TileTransformSample>(256);
        private readonly HashSet<int> _collectedTransformIds = new HashSet<int>();
        private readonly HashSet<Vector2Int> _usedCells = new HashSet<Vector2Int>();
        private readonly List<string> _sampleApplications = new List<string>(12);
        private int _applyPassIndex;
        private int _stableFullCoveragePasses;
        private int _lastLoggedRendererCount = -1;
        private int _lastLoggedTileTransformCount = -1;
        private int _lastLoggedUsedCellCount = -1;
        private int _lastLoggedSkippedSideWallRendererCount = -1;
        private bool _sideWallsRefreshedAfterStable;
        private bool _meshOptimizationRequestedAfterStable;
        private float _worldGenDiagStartTime;
        private bool _worldGenDiagEndLogged;

        public void Configure(Transform targetRoot, int[,] terrainLevelMap, float cellSize, int heightStep, float trackingSeconds)
        {
            _targetRoot = targetRoot != null ? targetRoot : transform;
            _terrainLevelMap = terrainLevelMap;
            _cellSize = cellSize > 0.0001f ? cellSize : DefaultCellSize;
            _heightStep = Mathf.Max(1, heightStep);
            _trackingSecondsRemaining = Mathf.Max(0f, trackingSeconds);
            _applyPassIndex = 0;
            _stableFullCoveragePasses = 0;
            _lastLoggedRendererCount = -1;
            _lastLoggedTileTransformCount = -1;
            _lastLoggedUsedCellCount = -1;
            _lastLoggedSkippedSideWallRendererCount = -1;
            _sideWallsRefreshedAfterStable = false;
            _meshOptimizationRequestedAfterStable = false;
            _worldGenDiagStartTime = Time.realtimeSinceStartup;
            _worldGenDiagEndLogged = false;
            _appliedYOffsetByTransformId.Clear();
            Debug.Log($"{LogTag} Projector.Configure target='{_targetRoot.name}', levelMap={FormatLevelStats(_terrainLevelMap)}, cellSize={_cellSize}, heightStep={_heightStep}, trackingSeconds={_trackingSecondsRemaining}, existingRenderers={_targetRoot.GetComponentsInChildren<Renderer>(true).Length}, existingMeshFilters={_targetRoot.GetComponentsInChildren<MeshFilter>(true).Length}.");
            Debug.Log($"{WorldGenDiagTag} HeightProjector.START frame={Time.frameCount}, tileCount={CountLevelMapCells(_terrainLevelMap)}");
            ApplyHeightsOnce();
        }

        private void LateUpdate()
        {
            if (_terrainLevelMap == null)
                return;

            if (_trackingSecondsRemaining <= 0f)
                return;

            _trackingSecondsRemaining -= Time.unscaledDeltaTime;
            ApplyHeightsOnce();

            if (_trackingSecondsRemaining <= 0f && !_worldGenDiagEndLogged)
            {
                _worldGenDiagEndLogged = true;
                Debug.Log(
                    $"{WorldGenDiagTag} HeightProjector.END frame={Time.frameCount}, tileCount={CountLevelMapCells(_terrainLevelMap)}, " +
                    $"elapsedMs={(Time.realtimeSinceStartup - _worldGenDiagStartTime) * 1000f:0}");
            }
        }

        private void ApplyHeightsOnce()
        {
            _applyPassIndex++;
            if (_terrainLevelMap == null)
            {
                LogPass($"pass={_applyPassIndex} skipped: TerrainLevelMap is null.");
                return;
            }

            int width = _terrainLevelMap.GetLength(0);
            int height = _terrainLevelMap.GetLength(1);
            if (width <= 0 || height <= 0)
            {
                LogPass($"pass={_applyPassIndex} skipped: TerrainLevelMap size is {width}x{height}.");
                return;
            }

            Transform root = _targetRoot != null ? _targetRoot : transform;
            int rendererCount = CollectTileTransforms(root, _scratchBuffer, _collectedTransformIds, out int skippedSideWallRenderers);
            if (_scratchBuffer.Count == 0)
            {
                if (ShouldLogNoTilePass(rendererCount, skippedSideWallRenderers))
                {
                    LogPass($"pass={_applyPassIndex} found no tile transforms yet. rendererCount={rendererCount}, skippedSideWallRenderers={skippedSideWallRenderers}, root='{root.name}', childTransforms={root.GetComponentsInChildren<Transform>(true).Length - 1}, trackingLeft={_trackingSecondsRemaining:0.###}s.");
                    RememberLoggedState(rendererCount, 0, 0, skippedSideWallRenderers);
                }

                _collectedTransformIds.Clear();
                return;
            }

            float minLocalX = float.PositiveInfinity;
            float minLocalZ = float.PositiveInfinity;
            float maxLocalX = float.NegativeInfinity;
            float maxLocalZ = float.NegativeInfinity;
            for (int i = 0; i < _scratchBuffer.Count; i++)
            {
                Vector3 localCenter = root.InverseTransformPoint(_scratchBuffer[i].WorldCenter);
                minLocalX = Mathf.Min(minLocalX, localCenter.x);
                minLocalZ = Mathf.Min(minLocalZ, localCenter.z);
                maxLocalX = Mathf.Max(maxLocalX, localCenter.x);
                maxLocalZ = Mathf.Max(maxLocalZ, localCenter.z);
            }

            int changedCount = 0;
            int unchangedCount = 0;
            int clampedCount = 0;
            int minLevel = int.MaxValue;
            int maxLevel = int.MinValue;
            _usedCells.Clear();
            _sampleApplications.Clear();

            for (int i = 0; i < _scratchBuffer.Count; i++)
            {
                var sample = _scratchBuffer[i];
                var tile = sample.Transform;
                int id = tile.GetInstanceID();

                Vector3 local = root.InverseTransformPoint(sample.WorldCenter);
                int rawCellX = Mathf.FloorToInt((local.x - minLocalX + 0.001f) / _cellSize);
                int rawCellY = Mathf.FloorToInt((local.z - minLocalZ + 0.001f) / _cellSize);
                int cellX = rawCellX;
                int cellY = rawCellY;
                cellX = Mathf.Clamp(cellX, 0, width - 1);
                cellY = Mathf.Clamp(cellY, 0, height - 1);
                if (cellX != rawCellX || cellY != rawCellY)
                    clampedCount++;

                int level = _terrainLevelMap[cellX, cellY];
                minLevel = Mathf.Min(minLevel, level);
                maxLevel = Mathf.Max(maxLevel, level);
                _usedCells.Add(new Vector2Int(cellX, cellY));
                float previousOffset = 0f;
                _appliedYOffsetByTransformId.TryGetValue(id, out previousOffset);
                float nextOffset = level * _heightStep;
                if (Mathf.Approximately(previousOffset, nextOffset))
                {
                    unchangedCount++;
                    continue;
                }

                Vector3 pos = tile.position;
                pos.y += nextOffset - previousOffset;
                tile.position = pos;
                _appliedYOffsetByTransformId[id] = nextOffset;
                changedCount++;

                if (_sampleApplications.Count < 12)
                {
                    _sampleApplications.Add($"'{tile.name}' local=({local.x:0.##},{local.z:0.##}) rawCell=({rawCellX},{rawCellY}) cell=({cellX},{cellY}) level={level} offset {previousOffset:0.##}->{nextOffset:0.##}");
                }
            }

            int tileTransformCount = _scratchBuffer.Count;
            int usedCellCount = _usedCells.Count;
            bool fullCoverage = usedCellCount >= width * height;
            bool stableFullCoverage = fullCoverage
                && changedCount == 0
                && rendererCount == _lastLoggedRendererCount
                && tileTransformCount == _lastLoggedTileTransformCount
                && skippedSideWallRenderers == _lastLoggedSkippedSideWallRendererCount;

            _stableFullCoveragePasses = stableFullCoverage ? _stableFullCoveragePasses + 1 : 0;
            bool shouldStopTracking = _stableFullCoveragePasses >= 3;
            bool shouldLog = ShouldLogAppliedPass(
                rendererCount,
                tileTransformCount,
                usedCellCount,
                skippedSideWallRenderers,
                changedCount,
                shouldStopTracking);

            if (shouldLog)
            {
                LogPass($"pass={_applyPassIndex} applied. root='{root.name}', renderers={rendererCount}, skippedSideWallRenderers={skippedSideWallRenderers}, tileTransforms={tileTransformCount}, usedCells={usedCellCount}/{width * height}, changed={changedCount}, unchanged={unchangedCount}, clamped={clampedCount}, levelRange={minLevel}..{maxLevel}, localBoundsX={minLocalX:0.###}..{maxLocalX:0.###}, localBoundsZ={minLocalZ:0.###}..{maxLocalZ:0.###}, cellSize={_cellSize}, heightStep={_heightStep}, trackingLeft={_trackingSecondsRemaining:0.###}s, stableFullCoveragePasses={_stableFullCoveragePasses}, samples={FormatSamples(_sampleApplications)}.");
                RememberLoggedState(rendererCount, tileTransformCount, usedCellCount, skippedSideWallRenderers);
            }

            if (shouldStopTracking)
            {
                RefreshSideWallsAfterStable(root, rendererCount, tileTransformCount, usedCellCount, width * height);
                OptimizeTerrainMeshesAfterStable(root, rendererCount, tileTransformCount, usedCellCount, width * height);
                _trackingSecondsRemaining = 0f;
                LogPass($"pass={_applyPassIndex} tracking stopped: stable full coverage reached. root='{root.name}', renderers={rendererCount}, tileTransforms={tileTransformCount}, usedCells={usedCellCount}/{width * height}, skippedSideWallRenderers={skippedSideWallRenderers}.");
            }

            if (rendererCount > 0 && _scratchBuffer.Count <= rendererCount / 4)
            {
                LogPass($"pass={_applyPassIndex} warning: tileTransforms ({_scratchBuffer.Count}) are far fewer than renderers ({rendererCount}). If this stays low, TWC may still be exposing merged cluster meshes instead of individual tile roots.");
            }

            _scratchBuffer.Clear();
            _collectedTransformIds.Clear();
            _usedCells.Clear();
            _sampleApplications.Clear();
        }

        private bool ShouldLogNoTilePass(int rendererCount, int skippedSideWallRenderers)
        {
            return _applyPassIndex <= 3
                || rendererCount != _lastLoggedRendererCount
                || skippedSideWallRenderers != _lastLoggedSkippedSideWallRendererCount
                || _applyPassIndex % 120 == 0;
        }

        private bool ShouldLogAppliedPass(
            int rendererCount,
            int tileTransformCount,
            int usedCellCount,
            int skippedSideWallRenderers,
            int changedCount,
            bool shouldStopTracking)
        {
            return shouldStopTracking
                || changedCount > 0
                || rendererCount != _lastLoggedRendererCount
                || tileTransformCount != _lastLoggedTileTransformCount
                || usedCellCount != _lastLoggedUsedCellCount
                || skippedSideWallRenderers != _lastLoggedSkippedSideWallRendererCount
                || _applyPassIndex <= 3
                || _applyPassIndex % 120 == 0;
        }

        private void RememberLoggedState(int rendererCount, int tileTransformCount, int usedCellCount, int skippedSideWallRenderers)
        {
            _lastLoggedRendererCount = rendererCount;
            _lastLoggedTileTransformCount = tileTransformCount;
            _lastLoggedUsedCellCount = usedCellCount;
            _lastLoggedSkippedSideWallRendererCount = skippedSideWallRenderers;
        }

        private void RefreshSideWallsAfterStable(Transform root, int rendererCount, int tileTransformCount, int usedCellCount, int totalCellCount)
        {
            if (_sideWallsRefreshedAfterStable || root == null)
                return;

            var sideWallBuilder = root.GetComponentInChildren<TileWorldCreatorTerrainSideWallBuilder>(true);
            if (sideWallBuilder == null)
                return;

            _sideWallsRefreshedAfterStable = true;
            sideWallBuilder.RebuildFromLastConfiguration($"height projector stable pass={_applyPassIndex}, renderers={rendererCount}, tileTransforms={tileTransformCount}, usedCells={usedCellCount}/{totalCellCount}");
        }

        private void OptimizeTerrainMeshesAfterStable(Transform root, int rendererCount, int tileTransformCount, int usedCellCount, int totalCellCount)
        {
            if (_meshOptimizationRequestedAfterStable || root == null)
                return;

            var optimizer = root.GetComponentInChildren<TileWorldCreatorRuntimeMeshOptimizer>(true);
            if (optimizer == null)
                return;

            _meshOptimizationRequestedAfterStable = true;
            optimizer.RequestOptimizeAfterStable($"height projector stable pass={_applyPassIndex}, renderers={rendererCount}, tileTransforms={tileTransformCount}, usedCells={usedCellCount}/{totalCellCount}");
        }

        private static int CollectTileTransforms(Transform root, List<TileTransformSample> buffer, HashSet<int> collectedIds, out int skippedSideWallRenderers)
        {
            skippedSideWallRenderers = 0;
            // Ієрархія TWC: root(manager) → layerGO → cluster → tileRoot → (optional inner meshes).
            // Знаходимо tileRoot як предка рендера, у якого parent — це cluster,
            // тобто t.parent.parent.parent == root.
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null)
                    continue;

                if (r.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() != null)
                {
                    skippedSideWallRenderers++;
                    continue;
                }

                Transform t = r.transform;
                while (t.parent != null
                    && t.parent.parent != null
                    && t.parent.parent.parent != null
                    && t.parent.parent.parent != root)
                {
                    t = t.parent;
                }

                if (t == root)
                    continue;

                if (!collectedIds.Add(t.GetInstanceID()))
                    continue;

                buffer.Add(new TileTransformSample(t, r.bounds.center));
            }

            return renderers.Length;
        }

        private static void LogPass(string message)
        {
            Debug.Log($"{LogTag} Projector {message}");
        }

        private static string FormatSamples(List<string> samples)
        {
            if (samples == null || samples.Count == 0)
                return "[]";

            var builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < samples.Count; i++)
            {
                if (i > 0)
                    builder.Append(" | ");
                builder.Append(samples[i]);
            }
            builder.Append(']');
            return builder.ToString();
        }

        private static string FormatLevelStats(int[,] levelMap)
        {
            if (levelMap == null)
                return "null";

            int width = levelMap.GetLength(0);
            int height = levelMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            int min = int.MaxValue;
            int max = int.MinValue;
            var histogram = new SortedDictionary<int, int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = levelMap[x, y];
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                    histogram.TryGetValue(value, out int count);
                    histogram[value] = count + 1;
                }
            }

            var builder = new StringBuilder();
            builder.Append(width).Append('x').Append(height)
                .Append(", min=").Append(min)
                .Append(", max=").Append(max)
                .Append(", histogram={");
            int index = 0;
            foreach (var pair in histogram)
            {
                if (index > 0)
                    builder.Append(", ");
                builder.Append(pair.Key).Append(':').Append(pair.Value);
                index++;
                if (index >= 16 && histogram.Count > index)
                {
                    builder.Append(", ...");
                    break;
                }
            }

            builder.Append('}')
                .Append(", samples=(0,0:").Append(levelMap[0, 0]).Append(')')
                .Append(", (mid:").Append(levelMap[width / 2, height / 2]).Append(')')
                .Append(", (last:").Append(levelMap[width - 1, height - 1]).Append(')');
            return builder.ToString();
        }

        private static int CountLevelMapCells(int[,] terrainLevelMap)
        {
            if (terrainLevelMap == null)
                return 0;

            return terrainLevelMap.GetLength(0) * terrainLevelMap.GetLength(1);
        }

        private readonly struct TileTransformSample
        {
            public TileTransformSample(Transform transform, Vector3 worldCenter)
            {
                Transform = transform;
                WorldCenter = worldCenter;
            }

            public Transform Transform { get; }
            public Vector3 WorldCenter { get; }
        }
    }

    [DisallowMultipleComponent]
    public sealed class TileWorldCreatorRuntimeMeshOptimizer : MonoBehaviour
    {
        private const string LogTag = "[MoyvaTWCHeight:MeshOptimize]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        private readonly List<string> _samples = new List<string>(16);
        private Transform _targetRoot;
        private int _clustersPerFrame = 4;
        private bool _deactivateSourceObjects;
        private bool _isOptimizing;
        private bool _hasOptimized;

        public void Configure(Transform targetRoot, int clustersPerFrame, bool deactivateSourceObjects)
        {
            _targetRoot = targetRoot != null ? targetRoot : transform;
            _clustersPerFrame = Mathf.Clamp(clustersPerFrame, 1, 64);
            _deactivateSourceObjects = deactivateSourceObjects;
            _isOptimizing = false;
            _hasOptimized = false;

            Debug.Log($"{LogTag} Configure root='{_targetRoot.name}', clustersPerFrame={_clustersPerFrame}, deactivateSourceObjects={_deactivateSourceObjects}, clusters={CountClusters(_targetRoot)}, rendererComponents={CountRendererComponents(_targetRoot)}, renderableMeshRenderers={CountRenderableMeshRenderers(_targetRoot)}, meshFiltersWithMesh={CountMeshFiltersWithMesh(_targetRoot)}.");
        }

        public void ClearConfiguration(string reason)
        {
            _targetRoot = null;
            _isOptimizing = false;
            _hasOptimized = false;
            Debug.Log($"{LogTag} Cleared optimizer configuration. reason='{reason}'.");
        }

        public void RequestOptimizeAfterStable(string reason)
        {
            if (_targetRoot == null)
            {
                Debug.LogWarning($"{LogTag} Optimization skipped: target root is null. reason='{reason}'.");
                return;
            }

            if (_hasOptimized)
            {
                Debug.Log($"{LogTag} Optimization skipped: already optimized this build. reason='{reason}'.");
                return;
            }

            if (_isOptimizing)
            {
                Debug.Log($"{LogTag} Optimization skipped: optimizer is already running. reason='{reason}'.");
                return;
            }

            Debug.Log($"{WorldGenDiagTag} TWCBuild.COROUTINE.START frame={Time.frameCount}");
            StartCoroutine(OptimizeCoroutine(reason));
        }

        private IEnumerator OptimizeCoroutine(string reason)
        {
            _isOptimizing = true;
            _samples.Clear();

            Transform root = _targetRoot != null ? _targetRoot : transform;
            var clusters = root.GetComponentsInChildren<ClusterIdentifier>(false);
            int beforeRendererComponents = CountRendererComponents(root);
            int beforeRenderableRenderers = CountRenderableMeshRenderers(root);
            int beforeMeshFilters = CountMeshFiltersWithMesh(root);
            float startTime = Time.realtimeSinceStartup;

            Debug.Log($"{LogTag} Optimization started. reason='{reason}', root='{root.name}', clusters={clusters.Length}, rendererComponentsBefore={beforeRendererComponents}, renderableMeshRenderersBefore={beforeRenderableRenderers}, meshFiltersWithMeshBefore={beforeMeshFilters}, clustersPerFrame={_clustersPerFrame}, deactivateSourceObjects={_deactivateSourceObjects}.");

            int processedClusters = 0;
            int combinedClusters = 0;
            int skippedSmallClusters = 0;
            int skippedNoMeshClusters = 0;
            int unchangedClusters = 0;
            int failedClusters = 0;
            int sourceRenderersHidden = 0;
            int totalVertices = 0;
            int processedThisFrame = 0;

            for (int clusterIndex = 0; clusterIndex < clusters.Length; clusterIndex++)
            {
                var cluster = clusters[clusterIndex];
                if (cluster == null)
                    continue;

                processedClusters++;
                var beforeStats = CollectClusterStats(cluster.transform);
                if (beforeStats.MeshFiltersWithMesh == 0)
                {
                    skippedNoMeshClusters++;
                    continue;
                }

                if (beforeStats.RenderableMeshRenderers <= 1)
                {
                    skippedSmallClusters++;
                    continue;
                }

                try
                {
                    var combiner = cluster.GetComponent<MeshCombiner>();
                    if (combiner == null)
                        combiner = cluster.gameObject.AddComponent<MeshCombiner>();

                    combiner.CreateMultiMaterialMesh = true;
                    combiner.CombineInactiveChildren = false;
                    combiner.DestroyCombinedChildren = false;
                    combiner.DeactivateCombinedChildren = _deactivateSourceObjects;
                    combiner.DeactivateCombinedChildrenMeshRenderers = !_deactivateSourceObjects;
                    combiner.GenerateUVMap = false;
                    combiner.CombineMeshes(false);

                    var afterStats = CollectClusterStats(cluster.transform);
                    var combinedMesh = cluster.GetComponent<MeshFilter>()?.sharedMesh;
                    if (combinedMesh != null)
                    {
                        combinedMesh.name = $"Moyva TWC Combined Cluster {cluster.clusterID}";
                        totalVertices += combinedMesh.vertexCount;
                    }

                    if (afterStats.RenderableMeshRenderers < beforeStats.RenderableMeshRenderers && combinedMesh != null)
                    {
                        combinedClusters++;
                        sourceRenderersHidden += beforeStats.RenderableMeshRenderers - afterStats.RenderableMeshRenderers;
                        AddSample($"cluster={cluster.clusterID} renderable {beforeStats.RenderableMeshRenderers}->{afterStats.RenderableMeshRenderers}, meshFilters={beforeStats.MeshFiltersWithMesh}, vertices={combinedMesh.vertexCount}, subMeshes={combinedMesh.subMeshCount}");
                    }
                    else
                    {
                        unchangedClusters++;
                        AddSample($"UNCHANGED cluster={cluster.clusterID} renderable {beforeStats.RenderableMeshRenderers}->{afterStats.RenderableMeshRenderers}, meshFilters={beforeStats.MeshFiltersWithMesh}, hasCombinedMesh={combinedMesh != null}");
                    }
                }
                catch (System.Exception ex)
                {
                    failedClusters++;
                    Debug.LogError($"{LogTag} Cluster optimization failed. cluster={cluster.clusterID}, name='{cluster.name}', error={ex}");
                }

                processedThisFrame++;
                if (processedThisFrame >= _clustersPerFrame)
                {
                    processedThisFrame = 0;
                    Debug.Log($"{LogTag} Optimization progress: processed={processedClusters}/{clusters.Length}, combined={combinedClusters}, hiddenSourceRenderers={sourceRenderersHidden}, elapsed={Time.realtimeSinceStartup - startTime:0.###}s.");
                    yield return null;
                }
            }

            int afterRendererComponents = CountRendererComponents(root);
            int afterRenderableRenderers = CountRenderableMeshRenderers(root);
            int afterMeshFilters = CountMeshFiltersWithMesh(root);
            _hasOptimized = true;
            _isOptimizing = false;

            Debug.Log($"{LogTag} Optimization complete. root='{root.name}', processedClusters={processedClusters}, combinedClusters={combinedClusters}, skippedSmallClusters={skippedSmallClusters}, skippedNoMeshClusters={skippedNoMeshClusters}, unchangedClusters={unchangedClusters}, failedClusters={failedClusters}, rendererComponents {beforeRendererComponents}->{afterRendererComponents}, renderableMeshRenderers {beforeRenderableRenderers}->{afterRenderableRenderers}, meshFiltersWithMesh {beforeMeshFilters}->{afterMeshFilters}, hiddenSourceRenderers={sourceRenderersHidden}, combinedVertices={totalVertices}, elapsed={Time.realtimeSinceStartup - startTime:0.###}s, samples={FormatSamples(_samples)}.");
            Debug.Log($"{WorldGenDiagTag} TWCBuild.COROUTINE.END frame={Time.frameCount}, childrenAfterCoroutine={root.childCount}");
        }

        private void AddSample(string sample)
        {
            if (_samples.Count < 16)
                _samples.Add(sample);
        }

        private static ClusterStats CollectClusterStats(Transform clusterRoot)
        {
            if (clusterRoot == null)
                return default;

            int rendererComponents = 0;
            int renderableMeshRenderers = 0;
            int meshFiltersWithMesh = 0;

            var meshRenderers = clusterRoot.GetComponentsInChildren<MeshRenderer>(false);
            for (int rendererIndex = 0; rendererIndex < meshRenderers.Length; rendererIndex++)
            {
                var renderer = meshRenderers[rendererIndex];
                if (renderer == null)
                    continue;

                rendererComponents++;
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (renderer.enabled && meshFilter != null && meshFilter.sharedMesh != null)
                    renderableMeshRenderers++;
            }

            var meshFilters = clusterRoot.GetComponentsInChildren<MeshFilter>(false);
            for (int filterIndex = 0; filterIndex < meshFilters.Length; filterIndex++)
            {
                if (meshFilters[filterIndex] != null && meshFilters[filterIndex].sharedMesh != null)
                    meshFiltersWithMesh++;
            }

            return new ClusterStats(rendererComponents, renderableMeshRenderers, meshFiltersWithMesh);
        }

        private static int CountClusters(Transform root)
            => root != null ? root.GetComponentsInChildren<ClusterIdentifier>(false).Length : 0;

        private static int CountRendererComponents(Transform root)
            => root != null ? root.GetComponentsInChildren<MeshRenderer>(true).Length : 0;

        private static int CountRenderableMeshRenderers(Transform root)
        {
            if (root == null)
                return 0;

            int count = 0;
            var renderers = root.GetComponentsInChildren<MeshRenderer>(false);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                if (renderer == null || !renderer.enabled)
                    continue;
                if (renderer.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() != null)
                    continue;

                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                    count++;
            }

            return count;
        }

        private static int CountMeshFiltersWithMesh(Transform root)
        {
            if (root == null)
                return 0;

            int count = 0;
            var meshFilters = root.GetComponentsInChildren<MeshFilter>(false);
            for (int filterIndex = 0; filterIndex < meshFilters.Length; filterIndex++)
            {
                var meshFilter = meshFilters[filterIndex];
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;
                if (meshFilter.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() != null)
                    continue;

                count++;
            }

            return count;
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

        private readonly struct ClusterStats
        {
            public ClusterStats(int rendererComponents, int renderableMeshRenderers, int meshFiltersWithMesh)
            {
                RendererComponents = rendererComponents;
                RenderableMeshRenderers = renderableMeshRenderers;
                MeshFiltersWithMesh = meshFiltersWithMesh;
            }

            public int RendererComponents { get; }
            public int RenderableMeshRenderers { get; }
            public int MeshFiltersWithMesh { get; }
        }
    }
}
