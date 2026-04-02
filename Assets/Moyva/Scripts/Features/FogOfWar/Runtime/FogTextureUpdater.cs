using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Lightweight fog texture: 1 pixel = 1 tile, R8 format.
    /// No blur, no noise — raw grid values go straight to the GPU.
    /// </summary>
    public class FogTextureUpdater : IFogTextureUpdater
    {
        private Texture2D _fogTexture;
        private byte[]    _buffer;
        private Material  _material;
        private int       _mapWidth;
        private int       _mapHeight;
        private bool      _renderingDisabled;

        public void Initialize(int width, int height, Material fogMaterial)
        {
            _mapWidth  = width;
            _mapHeight = height;

            if (fogMaterial == null)
            {
                Debug.LogError("[FogOfWar] FogTextureUpdater: fogMaterial is null. Rendering disabled.");
                _renderingDisabled = true;
            }
            else
            {
                _material = fogMaterial;
            }

            _buffer = new byte[width * height];

            _fogTexture            = new Texture2D(width, height, TextureFormat.R8, false, true);
            _fogTexture.filterMode = FilterMode.Point;
            _fogTexture.wrapMode   = TextureWrapMode.Clamp;
            _fogTexture.name       = "FogOfWar_Grid";

            ApplyBuffer();

            if (!_renderingDisabled)
                _material.SetTexture("_FogTex", _fogTexture);
        }

        public void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles)
        {
            if (_fogTexture == null) return;

            bool anyDirty = false;

            foreach (var pos in dirtyTiles)
            {
                if (pos.x < 0 || pos.x >= _mapWidth || pos.y < 0 || pos.y >= _mapHeight)
                    continue;

                _buffer[pos.y * _mapWidth + pos.x] = StateToPixel(fogService.GetFogState(pos));
                anyDirty = true;
            }

            if (anyDirty)
                ApplyBuffer();
        }

        public void RebuildFullTexture(IFogOfWarService fogService)
        {
            if (_fogTexture == null) return;

            for (int y = 0; y < _mapHeight; y++)
                for (int x = 0; x < _mapWidth; x++)
                    _buffer[y * _mapWidth + x] = StateToPixel(fogService.GetFogState(new Vector2Int(x, y)));

            ApplyBuffer();
        }

        private void ApplyBuffer()
        {
            _fogTexture.SetPixelData(_buffer, 0);
            _fogTexture.Apply(false, false);

            if (!_renderingDisabled && _material != null)
                _material.SetTexture("_FogTex", _fogTexture);
        }

        private static byte StateToPixel(FogStateType state)
        {
            switch (state)
            {
                case FogStateType.Visible:  return 255;
                case FogStateType.Explored: return 128;
                default:                    return 0;
            }
        }
    }
}