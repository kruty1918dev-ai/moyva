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
                MoyvaJsonRuntime.EnsureLoaded();

                if (Application.isEditor || Debug.isDebugBuild)
                {
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    "[MoyvaJson] CRITICAL bootstrap failure: " + ex);
                throw;
            }
        }
    }
}
