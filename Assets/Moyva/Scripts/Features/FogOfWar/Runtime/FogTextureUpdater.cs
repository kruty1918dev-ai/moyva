using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Lightweight fog texture: 1 pixel = 1 tile, R8 format.
    /// No blur, no noise — raw grid values go straight to the GPU.
    /// </summary>
    internal sealed class FogTextureUpdater : IFogTextureUpdater
    {
        private Texture2D _fogTexture;
        private Texture2D _fogMaskIndexTexture;
        private byte[]    _buffer;
        private byte[]    _maskIndexBuffer;
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
            _maskIndexBuffer = new byte[width * height];

            _fogTexture            = new Texture2D(width, height, TextureFormat.R8, false, true);
            _fogTexture.filterMode = FilterMode.Point;
            _fogTexture.wrapMode   = TextureWrapMode.Clamp;
            _fogTexture.name       = "FogOfWar_Grid";

            _fogMaskIndexTexture            = new Texture2D(width, height, TextureFormat.R8, false, true);
            _fogMaskIndexTexture.filterMode = FilterMode.Point;
            _fogMaskIndexTexture.wrapMode   = TextureWrapMode.Clamp;
            _fogMaskIndexTexture.name       = "FogOfWar_MaskIndex";

            ApplyBuffer();

            if (!_renderingDisabled)
            {
                _material.SetTexture("_FogTex", _fogTexture);
                _material.SetTexture("_FogMaskIndexTex", _fogMaskIndexTexture);
            }
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
            RebuildMaskIndexBuffer();

            _fogTexture.SetPixelData(_buffer, 0);
            _fogTexture.Apply(false, false);

            _fogMaskIndexTexture.SetPixelData(_maskIndexBuffer, 0);
            _fogMaskIndexTexture.Apply(false, false);

            if (!_renderingDisabled && _material != null)
            {
                _material.SetTexture("_FogTex", _fogTexture);
                _material.SetTexture("_FogMaskIndexTex", _fogMaskIndexTexture);
            }
        }

        private void RebuildMaskIndexBuffer()
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    int idx = y * _mapWidth + x;
                    if (IsVisible(x, y))
                    {
                        _maskIndexBuffer[idx] = 0;
                        continue;
                    }

                    int mask = 0;
                    if (IsFogged(x, y + 1)) mask |= 1; // N
                    if (IsFogged(x + 1, y)) mask |= 2; // E
                    if (IsFogged(x, y - 1)) mask |= 4; // S
                    if (IsFogged(x - 1, y)) mask |= 8; // W

                    _maskIndexBuffer[idx] = (byte)mask;
                }
            }
        }

        private bool IsVisible(int x, int y)
        {
            if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
                return false;

            return _buffer[y * _mapWidth + x] >= 255;
        }

        private bool IsFogged(int x, int y)
        {
            if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
                return false;

            return !IsVisible(x, y);
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