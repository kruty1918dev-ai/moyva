namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MenuWorldPreviewKingdomPlacementComposition
    {
        public static IMenuWorldPreviewKingdomPlacementService Create()
        {
            var geometry = new MenuPreviewKingdomPlacementGeometry();
            var validator = new MenuPreviewKingdomPlacementValidator(geometry);
            var picker = new MenuPreviewKingdomCandidatePicker(geometry, validator);
            var writer = new MenuPreviewKingdomPlacementWriter();
            var orchestrator = new MenuPreviewKingdomPlacementOrchestrator(picker, writer, geometry);
            return new MenuWorldPreviewKingdomPlacementService(orchestrator);
        }
    }
}
