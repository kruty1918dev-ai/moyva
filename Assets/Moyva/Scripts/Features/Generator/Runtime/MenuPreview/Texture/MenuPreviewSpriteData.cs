using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct MenuPreviewSpriteData
    {
        public MenuPreviewSpriteData(Color[] pixels, int width, int height)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
        }

        public readonly Color[] Pixels;
        public readonly int Width;
        public readonly int Height;
        public bool IsValid => Pixels != null && Pixels.Length > 0 && Width > 0 && Height > 0;
    }
}
