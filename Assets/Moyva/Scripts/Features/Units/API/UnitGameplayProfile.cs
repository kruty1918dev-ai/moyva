using Kruty1918.Moyva.Animations.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Runtime-ready projection of unit gameplay fields configured in Unit Designer.
    /// </summary>
    public readonly struct UnitGameplayProfile
    {
        public UnitGameplayProfile(
            string typeId,
            UnitRole role,
            UnitCombatType combatType,
            float baseStamina,
            Vector2 staminaRandomRange,
            int visionRange,
            float visionHeightBoostPerLevel,
            bool canSeeCrest,
            float crestVisibilityFactor,
            float downSlopeVisionBonus,
            float silhouettePenalty,
            int hitPoints,
            int baseLevel,
            int cuttingDamage,
            int penetratingDamage,
            int crushingDamage,
            int cuttingDefense,
            int penetratingDefense,
            int crushingDefense,
            PathAnimationSettings animationSettings)
        {
            TypeId = typeId;
            Role = role;
            CombatType = combatType;
            BaseStamina = Mathf.Max(0f, baseStamina);
            StaminaRandomRange = staminaRandomRange;
            VisionRange = Mathf.Max(1, visionRange);
            VisionHeightBoostPerLevel = Mathf.Max(0f, visionHeightBoostPerLevel);
            CanSeeCrest = canSeeCrest;
            CrestVisibilityFactor = Mathf.Clamp01(crestVisibilityFactor);
            DownSlopeVisionBonus = Mathf.Max(0f, downSlopeVisionBonus);
            SilhouettePenalty = Mathf.Clamp01(silhouettePenalty);
            HitPoints = Mathf.Max(1, hitPoints);
            BaseLevel = Mathf.Max(1, baseLevel);
            CuttingDamage = Mathf.Max(0, cuttingDamage);
            PenetratingDamage = Mathf.Max(0, penetratingDamage);
            CrushingDamage = Mathf.Max(0, crushingDamage);
            CuttingDefense = Mathf.Max(0, cuttingDefense);
            PenetratingDefense = Mathf.Max(0, penetratingDefense);
            CrushingDefense = Mathf.Max(0, crushingDefense);
            AnimationSettings = animationSettings;
        }

        public string TypeId { get; }
        public UnitRole Role { get; }
        public UnitCombatType CombatType { get; }
        public float BaseStamina { get; }
        public Vector2 StaminaRandomRange { get; }
        public int VisionRange { get; }
        public float VisionHeightBoostPerLevel { get; }
        public bool CanSeeCrest { get; }
        public float CrestVisibilityFactor { get; }
        public float DownSlopeVisionBonus { get; }
        public float SilhouettePenalty { get; }
        public int HitPoints { get; }
        public int BaseLevel { get; }
        public int CuttingDamage { get; }
        public int PenetratingDamage { get; }
        public int CrushingDamage { get; }
        public int CuttingDefense { get; }
        public int PenetratingDefense { get; }
        public int CrushingDefense { get; }
        public PathAnimationSettings AnimationSettings { get; }

        public int ResolveVisionRange(int terrainHeightLevel, int minVision = 1, int maxVision = 128)
        {
            int safeLevel = Mathf.Max(0, terrainHeightLevel);
            int bonus = Mathf.RoundToInt(safeLevel * VisionHeightBoostPerLevel);
            return Mathf.Clamp(VisionRange + Mathf.Max(0, bonus), Mathf.Max(1, minVision), Mathf.Max(minVision, maxVision));
        }

        public float RollStartingStamina(float random01)
        {
            float roll = Mathf.Clamp01(random01);
            float randomDelta = Mathf.Lerp(StaminaRandomRange.x, StaminaRandomRange.y, roll);
            return Mathf.Max(0f, BaseStamina + randomDelta);
        }
    }
}