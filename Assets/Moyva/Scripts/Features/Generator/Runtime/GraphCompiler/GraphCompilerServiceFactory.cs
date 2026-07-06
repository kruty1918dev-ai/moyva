namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphCompilerServiceFactory
    {
        public static IGraphToConfigurationCompilerService CreateDefault()
        {
            var diagnostics = new GraphCompilerDiagnosticsService();
            var buildLayerLookup = new GraphCompilerTileBuildLayerLookup();
            var contextFactory = new GraphCompilerRuntimeContextFactory();
            var maskUtility = new GraphCompilerMaskUtility();
            var maskService = new GraphCompilerPrecomputedMaskService(contextFactory, maskUtility);
            var blueprintService = new GraphCompilerBlueprintSyncService(buildLayerLookup);
            var buildLayerService = new GraphCompilerTileBuildLayerSyncService(buildLayerLookup);
            var modifierService = new GraphCompilerModifierService(maskUtility);
            var objectService = new GraphCompilerObjectPlacementService(contextFactory);
            var configService = new GraphCompilerConfigurationService();

            return new GraphToConfigurationCompilerService(
                configService,
                diagnostics,
                blueprintService,
                buildLayerService,
                maskService,
                modifierService,
                objectService);
        }
    }
}
