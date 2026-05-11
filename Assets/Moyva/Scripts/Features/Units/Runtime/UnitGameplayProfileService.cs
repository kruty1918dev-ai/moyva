using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitGameplayProfileService : IUnitGameplayProfileService
    {
        private readonly IUnitClassConfig _unitClassConfig;

        public UnitGameplayProfileService(IUnitClassConfig unitClassConfig)
        {
            _unitClassConfig = unitClassConfig;
        }

        public bool TryGetProfile(string typeId, out UnitGameplayProfile profile)
        {
            var config = _unitClassConfig?.GetConfig(typeId);
            if (config == null)
            {
                profile = default;
                return false;
            }

            profile = new UnitGameplayProfile(
                config.TypeId,
                config.Role,
                config.CombatType,
                config.BaseStamina,
                config.StaminaRandomRange,
                config.VisionRange,
                config.VisionHeightBoostPerLevel,
                config.CanSeeCrest,
                config.CrestVisibilityFactor,
                config.DownSlopeVisionBonus,
                config.SilhouettePenalty,
                config.HitPoints,
                config.BaseLevel,
                config.CuttingDamage,
                config.PenetratingDamage,
                config.CrushingDamage,
                config.CuttingDefense,
                config.PenetratingDefense,
                config.CrushingDefense,
                config.AnimationSettings);
            return true;
        }

        public UnitGameplayProfile GetOrDefault(string typeId)
        {
            if (TryGetProfile(typeId, out var profile))
                return profile;

            return new UnitGameplayProfile(
                typeId,
                UnitRole.Worker,
                UnitCombatType.Infantry,
                0f,
                Vector2.zero,
                1,
                0f,
                true,
                1f,
                0f,
                0f,
                1,
                1,
                0,
                0,
                0,
                0,
                0,
                0,
                PathAnimationSettings.Default);
        }

        public int ResolveVisionRange(string typeId, int terrainHeightLevel = 0, int minVision = 1, int maxVision = 128)
        {
            var profile = GetOrDefault(typeId);
            return profile.ResolveVisionRange(terrainHeightLevel, minVision, maxVision);
        }

        public float RollStartingStamina(string typeId)
        {
            var profile = GetOrDefault(typeId);
            return profile.RollStartingStamina(Random.value);
        }

        public PathAnimationSettings ResolveMovementAnimationSettings(string typeId)
        {
            var profile = GetOrDefault(typeId);
            return profile.AnimationSettings;
        }
    }
}