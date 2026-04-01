using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Updates a Texture2D (R8 format) based on fog state.
    /// Only dirty tiles are updated for efficiency.
    /// byte value: 0 = Unexplored, 128 = Explored, 255 = Visible.
    /// </summary>
    public class FogTextureUpdater : IFogTextureUpdater
    {
        private Texture2D _fogTexture;
        private byte[]    _buffer;
        private Material  _material;
        private int       _width;
        private int       _height;
        private bool      _renderingDisabled;

        public void Initialize(int width, int height, Material fogMaterial)
        {
            _width  = width;
            _height = height;

            if (fogMaterial == null)
            {
                Debug.LogError("[FogOfWar] FogTextureUpdater: fogMaterial is null. Rendering disabled.");
                _renderingDisabled = true;
            }
            else
            {
                _material = fogMaterial;
            }

            // Always allocate the buffer so logic works even without rendering
            _buffer     = new byte[width * height];
            _fogTexture = new Texture2D(width, height, TextureFormat.R8, false);
            _fogTexture.filterMode = FilterMode.Bilinear;

            // Initialize all pixels to 0 (Unexplored)
            for (int i = 0; i < _buffer.Length; i++)
                _buffer[i] = 0;

            ApplyBuffer();

            if (!_renderingDisabled)
                _material.SetTexture("_FogTex", _fogTexture);
        }

        public void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles)
        {
            if (_fogTexture == null) return;

            foreach (var pos in dirtyTiles)
            {
                if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height)
                    continue;

                int idx = pos.y * _width + pos.x;
                _buffer[idx] = StateToPixel(fogService.GetFogState(pos));
            }

            ApplyBuffer();
        }

        public void RebuildFullTexture(IFogOfWarService fogService)
        {
            if (_fogTexture == null) return;

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                {
                    int idx = y * _width + x;
                    _buffer[idx] = StateToPixel(fogService.GetFogState(new Vector2Int(x, y)));
                }

            ApplyBuffer();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static byte StateToPixel(FogStateType state)
        {
            switch (state)
            {
                case FogStateType.Visible:    return 255;
                case FogStateType.Explored:   return 128;
                default:                      return 0;
            }
        }

        private void ApplyBuffer()
        {
            _fogTexture.SetPixelData(_buffer, 0);
            _fogTexture.Apply(false, false);

            if (!_renderingDisabled && _material != null)
                _material.SetTexture("_FogTex", _fogTexture);
        }
    }
}
