using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Visuals
{
    [CreateAssetMenu(fileName = "TileTextureAtlas", menuName = "Moyva/Visuals/Tile Texture Atlas")]
    public sealed class TileTextureAtlasSO : ScriptableObject
    {
        [Serializable]
        public struct TileTextureEntry
        {
              public string TileId;
              public Sprite Sprite;
        }

        [SerializeField] private TileTextureEntry[] _entries = Array.Empty<TileTextureEntry>();
        [SerializeField] private int _atlasPadding = 2;

        [NonSerialized] private Texture2D _atlas;
        [NonSerialized] private readonly Dictionary<string, Rect> _uvRects = new();
        [NonSerialized] private readonly Dictionary<string, int> _tileIdToIndex = new();

        public Texture2D Atlas => _atlas;
        public bool IsBuilt => _atlas != null;
        public int TileCount => _tileIdToIndex.Count;

        public int GetTileIndex(string tileId)
        {
            if (string.IsNullOrEmpty(tileId)) return -1;
            return _tileIdToIndex.TryGetValue(tileId, out var idx) ? idx : -1;
        }

        public bool TryGetUVRect(string tileId, out Rect rect)
        {
            return _uvRects.TryGetValue(tileId, out rect);
        }

        public void BuildAtlas()
        {
            _uvRects.Clear();
            _tileIdToIndex.Clear();

            if (_entries == null || _entries.Length == 0)
            {
                _atlas = null;
                return;
            }

            var valid = new List<(string id, Sprite sprite)>();
            foreach (var entry in _entries)
                if (entry.Sprite != null && !string.IsNullOrEmpty(entry.TileId))
                    valid.Add((entry.TileId, entry.Sprite));

            if (valid.Count == 0)
            {
                _atlas = null;
                return;
            }

            // Use the source spritesheet directly — avoids re-packing pixel-art sprites
            // and preserves the original filtering / compression settings.
            _atlas = valid[0].sprite.texture;
            float tw = _atlas.width;
            float th = _atlas.height;

            for (int i = 0; i < valid.Count; i++)
            {
                var sprRect = valid[i].sprite.rect; // pixel coords, origin bottom-left
                _uvRects[valid[i].id] = new Rect(
                    sprRect.x / tw, sprRect.y / th,
                    sprRect.width / tw, sprRect.height / th);
                _tileIdToIndex[valid[i].id] = i;
            }
        }

        public Vector4 GetUVRectVector(string tileId)
        {
            if (_uvRects.TryGetValue(tileId, out var rect))
                return new Vector4(rect.x, rect.y, rect.width, rect.height);
            return Vector4.zero;
        }

        public Texture2D BuildUVLookupTexture()
        {
            int count = Mathf.Max(1, _tileIdToIndex.Count);
            var tex = new Texture2D(count, 1, TextureFormat.RGBAFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[count];
            foreach (var kvp in _tileIdToIndex)
            {
                if (_uvRects.TryGetValue(kvp.Key, out var rect))
                {
                    pixels[kvp.Value] = new Color(rect.x, rect.y, rect.width, rect.height);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(false, false);
            return tex;
        }
    }
}
