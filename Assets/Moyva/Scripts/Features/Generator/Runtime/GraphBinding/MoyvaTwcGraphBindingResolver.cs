using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphBindingResolver : IMoyvaTwcGraphBindingResolver
    {
        public int ResolveSeed(IMoyvaTwcGraphBindingContext context)
        {
            if (GameLaunchContext.TryGetSeed(out int launchSeed))
                return NormalizeSeed(launchSeed);

            if (context.GraphAsset?.Nodes != null)
            {
                foreach (var node in context.GraphAsset.Nodes)
                {
                    if (node is ISeedProvider seedProvider)
                        return NormalizeSeed(seedProvider.Seed);
                }
            }

            return context.EditorSeed;
        }

        public Vector2Int ResolveMapSize(IMoyvaTwcGraphBindingContext context)
        {
            if (context.GraphAsset?.SharedSettings != null && context.GraphAsset.SharedSettings.HasMapSize)
                return context.GraphAsset.SharedSettings.MapSize;

            var configuration = context.Manager != null ? context.Manager.configuration : null;
            return configuration != null
                ? new Vector2Int(Mathf.Max(1, configuration.width), Mathf.Max(1, configuration.height))
                : new Vector2Int(1, 1);
        }

        public int NormalizeSeed(int seed)
        {
            return seed == 0 ? 1 : seed;
        }
    }
}
