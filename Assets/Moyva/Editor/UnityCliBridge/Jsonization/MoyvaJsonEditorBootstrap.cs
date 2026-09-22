using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Jsonization;
using UnityEditor;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    /// <summary>
    /// Ensures EditMode entry points (tests, audit/export tooling) get the same
    /// JsonConfigRuntime settings that MoyvaJsonBootstrap installs at play time.
    /// </summary>
    internal static class MoyvaJsonEditorBootstrap
    {
        [InitializeOnLoadMethod]
        private static void ConfigureForEditorDomain()
        {
            JsonConfigRuntime.Configure(MoyvaJsonRuntimeSettings.Create());
        }
    }
}
