using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.BotAI.API;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal static class BotReasoningNarrator
    {
        public static string DescribeSite(
            BotSiteEvaluation evaluation)
        {
            if (evaluation == null)
                return "Немає даних для оцінки позиції.";

            var builder = new StringBuilder();
            builder.Append($"Клітинка {evaluation.Cell}: підсумкова оцінка {evaluation.TotalScore}. ");
            builder.Append(evaluation.Summary);

            IReadOnlyList<BotSiteScoreFactor> factors = evaluation.Factors;
            if (factors == null || factors.Count == 0)
                return builder.ToString();

            var ranked = new List<BotSiteScoreFactor>(factors);
            ranked.Sort((left, right) =>
            {
                int magnitude = Math.Abs(right.Contribution)
                    .CompareTo(Math.Abs(left.Contribution));
                return magnitude != 0
                    ? magnitude
                    : string.CompareOrdinal(left.Key, right.Key);
            });

            int take = Math.Min(5, ranked.Count);
            builder.Append(" Найважливіші фактори: ");
            for (int i = 0; i < take; i++)
            {
                BotSiteScoreFactor factor = ranked[i];
                if (i > 0)
                    builder.Append("; ");

                builder
                    .Append(factor.Label)
                    .Append(' ')
                    .Append(factor.Contribution >= 0 ? "+" : string.Empty)
                    .Append(factor.Contribution);
            }
            builder.Append('.');

            return builder.ToString();
        }

        public static string DescribeCandidate(
            BotActionCandidate candidate,
            int rank,
            int totalCandidates)
        {
            string target = candidate.TargetCell.HasValue
                ? $" цільова клітинка {candidate.TargetCell.Value}"
                : !string.IsNullOrWhiteSpace(candidate.TargetId)
                    ? $" ціль '{candidate.TargetId}'"
                    : string.Empty;

            return
                $"Кандидат #{rank}/{Math.Max(1, totalCandidates)}: {candidate.Kind}," +
                $"{target}, score={candidate.Score.Total}. " +
                $"Причина: {Fallback(candidate.Reason, candidate.Score.Explanation)}";
        }

        public static string DescribeStrategy(BotStrategicContext strategy)
            => $"Стратегічний режим: {strategy.Posture}, score={strategy.PostureScore}. " +
               $"Причина: {Fallback(strategy.Reason, "немає додаткового пояснення")}";

        public static string DescribeActionResult(
            BotActionCandidate candidate,
            BotActionExecutionResult result)
        {
            if (result.Succeeded && result.Mutated)
                return $"Дію {candidate.Kind} виконано успішно; стан світу змінено.";

            if (result.Succeeded)
                return $"Дію {candidate.Kind} оброблено без мутації світу.";

            return $"Дію {candidate.Kind} відхилено. Причина: {Fallback(result.Reason, "невідома")}";
        }

        private static string Fallback(string primary, string fallback)
            => !string.IsNullOrWhiteSpace(primary)
                ? primary.Trim()
                : !string.IsNullOrWhiteSpace(fallback)
                    ? fallback.Trim()
                    : string.Empty;
    }
}
