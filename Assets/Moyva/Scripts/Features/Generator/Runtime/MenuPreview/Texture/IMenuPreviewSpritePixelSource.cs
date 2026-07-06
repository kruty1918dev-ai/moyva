using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewSpritePixelSource
    {
        bool TryFromPrefab(GameObject prefab, string stableId, MoyvaProjectSettingsSO settings, out MenuPreviewSpriteData data);
        bool TryFromSprite(Sprite sprite, Color tint, out MenuPreviewSpriteData data);
    }
}
