using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    internal static class ResolverRegistryAdapterFactory
    {
        private static readonly IResolverRegistryAdapter[] Adapters =
        {
            new BuildingRegistryResolverAdapter(),
        };

        public static IResolverRegistryAdapter Resolve(Object registryAsset)
        {
            if (registryAsset == null)
                return null;

            for (int i = 0; i < Adapters.Length; i++)
            {
                if (Adapters[i].CanHandle(registryAsset))
                    return Adapters[i];
            }

            return null;
        }
    }
}
