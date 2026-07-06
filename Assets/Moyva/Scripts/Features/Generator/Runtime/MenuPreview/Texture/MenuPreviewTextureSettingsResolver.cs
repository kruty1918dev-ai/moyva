namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewTextureSettingsResolver : IMenuPreviewTextureSettingsResolver
    {
        private MoyvaProjectSettingsSO _runtimeFallback;

        public MoyvaProjectSettingsSO Resolve(MoyvaProjectSettingsSO settings)
        {
            settings ??= _runtimeFallback ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
            settings.Normalize();
            return settings;
        }
    }
}
