using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    internal static class BuildingResourceValidationUtility
    {
        public static void ValidateResourceAmounts(
            IReadOnlyList<BuildingResourceAmount> amounts,
            BuildingValidationContext context,
            string label,
            BuildingValidationCollector collector)
        {
            if (amounts == null)
                return;

            for (int i = 0; i < amounts.Count; i++)
            {
                var amount = amounts[i];
                if (amount == null)
                {
                    collector.AddError("RESOURCE_AMOUNT_NULL", $"{label} [{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(amount.ResourceId))
                {
                    collector.AddError("RESOURCE_ID_MISSING", $"{label} [{i}] has no resource ID.");
                }
                else
                {
                    ValidateResourceId(context, amount.ResourceId, $"{label} '{amount.ResourceId}'", collector);
                }

                if (amount.Amount <= 0)
                    collector.AddError("RESOURCE_AMOUNT_INVALID", $"{label} [{i}] has amount <= 0.");
            }
        }

        public static void ValidateResourceId(
            BuildingValidationContext context,
            string resourceId,
            string label,
            BuildingValidationCollector collector)
        {
            if (string.IsNullOrWhiteSpace(resourceId) || context?.ResourceIds == null || context.ResourceIds.Count == 0)
                return;

            if (!context.ResourceIds.Contains(resourceId))
                collector.AddError("RESOURCE_UNKNOWN", $"{label} is not present in the resource database.");
        }
    }
}
