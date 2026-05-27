using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraMapRenderMaskService : IInitializable, ITickable, IDisposable
    {
        private readonly CameraSettingsSO _settings;
        private readonly IGridService _gridService;
        private readonly IGridProjection _gridProjection;

        private SpriteMask _mapMask;
        private Sprite _maskSprite;
        private Texture2D _maskTexture;
        private float _nextRefreshTime;

        private readonly List<SpriteRenderer> _maskedSpriteRenderers = new List<SpriteRenderer>(256);
        private readonly List<TilemapRenderer> _maskedTilemapRenderers = new List<TilemapRenderer>(32);

        public CameraMapRenderMaskService(
            CameraSettingsSO settings,
            [InjectOptional] IGridService gridService = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _settings = settings;
            _gridService = gridService;
            _gridProjection = gridProjection;
        }

        public void Initialize()
        {
            if (!ShouldUseSpriteMask())
                return;

            EnsureMask();
            ApplyMaskToVisibleRenderers();
            _nextRefreshTime = Time.unscaledTime + ResolveRefreshInterval();
        }

        public void Tick()
        {
            if (!ShouldUseSpriteMask())
            {
                UnmaskTrackedRenderers();
                return;
            }

            if (Time.unscaledTime < _nextRefreshTime)
                return;

            EnsureMask();
            ApplyMaskToVisibleRenderers();
            _nextRefreshTime = Time.unscaledTime + ResolveRefreshInterval();
        }

        public void Dispose()
        {
            UnmaskTrackedRenderers();
            DestroyMask();
        }

        private float ResolveRefreshInterval()
        {
            return Mathf.Max(0.05f, _settings.mapMaskRefreshSeconds);
        }

        private bool ShouldUseSpriteMask()
        {
            if (_settings == null || !_settings.mapRenderMaskEnabled)
                return false;

            return _gridProjection == null || _gridProjection.WorldPlane != GridWorldPlane.XZ;
        }

        private void EnsureMask()
        {
            if (_mapMask == null)
            {
                var maskObject = new GameObject("GameplayMapRenderMask");
                _mapMask = maskObject.AddComponent<SpriteMask>();
                _mapMask.sprite = CreateMaskSprite();
                _mapMask.alphaCutoff = 0.5f;
                _mapMask.isCustomRangeActive = true;
            }

            MapBounds bounds = ResolveMapBounds();
            _mapMask.transform.position = new Vector3(bounds.Center.x, bounds.Center.y, 0f);
            _mapMask.transform.localScale = new Vector3(bounds.Width, bounds.Height, 1f);

            int sortingLayerId = SortingLayer.NameToID(string.IsNullOrWhiteSpace(_settings.mapMaskSortingLayerName)
                ? "Default"
                : _settings.mapMaskSortingLayerName);
            _mapMask.frontSortingLayerID = sortingLayerId;
            _mapMask.backSortingLayerID = sortingLayerId;
            _mapMask.frontSortingOrder = Mathf.Max(_settings.mapMaskFrontSortingOrder, _settings.mapMaskBackSortingOrder);
            _mapMask.backSortingOrder = Mathf.Min(_settings.mapMaskBackSortingOrder, _settings.mapMaskFrontSortingOrder);
        }

        private void ApplyMaskToVisibleRenderers()
        {
            ApplyToSpriteRenderers();
            ApplyToTilemapRenderers();
        }

        private void ApplyToSpriteRenderers()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || !renderer.gameObject.scene.IsValid())
                    continue;
                if (!IsLayerAllowed(renderer.gameObject.layer))
                    continue;
                if (renderer.maskInteraction != SpriteMaskInteraction.None)
                    continue;

                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                _maskedSpriteRenderers.Add(renderer);
            }
        }

        private void ApplyToTilemapRenderers()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                TilemapRenderer renderer = renderers[i];
                if (renderer == null || !renderer.gameObject.scene.IsValid())
                    continue;
                if (!IsLayerAllowed(renderer.gameObject.layer))
                    continue;
                if (renderer.maskInteraction != SpriteMaskInteraction.None)
                    continue;

                renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                _maskedTilemapRenderers.Add(renderer);
            }
        }

        private bool IsLayerAllowed(int layer)
        {
            int bit = 1 << layer;
            return (_settings.mapMaskLayers.value & bit) != 0;
        }

        private void UnmaskTrackedRenderers()
        {
            for (int i = 0; i < _maskedSpriteRenderers.Count; i++)
            {
                SpriteRenderer renderer = _maskedSpriteRenderers[i];
                if (renderer != null)
                    renderer.maskInteraction = SpriteMaskInteraction.None;
            }
            _maskedSpriteRenderers.Clear();

            for (int i = 0; i < _maskedTilemapRenderers.Count; i++)
            {
                TilemapRenderer renderer = _maskedTilemapRenderers[i];
                if (renderer != null)
                    renderer.maskInteraction = SpriteMaskInteraction.None;
            }
            _maskedTilemapRenderers.Clear();
        }

        private Sprite CreateMaskSprite()
        {
            if (_maskSprite != null)
                return _maskSprite;

            _maskTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
            {
                name = "GameplayMapRenderMaskTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            _maskTexture.SetPixel(0, 0, Color.white);
            _maskTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);

            _maskSprite = Sprite.Create(_maskTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            _maskSprite.name = "GameplayMapRenderMaskSprite";
            _maskSprite.hideFlags = HideFlags.HideAndDontSave;
            return _maskSprite;
        }

        private void DestroyMask()
        {
            if (_mapMask != null)
            {
                UnityEngine.Object.Destroy(_mapMask.gameObject);
                _mapMask = null;
            }

            if (_maskSprite != null)
            {
                UnityEngine.Object.Destroy(_maskSprite);
                _maskSprite = null;
            }

            if (_maskTexture != null)
            {
                UnityEngine.Object.Destroy(_maskTexture);
                _maskTexture = null;
            }
        }

        private MapBounds ResolveMapBounds()
        {
            if (TryBuildBoundsFromGridProjection(out MapBounds fromProjection))
            {
                return fromProjection;
            }

            var tilemapRenderers = UnityEngine.Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (TryBuildBoundsFromTilemaps(tilemapRenderers, out MapBounds fromTilemaps))
            {
                return fromTilemaps;
            }

            Vector2 halfSize = _settings.manualMapMaskSize * 0.5f;
            return new MapBounds(
                _settings.manualMapMaskCenter.x - halfSize.x,
                _settings.manualMapMaskCenter.x + halfSize.x,
                _settings.manualMapMaskCenter.y - halfSize.y,
                _settings.manualMapMaskCenter.y + halfSize.y);
        }

        private bool TryBuildBoundsFromGridProjection(out MapBounds bounds)
        {
            if (_gridService == null || _gridProjection == null)
            {
                bounds = default;
                return false;
            }

            int width = _gridService.GridWidth;
            int height = _gridService.GridHeight;
            if (width <= 0 || height <= 0)
            {
                bounds = default;
                return false;
            }

            Bounds worldBounds = _gridProjection.GetWorldBounds(width, height);
            float minPlaneY = _gridProjection.WorldPlane == GridWorldPlane.XZ ? worldBounds.min.z : worldBounds.min.y;
            float maxPlaneY = _gridProjection.WorldPlane == GridWorldPlane.XZ ? worldBounds.max.z : worldBounds.max.y;
            bounds = new MapBounds(worldBounds.min.x, worldBounds.max.x, minPlaneY, maxPlaneY);
            return true;
        }

        private bool TryBuildBoundsFromTilemaps(TilemapRenderer[] tilemapRenderers, out MapBounds bounds)
        {
            bool hasAny = false;
            float minX = 0f;
            float maxX = 0f;
            float minY = 0f;
            float maxY = 0f;

            for (int i = 0; i < tilemapRenderers.Length; i++)
            {
                TilemapRenderer renderer = tilemapRenderers[i];
                if (renderer == null || !renderer.gameObject.scene.IsValid())
                    continue;
                if (!IsLayerAllowed(renderer.gameObject.layer))
                    continue;

                Bounds worldBounds = renderer.bounds;
                if (!hasAny)
                {
                    minX = worldBounds.min.x;
                    maxX = worldBounds.max.x;
                    minY = worldBounds.min.y;
                    maxY = worldBounds.max.y;
                    hasAny = true;
                    continue;
                }

                minX = Mathf.Min(minX, worldBounds.min.x);
                maxX = Mathf.Max(maxX, worldBounds.max.x);
                minY = Mathf.Min(minY, worldBounds.min.y);
                maxY = Mathf.Max(maxY, worldBounds.max.y);
            }

            if (!hasAny)
            {
                bounds = default;
                return false;
            }

            bounds = new MapBounds(minX, maxX, minY, maxY);
            return true;
        }

        private readonly struct MapBounds
        {
            public readonly float MinX;
            public readonly float MaxX;
            public readonly float MinY;
            public readonly float MaxY;

            public float Width => Mathf.Max(0.01f, MaxX - MinX);
            public float Height => Mathf.Max(0.01f, MaxY - MinY);
            public Vector2 Center => new Vector2((MinX + MaxX) * 0.5f, (MinY + MaxY) * 0.5f);

            public MapBounds(float minX, float maxX, float minY, float maxY)
            {
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
            }
        }
    }
}
