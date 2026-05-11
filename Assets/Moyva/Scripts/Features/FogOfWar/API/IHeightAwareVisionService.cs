using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public readonly struct FogVisionModifiers
    {
        public FogVisionModifiers(
            bool canSeeCrest,
            float crestVisibilityFactor,
            float downSlopeVisionBonus,
            float silhouettePenalty)
        {
            HasOverrides = true;
            CanSeeCrest = canSeeCrest;
            CrestVisibilityFactor = Mathf.Clamp01(crestVisibilityFactor);
            DownSlopeVisionBonus = Mathf.Max(0f, downSlopeVisionBonus);
            SilhouettePenalty = Mathf.Clamp01(silhouettePenalty);
        }

        public bool HasOverrides { get; }
        public bool CanSeeCrest { get; }
        public float CrestVisibilityFactor { get; }
        public float DownSlopeVisionBonus { get; }
        public float SilhouettePenalty { get; }

        public bool EffectiveCanSeeCrest => !HasOverrides || CanSeeCrest;
        public float EffectiveCrestVisibilityFactor => HasOverrides ? Mathf.Clamp01(CrestVisibilityFactor) : 1f;
        public float EffectiveDownSlopeVisionBonus => HasOverrides ? Mathf.Max(0f, DownSlopeVisionBonus) : 0f;
        public float EffectiveSilhouettePenalty => HasOverrides ? Mathf.Clamp01(SilhouettePenalty) : 0f;

        public int GetSignature()
        {
            unchecked
            {
                int hash = HasOverrides ? 23 : 17;
                hash = hash * 31 + (EffectiveCanSeeCrest ? 1 : 0);
                hash = hash * 31 + Mathf.RoundToInt(EffectiveCrestVisibilityFactor * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(EffectiveDownSlopeVisionBonus * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(EffectiveSilhouettePenalty * 1000f);
                return hash;
            }
        }
    }

    public interface IHeightAwareVisionService
    {
        void SetHeightMap(float[,] heightMap);
        int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default);
        float GetVisibilityFactor(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default);
        bool IsTargetVisible(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default);
    }
}