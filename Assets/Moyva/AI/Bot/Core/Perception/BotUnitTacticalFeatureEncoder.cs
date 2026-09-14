using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    internal static class BotUnitTacticalFeatureEncoder
    {
        public const int UnitRole = 21;
        public const int Purpose = 22;
        public const int BaseVision = 23;
        public const int AttackPower = 24;
        public const int Durability = 25;
        public const int TerrainHeight = 26;
        public const int EffectiveVision = 29;
        public const int HeightAdvantage = 30;
        public const int RangedCombat = 31;

        public static UnitGameplayProfile ResolveProfile(
            IUnitService units,
            IUnitGameplayProfileService profiles,
            string unitId)
        {
            string typeId = units?.GetUnitTypeId(unitId);
            return profiles != null
                ? profiles.GetOrDefault(typeId)
                : default;
        }

        public static int ResolveTerrainLevel(
            IGeneratedTerrainLevelQuery terrain,
            Vector2Int position)
        {
            return terrain != null
                && terrain.TryGetTerrainLevel(position, out int level)
                    ? Mathf.Max(0, level)
                    : 0;
        }

        public static void WriteActorFeatures(
            float[] features,
            UnitGameplayProfile profile,
            int actorTerrainLevel,
            int targetTerrainLevel = 0,
            BotIntentType purpose = BotIntentType.None)
        {
            if (features == null)
                return;

            int effectiveVision = profile.ResolveVisionRange(actorTerrainLevel, 1, 128);
            int attackPower = profile.CuttingDamage + profile.PenetratingDamage + profile.CrushingDamage;
            int defense = profile.CuttingDefense + profile.PenetratingDefense + profile.CrushingDefense;
            features[UnitRole] = Mathf.Clamp01((int)profile.Role / 8f);
            features[Purpose] = Mathf.Clamp01((int)purpose / 12f);
            features[BaseVision] = Normalize(profile.VisionRange, 12f);
            features[AttackPower] = Normalize(attackPower, 30f);
            features[Durability] = Normalize(profile.HitPoints + defense, 60f);
            features[TerrainHeight] = Normalize(actorTerrainLevel, 6f);
            features[EffectiveVision] = Normalize(effectiveVision, 16f);
            features[HeightAdvantage] = Mathf.Clamp((actorTerrainLevel - targetTerrainLevel) / 6f, -1f, 1f);
            features[RangedCombat] = profile.CombatType == UnitCombatType.Ranged ? 1f : 0f;
        }

        private static float Normalize(float value, float scale)
        {
            value = Mathf.Max(0f, value);
            scale = Mathf.Max(1f, scale);
            return value / (value + scale);
        }
    }
}
