using System.Runtime.CompilerServices;
using UnityEngine;
using Kruty1918.Moyva.Construction.API;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualStyleService : IConstructionVisualStyleService
    {
        private readonly float _ghostAlpha;
        private readonly ConditionalWeakTable<GameObject, VisualComponentCache>
            _componentCache = new();
        private readonly MaterialPropertyBlock _propertyBlock = new();

        public ConstructionVisualStyleService(IConstructionVisualSettingsProvider visualSettingsProvider = null)
        {
            _ghostAlpha = Mathf.Clamp01(visualSettingsProvider?.GhostAlpha ?? 0.55f);
        }

        public void ApplyGhostStyle(GameObject rootObject, bool isValid)
        {
            var tint = isValid
                ? new Color(0.55f, 1f, 0.55f, _ghostAlpha)
                : new Color(1f, 0.45f, 0.45f, _ghostAlpha);

            VisualComponentCache cache = GetCache(rootObject);
            SpriteRenderer[] spriteRenderers = cache.SpriteRenderers;
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = tint;

            ApplyRendererTint(cache, tint, isValid);
        }

        public void ApplyUnaffordableGhostStyle(GameObject rootObject)
        {
            var tint = new Color(1f, 0.68f, 0.22f, _ghostAlpha);
            VisualComponentCache cache = GetCache(rootObject);
            SpriteRenderer[] spriteRenderers = cache.SpriteRenderers;
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = tint;

            ApplyRendererTint(
                cache,
                tint,
                isValid: true,
                emissionColor: new Color(0.30f, 0.15f, 0.03f, 1f));
        }

        public void ApplySolidStyle(GameObject rootObject)
        {
            VisualComponentCache cache = GetCache(rootObject);
            SpriteRenderer[] spriteRenderers = cache.SpriteRenderers;
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = Color.white;

            ClearRendererTint(cache);
        }

        public void EnsureBuildingSortingOrder(GameObject rootObject, int minOrder)
        {
            VisualComponentCache cache = GetCache(rootObject);
            SpriteRenderer[] spriteRenderers = cache.SpriteRenderers;
            foreach (var sr in spriteRenderers)
            {
                if (sr.sortingOrder < minOrder)
                    sr.sortingOrder = minOrder;
            }

            SortingGroup[] sortingGroups = cache.SortingGroups;
            foreach (var sg in sortingGroups)
            {
                if (sg.sortingOrder < minOrder)
                    sg.sortingOrder = minOrder;
            }
        }

        public void EnsureRenderersEnabled(GameObject rootObject)
        {
            Renderer[] renderers =
                GetCache(rootObject).Renderers;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = true;
            }
        }

        public void DisableColliders(GameObject rootObject)
        {
            VisualComponentCache cache = GetCache(rootObject);
            Collider[] colliders3D = cache.Colliders3D;
            for (int i = 0; i < colliders3D.Length; i++)
                colliders3D[i].enabled = false;

            Collider2D[] colliders2D = cache.Colliders2D;
            for (int i = 0; i < colliders2D.Length; i++)
                colliders2D[i].enabled = false;
        }

        private void ApplyRendererTint(
            VisualComponentCache cache,
            Color tint,
            bool isValid,
            Color? emissionColor = null)
        {
            Renderer[] renderers = cache.Renderers;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                _propertyBlock.Clear();
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", tint);
                _propertyBlock.SetColor("_BaseColor", tint);
                _propertyBlock.SetColor(
                    "_EmissionColor",
                    emissionColor
                    ?? (isValid
                        ? new Color(0.10f, 0.28f, 0.10f, 1f)
                        : new Color(0.28f, 0.08f, 0.08f, 1f)));
                renderer.SetPropertyBlock(_propertyBlock);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        public void ApplyUnderConstructionStyle(GameObject rootObject)
        {
            var tint = new Color(0.72f, 0.78f, 0.82f, 0.88f);
            var emission = new Color(0.20f, 0.16f, 0.08f, 1f);
            VisualComponentCache cache = GetCache(rootObject);
            SpriteRenderer[] spriteRenderers = cache.SpriteRenderers;
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = tint;

            ApplyRendererTint(
                cache,
                tint,
                isValid: true,
                emissionColor: emission);
        }

        private static void ClearRendererTint(
            VisualComponentCache cache)
        {
            Renderer[] renderers = cache.Renderers;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                renderer.SetPropertyBlock(null);
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private VisualComponentCache GetCache(GameObject rootObject)
        {
            return rootObject == null
                ? VisualComponentCache.Empty
                : _componentCache.GetValue(
                    rootObject,
                    CreateComponentCache);
        }

        private static VisualComponentCache CreateComponentCache(
            GameObject rootObject)
        {
            return new VisualComponentCache(
                rootObject.GetComponentsInChildren<Renderer>(true),
                rootObject.GetComponentsInChildren<SpriteRenderer>(true),
                rootObject.GetComponentsInChildren<SortingGroup>(true),
                rootObject.GetComponentsInChildren<Collider>(true),
                rootObject.GetComponentsInChildren<Collider2D>(true));
        }

        private sealed class VisualComponentCache
        {
            public static readonly VisualComponentCache Empty =
                new(
                    System.Array.Empty<Renderer>(),
                    System.Array.Empty<SpriteRenderer>(),
                    System.Array.Empty<SortingGroup>(),
                    System.Array.Empty<Collider>(),
                    System.Array.Empty<Collider2D>());

            public VisualComponentCache(
                Renderer[] renderers,
                SpriteRenderer[] spriteRenderers,
                SortingGroup[] sortingGroups,
                Collider[] colliders3D,
                Collider2D[] colliders2D)
            {
                Renderers =
                    renderers ?? System.Array.Empty<Renderer>();
                SpriteRenderers =
                    spriteRenderers ?? System.Array.Empty<SpriteRenderer>();
                SortingGroups =
                    sortingGroups ?? System.Array.Empty<SortingGroup>();
                Colliders3D =
                    colliders3D ?? System.Array.Empty<Collider>();
                Colliders2D =
                    colliders2D ?? System.Array.Empty<Collider2D>();
            }

            public Renderer[] Renderers { get; }
            public SpriteRenderer[] SpriteRenderers { get; }
            public SortingGroup[] SortingGroups { get; }
            public Collider[] Colliders3D { get; }
            public Collider2D[] Colliders2D { get; }
        }
    }
}
