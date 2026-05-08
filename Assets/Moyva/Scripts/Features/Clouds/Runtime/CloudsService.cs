using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
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
        private readonly List<CloudInstance> _clouds = new List<CloudInstance>();

        private UnityEngine.Camera _camera;
        private Transform _root;
        private bool _ownsRoot;
        private float _spawnTimer;
        private int _pendingInitialClouds;

        public int ActiveCloudsCount => _clouds.Count;

        public CloudsService(CloudsSettings settings, CloudsSceneReferences sceneReferences)
        {
            _settings = settings;
            _sceneReferences = sceneReferences;
        }

        public void Initialize()
        {
            _camera = _sceneReferences.SceneCamera != null ? _sceneReferences.SceneCamera : UnityEngine.Camera.main;
            _root = ResolveRoot();
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

            TrySpawnInitialClouds();
            TickClouds();
            TickSpawn();
        }

        public void Dispose()
        {
            ClearClouds();
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

            CameraBounds bounds = ResolveCameraBounds();
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
                shadowObject.transform.localPosition = new Vector3(_settings.ShadowOffset.x, _settings.ShadowOffset.y, 0f);
                shadowObject.transform.localScale = Vector3.one * _settings.ShadowScaleMultiplier;
                shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
                shadowRenderer.sprite = sprite;
                ApplySorting(shadowRenderer, _settings.SortingOrder + _settings.ShadowSortingOrderOffset);
            }

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
            if (_settings.FadeDuration <= 0f)
                return 1f;

            float fadeIn = Mathf.Clamp01(cloud.Age / _settings.FadeDuration);
            float remainingDistance = cloud.Direction > 0
                ? cloud.EndX - cloud.Root.transform.position.x
                : cloud.Root.transform.position.x - cloud.EndX;
            float fadeOutDistance = Mathf.Max(0.001f, cloud.Speed * _settings.FadeDuration);
            float fadeOut = Mathf.Clamp01(remainingDistance / fadeOutDistance);
            float dissolve = ResolveDissolveFade(cloud);
            return Mathf.Min(fadeIn, fadeOut, dissolve);
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
            shadowColor.a *= _settings.CloudAlpha * _settings.ShadowAlphaMultiplier * fade;
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

        private float ResolveLifetime()
        {
            return Random.Range(_settings.LifetimeRange.x, _settings.LifetimeRange.y);
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