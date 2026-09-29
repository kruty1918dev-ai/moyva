using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>Frozen analysis output for one (source, profile) pair.</summary>
    public readonly struct SpriteGroundingResult
    {
        public SpriteGroundingResult(VisibleBounds bounds, Vector2 supportUv)
        {
            Bounds = bounds;
            SupportUv = supportUv;
        }

        public readonly VisibleBounds Bounds;
        public readonly Vector2 SupportUv;
        public bool HasContent => Bounds.HasContent;
    }

    /// <summary>
    /// Keyed cache of grounding analyses. The key is
    /// (caller-supplied source id, profile hash) — typically
    /// <see cref="Object.GetEntityId"/> of the texture, or a stable
    /// content hash. Analysis is a pure function, so cached entries are
    /// valid across rebuilds; invalidate explicitly when inputs change.
    /// </summary>
    public sealed class SpriteGroundingCache
    {
        private readonly Dictionary<(EntityId sourceKey, int profileHash), SpriteGroundingResult> _results =
            new Dictionary<(EntityId, int), SpriteGroundingResult>();

        public int Count => _results.Count;

        public bool TryGet(
            EntityId sourceKey, in SpriteGroundingProfile profile, out SpriteGroundingResult result)
            => _results.TryGetValue((sourceKey, profile.GetHashCode()), out result);

        /// <summary>
        /// Reads the source once, computes visible bounds and the support
        /// point, and stores the result. Subsequent calls with the same key
        /// hit the cache; a source that cannot be read caches a
        /// <see cref="VisibleBounds.Empty"/> result.
        /// </summary>
        public SpriteGroundingResult GetOrAdd(
            EntityId sourceKey,
            IPixelSource source,
            in SpriteGroundingProfile profile,
            Color32[] buffer = null)
        {
            var key = (sourceKey, profile.GetHashCode());
            if (_results.TryGetValue(key, out SpriteGroundingResult cached))
                return cached;

            SpriteGroundingResult result = Compute(source, profile, buffer);
            _results[key] = result;
            return result;
        }

        /// <summary>Removes every profile variant stored for a source.</summary>
        public int Invalidate(EntityId sourceKey)
        {
            var remove = new List<(EntityId, int)>();
            foreach (var key in _results.Keys)
                if (key.sourceKey == sourceKey)
                    remove.Add(key);
            foreach (var key in remove)
                _results.Remove(key);
            return remove.Count;
        }

        public void Clear() => _results.Clear();

        private static SpriteGroundingResult Compute(
            IPixelSource source, in SpriteGroundingProfile profile, Color32[] buffer)
        {
            if (source == null || source.Width <= 0 || source.Height <= 0)
                return new SpriteGroundingResult(VisibleBounds.Empty, new Vector2(0.5f, 0f));

            int size = source.Width * source.Height;
            if (buffer == null || buffer.Length < size)
                buffer = new Color32[size];
            if (!source.TryCopyPixels(buffer))
                return new SpriteGroundingResult(VisibleBounds.Empty, new Vector2(0.5f, 0f));

            VisibleBounds bounds = AlphaBoundsAnalyzer.AnalyzePixels(
                buffer, source.Width, source.Height,
                profile.AlphaThreshold, profile.PaddingPixels);
            Vector2 supportUv = SupportPointResolver.SolveUv(
                bounds, buffer, source.Width, source.Height,
                profile.AlphaThreshold, profile);
            return new SpriteGroundingResult(bounds, supportUv);
        }
    }
}
