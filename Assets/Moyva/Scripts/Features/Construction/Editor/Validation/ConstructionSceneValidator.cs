#if UNITY_EDITOR
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;

namespace Kruty1918.Moyva.Construction.Editor
{
    internal static class ConstructionSceneValidator
    {
        public static IReadOnlyList<string> Validate(ConstructionSceneContext sceneContext)
        {
            var issues = new List<string>();
            if (sceneContext == null)
            {
                issues.Add("ConstructionSceneContext is missing.");
                return issues;
            }

            if (sceneContext.SystemProfile == null)
                issues.Add("System profile is not assigned.");
            if (sceneContext.BuildingRegistry == null)
                issues.Add("Building registry is not assigned.");
            if (sceneContext.SystemProfile != null && sceneContext.SystemProfile.BuildingRegistry != null
                && sceneContext.BuildingRegistry != null
                && sceneContext.SystemProfile.BuildingRegistry != sceneContext.BuildingRegistry)
            {
                issues.Add("Scene registry differs from system profile registry.");
            }
            if (sceneContext.SystemProfile != null && sceneContext.SystemProfile.EconomyRulesProfile == null)
                issues.Add("System profile economy rules are not assigned.");
            if (sceneContext.SystemProfile != null && sceneContext.SystemProfile.FogOfWarSettings == null)
                issues.Add("System profile fog settings are not assigned.");
            if (sceneContext.ResolvePlacementRulesProfile() == null)
                issues.Add("Placement rules profile is not resolved.");
            if (sceneContext.ResolveVisualProfile() == null)
                issues.Add("Visual profile is not resolved.");
            if (sceneContext.ResolveInputProfile() == null)
                issues.Add("Input profile is not resolved.");
            if (sceneContext.ResolveWallProfile() == null)
                issues.Add("Wall profile is not resolved.");
            if (sceneContext.ResolveDiagnosticsProfile() == null)
                issues.Add("Diagnostics profile is not resolved.");
            if (sceneContext.SceneRoots == null || sceneContext.SceneRoots.PreviewRoot == null)
                issues.Add("Preview root is not assigned.");
            if (sceneContext.SceneRoots == null || sceneContext.SceneRoots.PlacedRoot == null)
                issues.Add("Placed root is not assigned.");
            if (sceneContext.SceneRoots == null || sceneContext.SceneRoots.RadiusRoot == null)
                issues.Add("Radius root is not assigned.");
            if (sceneContext.SceneRoots == null || sceneContext.SceneRoots.UIRoot == null)
                issues.Add("UI root is not assigned.");
            if (sceneContext.SceneRoots == null || sceneContext.SceneRoots.DebugRoot == null)
                issues.Add("Debug root is not assigned.");

            return issues;
        }
    }
}
#endif
