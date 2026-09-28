using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.SpriteGrounding;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Boundary adapter between the SpriteGrounding package and the
    /// generator. For a prop prefab built from alpha-carded quads it
    /// produces a <see cref="CardFrame"/>: the prefab-local bounds of the
    /// *visible* content (not the full quads) and the support point where
    /// the card's silhouette roots in the ground. Placement code stays the
    /// single authority on where props go — this service only supplies the
    /// corrected local frame.
    ///
    /// Analysis results are cached per texture and profile; refit meshes
    /// are created once per card mesh and owned by this service. Frame
    /// building is a pure function of the prefab's texture content, so the
    /// same seed and assets always yield the same corrections.
    /// </summary>
    internal sealed class SpriteCardGroundingService : IDisposable
    {
        /// <summary>Source mesh → runtime bounds-refit copy.</summary>
        public readonly struct MeshRefit
        {
            public MeshRefit(Mesh source, Mesh refit)
            {
                Source = source;
                Refit = refit;
            }

            public readonly Mesh Source;
            public readonly Mesh Refit;
        }

        /// <summary>Prefab-local visible frame of a card prop.</summary>
        public readonly struct CardFrame
        {
            public CardFrame(Bounds visibleLocalBounds, Vector3 supportLocal, MeshRefit[] refits)
            {
                VisibleLocalBounds = visibleLocalBounds;
                SupportLocal = supportLocal;
                Refits = refits;
            }

            /// <summary>Combined visible bounds in prefab-root space.</summary>
            public readonly Bounds VisibleLocalBounds;
            /// <summary>Prefab-root-local support (ground contact) point.</summary>
            public readonly Vector3 SupportLocal;
            /// <summary>Bounds-refit mesh copies to swap in, or null.</summary>
            public readonly MeshRefit[] Refits;
        }

        private readonly SpriteGroundingRules _rules;
        private readonly SpriteGroundingCache _cache = new SpriteGroundingCache();
        private readonly Dictionary<GameObject, CardFrame?> _frames =
            new Dictionary<GameObject, CardFrame?>();
        private readonly List<Mesh> _ownedMeshes = new List<Mesh>();
        private readonly List<CardQuad> _quads = new List<CardQuad>(8);
        private int _buildEpoch;

        public SpriteCardGroundingService(
            [Zenject.InjectOptional] EnvironmentDecorationConfig config = null)
        {
            _rules = config?.SpriteGrounding ?? new SpriteGroundingRules();
        }

        public bool Enabled => _rules != null && _rules.Enabled;
        public float SinkMeters => _rules != null ? _rules.SinkMeters : 0f;

        /// <summary>Marks a new world build; guards future async passes.</summary>
        public int BeginBuild() => ++_buildEpoch;
        public int BuildEpoch => _buildEpoch;

        /// <summary>
        /// Batch pass over the prefab set before spawning starts: every
        /// unique texture is analyzed once, so the spawn loop itself only
        /// does dictionary lookups.
        /// </summary>
        public int Prewarm(IEnumerable<GameObject> prefabs)
        {
            if (!Enabled || prefabs == null)
                return 0;

            int built = 0;
            foreach (GameObject prefab in prefabs)
            {
                if (prefab == null || _frames.ContainsKey(prefab))
                    continue;
                _frames[prefab] = BuildFrame(prefab);
                if (_frames[prefab].HasValue)
                    built++;
            }
            return built;
        }

        /// <summary>
        /// Returns the card frame for a prop prefab, computing it lazily
        /// (and caching) when the prefab was not prewarmed. False for
        /// non-card prefabs — callers keep their authored bounds then.
        /// </summary>
        public bool TryGetFrame(GameObject prefab, out CardFrame frame)
        {
            frame = default;
            if (!Enabled || prefab == null)
                return false;
            if (!_frames.TryGetValue(prefab, out CardFrame? cached))
            {
                cached = BuildFrame(prefab);
                _frames[prefab] = cached;
            }
            if (!cached.HasValue)
                return false;
            frame = cached.Value;
            return true;
        }

        /// <summary>
        /// Destroys service-owned runtime meshes and drops all cached
        /// analyses. Called when the profile or source assets may have
        /// changed; normal world rebuilds keep the cache since results are
        /// content-identical.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _ownedMeshes.Count; i++)
            {
                Mesh mesh = _ownedMeshes[i];
                if (mesh == null)
                    continue;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(mesh);
                else
                    UnityEngine.Object.DestroyImmediate(mesh);
            }
            _ownedMeshes.Clear();
            _frames.Clear();
            _cache.Clear();
        }

        /// <summary>
        /// Swaps a spawned instance's card meshes for their bounds-refit
        /// copies. Shared per-source-mesh — O(1) per renderer.
        /// </summary>
        public void ApplyRefitMeshes(GameObject instance, in CardFrame frame)
        {
            if (instance == null || frame.Refits == null)
                return;

            var filters = instance.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter filter in filters)
            {
                if (filter == null)
                    continue;
                for (int i = 0; i < frame.Refits.Length; i++)
                {
                    if (filter.sharedMesh == frame.Refits[i].Source)
                    {
                        filter.sharedMesh = frame.Refits[i].Refit;
                        break;
                    }
                }
            }
        }

        public void Dispose() => Clear();

        private CardFrame? BuildFrame(GameObject prefab)
        {
            var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            if (filters == null || filters.Length == 0)
                return null;

            Matrix4x4 rootInverse = prefab.transform.worldToLocalMatrix;
            Bounds combined = default;
            bool hasBounds = false;
            Vector3 supportSum = Vector3.zero;
            int supportCount = 0;
            List<MeshRefit> refits = null;

            foreach (MeshFilter filter in filters)
            {
                if (filter == null)
                    continue;

                var renderer = filter.GetComponent<MeshRenderer>();
                Matrix4x4 toRoot = rootInverse * filter.transform.localToWorldMatrix;
                Mesh mesh = filter.sharedMesh;

                _quads.Clear();
                bool isCard = mesh != null
                              && QuadCardProjector.ExtractQuads(mesh, _quads) > 0;
                if (!isCard)
                {
                    // Non-card parts still contribute authored bounds.
                    if (renderer != null)
                        EncapsulateTransform(renderer.localBounds, toRoot, ref combined, ref hasBounds);
                    continue;
                }

                if (!TryResolveCardTexture(renderer, out Texture2D texture, out float alphaThreshold))
                {
                    EncapsulateTransform(mesh.bounds, toRoot, ref combined, ref hasBounds);
                    continue;
                }

                var profile = new SpriteGroundingProfile(
                    alphaThreshold,
                    _rules.PaddingPixels,
                    _rules.SupportMode,
                    _rules.SupportBandRows);
                SpriteGroundingResult result = _cache.GetOrAdd(
                    texture.GetInstanceID(), PixelSource.ForTexture(texture), profile);
                if (!result.HasContent)
                {
                    EncapsulateTransform(mesh.bounds, toRoot, ref combined, ref hasBounds);
                    continue;
                }

                Bounds localVisible = QuadCardProjector.ProjectLocalBounds(_quads, result.Bounds.Uv);
                EncapsulateTransform(localVisible, toRoot, ref combined, ref hasBounds);
                supportSum += toRoot.MultiplyPoint3x4(
                    QuadCardProjector.ProjectPoint(_quads, result.SupportUv));
                supportCount++;

                if (_rules.RefitMeshBounds)
                {
                    Mesh refit = CardMeshTrimmer.CreateBoundsRefitCopy(mesh, localVisible);
                    if (refit != null)
                    {
                        _ownedMeshes.Add(refit);
                        (refits ??= new List<MeshRefit>()).Add(new MeshRefit(mesh, refit));
                    }
                }
            }

            if (supportCount == 0)
                return null;
            if (!hasBounds)
                combined = new Bounds(Vector3.zero, Vector3.one * 0.1f);

            return new CardFrame(combined, supportSum / supportCount, refits?.ToArray());
        }

        /// <summary>
        /// Card texture and alpha cutoff for a renderer. The material's
        /// _AlphaClipThreshold wins over the configured default so analysis
        /// matches the actual shader clip.
        /// </summary>
        private bool TryResolveCardTexture(
            MeshRenderer renderer, out Texture2D texture, out float alphaThreshold)
        {
            texture = null;
            alphaThreshold = _rules.AlphaThreshold;
            if (renderer == null)
                return false;

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null || !material.HasProperty("_BaseMap"))
                    continue;
                if (material.HasProperty("_AlphaClipThreshold"))
                    alphaThreshold = material.GetFloat("_AlphaClipThreshold");
                if (material.GetTexture("_BaseMap") is Texture2D tex && tex != null)
                {
                    texture = tex;
                    return true;
                }
            }
            return false;
        }

        private static void EncapsulateTransform(
            Bounds local, Matrix4x4 toRoot, ref Bounds combined, ref bool hasBounds)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = local.center + Vector3.Scale(
                    local.extents,
                    new Vector3(
                        (i & 1) == 0 ? -1f : 1f,
                        (i & 2) == 0 ? -1f : 1f,
                        (i & 4) == 0 ? -1f : 1f));
                Vector3 rootSpace = toRoot.MultiplyPoint3x4(corner);
                if (!hasBounds)
                {
                    combined = new Bounds(rootSpace, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(rootSpace);
                }
            }
        }
    }
}
