#if UNITY_EDITOR
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Editor
{
    internal static class ConstructionProfileValidator
    {
        public static IReadOnlyList<string> Validate(ConstructionSystemProfileSO profile)
        {
            var issues = new List<string>();
            if (profile == null)
            {
                issues.Add("ConstructionSystemProfileSO is missing.");
                return issues;
            }

            if (profile.BuildingRegistry == null)
                issues.Add("BuildingRegistry is missing.");
            if (profile.PlacementRulesProfile == null)
                issues.Add("PlacementRulesProfile is missing.");
            if (profile.VisualProfile == null)
                issues.Add("VisualProfile is missing.");
            if (profile.InputProfile == null)
                issues.Add("InputProfile is missing.");
            if (profile.WallProfile == null)
                issues.Add("WallProfile is missing.");
            if (profile.DiagnosticsProfile == null)
                issues.Add("DiagnosticsProfile is missing.");
            if (profile.EconomyRulesProfile == null)
                issues.Add("EconomyRulesProfile is missing.");
            if (profile.FogOfWarSettings == null)
                issues.Add("FogOfWarSettings is missing.");

            return issues;
        }
    }
}
#endif
