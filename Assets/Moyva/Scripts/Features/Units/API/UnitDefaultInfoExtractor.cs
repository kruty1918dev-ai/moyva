using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public static class UnitDefaultInfoExtractor
    {
        public static bool AppendMeaningfulFacts(UnitClassConfig config, StringBuilder output)
        {
            if (config == null || output == null)
                return false;

            int startLength = output.Length;

            if (!string.IsNullOrWhiteSpace(config.TypeId))
                output.AppendLine($"TypeId: {config.TypeId}");

            output.AppendLine(config.Role == UnitRole.Military
                ? "Прапорець: бойовий юніт"
                : "Прапорець: економічний юніт");

            if (config.BaseStamina > 0f)
                output.AppendLine($"Базова стаміна: {config.BaseStamina:0.#}");

            if (config.VisionRange > 0)
                output.AppendLine($"Дальність огляду: {config.VisionRange}");

            if (config.HitPoints > 0)
                output.AppendLine($"HP: {config.HitPoints}");

            output.AppendLine($"Тип бою: {config.CombatType}");

            int totalDamage = config.CuttingDamage + config.PenetratingDamage + config.CrushingDamage;
            if (totalDamage > 0)
                output.AppendLine($"Шкода: {UnitCombatCalculator.FormatDamageTriplet(config)}");

            int totalDefense = config.CuttingDefense + config.PenetratingDefense + config.CrushingDefense;
            if (totalDefense > 0)
                output.AppendLine($"Захист: {UnitCombatCalculator.FormatDefenseTriplet(config)}");

            if (config.StaminaRandomRange != Vector2.zero)
                output.AppendLine($"Рандом стаміни: {config.StaminaRandomRange.x:0.#} .. {config.StaminaRandomRange.y:0.#}");

            if (config.Prefab != null)
                output.AppendLine("Прапорець: має prefab");

            return output.Length > startLength;
        }
    }
}