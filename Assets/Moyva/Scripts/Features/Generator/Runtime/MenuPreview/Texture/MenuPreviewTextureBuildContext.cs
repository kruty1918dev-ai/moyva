using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewTextureBuildContext
    {
        public MenuPreviewTextureBuildContext(
            MenuWorldPreviewTextureBuildRequest request,
            MoyvaProjectSettingsSO settings,
            MenuPreviewTextureLayout layout,
            Color[] canvas,
            Dictionary<string, MenuPreviewSpriteData> tileSprites,
            MenuPreviewSpriteData fallbackTile)
        {
            Request = request;
            Settings = settings;
            Layout = layout;
            Canvas = canvas;
            TileSprites = tileSprites;
            FallbackTile = fallbackTile;
        }

        public MenuWorldPreviewTextureBuildRequest Request { get; }
        public MoyvaProjectSettingsSO Settings { get; }
        public MenuPreviewTextureLayout Layout { get; }
        public Color[] Canvas { get; }
        public Dictionary<string, MenuPreviewSpriteData> TileSprites { get; }
        public MenuPreviewSpriteData FallbackTile { get; }
    }
}
