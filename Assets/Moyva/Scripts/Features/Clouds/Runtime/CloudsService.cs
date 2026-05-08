using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Kruty1918.Moyva.Clouds.Runtime
{
    internal sealed class CloudsService : ICloudsService, IInitializable, ITickable, IDisposable
    {
        private readonly CloudsSettings _settings;
        private readonly CloudsSceneReferences _sceneReferences;
        private readonly IGridService _gridService;
        private readonly List<CloudInstance> _clouds = new List<CloudInstance>();

        private const int MaskSortingRangePadding = 64;

        private UnityEngine.Camera _camera;
        private Transform _root;
        private SpriteMask _mapMask;
        private Texture2D _maskTexture;
        private Sprite _maskSprite;
        private bool _ownsRoot;
        private float _spawnTimer;
        private int _pendingInitialClouds;

        public int ActiveCloudsCount => _clouds.Count;

        public CloudsService(
            CloudsSettings settings,
            CloudsSceneReferences sceneReferences,
            [InjectOptional] IGridService gridService = null)
        {
            _settings = settings;
            _sceneReferences = sceneReferences;
            _gridService = gridService;
        }

        public void Initialize()
        {
            _camera = _sceneReferences.SceneCamera != null ? _sceneReferences.SceneCamera : UnityEngine.Camera.main;
            _root = ResolveRoot();
            EnsureMapMask();
            ResetSpawnTimer();

            _pendingInitialClouds = Mathf.Min(_settings.InitialClouds, _settings.MaxActiveClouds);
            TrySpawnInitialClouds();
        }

        public void Tick()
        {
            if (!_settings.Enabled)
            {
                ClearClouds();
                return;
            }

            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            EnsureMapMask();
            TrySpawnInitialClouds();
            TickClouds();
            TickSpawn();
        }

        public void Dispose()
        {
            ClearClouds();
            DestroyMapMask();
            if (_ownsRoot && _root != null)
                Object.Destroy(_root.gameObject);
        }

        public void ClearClouds()
        {
            for (int i = _clouds.Count - 1; i >= 0; i--)
                DestroyCloud(_clouds[i]);

            _clouds.Clear();
        }

        public void SpawnCloud()
        {
            SpawnCloudInternal(startInView: false);
        }

        private void TrySpawnInitialClouds()
        {
            if (_pendingInitialClouds <= 0 || _camera == null || _root == null || !HasUsableSprites())
                return;

            while (_pendingInitialClouds > 0 && _clouds.Count < _settings.MaxActiveClouds)
            {
                if (!SpawnCloudInternal(_settings.InitialCloudsStartInView))
                    return;

                _pendingInitialClouds--;
            }
        }

        private Transform ResolveRoot()
        {
            if (_sceneReferences.Root != null)
                return _sceneReferences.Root;

            var rootObject = new GameObject("CloudsRoot");
            _ownsRoot = true;
            return rootObject.transform;
        }

        private void TickSpawn()
        {
            if (_clouds.Count >= _settings.MaxActiveClouds || !HasUsableSprites())
                return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f)
                return;

            SpawnCloudInternal(startInView: false);
            ResetSpawnTimer();
        }

        private void TickClouds()
        {
            for (int i = _clouds.Count - 1; i >= 0; i--)
            {
                CloudInstance cloud = _clouds[i];
                cloud.Age += Time.deltaTime;
                cloud.Root.transform.position += Vector3.right * (cloud.Direction * cloud.Speed * Time.deltaTime);

                float fade = ResolveFade(cloud);
                ApplyAlpha(cloud, fade);

                if (HasPassedEnd(cloud) || HasDissolved(cloud))
                {
                    DestroyCloud(cloud);
                    _clouds.RemoveAt(i);
                }
            }
        }

        private bool SpawnCloudInternal(bool startInView)
        {
            if (_camera == null || _root == null || _clouds.Count >= _settings.MaxActiveClouds)
                return false;

            Sprite sprite = PickSprite();
            if (sprite == null)
                return false;

            CameraBounds bounds = ResolveSpawnBounds();
            int direction = Random.value <= _settings.LeftToRightChance ? 1 : -1;
            float spawnX = direction > 0
                ? bounds.MinX - _settings.SpawnHorizontalPadding
                : bounds.MaxX + _settings.SpawnHorizontalPadding;
            float endX = direction > 0
                ? bounds.MaxX + _settings.DespawnHorizontalPadding
                : bounds.MinX - _settings.DespawnHorizontalPadding;

            if (startInView)
                spawnX = Random.Range(bounds.MinX, bounds.MaxX);

            float y = Random.Range(
                startInView ? bounds.MinY : bounds.MinY - _settings.SpawnVerticalPadding,
                startInView ? bounds.MaxY : bounds.MaxY + _settings.SpawnVerticalPadding);

            CloudInstance cloud = CreateCloud(sprite, new Vector3(spawnX, y, 0f), direction, endX);
            if (startInView)
                cloud.Age = _settings.FadeDuration;

            _clouds.Add(cloud);
            ApplyAlpha(cloud, startInView ? 1f : 0f);
            return true;
        }

        private CloudInstance CreateCloud(Sprite sprite, Vector3 position, int direction, float endX)
        {
            var rootObject = new GameObject("Cloud");
            rootObject.transform.SetParent(_root, worldPositionStays: true);
            rootObject.transform.position = position;

            float scale = Random.Range(_settings.ScaleRange.x, _settings.ScaleRange.y);
            rootObject.transform.localScale = new Vector3(scale, scale, 1f);

            var cloudRenderer = rootObject.AddComponent<SpriteRenderer>();
            cloudRenderer.sprite = sprite;
            ApplySorting(cloudRenderer, _settings.SortingOrder);

            SpriteRenderer shadowRenderer = null;
            if (_settings.ShadowsEnabled)
            {
                var shadowObject = new GameObject("CloudShadow");
                shadowObject.transform.SetParent(rootObject.transform, worldPositionStays: false);
                Vector2 shadowOffset = ResolveShadowOffset();
                shadowObject.transform.localPosition = new Vector3(shadowOffset.x, shadowOffset.y, 0f);
                shadowObject.transform.localScale = Vector3.one * ResolveShadowScaleMultiplier();
                shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
                shadowRenderer.sprite = sprite;
                shadowRenderer.maskInteraction = ResolveMaskInteraction();
                ApplySorting(shadowRenderer, _settings.SortingOrder + _settings.ShadowSortingOrderOffset);
            }

            cloudRenderer.maskInteraction = ResolveMaskInteraction();

            return new CloudInstance(
                rootObject,
                cloudRenderer,
                shadowRenderer,
                Random.Range(_settings.SpeedRange.x, _settings.SpeedRange.y),
                direction,
                endX,
                ResolveLifetime());
        }

        private void ApplySorting(SpriteRenderer renderer, int sortingOrder)
        {
            if (!string.IsNullOrWhiteSpace(_settings.SortingLayerName))
                renderer.sortingLayerName = _settings.SortingLayerName;

            renderer.sortingOrder = sortingOrder;
        }

        private Sprite PickSprite()
        {
            CloudSpriteVariant[] variants = _settings.CloudSprites;
            if (variants == null || variants.Length == 0)
                return null;

            float totalChance = 0f;
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i]?.Sprite != null)
                    totalChance += Mathf.Max(0f, variants[i].Chance);
            }

            if (totalChance <= 0f)
                return null;

            float roll = Random.Range(0f, totalChance);
            float cursor = 0f;
            for (int i = 0; i < variants.Length; i++)
            {
                CloudSpriteVariant variant = variants[i];
                if (variant?.Sprite == null)
                    continue;

                cursor += Mathf.Max(0f, variant.Chance);
                if (roll <= cursor)
                    return variant.Sprite;
            }

            return null;
        }

        private bool HasUsableSprites()
        {
            CloudSpriteVariant[] variants = _settings.CloudSprites;
            if (variants == null)
                return false;

            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i]?.Sprite != null && variants[i].Chance > 0f)
                    return true;
            }

            return false;
        }

        private CameraBounds ResolveCameraBounds()
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * Mathf.Max(0.01f, _camera.aspect);
            Vector3 center = _camera.transform.position;
            return new CameraBounds(center.x - halfWidth, center.x + halfWidth, center.y - halfHeight, center.y + halfHeight);
        }

        private float ResolveFade(CloudInstance cloud)
        {
            float fadeIn = 1f;
            float fadeOut = 1f;
            if (_settings.FadeDuration > 0f)
            {
                fadeIn = Mathf.Clamp01(cloud.Age / _settings.FadeDuration);
                float remainingDistance = cloud.Direction > 0
                    ? cloud.EndX - cloud.Root.transform.position.x
                    : cloud.Root.transform.position.x - cloud.EndX;
                float fadeOutDistance = Mathf.Max(0.001f, cloud.Speed * _settings.FadeDuration);
                fadeOut = Mathf.Clamp01(remainingDistance / fadeOutDistance);
            }

            float dissolve = ResolveDissolveFade(cloud);
            float maskEdge = ResolveMaskEdgeFade(cloud);
            return Mathf.Min(fadeIn, fadeOut, dissolve, maskEdge);
        }

        private float ResolveDissolveFade(CloudInstance cloud)
        {
            if (!_settings.LifetimeDissolveEnabled)
                return 1f;

            if (cloud.Age <= cloud.Lifetime)
                return 1f;

            if (_settings.DissolveDuration <= 0f)
                return 0f;

            return 1f - Mathf.Clamp01((cloud.Age - cloud.Lifetime) / _settings.DissolveDuration);
        }

        private void ApplyAlpha(CloudInstance cloud, float fade)
        {
            Color cloudColor = _settings.CloudColor;
            cloudColor.a *= _settings.CloudAlpha * fade;
            cloud.CloudRenderer.color = cloudColor;

            if (cloud.ShadowRenderer == null)
                return;

            Color shadowColor = _settings.ShadowColor;
            shadowColor.a *= _settings.CloudAlpha * ResolveShadowAlphaMultiplier() * fade;
            cloud.ShadowRenderer.color = shadowColor;
        }

        private bool HasPassedEnd(CloudInstance cloud)
        {
            float x = cloud.Root.transform.position.x;
            return cloud.Direction > 0 ? x >= cloud.EndX : x <= cloud.EndX;
        }

        private bool HasDissolved(CloudInstance cloud)
        {
            return _settings.LifetimeDissolveEnabled && cloud.Age >= cloud.Lifetime + _settings.DissolveDuration;
        }

        private float ResolveMaskEdgeFade(CloudInstance cloud)
        {
            if (!_settings.MapMaskEnabled || _settings.MaskEdgeFadeWidth <= 0f)
                return 1f;

            MapBounds maskBounds = ResolveMapBounds();
            Bounds rendererBounds = cloud.CloudRenderer.bounds;
            float fadeWidth = Mathf.Max(0.001f, _settings.MaskEdgeFadeWidth);
            float left = Mathf.Clamp01((rendererBounds.max.x - maskBounds.MinX) / fadeWidth);
            float right = Mathf.Clamp01((maskBounds.MaxX - rendererBounds.min.x) / fadeWidth);
            float bottom = Mathf.Clamp01((rendererBounds.max.y - maskBounds.MinY) / fadeWidth);
            float top = Mathf.Clamp01((maskBounds.MaxY - rendererBounds.min.y) / fadeWidth);
            float fade = Mathf.Min(left, right, bottom, top);
            return PixelateFade(fade);
        }

        private float PixelateFade(float value)
        {
            int steps = Mathf.Max(1, _settings.MaskEdgeFadeSteps);
            return Mathf.Floor(Mathf.Clamp01(value) * steps) / steps;
        }

        private Vector2 ResolveShadowOffset()
        {
            return _settings.ShadowOffset + _settings.ShadowOffsetPerHeight * _settings.CloudHeight;
        }

        private float ResolveShadowScaleMultiplier()
        {
            return Mathf.Max(0.01f, _settings.ShadowScaleMultiplier + _settings.ShadowScalePerHeight * _settings.CloudHeight);
        }

        private float ResolveShadowAlphaMultiplier()
        {
            return Mathf.Clamp01(_settings.ShadowAlphaMultiplier / (1f + _settings.CloudHeight * _settings.ShadowAlphaHeightFade));
        }

        private float ResolveLifetime()
        {
            return Random.Range(_settings.LifetimeRange.x, _settings.LifetimeRange.y);
        }

        private CameraBounds ResolveSpawnBounds()
        {
            CameraBounds cameraBounds = ResolveCameraBounds();
            if (!_settings.MapMaskEnabled)
                return cameraBounds;

            MapBounds mapBounds = ResolveMapBounds();
            float minX = Mathf.Max(cameraBounds.MinX, mapBounds.MinX);
            float maxX = Mathf.Min(cameraBounds.MaxX, mapBounds.MaxX);
            float minY = Mathf.Max(cameraBounds.MinY, mapBounds.MinY);
            float maxY = Mathf.Min(cameraBounds.MaxY, mapBounds.MaxY);

            if (minX >= maxX)
            {
                minX = mapBounds.MinX;
                maxX = mapBounds.MaxX;
            }

            if (minY >= maxY)
            {
                minY = mapBounds.MinY;
                maxY = mapBounds.MaxY;
            }

            return new CameraBounds(minX, maxX, minY, maxY);
        }

        private MapBounds ResolveMapBounds()
        {
            if (_gridService != null)
            {
                return new MapBounds(
                    -0.5f,
                    _gridService.GridWidth - 0.5f,
                    -0.5f,
                    _gridService.GridHeight - 0.5f);
            }

            Vector2 halfSize = _settings.ManualMapSize * 0.5f;
            return new MapBounds(
                _settings.ManualMapCenter.x - halfSize.x,
                _settings.ManualMapCenter.x + halfSize.x,
                _settings.ManualMapCenter.y - halfSize.y,
                _settings.ManualMapCenter.y + halfSize.y);
        }

        private SpriteMaskInteraction ResolveMaskInteraction()
        {
            return _settings.MapMaskEnabled ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None;
        }

        private void EnsureMapMask()
        {
            if (_root == null)
                return;

            if (!_settings.MapMaskEnabled)
            {
                ApplyMaskInteractionToClouds(SpriteMaskInteraction.None);
                DestroyMapMask();
                return;
            }

            if (_mapMask == null)
            {
                var maskObject = new GameObject("CloudsMapMask");
                maskObject.transform.SetParent(_root, worldPositionStays: true);
                _mapMask = maskObject.AddComponent<SpriteMask>();
                _mapMask.sprite = CreateMaskSprite();
                _mapMask.isCustomRangeActive = true;
                _mapMask.alphaCutoff = 0.5f;
            }

            MapBounds bounds = ResolveMapBounds();
            _mapMask.transform.position = new Vector3(bounds.Center.x, bounds.Center.y, 0f);
            _mapMask.transform.localScale = new Vector3(bounds.Width, bounds.Height, 1f);
            int sortingLayerId = SortingLayer.NameToID(_settings.SortingLayerName);
            _mapMask.frontSortingLayerID = sortingLayerId;
            _mapMask.backSortingLayerID = sortingLayerId;
            _mapMask.frontSortingOrder = _settings.SortingOrder + MaskSortingRangePadding;
            _mapMask.backSortingOrder = _settings.SortingOrder + _settings.ShadowSortingOrderOffset - MaskSortingRangePadding;
            ApplyMaskInteractionToClouds(SpriteMaskInteraction.VisibleInsideMask);
        }

        private void ApplyMaskInteractionToClouds(SpriteMaskInteraction interaction)
        {
            for (int i = 0; i < _clouds.Count; i++)
            {
                if (_clouds[i].CloudRenderer != null)
                    _clouds[i].CloudRenderer.maskInteraction = interaction;

                if (_clouds[i].ShadowRenderer != null)
                    _clouds[i].ShadowRenderer.maskInteraction = interaction;
            }
        }

        private Sprite CreateMaskSprite()
        {
            if (_maskSprite != null)
                return _maskSprite;

            _maskTexture = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false)
            {
                name = "CloudsMapMaskTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            _maskTexture.SetPixel(0, 0, Color.white);
            _maskTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            _maskSprite = Sprite.Create(_maskTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            _maskSprite.name = "CloudsMapMaskSprite";
            _maskSprite.hideFlags = HideFlags.HideAndDontSave;
            return _maskSprite;
        }

        private void DestroyMapMask()
        {
            if (_mapMask != null)
            {
                Object.Destroy(_mapMask.gameObject);
                _mapMask = null;
            }

            if (_maskSprite != null)
            {
                Object.Destroy(_maskSprite);
                _maskSprite = null;
            }

            if (_maskTexture != null)
            {
                Object.Destroy(_maskTexture);
                _maskTexture = null;
            }
        }

        private void DestroyCloud(CloudInstance cloud)
        {
            if (cloud.Root != null)
                Object.Destroy(cloud.Root);
        }

        private void ResetSpawnTimer()
        {
            _spawnTimer = Random.Range(_settings.SpawnIntervalRange.x, _settings.SpawnIntervalRange.y);
        }

        private readonly struct CameraBounds
        {
            public readonly float MinX;
            public readonly float MaxX;
            public readonly float MinY;
            public readonly float MaxY;

            public CameraBounds(float minX, float maxX, float minY, float maxY)
            {
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
            }
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

        private sealed class CloudInstance
        {
            public readonly GameObject Root;
            public readonly SpriteRenderer CloudRenderer;
            public readonly SpriteRenderer ShadowRenderer;
            public readonly float Speed;
            public readonly int Direction;
            public readonly float EndX;
            public readonly float Lifetime;
            public float Age;

            public CloudInstance(
                GameObject root,
                SpriteRenderer cloudRenderer,
                SpriteRenderer shadowRenderer,
                float speed,
                int direction,
                float endX,
                float lifetime)
            {
                Root = root;
                CloudRenderer = cloudRenderer;
                ShadowRenderer = shadowRenderer;
                Speed = speed;
                Direction = direction;
                EndX = endX;
                Lifetime = lifetime;
            }
        }
    }
}