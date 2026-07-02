using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Lightweight fog texture: 1 pixel = 1 tile, R8 format.
    /// No blur, no noise — raw grid values go straight to the GPU.
    /// Це legacy visual path: gameplay fog state залишається у <see cref="IFogOfWarService"/>,
    /// а texture updater лише відображає його у shader representation.
    /// </summary>
    [Obsolete("Use FogOfWarVolumeUpdater for TWC dual-grid fog volume visuals.")]
    internal sealed class FogTextureUpdater : IFogTextureUpdater
    {
        private const string DebugTag = "[MoyvaFogTrace]";
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

        /// <summary>
        /// Готує legacy texture presentation для карти заданого розміру.
        /// Публікує shader globals і створює CPU/GPU buffer-и.
        /// </summary>
        /// <param name="width">Ширина карти у клітинках.</param>
        /// <param name="height">Висота карти у клітинках.</param>
        /// <param name="fogMaterial">Матеріал, який споживає legacy fog texture.</param>
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
                UnityEngine.Object.Destroy(_fogTexture);

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
            Debug.Log($"{DebugTag} FogTexture.Initialize map={_mapWidth}x{_mapHeight}, material={(_material != null ? _material.name : "null")}, renderingDisabled={_renderingDisabled}.");
        }

        /// <summary>
        /// Застосовує інкрементальні зміни лише до dirty-клітинок у texture presentation.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="dirtyTiles">Клітинки, які змінилися після останнього update.</param>
        public void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles)
        {
            if (_fogTexture == null)
            {
                Debug.LogWarning($"{DebugTag} FogTexture.UpdateDirtyTiles skipped: texture is null.");
                return;
            }

            bool anyDirty = false;
            int requested = 0;
            int applied = 0;
            int skipped = 0;

            foreach (var pos in dirtyTiles)
            {
                requested++;
                if (pos.x < 0 || pos.x >= _mapWidth || pos.y < 0 || pos.y >= _mapHeight)
                {
                    skipped++;
                    continue;
                }

                _buffer[pos.y * _mapWidth + pos.x] = StateToPixel(fogService.GetFogState(pos));
                anyDirty = true;
                applied++;
            }

            Debug.Log($"{DebugTag} FogTexture.UpdateDirtyTiles requested={requested}, applied={applied}, skipped={skipped}, map={_mapWidth}x{_mapHeight}, anyDirty={anyDirty}.");
            if (anyDirty)
                ApplyBuffer();
        }

        /// <summary>
        /// Повністю перебудовує texture presentation зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        public void RebuildFullTexture(IFogOfWarService fogService)
        {
            if (_fogTexture == null)
            {
                Debug.LogWarning($"{DebugTag} FogTexture.RebuildFullTexture skipped: texture is null.");
                return;
            }

            int visible = 0;
            int explored = 0;
            int unexplored = 0;
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    var state = fogService.GetFogState(new Vector2Int(x, y));
                    _buffer[y * _mapWidth + x] = StateToPixel(state);
                    switch (state)
                    {
                        case FogStateType.Visible:
                            visible++;
                            break;
                        case FogStateType.Explored:
                            explored++;
                            break;
                        default:
                            unexplored++;
                            break;
                    }
                }
            }

            ApplyBuffer();
            Debug.Log($"{DebugTag} FogTexture.RebuildFullTexture map={_mapWidth}x{_mapHeight}, visible={visible}, explored={explored}, unexplored={unexplored}.");
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
            Debug.Log($"{DebugTag} FogTexture.PublishShaderGlobals map={_mapWidth}x{_mapHeight}, texture={_fogTexture.name}, force={force}.");
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
