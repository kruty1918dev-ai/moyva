using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Foraging tick: residents gather small amounts of basic resources from the
    /// map each N turns, independent of buildings. Guarantees an owner can always
    /// recover from a zero-resource state instead of being permanently stuck.
    /// Pure logic, no MonoBehaviour; called from <see cref="EconomyTickOrchestrator"/>.
    /// </summary>
    public sealed class EconomyForagingService
    {
        /// <summary>
        /// Applies foraging yields for this turn. Mutates <paramref name="state"/>
        /// in place and returns granted (resourceId, amount) pairs for signalling.
        /// </summary>
        public List<KeyValuePair<string, float>> Tick(
            EconomySettlementState state,
            EconomyRulesConfigSO rules)
        {
            var granted = new List<KeyValuePair<string, float>>();
            var foraging = rules?.Foraging;
            if (state == null || !state.IsActive || foraging == null || !foraging.Enabled)
                return granted;

            int residents = state.Residents?.Count ?? 0;
            if (residents < Math.Max(1, foraging.MinResidents))
                return granted;

            var yields = foraging.Yields;
            if (yields == null || yields.Count == 0 || state.CurrentTurn <= 0)
                return granted;

            for (int index = 0; index < yields.Count; index++)
            {
                var yield = yields[index];
                if (yield == null || string.IsNullOrWhiteSpace(yield.ResourceId))
                    continue;

                int interval = Math.Max(1, yield.IntervalTurns);
                if (state.CurrentTurn % interval != 0)
                    continue;

                float amount = yield.BaseAmount + yield.PerResidentAmount * residents;
                if (amount <= 0f)
                    continue;

                state.AddResource(yield.ResourceId.Trim(), amount);
                granted.Add(new KeyValuePair<string, float>(yield.ResourceId.Trim(), amount));
            }

            return granted;
        }
    }
}
