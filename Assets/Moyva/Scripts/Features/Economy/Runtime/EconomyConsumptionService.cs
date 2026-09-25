using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyConsumptionProfile
    {
        public EconomyConsumptionProfile(float foodPerTurn, float waterPerTurn, float firewoodPerTurn, float clothingPerTurn)
        {
            FoodPerTurn = foodPerTurn;
            WaterPerTurn = waterPerTurn;
            FirewoodPerTurn = firewoodPerTurn;
            ClothingPerTurn = clothingPerTurn;
        }

        public float FoodPerTurn { get; }
        public float WaterPerTurn { get; }
        public float FirewoodPerTurn { get; }
        public float ClothingPerTurn { get; }
    }

    public sealed class EconomyConsumptionService
    {
        /// <summary>
        /// Resolves which concrete resources satisfy an abstract need id
        /// ("Food", "Water", ...). A need is modeled when the database contains
        /// either a resource with that exact id or resources whose
        /// <see cref="EconomyResourceCategory"/> name matches the need id.
        /// </summary>
        public List<string> ResolveNeedResourceIds(EconomyDatabaseSO database, string needId)
        {
            var result = new List<string>();
            if (database?.Resources == null || string.IsNullOrWhiteSpace(needId))
                return result;

            string trimmed = needId.Trim();
            EconomyResourceCategory? category =
                !char.IsDigit(trimmed[0])
                && Enum.TryParse(trimmed, true, out EconomyResourceCategory parsed)
                    ? parsed
                    : (EconomyResourceCategory?)null;

            for (int index = 0; index < database.Resources.Count; index++)
            {
                var resource = database.Resources[index];
                if (resource == null || string.IsNullOrWhiteSpace(resource.Id))
                    continue;

                if (string.Equals(resource.Id, trimmed, StringComparison.Ordinal)
                    || (category.HasValue && resource.Category == category.Value))
                {
                    result.Add(resource.Id);
                }
            }

            return result;
        }

        /// <summary>Total spendable amount across all resources that satisfy the need.</summary>
        public float GetAvailableNeedAmount(
            EconomySettlementState state,
            EconomyDatabaseSO database,
            string needId)
        {
            var ids = ResolveNeedResourceIds(database, needId);
            float total = 0f;
            for (int index = 0; index < ids.Count; index++)
                total += state.GetAvailableResource(ids[index]);
            return total;
        }

        /// <summary>
        /// True when the need is modeled by the resource set but the pool is empty.
        /// Unmodeled needs never report a deficit.
        /// </summary>
        public bool HasNeedDeficit(
            EconomySettlementState state,
            EconomyDatabaseSO database,
            string needId)
        {
            return ResolveNeedResourceIds(database, needId).Count > 0
                && GetAvailableNeedAmount(state, database, needId) <= 0f;
        }

        /// <summary>
        /// Consumes <paramref name="amount"/> across all resources satisfying the
        /// need (all-or-nothing, like <see cref="EconomySettlementState.ConsumeResource"/>).
        /// A need not modeled by the resource database is vacuously satisfied.
        /// </summary>
        public bool ConsumeNeed(
            EconomySettlementState state,
            EconomyDatabaseSO database,
            string needId,
            float amount)
        {
            if (amount <= 0f)
                return true;

            var ids = ResolveNeedResourceIds(database, needId);
            if (ids.Count == 0)
                return true;

            if (GetAvailableNeedAmount(state, database, needId) + 0.0001f < amount)
                return false;

            float remaining = amount;
            for (int index = 0; index < ids.Count && remaining > 0.0001f; index++)
            {
                float available = state.GetAvailableResource(ids[index]);
                if (available <= 0f)
                    continue;

                float take = Math.Min(available, remaining);
                if (state.ConsumeResource(ids[index], take))
                    remaining -= take;
            }

            return remaining <= 0.0001f;
        }

        public EconomyConsumptionProfile ResolveConsumption(EconomyRulesConfigSO rules, int age)
        {
            var ageRules = rules?.Consumption?.AgeConsumption;
            if (ageRules != null)
            {
                for (var i = 0; i < ageRules.Count; i++)
                {
                    var rule = ageRules[i];
                    if (rule == null)
                        continue;

                    if (age >= rule.MinAge && age <= rule.MaxAge)
                        return new EconomyConsumptionProfile(rule.FoodPerTurn, rule.WaterPerTurn, rule.FirewoodPerTurn, rule.ClothingPerTurn);
                }
            }

            return new EconomyConsumptionProfile(1f, 1f, 0.5f, 0.25f);
        }
    }
}
