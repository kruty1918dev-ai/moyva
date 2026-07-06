using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorTerrainSideWallFormat
    {
        public static string Number(float value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

        public static string Vector3(Vector3 value)
            => $"x={Number(value.x)}, y={Number(value.y)}, z={Number(value.z)}";

        public static string Bounds(Bounds bounds)
            => $"center=({Vector3(bounds.center)}), size=({Vector3(bounds.size)})";

        public static string Transform(Transform target)
        {
            if (target == null)
                return "<null>";

            return $"localPos=({Vector3(target.localPosition)}), worldPos=({Vector3(target.position)}), localScale=({Vector3(target.localScale)}), lossyScale=({Vector3(target.lossyScale)})";
        }

        public static string FormatMaterialCull(Material material)
        {
            if (material == null)
                return "<null>";

            return material.HasProperty("_Cull") ? Number(material.GetFloat("_Cull")) : "<no _Cull>";
        }

        public static string Histogram(SortedDictionary<int, int> histogram)
        {
            if (histogram == null || histogram.Count == 0)
                return "{}";

            var builder = new StringBuilder();
            builder.Append('{');
            int entryIndex = 0;
            foreach (var pair in histogram)
            {
                if (entryIndex > 0)
                    builder.Append(", ");
                builder.Append(pair.Key).Append(':').Append(pair.Value);
                entryIndex++;
            }

            return builder.Append('}').ToString();
        }

        public static string Samples(List<string> samples)
        {
            return TileWorldCreatorHeightProjectionUtility.FormatSamples(samples);
        }
    }
}
