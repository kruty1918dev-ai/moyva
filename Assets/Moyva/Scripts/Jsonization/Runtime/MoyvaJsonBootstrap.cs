using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    public static class MoyvaJsonBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            try
            {
                JsonConfigRuntime.Configure(MoyvaJsonRuntimeSettings.Create());
                JsonConfigRuntime.EnsureLoaded();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    "[JsonConfig] CRITICAL bootstrap failure: " + ex);
                throw;
            }
        }
    }
}
