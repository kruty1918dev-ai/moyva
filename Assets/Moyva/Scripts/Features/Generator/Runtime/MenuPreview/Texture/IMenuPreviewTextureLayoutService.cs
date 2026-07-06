using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewTextureLayoutService
    {
        MenuPreviewTextureLayout Create(MenuWorldPreviewTextureBuildRequest request, MoyvaProjectSettingsSO settings);
    }
}
