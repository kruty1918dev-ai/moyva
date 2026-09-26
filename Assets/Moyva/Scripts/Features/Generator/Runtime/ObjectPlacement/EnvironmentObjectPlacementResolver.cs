using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Resolves whether an environment prop (tree, rock, decoration) actually
    /// fits at its authored spot. Uses the prefab's full renderer bounds —
    /// children and LOD meshes included — transformed by the final rotation
    /// and scale, so a crown cannot hang over water or invalid cells. A prop
    /// that does not fit is shifted within a bounded radius; when no shifted
    /// placement fits, the caller skips the spawn.
    /// </summary>
    internal sealed class EnvironmentObjectPlacementResolver
    {
        private readonly Dictionary<GameObject, Bounds> _localBoundsCache = new();

        public readonly struct Request
        {
            public Request(
                GameObject prefab,
                Vector3 worldPosition,
                Quaternion rotation,
                Vector3 scale,
                int mapWidth,
                int mapHeight,
                float cellSize,
                Func<Vector2Int, bool> cellAccepted,
                Func<Vector2Int, float> surfaceHeight,
                float maxShiftCells,
                float maxGroundDeltaMeters,
                float footprintShrink)
            {
                Prefab = prefab;
                WorldPosition = worldPosition;
                Rotation = rotation;
                Scale = scale;
                MapWidth = mapWidth;
                MapHeight = mapHeight;
                CellSize = cellSize;
                CellAccepted = cellAccepted;
                SurfaceHeight = surfaceHeight;
                MaxShiftCells = maxShiftCells;
                MaxGroundDeltaMeters = maxGroundDeltaMeters;
                FootprintShrink = footprintShrink;
            }

            public GameObject Prefab { get; }
            public Vector3 WorldPosition { get; }
            public Quaternion Rotation { get; }
            public Vector3 Scale { get; }
            public int MapWidth { get; }
            public int MapHeight { get; }
            public float CellSize { get; }
            /// <summary>Every cell overlapped by the footprint must satisfy this.</summary>
            public Func<Vector2Int, bool> CellAccepted { get; }
            /// <summary>Surface height per cell; NaN when unknown.</summary>
            public Func<Vector2Int, float> SurfaceHeight { get; }
            public float MaxShiftCells { get; }
            public float MaxGroundDeltaMeters { get; }
            public float FootprintShrink { get; }
        }

        /// <summary>
        /// Returns the (possibly shifted) world XZ for the prop, or false when
        /// no candidate inside the shift bound satisfies the footprint rules.
        /// Y is left to the caller — grounding is a separate step.
        /// </summary>
        public bool TryResolve(
            in Request request,
            out Vector3 resolvedPosition)
        {
            resolvedPosition = request.WorldPosition;
            if (request.CellAccepted == null)
                return true;

            Bounds localBounds = ResolveLocalBounds(request.Prefab);
            float cellSize = Mathf.Max(0.0001f, request.CellSize);

            // Candidate ring: the authored spot first, then eight directions
            // at growing radii up to the shift bound. Deterministic order so
            // equal-quality outcomes are repeatable.
            if (IsFootprintValid(request, localBounds, request.WorldPosition))
                return true;

            float maxShift = Mathf.Max(0f, request.MaxShiftCells) * cellSize;
            if (maxShift <= 0.0001f)
                return false;

            for (int radiusStep = 1; radiusStep <= 3; radiusStep++)
            {
                float radius = maxShift * radiusStep / 3f;
                for (int direction = 0; direction < 8; direction++)
                {
                    float angle = direction * (Mathf.PI / 4f);
                    var candidate = new Vector3(
                        request.WorldPosition.x + Mathf.Cos(angle) * radius,
                        request.WorldPosition.y,
                        request.WorldPosition.z + Mathf.Sin(angle) * radius);
                    if (!IsFootprintValid(request, localBounds, candidate))
                        continue;

                    resolvedPosition = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Grounding height for a prop whose pivot may sit above or below the
        /// model's lower bound. The prop's lowest transformed point lands on
        /// the lowest terrain surface under its footprint, so rocks rest on
        /// the ground instead of floating over a slope the offset carried
        /// them onto.
        /// </summary>
        public float ResolveGroundedY(
            GameObject prefab,
            Vector3 worldPosition,
            Quaternion rotation,
            Vector3 scale,
            float cellSize,
            Func<Vector2Int, float> surfaceHeight,
            float fallbackSurfaceY,
            float footprintShrink = 1f)
        {
            Bounds localBounds = ResolveLocalBounds(prefab);
            float minLocalY = ResolveTransformedMinY(localBounds, rotation, scale);

            float minSurface = float.MaxValue;
            bool any = false;
            foreach (Vector2Int cell in CoveredCells(
                         localBounds, worldPosition, rotation, scale, cellSize, footprintShrink))
            {
                float h = surfaceHeight != null
                    ? surfaceHeight(cell)
                    : float.NaN;
                if (float.IsNaN(h) || float.IsInfinity(h))
                    continue;
                if (h < minSurface)
                    minSurface = h;
                any = true;
            }

            float surface = any ? minSurface : fallbackSurfaceY;
            return surface - minLocalY;
        }

        private bool IsFootprintValid(
            in Request request,
            Bounds localBounds,
            Vector3 worldPosition)
        {
            bool anyCell = false;
            float minSurface = float.MaxValue;
            float maxSurface = float.MinValue;

            foreach (Vector2Int cell in CoveredCells(
                         localBounds,
                         worldPosition,
                         request.Rotation,
                         request.Scale,
                         request.CellSize,
                         request.FootprintShrink))
            {
                if (cell.x < 0 || cell.y < 0
                    || cell.x >= request.MapWidth || cell.y >= request.MapHeight)
                {
                    return false;
                }

                if (!request.CellAccepted(cell))
                    return false;

                anyCell = true;
                if (request.MaxGroundDeltaMeters > 0f && request.SurfaceHeight != null)
                {
                    float h = request.SurfaceHeight(cell);
                    if (!float.IsNaN(h) && !float.IsInfinity(h))
                    {
                        minSurface = Mathf.Min(minSurface, h);
                        maxSurface = Mathf.Max(maxSurface, h);
                    }
                }
            }

            if (!anyCell)
                return false;

            return request.MaxGroundDeltaMeters <= 0f
                   || minSurface > maxSurface
                   || maxSurface - minSurface <= request.MaxGroundDeltaMeters;
        }

        /// <summary>
        /// Cells overlapped by the footprint: the transformed renderer bounds
        /// projected onto XZ, shrunk toward the prop centre by
        /// <paramref name="footprintShrink"/> so thin silhouette tips at the
        /// bounds corners do not claim whole cells.
        /// </summary>
        private IEnumerable<Vector2Int> CoveredCells(
            Bounds localBounds,
            Vector3 worldPosition,
            Quaternion rotation,
            Vector3 scale,
            float cellSize,
            float footprintShrink)
        {
            float size = Mathf.Max(0.0001f, cellSize);
            Bounds world = TransformBoundsXZ(localBounds, worldPosition, rotation, scale);

            if (footprintShrink < 1f && footprintShrink > 0f)
            {
                Vector3 c = world.center;
                Vector3 e = world.extents * Mathf.Clamp01(footprintShrink);
                world = new Bounds(c, e * 2f);
            }

            // Cells are centered on integer lattice positions (cell i spans
            // [i-0.5, i+0.5) in world units), so map by +0.5 — plain floor
            // mis-labels a half-cell border overhang as the anchor cell and
            // hides it from the in-bounds check. An edge exactly on a cell
            // border does not claim the next cell.
            float epsilon = size * 0.001f;
            int x0 = Mathf.FloorToInt(world.min.x / size + 0.5f);
            int x1 = Mathf.FloorToInt((world.max.x - epsilon) / size + 0.5f);
            int y0 = Mathf.FloorToInt(world.min.z / size + 0.5f);
            int y1 = Mathf.FloorToInt((world.max.z - epsilon) / size + 0.5f);

            for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                yield return new Vector2Int(x, y);
        }

        /// <summary>
        /// Prefab-local combined bounds of every renderer (children and LODs
        /// included). Cached per prefab; identity for null prefabs so a point
        /// footprint stays valid.
        /// </summary>
        private Bounds ResolveLocalBounds(GameObject prefab)
        {
            if (prefab == null)
                return new Bounds(Vector3.zero, Vector3.one * 0.1f);

            if (_localBoundsCache.TryGetValue(prefab, out Bounds cached))
                return cached;

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                var point = new Bounds(Vector3.zero, Vector3.one * 0.1f);
                _localBoundsCache[prefab] = point;
                return point;
            }

            Matrix4x4 rootInverse = prefab.transform.worldToLocalMatrix;
            Bounds combined = default;
            bool initialized = false;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                Bounds local = renderer.localBounds;
                Matrix4x4 toRoot =
                    rootInverse * renderer.transform.localToWorldMatrix;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = local.center + Vector3.Scale(
                        local.extents,
                        new Vector3(
                            (i & 1) == 0 ? -1f : 1f,
                            (i & 2) == 0 ? -1f : 1f,
                            (i & 4) == 0 ? -1f : 1f));
                    Vector3 rootSpace = toRoot.MultiplyPoint3x4(corner);
                    if (!initialized)
                    {
                        combined = new Bounds(rootSpace, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        combined.Encapsulate(rootSpace);
                    }
                }
            }

            if (!initialized)
                combined = new Bounds(Vector3.zero, Vector3.one * 0.1f);

            _localBoundsCache[prefab] = combined;
            return combined;
        }

        private static Bounds TransformBoundsXZ(
            Bounds localBounds,
            Vector3 worldPosition,
            Quaternion rotation,
            Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(worldPosition, rotation, scale);
            Vector3 min = new Vector3(float.MaxValue, 0f, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, 0f, float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = localBounds.center + Vector3.Scale(
                    localBounds.extents,
                    new Vector3(
                        (i & 1) == 0 ? -1f : 1f,
                        (i & 2) == 0 ? -1f : 1f,
                        (i & 4) == 0 ? -1f : 1f));
                Vector3 world = matrix.MultiplyPoint3x4(corner);
                min.x = Mathf.Min(min.x, world.x);
                min.z = Mathf.Min(min.z, world.z);
                max.x = Mathf.Max(max.x, world.x);
                max.z = Mathf.Max(max.z, world.z);
            }

            return new Bounds(
                (min + max) * 0.5f,
                max - min);
        }

        /// <summary>
        /// Lowest point of the transformed bounds relative to the pivot.
        /// 0 for base-pivot props, positive when the mesh floats above the
        /// pivot, negative when the model embeds below it.
        /// </summary>
        private static float ResolveTransformedMinY(
            Bounds localBounds,
            Quaternion rotation,
            Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, rotation, scale);
            float min = float.MaxValue;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = localBounds.center + Vector3.Scale(
                    localBounds.extents,
                    new Vector3(
                        (i & 1) == 0 ? -1f : 1f,
                        (i & 2) == 0 ? -1f : 1f,
                        (i & 4) == 0 ? -1f : 1f));
                float y = matrix.MultiplyPoint3x4(corner).y;
                if (y < min)
                    min = y;
            }

            return min == float.MaxValue ? 0f : min;
        }
    }
}
