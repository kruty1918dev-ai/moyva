using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed partial class HomeMenuBackgroundPreviewController
    {
        private void EnsureCloudLayer()
        {
            if (_cloudsLayer != null)
                return;

            Transform parent = null;
            if (_targetImage != null && _targetImage.rectTransform.parent != null)
                parent = _targetImage.rectTransform.parent;
            else if (transform is RectTransform)
                parent = transform.parent != null ? transform.parent : transform;

            if (parent == null)
                return;

            var existing = parent.Find("MenuCloudsLayer");
            if (existing is RectTransform existingRect)
            {
                _cloudsLayer = existingRect;
                return;
            }

            var cloudLayerObject = new GameObject("MenuCloudsLayer", typeof(RectTransform));
            _cloudsLayer = cloudLayerObject.GetComponent<RectTransform>();
            _cloudsLayer.SetParent(parent, false);
            _cloudsLayer.anchorMin = Vector2.zero;
            _cloudsLayer.anchorMax = Vector2.one;
            _cloudsLayer.offsetMin = Vector2.zero;
            _cloudsLayer.offsetMax = Vector2.zero;
            _cloudsLayer.SetAsLastSibling();
            _ownsCloudLayer = true;
        }

        private void ResetClouds()
        {
            ClearClouds();

            if (_cloudsSettings == null || !_cloudsSettings.Enabled || _cloudsLayer == null)
                return;

            _random = new System.Random(_currentSeed == 0 ? Guid.NewGuid().GetHashCode() : _currentSeed ^ 0x5f3759df);
            _pendingInitialClouds = Mathf.Min(ResolveEffectiveInitialClouds(), ResolveEffectiveMaxClouds());
            ResetSpawnTimer();
            TrySpawnInitialClouds();
        }

        private void TickClouds(float deltaTime)
        {
            if (_cloudsSettings == null || !_cloudsSettings.Enabled || _cloudsLayer == null)
                return;

            if (_cloudsLayer.rect.width <= 1f || _cloudsLayer.rect.height <= 1f)
                return;

            TrySpawnInitialClouds();

            for (int i = _clouds.Count - 1; i >= 0; i--)
            {
                var cloud = _clouds[i];
                if (cloud.RootRectTransform == null || cloud.CloudImage == null)
                {
                    DestroyMenuCloud(cloud);
                    _clouds.RemoveAt(i);
                    continue;
                }

                cloud.Age += deltaTime;
                var position = cloud.RootRectTransform.anchoredPosition;
                position.x += cloud.Direction * cloud.Speed * deltaTime;
                cloud.RootRectTransform.anchoredPosition = position;

                float fadeDuration = Mathf.Max(0.01f, _cloudsSettings.FadeDuration);
                float fadeIn = Mathf.Clamp01(cloud.Age / fadeDuration);
                float fadeDistance = Mathf.Max(1f, cloud.Speed * fadeDuration);
                float distanceToEnd = Mathf.Abs(cloud.EndX - position.x);
                float fadeOut = Mathf.Clamp01(distanceToEnd / fadeDistance);
                float alpha = Mathf.Min(fadeIn, fadeOut) * Mathf.Clamp01(_cloudsSettings.CloudAlpha);

                var color = _cloudsSettings.CloudColor;
                color.a = alpha;
                cloud.CloudImage.color = color;

                if (cloud.ShadowImage != null)
                {
                    var shadowColor = _cloudsSettings.ShadowColor;
                    shadowColor.a *= alpha * ResolveMenuShadowAlphaMultiplier(cloud.VisualHeight);
                    cloud.ShadowImage.color = shadowColor;
                }

                if ((cloud.Direction > 0 && position.x >= cloud.EndX) || (cloud.Direction < 0 && position.x <= cloud.EndX))
                {
                    DestroyMenuCloud(cloud);
                    _clouds.RemoveAt(i);
                }
            }

            if (_clouds.Count >= ResolveEffectiveMaxClouds())
                return;

            _spawnTimer -= deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnCloud(startInView: false);
                ResetSpawnTimer();
            }
        }

        private void TrySpawnInitialClouds()
        {
            if (_pendingInitialClouds <= 0 || _cloudsLayer == null || _cloudsLayer.rect.width <= 1f || _cloudsLayer.rect.height <= 1f)
                return;

            while (_pendingInitialClouds > 0 && _clouds.Count < ResolveEffectiveMaxClouds())
            {
                if (!SpawnCloud(startInView: true))
                    return;

                _pendingInitialClouds--;
            }
        }

        private bool SpawnCloud(bool startInView)
        {
            if (!TryPickSprite(out var sprite) || _cloudsLayer == null)
                return false;

            var bounds = _cloudsLayer.rect;
            if (bounds.width <= 1f || bounds.height <= 1f)
                return false;

            int direction = NextFloat() <= _cloudsSettings.LeftToRightChance ? 1 : -1;

            float left = -bounds.width * 0.5f;
            float right = bounds.width * 0.5f;
            float probeCloudHeight = bounds.height * _cloudHeightRatio * Mathf.Max(0.35f, _cloudsSettings.ScaleRange.y);
            float verticalPadding = probeCloudHeight * Mathf.Max(0f, _cloudsSettings.SpawnVerticalPadding) * 0.4f;
            float x = startInView
                ? Mathf.Lerp(left, right, NextFloat())
                : direction > 0 ? left - probeCloudHeight : right + probeCloudHeight;
            float y = Mathf.Lerp(-bounds.height * 0.5f - verticalPadding, bounds.height * 0.5f + verticalPadding, NextFloat());
            float altitude01 = ResolveMenuAltitude01(bounds, y);
            float scale = _cloudsSettings.EvaluateCloudScale(altitude01, NextFloat());
            scale = Mathf.Clamp(scale, 0.35f, 1.35f);

            float cloudVisualHeight = _cloudsSettings.EvaluateCloudVisualHeight(altitude01);
            float mipBias = _cloudsSettings.EvaluateCloudMipBias(altitude01);
            float cloudHeight = bounds.height * _cloudHeightRatio * scale;
            float aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;
            float cloudWidth = cloudHeight * aspect;
            float endX = direction > 0 ? right + cloudWidth : left - cloudWidth;
            float speed = Mathf.Lerp(_cloudsSettings.SpeedRange.x, _cloudsSettings.SpeedRange.y, NextFloat())
                * Mathf.Max(8f, bounds.width / 28f)
                * Mathf.Max(0.1f, _cloudSpeedMultiplier);

            var cloudRoot = new GameObject("MenuCloud", typeof(RectTransform));
            var rectTransform = cloudRoot.GetComponent<RectTransform>();
            rectTransform.SetParent(_cloudsLayer, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(cloudWidth, cloudHeight);
            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.SetAsLastSibling();

            Material cloudMaterial = CreateCloudMaterialInstance(mipBias);
            Image shadowImage = null;
            if (_cloudsSettings.ShadowsEnabled)
            {
                shadowImage = CreateMenuCloudImage(
                    rectTransform,
                    "Shadow",
                    sprite,
                    cloudMaterial,
                    ResolveMenuShadowOffset(cloudHeight, cloudVisualHeight),
                    new Vector2(cloudWidth, cloudHeight) * ResolveMenuShadowScaleMultiplier(cloudVisualHeight),
                    new Color(
                        _cloudsSettings.ShadowColor.r,
                        _cloudsSettings.ShadowColor.g,
                        _cloudsSettings.ShadowColor.b,
                        startInView ? _cloudsSettings.CloudAlpha * ResolveMenuShadowAlphaMultiplier(cloudVisualHeight) : 0f));
            }

            var image = CreateMenuCloudImage(
                rectTransform,
                "Sprite",
                sprite,
                cloudMaterial,
                Vector2.zero,
                new Vector2(cloudWidth, cloudHeight),
                new Color(_cloudsSettings.CloudColor.r, _cloudsSettings.CloudColor.g, _cloudsSettings.CloudColor.b, startInView ? _cloudsSettings.CloudAlpha : 0f));

            _clouds.Add(new MenuCloudVisual
            {
                RootRectTransform = rectTransform,
                CloudImage = image,
                ShadowImage = shadowImage,
                MaterialInstance = cloudMaterial,
                Speed = speed,
                EndX = endX,
                Direction = direction,
                Age = startInView ? Mathf.Max(0.01f, _cloudsSettings.FadeDuration) : 0f,
                VisualHeight = cloudVisualHeight
            });

            return true;
        }

        private void ClearClouds()
        {
            for (int i = 0; i < _clouds.Count; i++)
                DestroyMenuCloud(_clouds[i]);

            _clouds.Clear();
        }

        private void DisposeGeneratedTexture()
        {
            if (_targetImage != null && ReferenceEquals(_targetImage.texture, _generatedTexture))
                _targetImage.texture = null;

            if (_generatedTexture != null)
                Destroy(_generatedTexture);

            _generatedTexture = null;
        }

        private static float ResolveMenuAltitude01(Rect bounds, float y)
        {
            float minY = -bounds.height * 0.5f;
            float maxY = bounds.height * 0.5f;
            float height = Mathf.Max(0.001f, maxY - minY);
            return Mathf.Clamp01((y - minY) / height);
        }

        private Image CreateMenuCloudImage(
            RectTransform parent,
            string name,
            Sprite sprite,
            Material material,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.SetParent(parent, false);
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = anchoredPosition;
            imageRect.sizeDelta = size;

            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.material = material;
            image.color = color;
            return image;
        }

        private Material CreateCloudMaterialInstance(float mipBias)
        {
            Material baseMaterial = ResolveCloudMaterial();
            if (baseMaterial == null)
                return null;

            var material = new Material(baseMaterial)
            {
                name = "MenuCloudRuntimeMaterialInstance",
                hideFlags = HideFlags.HideAndDontSave,
            };

            if (material.HasProperty("_MipBias"))
                material.SetFloat("_MipBias", mipBias);

            return material;
        }

        private Vector2 ResolveMenuShadowOffset(float cloudPixelHeight, float cloudVisualHeight)
        {
            Vector2 worldOffset = _cloudsSettings.ShadowOffset + _cloudsSettings.ShadowOffsetPerHeight * cloudVisualHeight;
            float pixelFactor = Mathf.Max(1f, cloudPixelHeight * 0.35f);
            return worldOffset * pixelFactor;
        }

        private float ResolveMenuShadowScaleMultiplier(float cloudVisualHeight)
        {
            return Mathf.Max(0.01f, _cloudsSettings.ShadowScaleMultiplier + _cloudsSettings.ShadowScalePerHeight * cloudVisualHeight);
        }

        private float ResolveMenuShadowAlphaMultiplier(float cloudVisualHeight)
        {
            return Mathf.Clamp01(_cloudsSettings.ShadowAlphaMultiplier / (1f + cloudVisualHeight * _cloudsSettings.ShadowAlphaHeightFade));
        }

        private void DestroyMenuCloud(MenuCloudVisual cloud)
        {
            if (cloud?.MaterialInstance != null)
                Destroy(cloud.MaterialInstance);

            if (cloud?.RootRectTransform != null)
                Destroy(cloud.RootRectTransform.gameObject);
        }

        private int ResolveEffectiveMaxClouds()
        {
            if (_cloudsSettings == null)
                return 0;

            return Mathf.Max(0, Mathf.Min(_menuMaxClouds, _cloudsSettings.MaxActiveClouds));
        }

        private int ResolveEffectiveInitialClouds()
        {
            if (_cloudsSettings == null)
                return 0;

            return Mathf.Max(0, Mathf.Min(_menuInitialClouds, _cloudsSettings.InitialClouds));
        }

        private void ResetSpawnTimer()
        {
            if (_cloudsSettings == null)
            {
                _spawnTimer = 0f;
                return;
            }

            _spawnTimer = Mathf.Lerp(_cloudsSettings.SpawnIntervalRange.x, _cloudsSettings.SpawnIntervalRange.y, NextFloat());
        }

        private bool TryPickSprite(out Sprite sprite)
        {
            sprite = null;
            if (_cloudsSettings?.CloudSprites == null || _cloudsSettings.CloudSprites.Length == 0)
                return false;

            float totalWeight = 0f;
            for (int i = 0; i < _cloudsSettings.CloudSprites.Length; i++)
            {
                var variant = _cloudsSettings.CloudSprites[i];
                if (variant?.Sprite == null || variant.Chance <= 0f)
                    continue;

                totalWeight += variant.Chance;
            }

            if (totalWeight <= 0f)
                return false;

            float roll = NextFloat() * totalWeight;
            float accumulated = 0f;
            for (int i = 0; i < _cloudsSettings.CloudSprites.Length; i++)
            {
                var variant = _cloudsSettings.CloudSprites[i];
                if (variant?.Sprite == null || variant.Chance <= 0f)
                    continue;

                accumulated += variant.Chance;
                if (roll <= accumulated)
                {
                    sprite = variant.Sprite;
                    return true;
                }
            }

            return false;
        }

        private float NextFloat()
        {
            if (_random == null)
                _random = new System.Random(Guid.NewGuid().GetHashCode());

            return (float)_random.NextDouble();
        }

        private Material ResolveCloudMaterial()
        {
            if (_cloudsSettings != null && _cloudsSettings.SpriteMaterial != null)
                return _cloudsSettings.SpriteMaterial;

            if (_runtimeCloudMaterial != null)
                return _runtimeCloudMaterial;

            Shader shader = Shader.Find(CloudMipLodShaderName);
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
                return null;

            _runtimeCloudMaterial = new Material(shader)
            {
                name = "MenuCloudsRuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave,
            };
            return _runtimeCloudMaterial;
        }

        private void DestroyRuntimeCloudMaterial()
        {
            if (_runtimeCloudMaterial == null)
                return;

            Destroy(_runtimeCloudMaterial);
            _runtimeCloudMaterial = null;
        }

        private sealed class MenuCloudVisual
        {
            public RectTransform RootRectTransform;
            public Image CloudImage;
            public Image ShadowImage;
            public Material MaterialInstance;
            public float Speed;
            public float EndX;
            public int Direction;
            public float Age;
            public float VisualHeight;
        }
    }
}
