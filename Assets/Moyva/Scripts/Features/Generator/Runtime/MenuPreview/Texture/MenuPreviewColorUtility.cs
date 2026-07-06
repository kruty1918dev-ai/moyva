using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MenuPreviewColorUtility
    {
        public static Color AlphaBlend(Color destination, Color source)
        {
            float sourceAlpha = source.a;
            float destinationAlpha = destination.a * (1f - sourceAlpha);
            float outputAlpha = sourceAlpha + destinationAlpha;
            if (outputAlpha < 0.001f)
                return Color.clear;

            return new Color(
                (source.r * sourceAlpha + destination.r * destinationAlpha) / outputAlpha,
                (source.g * sourceAlpha + destination.g * destinationAlpha) / outputAlpha,
                (source.b * sourceAlpha + destination.b * destinationAlpha) / outputAlpha,
                outputAlpha);
        }

        public static Color Shade(Color color, float shade)
        {
            shade = Mathf.Max(0f, shade);
            color.r = Mathf.Clamp01(color.r * shade);
            color.g = Mathf.Clamp01(color.g * shade);
            color.b = Mathf.Clamp01(color.b * shade);
            return color;
        }

        public static Color HashColor(string id, float alpha)
        {
            unchecked
            {
                int hash = 23;
                id ??= string.Empty;
                for (int i = 0; i < id.Length; i++)
                    hash = hash * 31 + id[i];

                float r = ((hash >> 16) & 0xFF) / 255f;
                float g = ((hash >> 8) & 0xFF) / 255f;
                float b = (hash & 0xFF) / 255f;
                var color = Color.Lerp(new Color(0.25f, 0.35f, 0.25f, 1f), new Color(0.95f, 0.9f, 0.55f, 1f), 0.65f);
                color.r = Mathf.Lerp(color.r, r, 0.45f);
                color.g = Mathf.Lerp(color.g, g, 0.45f);
                color.b = Mathf.Lerp(color.b, b, 0.45f);
                color.a = Mathf.Clamp01(alpha);
                return color;
            }
        }
    }
}
