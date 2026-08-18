using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;


namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Diagnostic-only runtime truth source for chunk ownership, generated
    /// tile sources and the final terrain mesh. It does not change generation.
    /// </summary>
    internal static class ChunkAuditRuntime
    {
        internal const string LogPrefix = "[MOYVA_CHUNK_AUDIT]";

        private static readonly Dictionary<MapChunkCoord, ChunkAuditRecord>
            Records = new Dictionary<MapChunkCoord, ChunkAuditRecord>();

        private static readonly Dictionary<Vector2Int, HashSet<MapChunkCoord>>
            LogicalOwners =
                new Dictionary<Vector2Int, HashSet<MapChunkCoord>>();

        private static readonly Dictionary<Vector2Int, HashSet<MapChunkCoord>>
            EmittedCellOwners =
                new Dictionary<Vector2Int, HashSet<MapChunkCoord>>();

        private static readonly Dictionary<string, HashSet<MapChunkCoord>>
            FragmentOwners =
                new Dictionary<string, HashSet<MapChunkCoord>>(
                    StringComparer.Ordinal);

        private static int _mapWidth;
        private static int _mapHeight;
        private static float _cellSize = 1f;
        private static float _worldMinX = -0.5f;
        private static float _worldMinZ = -0.5f;
        private static int _chunkSize = 1;
        private static int _physicalOwnerMismatchFragments;
        private static int _physicalMismatchLogs;
        private const int MaxPhysicalMismatchLogs = 48;
        private static bool _active;
        private static bool _hasReported;

        public static void BeginBuild(
            int width,
            int height,
            float cellSize,
            bool hasWorldBounds,
            Bounds worldBounds)
        {
            Records.Clear();
            LogicalOwners.Clear();
            EmittedCellOwners.Clear();
            FragmentOwners.Clear();

            _mapWidth = Mathf.Max(1, width);
            _mapHeight = Mathf.Max(1, height);
            _cellSize = Mathf.Max(0.0001f, cellSize);
            _worldMinX = hasWorldBounds
                ? worldBounds.min.x
                : -0.5f * _cellSize;
            _worldMinZ = hasWorldBounds
                ? worldBounds.min.z
                : -0.5f * _cellSize;
            _chunkSize = 1;
            _physicalOwnerMismatchFragments = 0;
            _physicalMismatchLogs = 0;
            _active = true;
            _hasReported = false;

            Debug.Log(
                $"{LogPrefix} BEGIN " +
                $"map={_mapWidth}x{_mapHeight} " +
                $"cellSize={F(_cellSize)} " +
                $"hasWorldBounds={hasWorldBounds} " +
                $"worldBounds={FormatBounds(worldBounds)}");
        }

        public static void BeginChunk(
            ChunkBuildArea area)
        {
            if (!_active)
                return;

            _chunkSize =
                Mathf.Max(
                    _chunkSize,
                    Mathf.Max(
                        area.CoreRect.width,
                        area.CoreRect.height));

            var record =
                new ChunkAuditRecord(
                    area.Coord,
                    area.CoreRect,
                    area.SampleRect,
                    area.CellSize);

            Records[area.Coord] =
                record;

            for (int y = area.CoreRect.yMin;
                 y < area.CoreRect.yMax;
                 y++)
            {
                for (int x = area.CoreRect.xMin;
                     x < area.CoreRect.xMax;
                     x++)
                {
                    Claim(
                        LogicalOwners,
                        new Vector2Int(x, y),
                        area.Coord);
                }
            }
        }

        public static void RecordSource(
            ChunkBuildArea area,
            Vector2Int logicalCell,
            TileMeshSource source)
        {
            if (!_active
                || !Records.TryGetValue(
                    area.Coord,
                    out ChunkAuditRecord record))
            {
                return;
            }

            record.SourceCount++;

            if (!source.IsValid)
            {
                record.InvalidSourceCount++;
                return;
            }

            record.ProviderEmittedCells.Add(
                logicalCell);

            Claim(
                EmittedCellOwners,
                logicalCell,
                area.Coord);

            if (!area.CoreRect.Contains(logicalCell))
            {
                record.SourceCellsOutsideCore++;

                Debug.LogError(
                    $"{LogPrefix} ERROR SOURCE_CELL_OUTSIDE_CORE " +
                    $"chunk={area.Coord} " +
                    $"cell={FormatCell(logicalCell)} " +
                    $"core={FormatRect(area.CoreRect)} " +
                    $"mesh={source.Mesh?.name ?? "<null>"}");
            }

            Bounds transformedBounds =
                TransformBounds(
                    source.Mesh.bounds,
                    source.LocalMatrix);

            record.IncludeSourceBounds(
                transformedBounds);

            if (source.HasTileFootprint)
            {
                Bounds footprintBounds =
                    new Bounds(
                        new Vector3(
                            source.TileCenterXZ.x,
                            transformedBounds.center.y,
                            source.TileCenterXZ.y),
                        new Vector3(
                            source.TileHalfExtent * 2f,
                            Mathf.Max(
                                0.001f,
                                transformedBounds.size.y),
                            source.TileHalfExtent * 2f));

                record.IncludeSourceFootprint(
                    footprintBounds);

                RecordPhysicalOwner(
                    record,
                    area,
                    logicalCell,
                    source);

                string fragmentKey =
                    BuildFragmentKey(source);

                Claim(
                    FragmentOwners,
                    fragmentKey,
                    area.Coord);
            }
        }

        public static void CompleteChunk(
            Transform chunkRoot,
            Transform terrainRoot,
            ChunkBuildArea area,
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition>
                resolvedCells,
            IReadOnlyCollection<Vector2Int> providerEmittedCells,
            Mesh finalMesh)
        {
            if (!_active)
                return;

            if (!Records.TryGetValue(
                    area.Coord,
                    out ChunkAuditRecord record))
            {
                BeginChunk(area);
                record = Records[area.Coord];
            }

            if (providerEmittedCells != null)
            {
                foreach (Vector2Int cell
                         in providerEmittedCells)
                {
                    record.ProviderEmittedCells.Add(
                        cell);

                    Claim(
                        EmittedCellOwners,
                        cell,
                        area.Coord);
                }
            }

            record.ExpectedTerrainCells = 0;
            record.MissingTerrainMeshCells.Clear();

            for (int y = area.CoreRect.yMin;
                 y < area.CoreRect.yMax;
                 y++)
            {
                for (int x = area.CoreRect.xMin;
                     x < area.CoreRect.xMax;
                     x++)
                {
                    var cell =
                        new Vector2Int(x, y);

                    if (resolvedCells == null
                        || !resolvedCells.TryGetValue(
                            cell,
                            out ResolvedTileComposition composition)
                        || !composition.HasMainTerrain)
                    {
                        continue;
                    }

                    record.ExpectedTerrainCells++;

                    if (!record.ProviderEmittedCells.Contains(
                            cell))
                    {
                        record.MissingTerrainMeshCells.Add(
                            cell);
                    }
                }
            }

            record.FinalMesh =
                finalMesh;

            record.FinalMeshBounds =
                ResolveWorldBounds(
                    terrainRoot,
                    finalMesh);

            record.LogicalCoreBounds =
                CreateLogicalBounds(
                    area.CoreRect,
                    record.FinalMeshBounds);

            record.LogicalSampleBounds =
                CreateLogicalBounds(
                    area.SampleRect,
                    record.FinalMeshBounds);

            record.ResolveOverhang();

            AttachGizmo(
                chunkRoot,
                terrainRoot,
                record);

            LogChunk(record);
        }

        public static void CompleteBuild()
        {
            if (!_active
                || _hasReported)
            {
                return;
            }

            _hasReported = true;

            int missingLogicalOwners = 0;
            int duplicateLogicalOwners = 0;
            int duplicateEmittedCells = 0;
            int duplicateFragments = 0;
            int missingTerrainMeshes = 0;
            int sourceCellsOutsideCore = 0;
            int invalidSources = 0;
            float maxOverhangNegativeX = 0f;
            float maxOverhangPositiveX = 0f;
            float maxOverhangNegativeZ = 0f;
            float maxOverhangPositiveZ = 0f;

            for (int y = 0;
                 y < _mapHeight;
                 y++)
            {
                for (int x = 0;
                     x < _mapWidth;
                     x++)
                {
                    var cell =
                        new Vector2Int(x, y);

                    if (!LogicalOwners.TryGetValue(
                            cell,
                            out HashSet<MapChunkCoord> owners)
                        || owners.Count == 0)
                    {
                        missingLogicalOwners++;

                        Debug.LogError(
                            $"{LogPrefix} ERROR TILE_WITHOUT_OWNER " +
                            $"cell={FormatCell(cell)}");

                        continue;
                    }

                    if (owners.Count <= 1)
                        continue;

                    duplicateLogicalOwners++;

                    Debug.LogError(
                        $"{LogPrefix} ERROR TILE_MULTIPLE_OWNERS " +
                        $"cell={FormatCell(cell)} " +
                        $"owners={FormatOwners(owners)}");
                }
            }

            foreach (KeyValuePair<
                         Vector2Int,
                         HashSet<MapChunkCoord>> pair
                     in EmittedCellOwners)
            {
                if (pair.Value.Count <= 1)
                    continue;

                duplicateEmittedCells++;

                Debug.LogError(
                    $"{LogPrefix} ERROR EMITTED_CELL_MULTIPLE_CHUNKS " +
                    $"cell={FormatCell(pair.Key)} " +
                    $"owners={FormatOwners(pair.Value)}");
            }

            foreach (KeyValuePair<
                         string,
                         HashSet<MapChunkCoord>> pair
                     in FragmentOwners)
            {
                if (pair.Value.Count <= 1)
                    continue;

                duplicateFragments++;

                Debug.LogError(
                    $"{LogPrefix} ERROR FRAGMENT_MULTIPLE_CHUNKS " +
                    $"fragment={pair.Key} " +
                    $"owners={FormatOwners(pair.Value)}");
            }

            foreach (ChunkAuditRecord record
                     in Records.Values)
            {
                missingTerrainMeshes +=
                    record.MissingTerrainMeshCells.Count;

                sourceCellsOutsideCore +=
                    record.SourceCellsOutsideCore;

                invalidSources +=
                    record.InvalidSourceCount;

                maxOverhangNegativeX = Mathf.Max(
                    maxOverhangNegativeX,
                    record.OverhangNegativeX);
                maxOverhangPositiveX = Mathf.Max(
                    maxOverhangPositiveX,
                    record.OverhangPositiveX);
                maxOverhangNegativeZ = Mathf.Max(
                    maxOverhangNegativeZ,
                    record.OverhangNegativeZ);
                maxOverhangPositiveZ = Mathf.Max(
                    maxOverhangPositiveZ,
                    record.OverhangPositiveZ);

                for (int i = 0;
                     i < record.MissingTerrainMeshCells.Count;
                     i++)
                {
                    Debug.LogError(
                        $"{LogPrefix} ERROR TERRAIN_CELL_WITHOUT_SOURCE " +
                        $"chunk={record.Coord} " +
                        $"cell={FormatCell(record.MissingTerrainMeshCells[i])}");
                }
            }

            Debug.Log(
                $"{LogPrefix} COMPLETE " +
                $"chunks={Records.Count} " +
                $"logicalCells={_mapWidth * _mapHeight} " +
                $"missingLogicalOwners={missingLogicalOwners} " +
                $"duplicateLogicalOwners={duplicateLogicalOwners} " +
                $"duplicateEmittedCells={duplicateEmittedCells} " +
                $"duplicateFragments={duplicateFragments} " +
                $"physicalOwnerMismatchFragments=" +
                $"{_physicalOwnerMismatchFragments} " +
                $"missingTerrainMeshes={missingTerrainMeshes} " +
                $"sourceCellsOutsideCore={sourceCellsOutsideCore} " +
                $"invalidSources={invalidSources} " +
                $"maxOverhang=" +
                $"({F(maxOverhangNegativeX)}," +
                $"{F(maxOverhangPositiveX)}," +
                $"{F(maxOverhangNegativeZ)}," +
                $"{F(maxOverhangPositiveZ)})");

            _active = false;
        }

        private static void LogChunk(
            ChunkAuditRecord record)
        {
            Debug.Log(
                $"{LogPrefix} CHUNK " +
                $"coord={record.Coord} " +
                $"core={FormatRect(record.CoreRect)} " +
                $"sample={FormatRect(record.SampleRect)} " +
                $"coreCells={record.CoreRect.width * record.CoreRect.height} " +
                $"terrainCells={record.ExpectedTerrainCells} " +
                $"providerEmittedCells={record.ProviderEmittedCells.Count} " +
                $"sources={record.SourceCount} " +
                $"invalidSources={record.InvalidSourceCount} " +
                $"physicalOwnerMismatches=" +
                $"{record.PhysicalOwnerMismatchCount} " +
                $"sourceCellsOutsideCore={record.SourceCellsOutsideCore} " +
                $"missingTerrainSources={record.MissingTerrainMeshCells.Count} " +
                $"mesh={FormatMesh(record.FinalMesh)} " +
                $"coreBounds={FormatBounds(record.LogicalCoreBounds)} " +
                $"meshBounds={FormatBounds(record.FinalMeshBounds)} " +
                $"sourceBounds={FormatBounds(record.SourceBounds)} " +
                $"footprintBounds={FormatBounds(record.SourceFootprintBounds)} " +
                $"overhang=" +
                $"({F(record.OverhangNegativeX)}," +
                $"{F(record.OverhangPositiveX)}," +
                $"{F(record.OverhangNegativeZ)}," +
                $"{F(record.OverhangPositiveZ)})");
        }

        private static void AttachGizmo(
            Transform chunkRoot,
            Transform terrainRoot,
            ChunkAuditRecord record)
        {
            if (chunkRoot == null)
                return;

            ChunkAuditGizmo gizmo =
                chunkRoot.GetComponent<ChunkAuditGizmo>();

            if (gizmo == null)
            {
                gizmo =
                    chunkRoot.gameObject
                        .AddComponent<ChunkAuditGizmo>();
            }

            gizmo.Configure(
                record.Coord.X,
                record.Coord.Y,
                record.CoreRect,
                record.SampleRect,
                record.CellSize,
                record.LogicalCoreBounds,
                record.LogicalSampleBounds,
                record.FinalMeshBounds,
                record.SourceBounds,
                record.SourceFootprintBounds,
                terrainRoot,
                record.FinalMesh,
                record.ProviderEmittedCells);
        }

        private static Bounds CreateLogicalBounds(
            RectInt rect,
            Bounds verticalReference)
        {
            /*
             * TWC places normal-grid tile centers at x*cellSize/z*cellSize.
             * A logical tile therefore occupies center +/- half a cell.
             */
            float xMin =
                (rect.xMin - 0.5f)
                * _cellSize;

            float xMax =
                (rect.xMax - 0.5f)
                * _cellSize;

            float zMin =
                (rect.yMin - 0.5f)
                * _cellSize;

            float zMax =
                (rect.yMax - 0.5f)
                * _cellSize;

            float minY =
                verticalReference.size.y > 0.0001f
                    ? verticalReference.min.y
                    : -0.05f;

            float maxY =
                verticalReference.size.y > 0.0001f
                    ? verticalReference.max.y
                    : 0.05f;

            return BoundsFromMinMax(
                new Vector3(
                    xMin,
                    minY,
                    zMin),
                new Vector3(
                    xMax,
                    maxY,
                    zMax));
        }

        private static Bounds ResolveWorldBounds(
            Transform terrainRoot,
            Mesh mesh)
        {
            if (terrainRoot == null
                || mesh == null)
            {
                return default;
            }

            return TransformBounds(
                mesh.bounds,
                terrainRoot.localToWorldMatrix);
        }

        private static Bounds TransformBounds(
            Bounds localBounds,
            Matrix4x4 matrix)
        {
            Vector3 center =
                matrix.MultiplyPoint3x4(
                    localBounds.center);

            Vector3 extents =
                localBounds.extents;

            Vector3 axisX =
                matrix.MultiplyVector(
                    new Vector3(
                        extents.x,
                        0f,
                        0f));

            Vector3 axisY =
                matrix.MultiplyVector(
                    new Vector3(
                        0f,
                        extents.y,
                        0f));

            Vector3 axisZ =
                matrix.MultiplyVector(
                    new Vector3(
                        0f,
                        0f,
                        extents.z));

            extents =
                Abs(axisX)
                + Abs(axisY)
                + Abs(axisZ);

            return new Bounds(
                center,
                extents * 2f);
        }

        private static Vector3 Abs(
            Vector3 value)
        {
            return new Vector3(
                Mathf.Abs(value.x),
                Mathf.Abs(value.y),
                Mathf.Abs(value.z));
        }

        private static Bounds BoundsFromMinMax(
            Vector3 min,
            Vector3 max)
        {
            var bounds =
                new Bounds();

            bounds.SetMinMax(
                Vector3.Min(min, max),
                Vector3.Max(min, max));

            return bounds;
        }

        private static void RecordPhysicalOwner(
            ChunkAuditRecord record,
            ChunkBuildArea area,
            Vector2Int logicalCell,
            TileMeshSource source)
        {
            MapChunkCoord physicalOwner =
                ResolvePhysicalOwner(
                    source.TileCenterXZ);

            if (physicalOwner.Equals(area.Coord))
                return;

            record.PhysicalOwnerMismatchCount++;
            _physicalOwnerMismatchFragments++;

            if (_physicalMismatchLogs
                >= MaxPhysicalMismatchLogs)
            {
                return;
            }

            _physicalMismatchLogs++;

            Debug.LogWarning(
                $"{LogPrefix} PHYSICAL_OWNER_MISMATCH " +
                $"actualChunk={area.Coord} " +
                $"physicalChunk={physicalOwner} " +
                $"sourceCell={FormatCell(logicalCell)} " +
                $"center=({F(source.TileCenterXZ.x)}," +
                $"{F(source.TileCenterXZ.y)}) " +
                $"halfExtent={F(source.TileHalfExtent)} " +
                $"mesh={source.Mesh?.name ?? "<null>"} " +
                $"layer={source.GraphLayerName ?? "<none>"}");
        }

        private static MapChunkCoord ResolvePhysicalOwner(
            Vector2 center)
        {
            int tileX =
                Mathf.Clamp(
                    Mathf.FloorToInt(
                        (center.x - _worldMinX)
                        / _cellSize),
                    0,
                    _mapWidth - 1);

            int tileY =
                Mathf.Clamp(
                    Mathf.FloorToInt(
                        (center.y - _worldMinZ)
                        / _cellSize),
                    0,
                    _mapHeight - 1);

            return new MapChunkCoord(
                tileX / Mathf.Max(1, _chunkSize),
                tileY / Mathf.Max(1, _chunkSize));
        }

        private static string F(
            float value)
        {
            return value.ToString(
                "0.###",
                CultureInfo.InvariantCulture);
        }

        private static string BuildFragmentKey(
            TileMeshSource source)
        {
            return
                $"{source.GraphLayerId ?? "<none>"}|" +
                $"{Quantize(source.TileCenterXZ.x)}|" +
                $"{Quantize(source.TileCenterXZ.y)}|" +
                $"{Quantize(source.TileHalfExtent)}|" +
                $"{source.TileGeometryMode}";
        }

        private static int Quantize(
            float value)
        {
            return Mathf.RoundToInt(
                value * 1000f);
        }

        private static void Claim<TKey>(
            Dictionary<TKey, HashSet<MapChunkCoord>> table,
            TKey key,
            MapChunkCoord coord)
        {
            if (!table.TryGetValue(
                    key,
                    out HashSet<MapChunkCoord> owners))
            {
                owners =
                    new HashSet<MapChunkCoord>();

                table[key] =
                    owners;
            }

            owners.Add(
                coord);
        }

        private static string FormatOwners(
            HashSet<MapChunkCoord> owners)
        {
            var builder =
                new StringBuilder();

            bool first = true;

            foreach (MapChunkCoord owner
                     in owners)
            {
                if (!first)
                    builder.Append(',');

                builder.Append(
                    owner);

                first = false;
            }

            return builder.ToString();
        }

        private static string FormatRect(
            RectInt rect)
        {
            return
                $"({rect.xMin},{rect.yMin}," +
                $"{rect.width},{rect.height})";
        }

        private static string FormatCell(
            Vector2Int cell)
        {
            return
                $"({cell.x},{cell.y})";
        }

        private static string FormatBounds(
            Bounds bounds)
        {
            if (bounds.size.sqrMagnitude
                <= 0.0000001f)
            {
                return "<empty>";
            }

            return
                $"center=({F(bounds.center.x)}," +
                $"{F(bounds.center.y)}," +
                $"{F(bounds.center.z)})/" +
                $"size=({F(bounds.size.x)}," +
                $"{F(bounds.size.y)}," +
                $"{F(bounds.size.z)})";
        }

        private static string FormatMesh(
            Mesh mesh)
        {
            if (mesh == null)
                return "<none>";

            long indices = 0;

            for (int subMesh = 0;
                 subMesh < mesh.subMeshCount;
                 subMesh++)
            {
                indices +=
                    (long)mesh.GetIndexCount(
                        subMesh);
            }

            return
                $"{mesh.name}/v={mesh.vertexCount}/" +
                $"i={indices}/sub={mesh.subMeshCount}";
        }

        private sealed class ChunkAuditRecord
        {
            public ChunkAuditRecord(
                MapChunkCoord coord,
                RectInt coreRect,
                RectInt sampleRect,
                float cellSize)
            {
                Coord = coord;
                CoreRect = coreRect;
                SampleRect = sampleRect;
                CellSize = cellSize;
            }

            public MapChunkCoord Coord { get; }
            public RectInt CoreRect { get; }
            public RectInt SampleRect { get; }
            public float CellSize { get; }
            public int SourceCount { get; set; }
            public int InvalidSourceCount { get; set; }
            public int PhysicalOwnerMismatchCount { get; set; }
            public int SourceCellsOutsideCore { get; set; }
            public int ExpectedTerrainCells { get; set; }

            public readonly HashSet<Vector2Int>
                ProviderEmittedCells =
                    new HashSet<Vector2Int>();

            public readonly List<Vector2Int>
                MissingTerrainMeshCells =
                    new List<Vector2Int>();

            public Mesh FinalMesh { get; set; }
            public Bounds LogicalCoreBounds { get; set; }
            public Bounds LogicalSampleBounds { get; set; }
            public Bounds FinalMeshBounds { get; set; }
            public Bounds SourceBounds { get; private set; }
            public Bounds SourceFootprintBounds { get; private set; }

            public float OverhangNegativeX { get; private set; }
            public float OverhangPositiveX { get; private set; }
            public float OverhangNegativeZ { get; private set; }
            public float OverhangPositiveZ { get; private set; }

            private bool _hasSourceBounds;
            private bool _hasSourceFootprintBounds;

            public void IncludeSourceBounds(
                Bounds bounds)
            {
                if (!_hasSourceBounds)
                {
                    SourceBounds = bounds;
                    _hasSourceBounds = true;
                    return;
                }

                Bounds combined =
                    SourceBounds;

                combined.Encapsulate(
                    bounds);

                SourceBounds =
                    combined;
            }

            public void IncludeSourceFootprint(
                Bounds bounds)
            {
                if (!_hasSourceFootprintBounds)
                {
                    SourceFootprintBounds = bounds;
                    _hasSourceFootprintBounds = true;
                    return;
                }

                Bounds combined =
                    SourceFootprintBounds;

                combined.Encapsulate(
                    bounds);

                SourceFootprintBounds =
                    combined;
            }

            public void ResolveOverhang()
            {
                if (FinalMeshBounds.size.sqrMagnitude
                    <= 0.0000001f)
                {
                    OverhangNegativeX = 0f;
                    OverhangPositiveX = 0f;
                    OverhangNegativeZ = 0f;
                    OverhangPositiveZ = 0f;
                    return;
                }

                OverhangNegativeX =
                    Mathf.Max(
                        0f,
                        LogicalCoreBounds.min.x
                        - FinalMeshBounds.min.x);

                OverhangPositiveX =
                    Mathf.Max(
                        0f,
                        FinalMeshBounds.max.x
                        - LogicalCoreBounds.max.x);

                OverhangNegativeZ =
                    Mathf.Max(
                        0f,
                        LogicalCoreBounds.min.z
                        - FinalMeshBounds.min.z);

                OverhangPositiveZ =
                    Mathf.Max(
                        0f,
                        FinalMeshBounds.max.z
                        - LogicalCoreBounds.max.z);
            }
        }
    }

}
