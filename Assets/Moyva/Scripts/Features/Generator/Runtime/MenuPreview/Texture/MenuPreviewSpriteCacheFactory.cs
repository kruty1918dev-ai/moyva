using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewSpriteCacheFactory : IMenuPreviewSpriteCacheFactory
    {
        private readonly IMenuPreviewSpritePixelSource _source;

        public MenuPreviewSpriteCacheFactory(IMenuPreviewSpritePixelSource source)
        {
            _source = source;
        }

        public Dictionary<string, MenuPreviewSpriteData> BuildTiles(
            TileRegistrySO registry,
            MoyvaProjectSettingsSO settings,
            out MenuPreviewSpriteData fallback)
        {
            var cache = CreateCache();
            fallback = default;
            if (registry?.Definitions == null)
            {
                Debug.LogWarning("[MenuPreview] TileRegistry is null or has no definitions.");
                return cache;
            }

            var failed = new System.Text.StringBuilder();
            foreach (var definition in registry.Definitions)
            {
                string id = MenuPreviewIdUtility.Normalize(definition?.Id);
                if (string.IsNullOrEmpty(id))
                    continue;

                if (TryCachePrefab(cache, id, definition.VisualPrefab, settings, out var data))
                {
                    if (!fallback.IsValid)
                        fallback = data;
                    continue;
                }

                failed.Append(id).Append(' ');
            }

            Debug.Log($"[MenuPreview] Tile sprite cache: {cache.Count}/{registry.Definitions.Length} entries loaded.");
            if (failed.Length > 0)
                Debug.LogWarning($"[MenuPreview] Failed to load tile previews for: {failed}");

            return cache;
        }

        public Dictionary<string, MenuPreviewSpriteData> BuildObjects(MapObjectRegistrySO registry, MoyvaProjectSettingsSO settings)
        {
            var cache = CreateCache();
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = MenuPreviewIdUtility.Normalize(definition?.Id);
                if (!string.IsNullOrEmpty(id))
                    TryCachePrefab(cache, id, definition.VisualPrefab, settings, out _);
            }

            return cache;
        }

        public Dictionary<string, MenuPreviewSpriteData> BuildBuildings(BuildingRegistrySO registry, MoyvaProjectSettingsSO settings)
        {
            var cache = CreateCache();
            var definitions = registry?.GetAll();
            if (definitions == null)
                return cache;

            foreach (var definition in definitions)
            {
                string id = MenuPreviewIdUtility.Normalize(definition?.Id);
                if (string.IsNullOrEmpty(id))
                    continue;

                if (definition.Icon != null && _source.TryFromSprite(definition.Icon, Color.white, out var iconData))
                {
                    cache[id] = iconData;
                    continue;
                }

                TryCachePrefab(cache, id, definition.Prefab, settings, out _);
            }

            return cache;
        }

        private bool TryCachePrefab(
            Dictionary<string, MenuPreviewSpriteData> cache,
            string id,
            GameObject prefab,
            MoyvaProjectSettingsSO settings,
            out MenuPreviewSpriteData data)
        {
            data = default;
            if (prefab == null || !_source.TryFromPrefab(prefab, id, settings, out data))
                return false;

            cache[id] = data;
            return true;
        }

        private static Dictionary<string, MenuPreviewSpriteData> CreateCache()
        {
            return new Dictionary<string, MenuPreviewSpriteData>(System.StringComparer.OrdinalIgnoreCase);
        }
    }
}
