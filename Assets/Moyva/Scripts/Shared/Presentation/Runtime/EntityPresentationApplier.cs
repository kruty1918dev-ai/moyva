using Kruty1918.Moyva.Presentation.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Presentation.Runtime
{
    public static class EntityPresentationApplier
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int TeamColorPropertyId = Shader.PropertyToID("_TeamColor");
        private static readonly int TeamColorEnabledPropertyId = Shader.PropertyToID("_TeamColorEnabled");
        private static readonly int OutlineEnabledPropertyId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineWidthPropertyId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int OutlineColorPropertyId = Shader.PropertyToID("_OutlineColor");

        public static Vector3 ResolveScale(
            Vector3 authoredLocalScale,
            EntityPresentationConfig presentation)
            => presentation == null
                ? authoredLocalScale
                : authoredLocalScale * presentation.ResolveScaleMultiplier();

        public static Quaternion ResolveRotation(
            Quaternion baseRotation,
            EntityPresentationConfig presentation)
        {
            if (presentation == null || presentation.RotationOffset == Vector3.zero)
                return baseRotation;

            return baseRotation * Quaternion.Euler(presentation.RotationOffset);
        }

        public static Vector3 ResolvePosition(
            Vector3 canonicalPosition,
            Quaternion baseRotation,
            EntityPresentationConfig presentation,
            float fallbackGroundOffsetY = 0f)
        {
            float groundOffsetY =
                presentation?.ResolveGroundOffsetY(fallbackGroundOffsetY)
                ?? fallbackGroundOffsetY;

            return ResolvePositionOffset(
                canonicalPosition + Vector3.up * groundOffsetY,
                baseRotation,
                presentation);
        }

        public static Vector3 ResolvePositionOffset(
            Vector3 canonicalPosition,
            Quaternion baseRotation,
            EntityPresentationConfig presentation)
        {
            Vector3 offset =
                presentation != null
                    ? baseRotation * presentation.PositionOffset
                    : Vector3.zero;

            return canonicalPosition + offset;
        }

        public static void ApplyTransform(
            GameObject instance,
            EntityPresentationConfig presentation,
            Vector3 canonicalPosition,
            Quaternion baseRotation,
            Vector3 authoredLocalScale,
            float fallbackGroundOffsetY = 0f)
        {
            if (instance == null)
                return;

            Transform transform = instance.transform;
            transform.localScale = ResolveScale(authoredLocalScale, presentation);
            transform.rotation = ResolveRotation(baseRotation, presentation);
            transform.position = ResolvePosition(
                canonicalPosition,
                baseRotation,
                presentation,
                fallbackGroundOffsetY);
        }

        public static void ApplyPosition(
            GameObject instance,
            EntityPresentationConfig presentation,
            Vector3 canonicalPosition,
            Quaternion baseRotation,
            float fallbackGroundOffsetY = 0f)
        {
            if (instance == null)
                return;

            instance.transform.position = ResolvePosition(
                canonicalPosition,
                baseRotation,
                presentation,
                fallbackGroundOffsetY);
        }

        public static void ApplyPositionOffset(
            GameObject instance,
            EntityPresentationConfig presentation,
            Vector3 canonicalPosition,
            Quaternion baseRotation)
        {
            if (instance == null)
                return;

            instance.transform.position = ResolvePositionOffset(
                canonicalPosition,
                baseRotation,
                presentation);
        }

        public static void ApplyStyleAndShadows(
            GameObject instance,
            EntityPresentationConfig presentation,
            Color? teamColor = null)
        {
            if (instance == null || presentation == null)
                return;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
                return;

            var block = new MaterialPropertyBlock();
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null)
                    continue;

                ApplyRendererStyle(renderer, presentation, teamColor, block);
                ApplyRendererShadows(renderer, presentation.Shadows);
            }
        }

        private static void ApplyRendererStyle(
            Renderer renderer,
            EntityPresentationConfig presentation,
            Color? teamColor,
            MaterialPropertyBlock block)
        {
            renderer.GetPropertyBlock(block);

            if (presentation.Tint.HasValue)
            {
                Color tint = presentation.Tint.Value;
                block.SetColor(ColorPropertyId, tint);
                block.SetColor(BaseColorPropertyId, tint);

                if (renderer is SpriteRenderer spriteRenderer)
                    spriteRenderer.color = tint;
            }

            if (presentation.TeamColorEnabled.HasValue)
            {
                block.SetFloat(
                    TeamColorEnabledPropertyId,
                    presentation.TeamColorEnabled.Value ? 1f : 0f);
            }

            if (presentation.TeamColorEnabled == true && teamColor.HasValue)
                block.SetColor(TeamColorPropertyId, teamColor.Value);

            EntityOutlineConfig outline = presentation.Outline;
            if (outline != null)
            {
                if (outline.Enabled.HasValue)
                {
                    block.SetFloat(
                        OutlineEnabledPropertyId,
                        outline.Enabled.Value ? 1f : 0f);
                }

                if (outline.Width.HasValue
                    && EntityPresentationConfig.IsFinite(outline.Width.Value))
                {
                    block.SetFloat(
                        OutlineWidthPropertyId,
                        Mathf.Max(0f, outline.Width.Value));
                }

                if (outline.Color.HasValue)
                    block.SetColor(OutlineColorPropertyId, outline.Color.Value);
            }

            renderer.SetPropertyBlock(block);
            block.Clear();
        }

        private static void ApplyRendererShadows(
            Renderer renderer,
            EntityShadowConfig shadows)
        {
            if (shadows == null)
                return;

            if (shadows.Cast.HasValue)
            {
                renderer.shadowCastingMode =
                    shadows.Cast.Value
                        ? ShadowCastingMode.On
                        : ShadowCastingMode.Off;
            }

            if (shadows.Receive.HasValue)
                renderer.receiveShadows = shadows.Receive.Value;
        }
    }
}
