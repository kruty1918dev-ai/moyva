using System;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>How the ground-contact point inside the visible bounds is chosen.</summary>
    public enum SupportPointMode
    {
        /// <summary>Horizontal centre of the visible rect's bottom edge.</summary>
        BottomCenter = 0,

        /// <summary>
        /// X centroid of opaque pixels inside the lowest
        /// <see cref="SpriteGroundingProfile.SupportBandRows"/> rows of the
        /// visible rect — follows asymmetric stems/trunks instead of the
        /// geometric centre.
        /// </summary>
        LowestRowCentroid = 1,
    }

    /// <summary>
    /// Immutable parameter set for a grounding analysis. Results are pure
    /// functions of (pixels, profile); two profiles with different values
    /// never share a cache entry.
    /// </summary>
    public readonly struct SpriteGroundingProfile : IEquatable<SpriteGroundingProfile>
    {
        /// <summary>Alpha cutoff matching shader alpha-clip semantics.</summary>
        public readonly float AlphaThreshold;
        /// <summary>Pixel margin kept around the visible rect on every side.</summary>
        public readonly int PaddingPixels;
        /// <summary>Support point strategy.</summary>
        public readonly SupportPointMode Support;
        /// <summary>Rows scanned by <see cref="SupportPointMode.LowestRowCentroid"/>.</summary>
        public readonly int SupportBandRows;

        public SpriteGroundingProfile(
            float alphaThreshold,
            int paddingPixels,
            SupportPointMode support,
            int supportBandRows)
        {
            AlphaThreshold = alphaThreshold;
            PaddingPixels = Math.Max(0, paddingPixels);
            Support = support;
            SupportBandRows = Math.Max(1, supportBandRows);
        }

        public static SpriteGroundingProfile Default =>
            new SpriteGroundingProfile(0.35f, 1, SupportPointMode.LowestRowCentroid, 8);

        public bool Equals(SpriteGroundingProfile other)
            => AlphaThreshold.Equals(other.AlphaThreshold)
               && PaddingPixels == other.PaddingPixels
               && Support == other.Support
               && SupportBandRows == other.SupportBandRows;

        public override bool Equals(object obj)
            => obj is SpriteGroundingProfile other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = AlphaThreshold.GetHashCode();
                hash = (hash * 397) ^ PaddingPixels;
                hash = (hash * 397) ^ (int)Support;
                hash = (hash * 397) ^ SupportBandRows;
                return hash;
            }
        }
    }
}
