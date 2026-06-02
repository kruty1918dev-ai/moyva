using System;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class DisabledMapDataGenerator : IMapDataGenerator
    {
        public void GenerateMapData(int width, int height, Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);

            Debug.LogError("[Generator] Legacy map generation is disabled. Configure GraphAsset + TileWorldCreatorManager and enable TWC graph pipeline.");

            onComplete?.Invoke(
                new string[safeWidth, safeHeight],
                new string[safeWidth, safeHeight],
                new float[safeWidth, safeHeight],
                new string[safeWidth, safeHeight]);
        }
    }
}
