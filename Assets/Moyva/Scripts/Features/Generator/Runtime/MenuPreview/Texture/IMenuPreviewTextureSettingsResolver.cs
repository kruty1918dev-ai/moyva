using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewTextureSettingsResolver
    {
        MoyvaProjectSettingsSO Resolve(MoyvaProjectSettingsSO settings);
    }
}
