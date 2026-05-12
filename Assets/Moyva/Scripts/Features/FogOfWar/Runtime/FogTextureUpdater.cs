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
        private static readonly int GlobalFogTextureId = Shader.PropertyToID("_MoyvaFogTex");
        private static readonly int GlobalFogMapParamsId = Shader.PropertyToID("_MoyvaFogMapParams");

        private Texture2D _fogTexture;
        private byte[]    _buffer;
        private Material  _material;
        private int       _mapWidth;
        private int       _mapHeight;
        private bool      _renderingDisabled;
        private bool      _shaderGlobalsPublished;
        private Vector4   _mapParams;

        public void Initialize(int width, int height, Material fogMaterial)
        {
            _mapWidth  = Mathf.Max(1, width);
            _mapHeight = Mathf.Max(1, height);

            if (fogMaterial == null)
            {
                Debug.LogError("[FogOfWar] FogTextureUpdater: fogMaterial is null. Rendering disabled.");
                _renderingDisabled = true;
                _material = null;
            }
            else
            {
                _material = fogMaterial;
                _renderingDisabled = false;
            }

            if (_fogTexture != null)
                Object.Destroy(_fogTexture);

            _buffer = new byte[_mapWidth * _mapHeight];
            _mapParams = new Vector4(
                _mapWidth,
                _mapHeight,
                1f / Mathf.Max(1, _mapWidth),
                1f / Mathf.Max(1, _mapHeight));
            _shaderGlobalsPublished = false;

            _fogTexture            = new Texture2D(_mapWidth, _mapHeight, TextureFormat.R8, false, true);
            _fogTexture.filterMode = FilterMode.Point;
            _fogTexture.wrapMode   = TextureWrapMode.Clamp;
            _fogTexture.name       = "FogOfWar_Grid";

            ApplyBuffer();
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

            if (!_renderingDisabled && _material != null && !_shaderGlobalsPublished)
                _material.SetTexture("_FogTex", _fogTexture);

            PublishShaderGlobals(force: false);
        }

        private void PublishShaderGlobals(bool force)
        {
            if (_fogTexture == null)
                return;

            if (!force && _shaderGlobalsPublished)
                return;

            Shader.SetGlobalTexture(GlobalFogTextureId, _fogTexture);
            Shader.SetGlobalVector(GlobalFogMapParamsId, _mapParams);
            _shaderGlobalsPublished = true;
        }

        private static byte StateToPixel(FogStateType state)
        {
            switch (state)
            {
                case FogStateType.Visible:  return 255;
                case FogStateType.Forgotten:
                case FogStateType.Explored: return 160;
                default:                    return 0;
            }
        }
    }
}