using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
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

            return NormalizeSeed(context.EditorSeed);
        }

        public Vector2Int ResolveMapSize(IMoyvaTwcGraphBindingContext context)
        {
            if (GameLaunchContext.TryGetWorldDimensions(out int launchWidth, out int launchHeight))
                return ClampMapSize(launchWidth, launchHeight);

            if (context.GraphAsset?.SharedSettings != null && context.GraphAsset.SharedSettings.HasMapSize)
                return ClampMapSize(
                    context.GraphAsset.SharedSettings.MapWidth,
                    context.GraphAsset.SharedSettings.MapHeight);

            var configuration = context.Manager != null ? context.Manager.configuration : null;
            return configuration != null
                ? ClampMapSize(configuration.width, configuration.height)
                : new Vector2Int(1, 1);
        }

        public int NormalizeSeed(int seed)
        {
            return GlobalSeed.Normalize(seed);
        }

        private static Vector2Int ClampMapSize(int width, int height)
        {
            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }
    }
}
