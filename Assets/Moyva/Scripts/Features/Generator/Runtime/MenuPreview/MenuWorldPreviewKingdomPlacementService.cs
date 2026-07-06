using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuWorldPreviewKingdomPlacementService : IMenuWorldPreviewKingdomPlacementService
    {
        private readonly IMenuPreviewKingdomPlacementOrchestrator _orchestrator;

        public MenuWorldPreviewKingdomPlacementService(IMenuPreviewKingdomPlacementOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public MenuWorldPreviewKingdomPlacementReport Apply(
            MenuWorldPreviewData previewData,
            MenuPreviewKingdomPlacementSettings settings)
        {
            var report = new MenuWorldPreviewKingdomPlacementReport();

            if (previewData == null)
            {
                report.Warning = "Preview data is null.";
                return report;
            }

            if (settings == null || !settings.Enabled)
                return report;

            settings.ClampAndNormalize();
            var context = new MenuWorldPreviewKingdomPlacementContext(previewData, settings, report);
            _orchestrator.Run(context);
            return report;
        }
    }
}
