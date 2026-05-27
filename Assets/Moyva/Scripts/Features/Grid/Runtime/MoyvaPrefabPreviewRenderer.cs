using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public readonly struct MoyvaPreviewPixelData
    {
        public MoyvaPreviewPixelData(Color[] pixels, int width, int height)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
        }

        public Color[] Pixels { get; }
        public int Width { get; }
        public int Height { get; }
        public bool IsValid => Pixels != null && Pixels.Length > 0 && Width > 0 && Height > 0;
    }

    public static class MoyvaPrefabPreviewRenderer
    {
        private const int PreviewRenderLayer = 30;
        private static MoyvaProjectSettingsSO _runtimeFallbackSettings;

        public static bool HasMeshPreviewRenderer(GameObject prefab)
        {
            if (prefab == null)
                return false;

            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var renderer = meshRenderers[i];
                var filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
                if (renderer != null && renderer.enabled && filter != null && filter.sharedMesh != null)
                    return true;
            }

            var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                var renderer = skinnedRenderers[i];
                if (renderer != null && renderer.enabled && renderer.sharedMesh != null)
                    return true;
            }

            return false;
        }

        public static bool TryRenderMeshPrefabPreview(GameObject prefab, MoyvaProjectSettingsSO settings, out MoyvaPreviewPixelData pixelData)
        {
            pixelData = default;
            if (prefab == null)
                return false;

            settings = ResolveSettings(settings);
            if (!settings.EnableMeshPrefabPreviews || !TryCollectMeshPreviewDraws(prefab, out var draws, out var bounds))
                return false;

            RenderTexture temporary = null;
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;
            GameObject cameraObject = null;
            GameObject lightObject = null;

            try
            {
                int textureSize = settings.ResolvePreviewTextureSize();
                bool generateMipmaps = settings.GeneratePreviewMipmaps;
                temporary = RenderTexture.GetTemporary(
                    textureSize,
                    textureSize,
                    24,
                    settings.PreviewRenderTextureFormat,
                    settings.PreviewRenderTextureReadWrite);

                cameraObject = new GameObject("MoyvaPrefabPreviewCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                var camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                bool usePerspective = settings.ResolveUsePerspectivePreviewCamera();
                camera.orthographic = !usePerspective;
                camera.cullingMask = 1 << PreviewRenderLayer;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = temporary;

                Quaternion cameraRotation = Quaternion.Euler(settings.ResolvePreviewCameraEuler());
                camera.transform.rotation = cameraRotation;
                float boundsMagnitude = Mathf.Max(1f, bounds.size.magnitude);
                Vector3 viewExtents = ResolveViewExtents(bounds, cameraRotation);

                float cameraDistance;
                if (usePerspective)
                {
                    float fov = settings.ResolvePreviewPerspectiveFieldOfView();
                    camera.fieldOfView = fov;
                    float halfFrustum = Mathf.Max(viewExtents.x, viewExtents.y) * settings.ResolvePreviewPadding();
                    float halfFovRadians = Mathf.Max(0.01f, fov * Mathf.Deg2Rad * 0.5f);
                    cameraDistance = halfFrustum / Mathf.Tan(halfFovRadians) + Mathf.Max(0.5f, viewExtents.z);
                }
                else
                {
                    camera.orthographicSize = Mathf.Max(viewExtents.x, viewExtents.y) * settings.ResolvePreviewPadding();
                    cameraDistance = boundsMagnitude * 2.5f + 2f;
                }

                camera.transform.position = -camera.transform.forward * cameraDistance;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = Mathf.Max(cameraDistance + boundsMagnitude * 3f + 5f, boundsMagnitude * 6f + 10f);

                lightObject = new GameObject("MoyvaPrefabPreviewLight")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = settings.ResolvePreviewLightIntensity();
                light.cullingMask = 1 << PreviewRenderLayer;
                light.transform.rotation = Quaternion.Euler(settings.PreviewLightEuler);

                RenderTexture.active = temporary;
                GL.Clear(true, true, Color.clear);

                Matrix4x4 centerMatrix = Matrix4x4.Translate(-bounds.center);
                for (int drawIndex = 0; drawIndex < draws.Count; drawIndex++)
                {
                    var draw = draws[drawIndex];
                    Matrix4x4 matrix = centerMatrix * draw.LocalMatrix;
                    int subMeshCount = draw.Mesh.subMeshCount;
                    for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                    {
                        Material material = draw.Materials[Mathf.Min(subMeshIndex, draw.Materials.Length - 1)];
                        if (material == null)
                            continue;

                        Graphics.DrawMesh(
                            draw.Mesh,
                            matrix,
                            material,
                            PreviewRenderLayer,
                            camera,
                            subMeshIndex,
                            null,
                            ShadowCastingMode.Off,
                            receiveShadows: false);
                    }
                }

                camera.Render();

                readable = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, generateMipmaps, true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = settings.PreviewFilterMode,
                    wrapMode = TextureWrapMode.Clamp
                };
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0, false);
                readable.Apply(generateMipmaps, false);

                Color[] pixels = readable.GetPixels();
                if (!HasVisiblePixels(pixels))
                    return false;

                pixelData = new MoyvaPreviewPixelData(pixels, textureSize, textureSize);
                return pixelData.IsValid;
            }
            catch
            {
                return false;
            }
            finally
            {
                RenderTexture.active = previous;
                if (readable != null)
                    DestroyPreviewObject(readable);
                if (cameraObject != null)
                    DestroyPreviewObject(cameraObject);
                if (lightObject != null)
                    DestroyPreviewObject(lightObject);
                if (temporary != null)
                    RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static MoyvaProjectSettingsSO ResolveSettings(MoyvaProjectSettingsSO settings)
        {
            settings ??= _runtimeFallbackSettings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
            settings.Normalize();
            return settings;
        }

        private static bool TryCollectMeshPreviewDraws(GameObject prefab, out List<PreviewMeshDraw> draws, out Bounds bounds)
        {
            draws = new List<PreviewMeshDraw>();
            bounds = default;
            bool hasBounds = false;
            Matrix4x4 rootWorldToLocal = prefab.transform.worldToLocalMatrix;

            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var renderer = meshRenderers[i];
                var filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
                Mesh mesh = filter != null ? filter.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                draws.Add(new PreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix));
                EncapsulateBounds(ref bounds, ref hasBounds, TransformBounds(localMatrix, mesh.bounds));
            }

            var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                var renderer = skinnedRenderers[i];
                Mesh mesh = renderer != null ? renderer.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                draws.Add(new PreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix));
                EncapsulateBounds(ref bounds, ref hasBounds, TransformBounds(localMatrix, renderer.localBounds));
            }

            return draws.Count > 0 && hasBounds && IsFinite(bounds.center) && IsFinite(bounds.size) && bounds.size.sqrMagnitude > 0.0001f;
        }

        private static Vector3 ResolveViewExtents(Bounds bounds, Quaternion cameraRotation)
        {
            Quaternion worldToView = Quaternion.Inverse(cameraRotation);
            Vector3 extents = bounds.extents;
            float maxX = 0.01f;
            float maxY = 0.01f;
            float maxZ = 0.01f;

            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = new Vector3(extents.x * x, extents.y * y, extents.z * z);
                Vector3 view = worldToView * corner;
                maxX = Mathf.Max(maxX, Mathf.Abs(view.x));
                maxY = Mathf.Max(maxY, Mathf.Abs(view.y));
                maxZ = Mathf.Max(maxZ, Mathf.Abs(view.z));
            }

            return new Vector3(maxX, maxY, maxZ);
        }

        private static Bounds TransformBounds(Matrix4x4 matrix, Bounds source)
        {
            Vector3 extents = source.extents;
            var result = new Bounds(matrix.MultiplyPoint3x4(source.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = source.center + new Vector3(extents.x * x, extents.y * y, extents.z * z);
                result.Encapsulate(matrix.MultiplyPoint3x4(corner));
            }

            return result;
        }

        private static void EncapsulateBounds(ref Bounds aggregate, ref bool hasBounds, Bounds value)
        {
            if (!hasBounds)
            {
                aggregate = value;
                hasBounds = true;
                return;
            }

            aggregate.Encapsulate(value);
        }

        private static bool HasVisiblePixels(Color[] pixels)
        {
            if (pixels == null)
                return false;

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.01f)
                    return true;
            }

            return false;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void DestroyPreviewObject(Object instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(instance);
            else
                Object.DestroyImmediate(instance);
        }

        private readonly struct PreviewMeshDraw
        {
            public PreviewMeshDraw(Mesh mesh, Material[] materials, Matrix4x4 localMatrix)
            {
                Mesh = mesh;
                Materials = materials;
                LocalMatrix = localMatrix;
            }

            public readonly Mesh Mesh;
            public readonly Material[] Materials;
            public readonly Matrix4x4 LocalMatrix;
        }
    }
}