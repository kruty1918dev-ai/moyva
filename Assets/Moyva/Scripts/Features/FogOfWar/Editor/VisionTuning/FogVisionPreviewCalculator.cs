using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Editor
{
    internal static class FogVisionPreviewCalculator
    {
        internal readonly struct Input
        {
            public Input(
                int baseRange,
                int distance,
                float observerHeight,
                float targetHeight,
                bool useBlocker,
                float blockerHeight,
                int blockerDistance)
            {
                BaseRange = Mathf.Max(1, baseRange);
                Distance = Mathf.Max(1, distance);
                ObserverHeight = observerHeight;
                TargetHeight = targetHeight;
                UseBlocker = useBlocker;
                BlockerHeight = blockerHeight;
                BlockerDistance = blockerDistance;
            }

            public int BaseRange { get; }
            public int Distance { get; }
            public float ObserverHeight { get; }
            public float TargetHeight { get; }
            public bool UseBlocker { get; }
            public float BlockerHeight { get; }
            public int BlockerDistance { get; }
        }

        internal readonly struct Settings
        {
            public Settings(
                float elevationStep,
                int maxVisionRange,
                int observerHeightBonusPerStep,
                int downhillVisionBonusPerStep,
                int uphillVisionPenaltyPerStep,
                int maxObserverHeightBonus,
                int maxDownhillVisionBonus,
                int maxUphillVisionPenalty,
                float occlusionSlopeBias)
            {
                ElevationStep = Mathf.Max(0.01f, elevationStep);
                MaxVisionRange = Mathf.Max(1, maxVisionRange);
                ObserverHeightBonusPerStep = Mathf.Max(0, observerHeightBonusPerStep);
                DownhillVisionBonusPerStep = Mathf.Max(0, downhillVisionBonusPerStep);
                UphillVisionPenaltyPerStep = Mathf.Max(0, uphillVisionPenaltyPerStep);
                MaxObserverHeightBonus = Mathf.Max(0, maxObserverHeightBonus);
                MaxDownhillVisionBonus = Mathf.Max(0, maxDownhillVisionBonus);
                MaxUphillVisionPenalty = Mathf.Max(0, maxUphillVisionPenalty);
                OcclusionSlopeBias = Mathf.Max(0f, occlusionSlopeBias);
            }

            public float ElevationStep { get; }
            public int MaxVisionRange { get; }
            public int ObserverHeightBonusPerStep { get; }
            public int DownhillVisionBonusPerStep { get; }
            public int UphillVisionPenaltyPerStep { get; }
            public int MaxObserverHeightBonus { get; }
            public int MaxDownhillVisionBonus { get; }
            public int MaxUphillVisionPenalty { get; }
            public float OcclusionSlopeBias { get; }
        }

        internal readonly struct Result
        {
            public Result(
                int observerBonus,
                int downhillBonus,
                int uphillPenalty,
                int effectiveRange,
                bool inRange,
                bool occluded)
            {
                ObserverBonus = observerBonus;
                DownhillBonus = downhillBonus;
                UphillPenalty = uphillPenalty;
                EffectiveRange = effectiveRange;
                InRange = inRange;
                Occluded = occluded;
            }

            public int ObserverBonus { get; }
            public int DownhillBonus { get; }
            public int UphillPenalty { get; }
            public int EffectiveRange { get; }
            public bool InRange { get; }
            public bool Occluded { get; }
        }

        internal static Result Compute(Input input, Settings settings)
        {
            int observerBonus = Mathf.Min(
                Mathf.FloorToInt(input.ObserverHeight / settings.ElevationStep) * settings.ObserverHeightBonusPerStep,
                settings.MaxObserverHeightBonus);

            float downDelta = input.ObserverHeight - input.TargetHeight;
            int downhillBonus = downDelta > 0f
                ? Mathf.Min(
                    Mathf.CeilToInt(downDelta / settings.ElevationStep) * settings.DownhillVisionBonusPerStep,
                    settings.MaxDownhillVisionBonus)
                : 0;

            float upDelta = input.TargetHeight - input.ObserverHeight;
            int uphillPenalty = upDelta > 0f
                ? Mathf.Min(
                    Mathf.CeilToInt(upDelta / settings.ElevationStep) * settings.UphillVisionPenaltyPerStep,
                    settings.MaxUphillVisionPenalty)
                : 0;

            int effectiveRange = Mathf.Clamp(
                input.BaseRange + observerBonus + downhillBonus - uphillPenalty,
                0,
                settings.MaxVisionRange);
            bool inRange = input.Distance <= effectiveRange;

            bool occluded = false;
            if (input.UseBlocker && input.Distance > 1)
            {
                float targetSlope = (input.TargetHeight - input.ObserverHeight) / input.Distance;
                float sampleSlope = (input.BlockerHeight - input.ObserverHeight) / Mathf.Max(1, input.BlockerDistance);
                occluded = sampleSlope > targetSlope + settings.OcclusionSlopeBias;
            }

            return new Result(observerBonus, downhillBonus, uphillPenalty, effectiveRange, inRange, occluded);
        }
    }
}
