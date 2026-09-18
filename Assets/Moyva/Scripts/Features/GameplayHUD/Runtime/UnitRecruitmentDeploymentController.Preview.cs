using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class UnitRecruitmentDeploymentController
    {
        private void MovePreviewTo(Vector2Int tile)
        {
            if (_previewObject == null)
                _previewObject = CreatePreviewObject();

            if (_previewObject == null)
                return;

            PositionPreviewObject(_previewObject, tile);
        }

        private GameObject CreatePreviewObject()
        {
            UnitClassConfig config = _unitConfigs.GetConfig(_session.UnitTypeId);
            EnsureWorldRoots();

            GameObject preview = null;
            GameObject prefab = config?.ResolvePreviewPrefab();
            if (prefab != null)
            {
                preview = Object.Instantiate(prefab, _previewRoot);
                preview.name = $"UnitDeploymentPreview_{_session.UnitTypeId}";
                PreparePrefabPreview(preview, config, prefab);
            }
            else if (config?.ResolveCustomSprite() != null)
            {
                preview = CreateSpritePreview(
                    config.ResolveCustomSprite(),
                    config);
            }
            else
            {
                preview = CreateFallbackPreview(config);
            }

            return preview;
        }

        private void PreparePrefabPreview(
            GameObject preview,
            UnitClassConfig config,
            GameObject prefab)
        {
            DisablePreviewGameplayComponents(preview);
            EntityPresentationConfig presentation = config?.ResolvePresentation();
            preview.transform.localScale = EntityPresentationApplier.ResolveScale(
                prefab != null ? prefab.transform.localScale : preview.transform.localScale,
                presentation);
            preview.transform.rotation = EntityPresentationApplier.ResolveRotation(
                Quaternion.identity,
                presentation);
            EntityPresentationApplier.ApplyStyleAndShadows(preview, presentation);
            ApplyPreviewTint(preview);
        }

        private GameObject CreateSpritePreview(
            Sprite sprite,
            UnitClassConfig config)
        {
            var preview = new GameObject(
                $"UnitDeploymentPreview_{_session.UnitTypeId}",
                typeof(SpriteRenderer));
            preview.transform.SetParent(_previewRoot, false);

            SpriteRenderer renderer = preview.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = PreviewTint;
            renderer.sortingOrder = 80;

            float height = Mathf.Max(0.1f, sprite.bounds.size.y);
            float scale = SpritePreviewHeight / height;
            preview.transform.localScale = EntityPresentationApplier.ResolveScale(
                new Vector3(scale, scale, scale),
                config?.ResolvePresentation());
            return preview;
        }

        private GameObject CreateFallbackPreview(UnitClassConfig config)
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            preview.name = $"UnitDeploymentPreview_{_session.UnitTypeId}";
            preview.transform.SetParent(_previewRoot, false);
            preview.transform.localScale = EntityPresentationApplier.ResolveScale(
                new Vector3(
                    0.45f,
                    FallbackPreviewHeight * 0.5f,
                    0.45f),
                config?.ResolvePresentation());

            Collider collider = preview.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            EnsureFallbackPreviewMaterial();
            Renderer renderer = preview.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = _fallbackPreviewMaterial;

            EntityPresentationApplier.ApplyStyleAndShadows(
                preview,
                config?.ResolvePresentation());
            return preview;
        }

        private void PositionPreviewObject(GameObject preview, Vector2Int tile)
        {
            if (preview == null)
                return;

            EntityPresentationConfig presentation =
                ResolveActiveUnitPresentation();
            Vector3 tilePosition = ResolveWorldPosition(tile, PreviewLift);
            if (preview.GetComponent<SpriteRenderer>() != null)
            {
                preview.transform.position =
                    EntityPresentationApplier.ResolvePosition(
                        tilePosition + Vector3.up * SpritePreviewHeight * 0.5f,
                        Quaternion.identity,
                        presentation);
                FaceSpritePreviewToCamera();
                return;
            }

            preview.transform.position = tilePosition;
            if (TryResolveRendererBounds(preview, out Bounds bounds))
            {
                Vector3 root = preview.transform.position;
                preview.transform.position = new Vector3(
                    tilePosition.x - (bounds.center.x - root.x),
                    tilePosition.y - (bounds.min.y - root.y),
                    tilePosition.z - (bounds.center.z - root.z));
            }

            EntityPresentationApplier.ApplyPosition(
                preview,
                presentation,
                preview.transform.position,
                Quaternion.identity);
        }

        private void DisablePreviewGameplayComponents(GameObject preview)
        {
            Collider[] colliders = preview.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
                colliders[index].enabled = false;

            Collider2D[] colliders2D =
                preview.GetComponentsInChildren<Collider2D>(true);
            for (int index = 0; index < colliders2D.Length; index++)
                colliders2D[index].enabled = false;

            MonoBehaviour[] behaviours =
                preview.GetComponentsInChildren<MonoBehaviour>(true);
            for (int index = 0; index < behaviours.Length; index++)
                behaviours[index].enabled = false;

            Animator[] animators = preview.GetComponentsInChildren<Animator>(true);
            for (int index = 0; index < animators.Length; index++)
                animators[index].enabled = false;
        }

        private void ApplyPreviewTint(GameObject preview)
        {
            SpriteRenderer[] sprites =
                preview.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < sprites.Length; index++)
                sprites[index].color = PreviewTint;

            Renderer[] renderers = preview.GetComponentsInChildren<Renderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                _previewPropertyBlock.Clear();
                renderer.GetPropertyBlock(_previewPropertyBlock);
                _previewPropertyBlock.SetColor("_Color", PreviewTint);
                _previewPropertyBlock.SetColor("_BaseColor", PreviewTint);
                _previewPropertyBlock.SetColor(
                    "_EmissionColor",
                    new Color(0.08f, 0.18f, 0.08f, 1f));
                renderer.SetPropertyBlock(_previewPropertyBlock);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void FaceSpritePreviewToCamera()
        {
            if (_previewObject == null
                || _previewObject.GetComponent<SpriteRenderer>() == null)
            {
                return;
            }

            UnityEngine.Camera camera = ResolveCamera();
            if (camera != null)
            {
                _previewObject.transform.rotation =
                    EntityPresentationApplier.ResolveRotation(
                        camera.transform.rotation,
                        ResolveActiveUnitPresentation());
            }
        }

        private EntityPresentationConfig ResolveActiveUnitPresentation()
        {
            if (_session == null || _unitConfigs == null)
                return null;

            return _unitConfigs
                .GetConfig(_session.UnitTypeId)
                ?.ResolvePresentation();
        }


        private Vector3 ResolveWorldPosition(
            Vector2Int tile,
            float layerOffset)
        {
            if (_worldPositionResolver != null)
                return _worldPositionResolver.ResolveWorldPosition(tile, layerOffset);

            if (_gridProjection == null)
                return new Vector3(tile.x, tile.y, 0f);

            return _gridProjection.GridToWorld(tile, 0f, layerOffset);
        }

        private void EnsureWorldRoots()
        {
            if (_previewRoot == null)
            {
                var root = new GameObject(PreviewRootName);
                _previewRoot = root.transform;
            }
        }

        private void EnsureFallbackPreviewMaterial()
        {
            _fallbackPreviewMaterial ??= CreateTransparentMaterial(
                "UnitDeploymentFallbackPreview",
                PreviewTint);
        }

        private static Material CreateTransparentMaterial(
            string name,
            Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
                return null;

            var material = new Material(shader)
            {
                name = name,
                color = color,
                renderQueue = OverlayRenderQueue,
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            return material;
        }

    }
}
