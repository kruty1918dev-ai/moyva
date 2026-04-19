using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public sealed class EconomyMarketPricingService
    {
        public int CalculateUnitPrice(EconomyRulesConfigSO rules, string resourceId, int currentStock, int tradeVolume)
        {
            if (rules == null || rules.Market == null)
                return 1;

            var market = rules.Market;
            var basePrice = ResolveBasePrice(market.ResourceBasePrices, resourceId);

            var targetStock = Mathf.Max(1f, market.TargetStock);
            var referenceVolume = Mathf.Max(1f, market.ReferenceTradeVolume);

            var stockRatio = Mathf.Clamp(currentStock / targetStock, 0.1f, 3f);
            var volumeRatio = Mathf.Clamp(tradeVolume / referenceVolume, 0.1f, 3f);

            var scarcityFactor = Mathf.Pow(1f / stockRatio, Mathf.Max(0f, market.StockExponent));
            var volumeFactor = Mathf.Pow(volumeRatio, Mathf.Max(0f, market.VolumeExponent));

            var multiplier = scarcityFactor * volumeFactor;
            multiplier = Mathf.Clamp(multiplier, Mathf.Max(0.01f, market.MinPriceMultiplier), Mathf.Max(market.MinPriceMultiplier, market.MaxPriceMultiplier));

            return Mathf.Max(1, Mathf.RoundToInt(basePrice * multiplier));
        }

        private static int ResolveBasePrice(IReadOnlyList<EconomyResourceBasePrice> basePrices, string resourceId)
        {
            var normalized = (resourceId ?? string.Empty).Trim();
            if (basePrices != null)
            {
                for (var i = 0; i < basePrices.Count; i++)
                {
                    var entry = basePrices[i];
                    if (entry == null)
                        continue;

                    if (string.Equals((entry.ResourceId ?? string.Empty).Trim(), normalized, StringComparison.OrdinalIgnoreCase))
                        return Mathf.Max(1, entry.BasePrice);
                }
            }

            return 1;
        }
    }
}
