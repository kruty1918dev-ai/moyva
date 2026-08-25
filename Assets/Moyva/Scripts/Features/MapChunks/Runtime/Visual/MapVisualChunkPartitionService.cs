using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    /// <summary>
    /// Assigns authored map visual owners to generated chunk roots.
    ///
    /// Important invariants:
    /// - generated terrain already has an explicit chunk root and is never
    ///   repartitioned here;
    /// - only renderers below known map-content roots are considered;
    /// - a complete visual owner transform is moved, never an individual
    ///   child renderer;
    /// - owner bounds must fit completely inside exactly one chunk.
    /// </summary>
    internal sealed class MapVisualChunkPartitionService :
        IMapVisualChunkPartitionService,
        IInitializable,
        ITickable,
        IDisposable
    {
        private const string AuditPrefix =
            "[MOYVA_CHUNK_PARTITION]";

        private const int PassesPerRequest = 2;

        private static readonly string[] PartitionRootNames =
        {
            "TilesRoot",
            "ObjectsRoot",
            "BuildingsRoot",
            "PlayerBuildingsRoot",
            "LayersRoot"
        };

        private readonly SignalBus _signalBus;
        private readonly IMapChunkSettingsProvider _settings;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly IMapVisualRendererCollector _collector;
        private readonly IMapVisualRendererFilter _filter;

        private readonly List<Renderer> _renderers =
            new List<Renderer>(512);

        private readonly List<Renderer> _ownerRenderers =
            new List<Renderer>(32);

        private readonly List<MapChunkCoord> _chunks =
            new List<MapChunkCoord>(16);

        private readonly List<Transform> _owners =
            new List<Transform>(256);

        private readonly HashSet<Transform> _ownerSet =
            new HashSet<Transform>();

        private int _pendingPasses;
        private float _nextPartitionAt;

        public MapVisualChunkPartitionService(
            SignalBus signalBus,
            IMapChunkSettingsProvider settings,
            IMapChunkLayoutService layout,
            IMapVisualChunkRootService roots,
            IMapVisualRendererCollector collector,
            IMapVisualRendererFilter filter)
        {
            _signalBus = signalBus;
            _settings = settings;
            _layout = layout;
            _roots = roots;
            _collector = collector;
            _filter = filter;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldBuiltSignal>(
                OnWorldBuilt);

            _signalBus.Subscribe<WorldGeneratedDataSignal>(
                OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(
                OnWorldBuilt);

            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(
                OnWorldGenerated);
        }

        public void Tick()
        {
            if (!_settings.EnableVisualChunkPartitioning
                || !_layout.IsConfigured)
            {
                return;
            }

            if (_pendingPasses <= 0)
                return;

            if (Time.unscaledTime < _nextPartitionAt)
                return;

            PartitionOnce();

            _pendingPasses--;

            _nextPartitionAt =
                Time.unscaledTime
                + _settings.VisualDiscoveryIntervalSeconds;
        }

        public void RequestPartition()
        {
            /*
             * WorldGenerated and WorldBuilt can arrive close together.
             * Coalesce them into at most two deterministic passes instead of
             * repeatedly scanning for the full duration window.
             */
            _pendingPasses =
                Mathf.Max(
                    _pendingPasses,
                    PassesPerRequest);

            _nextPartitionAt = 0f;
        }

        private void OnWorldBuilt(
            WorldBuiltSignal _)
        {
            RequestPartition();
        }

        private void OnWorldGenerated(
            WorldGeneratedDataSignal _)
        {
            RequestPartition();
        }

        private void PartitionOnce()
        {
            /*
             * Never use CollectScene here. Scene-wide discovery was pulling
             * clouds, construction preview renderers, cameras and unrelated
             * test objects into the chunk hierarchy.
             */
            _collector.CollectPreferredRoots(
                _renderers);

            _owners.Clear();
            _ownerSet.Clear();

            int scannedRenderers =
                _renderers.Count;

            int filteredRenderers = 0;
            int renderersWithoutOwner = 0;
            int alreadyChunkOwnedRenderers = 0;

            for (int i = 0;
                 i < _renderers.Count;
                 i++)
            {
                Renderer renderer =
                    _renderers[i];

                if (!_filter.CanPartition(
                        renderer,
                        _roots))
                {
                    filteredRenderers++;
                    continue;
                }

                if (IsUnderChunkRoot(
                        renderer.transform))
                {
                    alreadyChunkOwnedRenderers++;
                    continue;
                }

                if (!TryResolveOwner(
                        renderer.transform,
                        out Transform owner))
                {
                    renderersWithoutOwner++;
                    continue;
                }

                if (_ownerSet.Add(owner))
                    _owners.Add(owner);
            }

            int movedOwners = 0;
            int movedRenderers = 0;
            int multiChunkOwners = 0;
            int oversizedOwners = 0;
            int emptyOwners = 0;
            int alreadyOwnedOwners = 0;

            for (int i = 0;
                 i < _owners.Count;
                 i++)
            {
                Transform owner =
                    _owners[i];

                if (owner == null)
                {
                    emptyOwners++;
                    continue;
                }

                if (IsUnderChunkRoot(owner))
                {
                    alreadyOwnedOwners++;
                    continue;
                }

                if (!TryCalculateOwnerBounds(
                        owner,
                        out Bounds ownerBounds,
                        out int rendererCount))
                {
                    emptyOwners++;
                    continue;
                }

                int overlapCount =
                    _layout.GetChunksOverlapping(
                        ownerBounds,
                        _chunks);

                if (overlapCount != 1)
                {
                    if (overlapCount > 1)
                    {
                        multiChunkOwners++;

                        LogOwnerDetail(
                            "MULTI_CHUNK_OWNER",
                            owner,
                            ownerBounds,
                            rendererCount,
                            _chunks,
                            default,
                            false);
                    }
                    else
                    {
                        LogOwnerDetail(
                            "NO_CHUNK_OWNER",
                            owner,
                            ownerBounds,
                            rendererCount,
                            _chunks,
                            default,
                            false);
                    }

                    continue;
                }

                MapChunkCoord target =
                    _chunks[0];

                if (!MapChunkBoundsContainment
                    .ContainsRendererXZ(
                        _layout,
                        ownerBounds,
                        target))
                {
                    oversizedOwners++;

                    LogOwnerDetail(
                        "OVERSIZED_OWNER",
                        owner,
                        ownerBounds,
                        rendererCount,
                        _chunks,
                        target,
                        true);

                    continue;
                }

                Transform chunkRoot =
                    _roots.GetOrCreateRoot(
                        target);

                if (owner.parent == chunkRoot)
                {
                    alreadyOwnedOwners++;
                    continue;
                }

                LogOwnerDetail(
                    "MOVE_OWNER",
                    owner,
                    ownerBounds,
                    rendererCount,
                    _chunks,
                    target,
                    true);

                owner.SetParent(
                    chunkRoot,
                    true);

                movedOwners++;
                movedRenderers += rendererCount;
            }

            _ownerRenderers.Clear();
            _owners.Clear();
            _ownerSet.Clear();
            _renderers.Clear();
            _chunks.Clear();
        }

        private bool TryCalculateOwnerBounds(
            Transform owner,
            out Bounds bounds,
            out int rendererCount)
        {
            bounds = default;
            rendererCount = 0;

            _ownerRenderers.Clear();

            owner.GetComponentsInChildren(
                true,
                _ownerRenderers);

            bool hasBounds = false;

            for (int i = 0;
                 i < _ownerRenderers.Count;
                 i++)
            {
                Renderer renderer =
                    _ownerRenderers[i];

                if (!_filter.CanRegister(renderer))
                    continue;

                if (IsUnderDifferentChunkRoot(
                        owner,
                        renderer.transform))
                {
                    continue;
                }

                Bounds rendererBounds =
                    renderer.bounds;

                if (!IsFiniteBounds(rendererBounds))
                    continue;

                if (!hasBounds)
                {
                    bounds = rendererBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(
                        rendererBounds);
                }

                rendererCount++;
            }

            return hasBounds
                && rendererCount > 0;
        }

        private bool TryResolveOwner(
            Transform rendererTransform,
            out Transform owner)
        {
            owner = null;

            if (rendererTransform == null)
                return false;

            Transform current =
                rendererTransform;

            while (current != null
                   && current.parent != null)
            {
                if (_roots.IsChunkRoot(current)
                    || _roots.IsChunkRoot(current.parent))
                {
                    return false;
                }

                if (IsPartitionRootName(
                        current.parent.name))
                {
                    owner = current;
                    return true;
                }

                current =
                    current.parent;
            }

            return false;
        }

        private bool IsUnderChunkRoot(
            Transform transform)
        {
            for (Transform current = transform;
                 current != null;
                 current = current.parent)
            {
                if (_roots.IsChunkRoot(current))
                    return true;
            }

            return false;
        }

        private bool IsUnderDifferentChunkRoot(
            Transform owner,
            Transform rendererTransform)
        {
            for (Transform current = rendererTransform;
                 current != null
                 && current != owner;
                 current = current.parent)
            {
                if (_roots.IsChunkRoot(current))
                    return true;
            }

            return false;
        }

        private static bool IsPartitionRootName(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            for (int i = 0;
                 i < PartitionRootNames.Length;
                 i++)
            {
                if (string.Equals(
                        name,
                        PartitionRootNames[i],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFiniteBounds(
            Bounds bounds)
        {
            Vector3 center =
                bounds.center;

            Vector3 size =
                bounds.size;

            return IsFinite(center.x)
                && IsFinite(center.y)
                && IsFinite(center.z)
                && IsFinite(size.x)
                && IsFinite(size.y)
                && IsFinite(size.z)
                && size.sqrMagnitude > 0.00000001f;
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value);
        }

        private static void LogOwnerDetail(
            string kind,
            Transform owner,
            Bounds bounds,
            int rendererCount,
            IReadOnlyList<MapChunkCoord> overlaps,
            MapChunkCoord target,
            bool hasTarget)
        {
        }

        private static string BuildPath(
            Transform transform)
        {
            if (transform == null)
                return "<null>";

            var builder =
                new StringBuilder(
                    transform.name);

            Transform current =
                transform.parent;

            while (current != null)
            {
                builder.Insert(
                    0,
                    '/');

                builder.Insert(
                    0,
                    current.name);

                current =
                    current.parent;
            }

            return builder.ToString();
        }

        private static string FormatChunks(
            IReadOnlyList<MapChunkCoord> chunks)
        {
            if (chunks == null
                || chunks.Count == 0)
            {
                return "[]";
            }

            var builder =
                new StringBuilder("[");

            for (int i = 0;
                 i < chunks.Count;
                 i++)
            {
                if (i > 0)
                    builder.Append(',');

                builder.Append(
                    chunks[i]);
            }

            builder.Append(']');
            return builder.ToString();
        }

        private static string FormatBounds(
            Bounds bounds)
        {
            return
                $"center=({F(bounds.center.x)}," +
                $"{F(bounds.center.y)}," +
                $"{F(bounds.center.z)})/" +
                $"size=({F(bounds.size.x)}," +
                $"{F(bounds.size.y)}," +
                $"{F(bounds.size.z)})";
        }

        private static string F(
            float value)
        {
            return value.ToString(
                "0.###",
                CultureInfo.InvariantCulture);
        }
    }
}
