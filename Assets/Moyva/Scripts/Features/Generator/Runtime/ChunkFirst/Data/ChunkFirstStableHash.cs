using Unity.Mathematics;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal static class ChunkFirstStableHash
    {
        public static uint TileVariant(int seed, Vector2Int worldCell, string layerId, string tilePresetId, int tileLayerIndex, string purpose)
        {
            return Hash(seed, worldCell.x, worldCell.y, layerId, tilePresetId, tileLayerIndex, purpose);
        }

        public static uint ObjectVariant(int seed, Vector2Int worldCell, string objectLayerId, int candidateIndex, string prefabId)
        {
            return Hash(seed, worldCell.x, worldCell.y, objectLayerId, prefabId, candidateIndex, "object");
        }

        private static uint Hash(int seed, int x, int y, string a, string b, int value, string purpose)
        {
            uint hash = math.hash(new int4(seed == 0 ? 1 : seed, x, y, value));
            hash = Mix(hash, a);
            hash = Mix(hash, b);
            hash = Mix(hash, purpose);
            return hash == 0 ? 1u : hash;
        }

        private static uint Mix(uint hash, string value)
        {
            if (string.IsNullOrEmpty(value))
                return math.hash(new uint2(hash, 0u));

            uint textHash = 2166136261u;
            for (int i = 0; i < value.Length; i++)
            {
                textHash ^= value[i];
                textHash *= 16777619u;
            }

            return math.hash(new uint2(hash, textHash));
        }
    }
}
