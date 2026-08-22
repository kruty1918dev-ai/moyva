using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal static class BotDeterministicGeometry
    {
        internal static List<Vector2Int> BuildRingCandidates(
            Vector2Int center,
            int maxRadius)
        {
            var result = new List<Vector2Int>();
            int radiusLimit = Math.Max(0, maxRadius);
            for (int radius = 1; radius <= radiusLimit; radius++)
            {
                int minX = center.x - radius;
                int maxX = center.x + radius;
                int minY = center.y - radius;
                int maxY = center.y + radius;

                for (int x = minX; x <= maxX; x++)
                    result.Add(new Vector2Int(x, maxY));
                for (int y = maxY - 1; y >= minY; y--)
                    result.Add(new Vector2Int(maxX, y));
                for (int x = maxX - 1; x >= minX; x--)
                    result.Add(new Vector2Int(x, minY));
                for (int y = minY + 1; y < maxY; y++)
                    result.Add(new Vector2Int(minX, y));
            }
            return result;
        }

        internal static int ComparePosition(Vector2Int left, Vector2Int right)
        {
            int x = left.x.CompareTo(right.x);
            return x != 0 ? x : left.y.CompareTo(right.y);
        }

        internal static int StableNoise(
            string ownerId,
            long globalTurn,
            int actionOrdinal,
            string candidateId,
            int magnitude)
        {
            if (magnitude <= 0)
                return 0;

            unchecked
            {
                uint hash = 2166136261u;
                AddStableHash(ref hash, ownerId);
                AddStableHash(ref hash, globalTurn.ToString(System.Globalization.CultureInfo.InvariantCulture));
                AddStableHash(ref hash, actionOrdinal.ToString(System.Globalization.CultureInfo.InvariantCulture));
                AddStableHash(ref hash, candidateId);

                int range = (magnitude * 2) + 1;
                return (int)(hash % (uint)range) - magnitude;
            }
        }
    
        private static void AddStableHash(ref uint hash, string value)
        {
            value ??= string.Empty;
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= 16777619u;
            }
        }
}
}
