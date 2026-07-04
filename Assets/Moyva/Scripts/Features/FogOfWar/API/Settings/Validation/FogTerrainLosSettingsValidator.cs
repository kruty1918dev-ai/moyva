using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation
{
    internal static class FogTerrainLosSettingsValidator
    {
        public static void Normalize(FogOfWarSettings settings)
        {
            settings.ElevationStep = Mathf.Max(0.01f, settings.ElevationStep);
            settings.ObserverHeightBonusPerStep = Mathf.Max(0, settings.ObserverHeightBonusPerStep);
            settings.DownhillVisionBonusPerStep = Mathf.Max(0, settings.DownhillVisionBonusPerStep);
            settings.UphillVisionPenaltyPerStep = Mathf.Max(0, settings.UphillVisionPenaltyPerStep);
            settings.MaxObserverHeightBonus = Mathf.Max(0, settings.MaxObserverHeightBonus);
            settings.MaxDownhillVisionBonus = Mathf.Max(0, settings.MaxDownhillVisionBonus);
            settings.MaxUphillVisionPenalty = Mathf.Max(0, settings.MaxUphillVisionPenalty);
            settings.OcclusionSlopeBias = Mathf.Max(0f, settings.OcclusionSlopeBias);
            settings.TerrainRaySamplesPerTile = Mathf.Clamp(settings.TerrainRaySamplesPerTile, 1, 9);
            settings.TerrainVisibilityThreshold = Mathf.Clamp(settings.TerrainVisibilityThreshold, 0.01f, 1f);
            settings.PartialVisibilityDetectionMultiplier = Mathf.Clamp01(settings.PartialVisibilityDetectionMultiplier);
            settings.TerrainRayStepTiles = Mathf.Clamp(settings.TerrainRayStepTiles, 0.25f, 1f);
            settings.ObserverEyeHeightOffset = Mathf.Max(0f, settings.ObserverEyeHeightOffset);
            settings.TargetSampleHeightOffset = Mathf.Max(0f, settings.TargetSampleHeightOffset);
            settings.TerrainFarSampleDistanceRatio = Mathf.Clamp(settings.TerrainFarSampleDistanceRatio, 0.1f, 1f);
            settings.TerrainVisibilityCacheCapacity = Mathf.Max(0, settings.TerrainVisibilityCacheCapacity);
            settings.TerrainEdgeHeightThreshold = Mathf.Max(0.001f, settings.TerrainEdgeHeightThreshold);
            settings.TerrainEdgePeekDistanceTiles = Mathf.Max(0, settings.TerrainEdgePeekDistanceTiles);
            settings.TerrainEdgeBlindZoneTiles = Mathf.Max(0, settings.TerrainEdgeBlindZoneTiles);
            settings.TerrainEdgeBlindZoneDistanceScale = Mathf.Max(0f, settings.TerrainEdgeBlindZoneDistanceScale);
            settings.TerrainEdgeMaxBlindZoneTiles = Mathf.Max(settings.TerrainEdgeBlindZoneTiles, settings.TerrainEdgeMaxBlindZoneTiles);
            settings.TerrainEdgeUphillPeekStrength = Mathf.Clamp01(settings.TerrainEdgeUphillPeekStrength);
        }
    }
}
