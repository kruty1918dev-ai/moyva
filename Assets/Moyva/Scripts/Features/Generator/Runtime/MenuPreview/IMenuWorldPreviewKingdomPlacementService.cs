using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public interface IMenuWorldPreviewKingdomPlacementService
    {
        MenuWorldPreviewKingdomPlacementReport Apply(
            MenuWorldPreviewData previewData,
            MenuPreviewKingdomPlacementSettings settings);
    }
}
