namespace Kruty1918.Moyva.Construction.Runtime
{
    internal static class ConstructionSettingsResolver
    {
        public static ConstructionResolvedSettings Resolve(ConstructionSceneContext sceneContext)
        {
            if (sceneContext == null)
                return default;

            return new ConstructionResolvedSettings(
                sceneContext.ResolvePlacementRulesProfile(),
                sceneContext.ResolveVisualProfile(),
                sceneContext.ResolveInputProfile(),
                sceneContext.ResolveWallProfile(),
                sceneContext.ResolveDiagnosticsProfile());
        }
    }
}
