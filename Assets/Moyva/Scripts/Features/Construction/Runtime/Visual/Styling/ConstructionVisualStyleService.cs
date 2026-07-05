using UnityEngine;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualStyleService : IConstructionVisualStyleService
    {
        private readonly float _ghostAlpha;

        public ConstructionVisualStyleService(IConstructionVisualSettingsProvider visualSettingsProvider = null)
        {
            _ghostAlpha = Mathf.Clamp01(visualSettingsProvider?.GhostAlpha ?? 0.55f);
        }

        public void ApplyGhostStyle(GameObject rootObject, bool isValid)
        {
            var tint = isValid
                ? new Color(0.55f, 1f, 0.55f, _ghostAlpha)
                : new Color(1f, 0.45f, 0.45f, _ghostAlpha);

            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = tint;

            ApplyRendererTint(rootObject, tint, isValid);
        }

        public void ApplySolidStyle(GameObject rootObject)
        {
            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteRenderers[i].color = Color.white;

            ClearRendererTint(rootObject);
        }

        public void EnsureBuildingSortingOrder(GameObject rootObject, int minOrder)
        {
            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr.sortingOrder < minOrder)
                    sr.sortingOrder = minOrder;
            }

            var sortingGroups = rootObject.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true);
            foreach (var sg in sortingGroups)
            {
                if (sg.sortingOrder < minOrder)
                    sg.sortingOrder = minOrder;
            }
        }

        public void EnsureRenderersEnabled(GameObject rootObject)
        {
            var renderers = rootObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].enabled = true;
            }
        }

        public void DisableColliders(GameObject rootObject)
        {
            var colliders3D = rootObject.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders3D.Length; i++)
                colliders3D[i].enabled = false;

            var colliders2D = rootObject.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders2D.Length; i++)
                colliders2D[i].enabled = false;
        }

        private static void ApplyRendererTint(GameObject rootObject, Color tint, bool isValid)
        {
            var renderers = rootObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor("_Color", tint);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_EmissionColor", isValid ? new Color(0.10f, 0.28f, 0.10f, 1f) : new Color(0.28f, 0.08f, 0.08f, 1f));
                renderer.SetPropertyBlock(block);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static void ClearRendererTint(GameObject rootObject)
        {
            var renderers = rootObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                renderer.SetPropertyBlock(null);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }
    }
}
