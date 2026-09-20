using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Clouds.Runtime
{
    /// <summary>CloudsWorldPresenter — class: Clouds світу презентера.</summary>
    public sealed class CloudsWorldPresenter : IDisposable
    {
        private static readonly int FadePropertyId = Shader.PropertyToID("_MoyvaCloudFade");
        private static readonly int TintPropertyId = Shader.PropertyToID("_MoyvaCloudTint");

        private readonly CloudsSettings _settings;
        private readonly Func<Rect> _boundsProvider;
        private readonly Transform _parent;
        private readonly int _layer;
        private readonly int _maxClouds;
        private readonly int _initialClouds;
        private readonly float _speedMultiplier;
        private readonly System.Random _random;

        private readonly List<CloudInstance> _active = new List<CloudInstance>();
        private readonly List<PooledInstance> _pool = new List<PooledInstance>();

        private UnityEngine.Camera _camera;
        private float _spawnTimer;
        private int _pendingInitialClouds;
        private bool _initialized;

        /// <summary>активної Clouds кількості — int.</summary>
        public int ActiveCloudsCount => _active.Count;

        /// <summary>Виконує CloudsWorldPresenter.</summary>
        public CloudsWorldPresenter(
            CloudsSettings settings,
            Func<Rect> boundsProvider,
            UnityEngine.Camera camera = null,
            Transform parent = null,
            int layer = -1,
            int maxCloudsOverride = -1,
            int initialCloudsOverride = -1,
            float speedMultiplier = 1f,
            System.Random random = null)
        {
            _settings = settings;
            _boundsProvider = boundsProvider;
            _camera = camera;
            _parent = parent;
            _layer = layer;
            _maxClouds = maxCloudsOverride >= 0 ? Mathf.Min(maxCloudsOverride, settings.MaxActiveClouds) : settings.MaxActiveClouds;
            _initialClouds = initialCloudsOverride >= 0 ? Mathf.Min(initialCloudsOverride, settings.InitialClouds) : settings.InitialClouds;
            _speedMultiplier = Mathf.Max(0.01f, speedMultiplier);
            _random = random ?? new System.Random(Environment.TickCount);
        }

        /// <summary>Ініціалізує компонент і підписує на події.</summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            ResetSpawnTimer();
            _pendingInitialClouds = Mathf.Min(_initialClouds, _maxClouds);
            TrySpawnInitialClouds();
        }

        /// <summary>Оновлює стан за тік.</summary>
        public void Tick(float deltaTime)
        {
            if (!_initialized)
                return;

            if (_settings == null || !_settings.Enabled)
            {
                ClearClouds();
                return;
            }

            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            TrySpawnInitialClouds();
            TickClouds(deltaTime);
            TickSpawn(deltaTime);
        }

        /// <summary>Спавнить хмари.</summary>
        public void SpawnCloud()
        {
            SpawnCloudInternal(startInView: false);
        }

        /// <summary>Очищує Clouds.</summary>
        public void ClearClouds()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                ReleaseToPool(_active[i]);

            _active.Clear();
        }

        /// <summary>Звільняє ресурси та відписує від подій.</summary>
        public void Dispose()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                DestroyInstance(_active[i].Pooled);

            _active.Clear();

            for (int i = 0; i < _pool.Count; i++)
                DestroyInstance(_pool[i]);

            _pool.Clear();
            _initialized = false;
        }

        private void TrySpawnInitialClouds()
        {
            if (_pendingInitialClouds <= 0 || !HasUsablePrefabs())
                return;

            while (_pendingInitialClouds > 0 && _active.Count < _maxClouds)
            {
                if (!SpawnCloudInternal(_settings.InitialCloudsStartInView))
                    return;

                _pendingInitialClouds--;
            }
        }

        private void TickSpawn(float deltaTime)
        {
            if (_active.Count >= _maxClouds || !HasUsablePrefabs())
                return;

            _spawnTimer -= deltaTime;
            if (_spawnTimer > 0f)
                return;

            SpawnCloudInternal(startInView: false);
            ResetSpawnTimer();
        }

        private void TickClouds(float deltaTime)
        {
            float cameraFade = ResolveCameraProximityFade();
            Rect bounds = ResolveWorldBounds();
            Rect despawnBounds = Expand(bounds, _settings.DespawnPadding);

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                CloudInstance cloud = _active[i];
                if (cloud.Pooled.Root == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                cloud.Age += deltaTime;

                Transform rootTransform = cloud.Pooled.Root.transform;
                Vector3 position = rootTransform.position + cloud.Velocity * deltaTime;
                position.y = cloud.Altitude + ComputeBobOffset(cloud);
                rootTransform.position = position;

                if (cloud.YawDegreesPerSecond != 0f)
                {
                    Vector3 euler = rootTransform.eulerAngles;
                    euler.y += cloud.YawDegreesPerSecond * deltaTime;
                    rootTransform.eulerAngles = euler;
                }

                float fade = ResolveFade(cloud, bounds, cameraFade);
                ApplyVisualState(cloud, fade);

                if (!despawnBounds.Contains(new Vector2(position.x, position.z)) || HasDissolved(cloud))
                {
                    ReleaseToPool(cloud);
                    _active.RemoveAt(i);
                }
            }
        }

        private bool SpawnCloudInternal(bool startInView)
        {
            if (_active.Count >= _maxClouds)
                return false;

            int variantIndex = PickVariantIndex();
            if (variantIndex < 0)
                return false;

            CloudPrefabVariant variant = _settings.CloudPrefabs[variantIndex];
            Rect bounds = ResolveWorldBounds();
            if (bounds.width <= 0.01f || bounds.height <= 0.01f)
                return false;

            int direction = NextFloat() <= _settings.LeftToRightChance ? 1 : -1;
            float angleJitter = Mathf.Lerp(-_settings.DirectionAngleJitterDegrees, _settings.DirectionAngleJitterDegrees, NextFloat());
            float angleRad = angleJitter * Mathf.Deg2Rad;
            var velocity = new Vector3(Mathf.Cos(angleRad) * direction, 0f, Mathf.Sin(angleRad));

            float altitude = Mathf.Lerp(_settings.AltitudeRange.x, _settings.AltitudeRange.y, NextFloat());
            float altitude01 = Mathf.InverseLerp(_settings.AltitudeRange.x, Mathf.Max(_settings.AltitudeRange.x + 0.01f, _settings.AltitudeRange.y), altitude);

            Rect spawnBounds = Expand(bounds, _settings.SpawnPadding);
            Vector3 position = PickSpawnPosition(spawnBounds, velocity, direction, altitude, startInView);
            float exitDistance = ResolveExitDistance(position, velocity, Expand(bounds, _settings.DespawnPadding));

            PooledInstance pooled = AcquireInstance(variantIndex, variant);
            if (pooled == null)
                return false;

            float scale = _settings.EvaluateCloudScale(altitude01, NextFloat()) * variant.ScaleMultiplier;
            float nonUniform = Mathf.Lerp(1f - _settings.NonUniformScaleJitter, 1f + _settings.NonUniformScaleJitter, NextFloat());

            Transform rootTransform = pooled.Root.transform;
            if (_parent != null)
                rootTransform.SetParent(_parent, worldPositionStays: false);

            rootTransform.position = position;
            rootTransform.rotation = Quaternion.Euler(0f, NextFloat() * 360f, 0f);
            rootTransform.localScale = new Vector3(scale * nonUniform, scale, scale / Mathf.Max(0.01f, nonUniform));

            var cloud = new CloudInstance(
                pooled,
                velocity * Mathf.Lerp(_settings.SpeedRange.x, _settings.SpeedRange.y, NextFloat()) * _speedMultiplier,
                exitDistance,
                ResolveLifetime(),
                altitude,
                NextFloat() * Mathf.PI * 2f,
                Mathf.Lerp(-_settings.MaxYawDegreesPerSecond, _settings.MaxYawDegreesPerSecond, NextFloat()),
                ResolveTint());

            if (startInView)
                cloud.Age = _settings.FadeDuration;

            _active.Add(cloud);
            ApplyVisualState(cloud, startInView ? ResolveFade(cloud, bounds, ResolveCameraProximityFade()) : 0f);
            return true;
        }

        private Vector3 PickSpawnPosition(Rect spawnBounds, Vector3 velocity, int direction, float altitude, bool startInView)
        {
            Vector3 fallback = new Vector3(spawnBounds.center.x, altitude, spawnBounds.center.y);
            int attempts = Mathf.Max(1, _settings.SpawnPlacementAttempts);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                Vector3 position;
                if (startInView)
                {
                    position = new Vector3(
                        Mathf.Lerp(spawnBounds.xMin, spawnBounds.xMax, NextFloat()),
                        altitude,
                        Mathf.Lerp(spawnBounds.yMin, spawnBounds.yMax, NextFloat()));
                }
                else
                {
                    // Spawn just past the edge the cloud enters through, projected back along its velocity.
                    float entryX = direction > 0 ? spawnBounds.xMin : spawnBounds.xMax;
                    float z = Mathf.Lerp(spawnBounds.yMin, spawnBounds.yMax, NextFloat());
                    float backTrack = Mathf.Abs(velocity.x) > 0.001f
                        ? Mathf.Abs(velocity.z / velocity.x) * Mathf.Abs(z - spawnBounds.center.y) * 0.25f
                        : 0f;
                    position = new Vector3(entryX - Mathf.Sign(velocity.x) * backTrack, altitude, z);
                }

                fallback = position;
                if (IsFarEnoughFromExistingClouds(position))
                    return position;
            }

            return fallback;
        }

        private bool IsFarEnoughFromExistingClouds(Vector3 position)
        {
            float minimumDistance = Mathf.Max(0f, _settings.MinimumSpawnDistance);
            if (minimumDistance <= 0f)
                return true;

            float sqrDistance = minimumDistance * minimumDistance;
            for (int i = 0; i < _active.Count; i++)
            {
                CloudInstance cloud = _active[i];
                if (cloud.Pooled.Root == null)
                    continue;

                Vector3 other = cloud.Pooled.Root.transform.position;
                float dx = other.x - position.x;
                float dz = other.z - position.z;
                if (dx * dx + dz * dz < sqrDistance)
                    return false;
            }

            return true;
        }

        private float ResolveExitDistance(Vector3 position, Vector3 direction, Rect despawnBounds)
        {
            // Distance along the velocity until the XZ point leaves the expanded bounds.
            float best = float.MaxValue;
            if (Mathf.Abs(direction.x) > 0.0001f)
            {
                float tx = direction.x > 0f
                    ? (despawnBounds.xMax - position.x) / direction.x
                    : (despawnBounds.xMin - position.x) / direction.x;
                if (tx > 0f)
                    best = Mathf.Min(best, tx);
            }

            if (Mathf.Abs(direction.z) > 0.0001f)
            {
                float tz = direction.z > 0f
                    ? (despawnBounds.yMax - position.z) / direction.z
                    : (despawnBounds.yMin - position.z) / direction.z;
                if (tz > 0f)
                    best = Mathf.Min(best, tz);
            }

            return best == float.MaxValue ? despawnBounds.width + despawnBounds.height : best;
        }

        private float ResolveFade(CloudInstance cloud, Rect bounds, float cameraFade)
        {
            float fadeIn = 1f;
            float fadeOut = 1f;
            if (_settings.FadeDuration > 0f)
            {
                fadeIn = Mathf.Clamp01(cloud.Age / _settings.FadeDuration);
                float remaining = cloud.ExitDistance - cloud.SpeedMagnitude * cloud.Age;
                float fadeOutDistance = Mathf.Max(0.001f, cloud.SpeedMagnitude * _settings.FadeDuration);
                fadeOut = Mathf.Clamp01(remaining / fadeOutDistance);
            }

            float dissolve = ResolveDissolveFade(cloud);
            float edgeFade = ResolveMapEdgeFade(cloud, bounds);
            return Mathf.Min(fadeIn, fadeOut, dissolve, edgeFade, cameraFade);
        }

        private float ResolveCameraProximityFade()
        {
            if (!_settings.CameraProximityFadeEnabled || _camera == null)
                return 1f;

            float currentZoom = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            float t = Mathf.InverseLerp(_settings.CameraFadeZoomRange.x, _settings.CameraFadeZoomRange.y, currentZoom);
            return Mathf.Lerp(_settings.CloseCameraAlphaMultiplier, 1f, Mathf.SmoothStep(0f, 1f, t));
        }

        private float ResolveDissolveFade(CloudInstance cloud)
        {
            if (!_settings.LifetimeDissolveEnabled || cloud.Age <= cloud.Lifetime)
                return 1f;

            if (_settings.DissolveDuration <= 0f)
                return 0f;

            return 1f - Mathf.Clamp01((cloud.Age - cloud.Lifetime) / _settings.DissolveDuration);
        }

        private float ResolveMapEdgeFade(CloudInstance cloud, Rect bounds)
        {
            if (!_settings.MapEdgeFadeEnabled || _settings.MapEdgeFadeWidth <= 0f)
                return 1f;

            Vector3 position = cloud.Pooled.Root.transform.position;
            float fadeWidth = Mathf.Max(0.001f, _settings.MapEdgeFadeWidth);
            float left = Mathf.Clamp01((position.x - bounds.xMin) / fadeWidth);
            float right = Mathf.Clamp01((bounds.xMax - position.x) / fadeWidth);
            float bottom = Mathf.Clamp01((position.z - bounds.yMin) / fadeWidth);
            float top = Mathf.Clamp01((bounds.yMax - position.z) / fadeWidth);
            return Mathf.Min(left, right, bottom, top);
        }

        private bool HasDissolved(CloudInstance cloud)
        {
            return _settings.LifetimeDissolveEnabled && cloud.Age >= cloud.Lifetime + _settings.DissolveDuration;
        }

        private float ComputeBobOffset(CloudInstance cloud)
        {
            if (_settings.VerticalBobAmplitude <= 0f || _settings.VerticalBobSpeed <= 0f)
                return 0f;

            return Mathf.Sin(cloud.BobPhase + cloud.Age * _settings.VerticalBobSpeed * Mathf.PI * 2f) * _settings.VerticalBobAmplitude;
        }

        private void ApplyVisualState(CloudInstance cloud, float fade)
        {
            float alpha = Mathf.Clamp01(_settings.CloudAlpha * fade);
            PooledInstance pooled = cloud.Pooled;

            MaterialPropertyBlock block = pooled.PropertyBlock;
            for (int i = 0; i < pooled.Renderers.Length; i++)
            {
                Renderer renderer = pooled.Renderers[i];
                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(block);
                block.SetFloat(FadePropertyId, alpha);
                block.SetColor(TintPropertyId, cloud.Tint);
                renderer.SetPropertyBlock(block);
            }
        }

        private Color ResolveTint()
        {
            Color tint = _settings.CloudColor;
            if (_settings.CloudTintJitter > 0f)
            {
                float brightness = Mathf.Lerp(1f - _settings.CloudTintJitter, 1f + _settings.CloudTintJitter, NextFloat());
                tint = new Color(
                    Mathf.Clamp01(tint.r * brightness),
                    Mathf.Clamp01(tint.g * brightness),
                    Mathf.Clamp01(tint.b * brightness),
                    tint.a);
            }

            return tint;
        }

        private PooledInstance AcquireInstance(int variantIndex, CloudPrefabVariant variant)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].VariantIndex == variantIndex && _pool[i].Root != null)
                {
                    PooledInstance pooled = _pool[i];
                    _pool.RemoveAt(i);
                    pooled.Root.SetActive(true);
                    ApplyShadowMode(pooled);
                    return pooled;
                }
            }

            // Keep memory bounded: recycle an idle pooled instance of a different variant.
            if (_pool.Count > 0 && _active.Count + _pool.Count >= Mathf.Max(_maxClouds * 2, _maxClouds + _settings.CloudPrefabs.Length))
            {
                DestroyInstance(_pool[0]);
                _pool.RemoveAt(0);
            }

            GameObject instance = Object.Instantiate(variant.Prefab, _parent, worldPositionStays: false);
            instance.name = variant.Prefab.name + "_Pooled";
            if (_layer >= 0)
                SetLayerRecursively(instance, _layer);

            var pooledInstance = new PooledInstance
            {
                Root = instance,
                VariantIndex = variantIndex,
                Renderers = instance.GetComponentsInChildren<Renderer>(includeInactive: true),
                PropertyBlock = new MaterialPropertyBlock(),
            };
            ApplyShadowMode(pooledInstance);
            return pooledInstance;
        }

        private void ApplyShadowMode(PooledInstance pooled)
        {
            ShadowCastingMode mode = _settings.ShadowsEnabled ? ShadowCastingMode.On : ShadowCastingMode.Off;
            for (int i = 0; i < pooled.Renderers.Length; i++)
            {
                if (pooled.Renderers[i] != null)
                    pooled.Renderers[i].shadowCastingMode = mode;
            }
        }

        private void ReleaseToPool(CloudInstance cloud)
        {
            if (cloud.Pooled.Root != null)
                cloud.Pooled.Root.SetActive(false);

            _pool.Add(cloud.Pooled);
        }

        private void DestroyInstance(PooledInstance pooled)
        {
            if (pooled?.Root != null)
                Object.Destroy(pooled.Root);
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            Transform child = target.transform;
            for (int i = 0; i < child.childCount; i++)
                SetLayerRecursively(child.GetChild(i).gameObject, layer);
        }

        private int PickVariantIndex()
        {
            CloudPrefabVariant[] variants = _settings.CloudPrefabs;
            if (variants == null || variants.Length == 0)
                return -1;

            float totalChance = 0f;
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i]?.Prefab != null)
                    totalChance += Mathf.Max(0f, variants[i].Chance);
            }

            if (totalChance <= 0f)
                return -1;

            float roll = NextFloat() * totalChance;
            float cursor = 0f;
            for (int i = 0; i < variants.Length; i++)
            {
                CloudPrefabVariant variant = variants[i];
                if (variant?.Prefab == null)
                    continue;

                cursor += Mathf.Max(0f, variant.Chance);
                if (roll <= cursor)
                    return i;
            }

            return -1;
        }

        private bool HasUsablePrefabs()
        {
            CloudPrefabVariant[] variants = _settings?.CloudPrefabs;
            if (variants == null)
                return false;

            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i]?.Prefab != null && variants[i].Chance > 0f)
                    return true;
            }

            return false;
        }

        private Rect ResolveWorldBounds()
        {
            if (_settings.SpawnAreaMode == CloudSpawnAreaMode.MapBounds || _camera == null)
                return _boundsProvider != null ? _boundsProvider() : ResolveManualBounds();

            Rect cameraBounds = ResolveCameraFootprintBounds();
            Rect mapBounds = _boundsProvider != null ? _boundsProvider() : ResolveManualBounds();
            if (!_settings.MapEdgeFadeEnabled)
                return cameraBounds;

            // Viewport mode still respects map limits when they are known.
            float minX = Mathf.Max(cameraBounds.xMin, mapBounds.xMin);
            float maxX = Mathf.Min(cameraBounds.xMax, mapBounds.xMax);
            float minY = Mathf.Max(cameraBounds.yMin, mapBounds.yMin);
            float maxY = Mathf.Min(cameraBounds.yMax, mapBounds.yMax);
            if (minX >= maxX || minY >= maxY)
                return mapBounds;

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private Rect ResolveCameraFootprintBounds()
        {
            // Approximate footprint of the camera frustum on the cloud altitude plane.
            float altitude = (_settings.AltitudeRange.x + _settings.AltitudeRange.y) * 0.5f;
            Vector3 cameraPosition = _camera.transform.position;
            Vector3 forward = _camera.transform.forward;
            float distance = Mathf.Abs(forward.y) > 0.01f
                ? (altitude - cameraPosition.y) / forward.y
                : _camera.farClipPlane * 0.5f;
            distance = Mathf.Max(1f, distance);

            float halfHeight = _camera.orthographic
                ? _camera.orthographicSize
                : Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            float halfWidth = halfHeight * Mathf.Max(0.01f, _camera.aspect);

            Vector3 center = cameraPosition + forward * distance;
            return Rect.MinMaxRect(
                center.x - halfWidth, center.z - halfHeight,
                center.x + halfWidth, center.z + halfHeight);
        }

        private Rect ResolveManualBounds()
        {
            Vector2 halfSize = _settings.ManualMapSize * 0.5f;
            return Rect.MinMaxRect(
                _settings.ManualMapCenter.x - halfSize.x,
                _settings.ManualMapCenter.y - halfSize.y,
                _settings.ManualMapCenter.x + halfSize.x,
                _settings.ManualMapCenter.y + halfSize.y);
        }

        private static Rect Expand(Rect rect, float padding)
        {
            return Rect.MinMaxRect(rect.xMin - padding, rect.yMin - padding, rect.xMax + padding, rect.yMax + padding);
        }

        private float ResolveLifetime()
        {
            return Mathf.Lerp(_settings.LifetimeRange.x, _settings.LifetimeRange.y, NextFloat());
        }

        private void ResetSpawnTimer()
        {
            _spawnTimer = Mathf.Lerp(_settings.SpawnIntervalRange.x, _settings.SpawnIntervalRange.y, NextFloat());
        }

        private float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>Чи інстанс перебуває в пулі.</summary>
        private sealed class PooledInstance
        {
            /// <summary>кореня — GameObject.</summary>
            public GameObject Root;
            /// <summary>Рендерери хмарного інстанса.</summary>
            public Renderer[] Renderers;
            /// <summary>властивості Block — MaterialPropertyBlock.</summary>
            public MaterialPropertyBlock PropertyBlock;
            /// <summary>варіанту індексу — int.</summary>
            public int VariantIndex;
        }

        /// <summary>CloudInstance — class: хмари інстанса.</summary>
        private sealed class CloudInstance
        {
            /// <summary>Чи інстанс перебуває в пулі.</summary>
            public readonly PooledInstance Pooled;
            /// <summary>швидкості — Vector3.</summary>
            public readonly Vector3 Velocity;
            /// <summary>швидкість Magnitude — float.</summary>
            public readonly float SpeedMagnitude;
            /// <summary>виходу відстані — float.</summary>
            public readonly float ExitDistance;
            /// <summary>життєвого циклу — float.</summary>
            public readonly float Lifetime;
            /// <summary>висоти — float.</summary>
            public readonly float Altitude;
            /// <summary>гойдання фази — float.</summary>
            public readonly float BobPhase;
            /// <summary>рискання у градусах на секунду — float.</summary>
            public readonly float YawDegreesPerSecond;
            /// <summary>тінтування — Color.</summary>
            public readonly Color Tint;
            /// <summary>віку — float.</summary>
            public float Age;

            /// <summary>Виконує CloudInstance.</summary>
            public CloudInstance(
                PooledInstance pooled,
                Vector3 velocity,
                float exitDistance,
                float lifetime,
                float altitude,
                float bobPhase,
                float yawDegreesPerSecond,
                Color tint)
            {
                Pooled = pooled;
                Velocity = velocity;
                SpeedMagnitude = velocity.magnitude;
                ExitDistance = exitDistance;
                Lifetime = lifetime;
                Altitude = altitude;
                BobPhase = bobPhase;
                YawDegreesPerSecond = yawDegreesPerSecond;
                Tint = tint;
            }
        }
    }
}
