using System;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public enum UnitDamageType
    {
        /// <summary>Ріжуча шкода: мечі, шаблі та інші удари лезом.</summary>
        Cutting = 0,
        /// <summary>Колюча шкода: списи, стріли та інші точкові пробивні удари.</summary>
        Penetrating = 1,
        /// <summary>Дробляча шкода: тарани, катапульти, булави та важкі удари.</summary>
        Crushing = 2,
    }

    public enum UnitCombatOutcome
    {
        Even = 0,
        AttackerAdvantage = 1,
        DefenderAdvantage = 2,
    }

    public readonly struct UnitCombatBreakdown
    {
        public UnitCombatBreakdown(
            int cuttingRaw,
            int penetratingRaw,
            int crushingRaw,
            int cuttingDefense,
            int penetratingDefense,
            int crushingDefense,
            int cuttingEffective,
            int penetratingEffective,
            int crushingEffective,
            float levelMultiplier,
            int totalDamage,
            int defenderHitPoints)
        {
            CuttingRawDamage = cuttingRaw;
            PenetratingRawDamage = penetratingRaw;
            CrushingRawDamage = crushingRaw;
            CuttingDefense = cuttingDefense;
            PenetratingDefense = penetratingDefense;
            CrushingDefense = crushingDefense;
            CuttingEffectiveDamage = cuttingEffective;
            PenetratingEffectiveDamage = penetratingEffective;
            CrushingEffectiveDamage = crushingEffective;
            LevelMultiplier = levelMultiplier;
            TotalDamage = totalDamage;
            DefenderHitPoints = Mathf.Max(1, defenderHitPoints);
            HitsToDefeat = totalDamage > 0 ? Mathf.CeilToInt(DefenderHitPoints / (float)totalDamage) : int.MaxValue;
            DominantDamageType = ResolveDominantDamageType(cuttingEffective, penetratingEffective, crushingEffective);
        }

        public int CuttingRawDamage { get; }
        public int PenetratingRawDamage { get; }
        public int CrushingRawDamage { get; }
        public int CuttingDefense { get; }
        public int PenetratingDefense { get; }
        public int CrushingDefense { get; }
        public int CuttingEffectiveDamage { get; }
        public int PenetratingEffectiveDamage { get; }
        public int CrushingEffectiveDamage { get; }
        public float LevelMultiplier { get; }
        public int TotalDamage { get; }
        public int DefenderHitPoints { get; }
        public int HitsToDefeat { get; }
        public UnitDamageType DominantDamageType { get; }
        public int RawDamageTotal => CuttingRawDamage + PenetratingRawDamage + CrushingRawDamage;
        public int EffectiveDamageBeforeLevel => CuttingEffectiveDamage + PenetratingEffectiveDamage + CrushingEffectiveDamage;
        public int BlockedDamageTotal => Mathf.Max(0, RawDamageTotal - EffectiveDamageBeforeLevel);
        public bool CanDealDamage => RawDamageTotal > 0;

        private static UnitDamageType ResolveDominantDamageType(int cutting, int penetrating, int crushing)
        {
            if (penetrating >= cutting && penetrating >= crushing)
                return UnitDamageType.Penetrating;

            if (crushing >= cutting && crushing >= penetrating)
                return UnitDamageType.Crushing;

            return UnitDamageType.Cutting;
        }
    }

    public readonly struct UnitCombatDuel
    {
        public UnitCombatDuel(UnitCombatBreakdown attackerAttack, UnitCombatBreakdown defenderCounterAttack)
        {
            AttackerAttack = attackerAttack;
            DefenderCounterAttack = defenderCounterAttack;

            if (attackerAttack.HitsToDefeat == defenderCounterAttack.HitsToDefeat)
                Outcome = UnitCombatOutcome.Even;
            else
                Outcome = attackerAttack.HitsToDefeat < defenderCounterAttack.HitsToDefeat
                    ? UnitCombatOutcome.AttackerAdvantage
                    : UnitCombatOutcome.DefenderAdvantage;

            AdvantageScore = defenderCounterAttack.HitsToDefeat - attackerAttack.HitsToDefeat;
        }

        public UnitCombatBreakdown AttackerAttack { get; }
        public UnitCombatBreakdown DefenderCounterAttack { get; }
        public UnitCombatOutcome Outcome { get; }
        public int AdvantageScore { get; }
    }

    public static class UnitCombatCalculator
    {
        public const float LevelMultiplierStep = 0.1f;
        public const float MinLevelMultiplier = 0.5f;
        public const float MaxLevelMultiplier = 1.5f;

        public static UnitCombatBreakdown CalculateAttack(UnitClassConfig attacker, UnitClassConfig defender)
        {
            if (attacker == null)
                throw new ArgumentNullException(nameof(attacker));
            if (defender == null)
                throw new ArgumentNullException(nameof(defender));

            int cuttingRaw = Mathf.Max(0, attacker.CuttingDamage);
            int penetratingRaw = Mathf.Max(0, attacker.PenetratingDamage);
            int crushingRaw = Mathf.Max(0, attacker.CrushingDamage);
            int cuttingDefense = Mathf.Max(0, defender.CuttingDefense);
            int penetratingDefense = Mathf.Max(0, defender.PenetratingDefense);
            int crushingDefense = Mathf.Max(0, defender.CrushingDefense);

            int cuttingEffective = Mathf.Max(0, cuttingRaw - cuttingDefense);
            int penetratingEffective = Mathf.Max(0, penetratingRaw - penetratingDefense);
            int crushingEffective = Mathf.Max(0, crushingRaw - crushingDefense);
            int effectiveBeforeLevel = cuttingEffective + penetratingEffective + crushingEffective;
            float levelMultiplier = CalculateLevelMultiplier(attacker.BaseLevel, defender.BaseLevel);
            int totalDamage = Mathf.RoundToInt(effectiveBeforeLevel * levelMultiplier);

            if (totalDamage <= 0 && cuttingRaw + penetratingRaw + crushingRaw > 0)
                totalDamage = 1;

            return new UnitCombatBreakdown(
                cuttingRaw,
                penetratingRaw,
                crushingRaw,
                cuttingDefense,
                penetratingDefense,
                crushingDefense,
                cuttingEffective,
                penetratingEffective,
                crushingEffective,
                levelMultiplier,
                totalDamage,
                defender.HitPoints);
        }

        public static UnitCombatDuel CalculateDuel(UnitClassConfig attacker, UnitClassConfig defender)
        {
            return new UnitCombatDuel(
                CalculateAttack(attacker, defender),
                CalculateAttack(defender, attacker));
        }

        public static float CalculateLevelMultiplier(int attackerLevel, int defenderLevel)
        {
            int levelDelta = Mathf.Max(1, attackerLevel) - Mathf.Max(1, defenderLevel);
            return Mathf.Clamp(1f + levelDelta * LevelMultiplierStep, MinLevelMultiplier, MaxLevelMultiplier);
        }

        public static int GetDamage(UnitClassConfig config, UnitDamageType type)
        {
            if (config == null)
                return 0;

            return type switch
            {
                UnitDamageType.Cutting => Mathf.Max(0, config.CuttingDamage),
                UnitDamageType.Penetrating => Mathf.Max(0, config.PenetratingDamage),
                UnitDamageType.Crushing => Mathf.Max(0, config.CrushingDamage),
                _ => 0,
            };
        }

        public static int GetDefense(UnitClassConfig config, UnitDamageType type)
        {
            if (config == null)
                return 0;

            return type switch
            {
                UnitDamageType.Cutting => Mathf.Max(0, config.CuttingDefense),
                UnitDamageType.Penetrating => Mathf.Max(0, config.PenetratingDefense),
                UnitDamageType.Crushing => Mathf.Max(0, config.CrushingDefense),
                _ => 0,
            };
        }

        public static string GetDamageTypeLabel(UnitDamageType type)
        {
            return type switch
            {
                UnitDamageType.Cutting => "Ріжуча",
                UnitDamageType.Penetrating => "Колюча",
                UnitDamageType.Crushing => "Дробляча",
                _ => type.ToString(),
            };
        }

        public static string FormatDamageTriplet(UnitClassConfig config)
        {
            if (config == null)
                return "Колюча 0 / Ріжуча 0 / Дробляча 0";

            return $"Колюча {Mathf.Max(0, config.PenetratingDamage)} / Ріжуча {Mathf.Max(0, config.CuttingDamage)} / Дробляча {Mathf.Max(0, config.CrushingDamage)}";
        }

        public static string FormatDefenseTriplet(UnitClassConfig config)
        {
            if (config == null)
                return "Колючий 0 / Ріжучий 0 / Дроблячий 0";

            return $"Колючий {Mathf.Max(0, config.PenetratingDefense)} / Ріжучий {Mathf.Max(0, config.CuttingDefense)} / Дроблячий {Mathf.Max(0, config.CrushingDefense)}";
        }
    }
}