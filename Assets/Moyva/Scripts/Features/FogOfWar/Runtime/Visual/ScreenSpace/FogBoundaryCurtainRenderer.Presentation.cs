using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogBoundaryCurtainRenderer
    {
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            FogBoundaryCurtainRendererFeature
                .ClearDrawData(
                    _mesh);

            MoyvaJsonObjectFactory.DestroyImmediate(_material);
            MoyvaJsonObjectFactory.DestroyImmediate(_mesh);
            MoyvaJsonObjectFactory.DestroyImmediate(_root);
            _material = null;
            _mesh = null;
            _root = null;

            _vertices.Clear();
            _uvs.Clear();
            _colors.Clear();
            _triangles.Clear();
            _diagnosticSegments.Clear();
            _surfaceOffsetSamples.Clear();
        }

        private bool EnsurePresentation()
        {
            if (_root == null)
            {
                _root =
                    new GameObject(
                        CurtainObjectName)
                    {
                        hideFlags =
                            HideFlags.DontSave
                    };

                _root.transform.SetPositionAndRotation(
                    Vector3.zero,
                    Quaternion.identity);

                _root.transform.localScale =
                    Vector3.one;

                /*
                 * MeshRenderer тут навмисно відсутній.
                 *
                 * Flat Kit Outline та інші Renderer Features працюють
                 * через scene renderer lists. Без MeshRenderer вони
                 * фізично не можуть намалювати curtain повторно.
                 */
            }

            if (_mesh == null)
            {
                _mesh =
                    new Mesh
                    {
                        name =
                            "Moyva_FogBoundaryCurtainMesh",

                        indexFormat =
                            IndexFormat.UInt32,

                        hideFlags =
                            HideFlags.DontSave
                    };

                _mesh.MarkDynamic();
            }

            if (_material == null)
            {
                Shader shader =
                    Shader.Find(
                        CurtainShaderName);

                if (shader == null
                    || !shader.isSupported)
                {
                    if (!_shaderErrorLogged)
                    {
                        _shaderErrorLogged = true;

                        Debug.LogError(
                            "[FogOfWar] Shader '" +
                            CurtainShaderName +
                            "' was not found or is unsupported. " +
                            "Fog boundary curtain is disabled.");
                    }

                    _root.SetActive(false);

                    FogBoundaryCurtainRendererFeature
                        .ClearDrawData(
                            _mesh);

                    return false;
                }

                _material =
                    new Material(shader)
                    {
                        name =
                            "Moyva_FogBoundaryCurtainMaterial",

                        hideFlags =
                            HideFlags.DontSave
                    };

                int passIndex =
                    _material.FindPass(
                        "MoyvaFogCurtain");

                if (passIndex < 0)
                {
                    Debug.LogError(
                        "[MOYVA_FOG_CURTAIN_RENDER] " +
                        "Material does not contain the " +
                        "MoyvaFogCurtain pass.");

                    MoyvaJsonObjectFactory.DestroyImmediate(
                        _material);
                    _material = null;

                    FogBoundaryCurtainRendererFeature
                        .ClearDrawData(
                            _mesh);

                    return false;
                }
            }

            return true;
        }

        private void UpdateMaterial(
            FogScreenSpaceSettings settings)
        {
            /*
             * Top seam matches fullscreen fog; only the lower side darkens.
             */
            Color topColor =
                settings.UnexploredColor;

            topColor.a = 1f;

            Color bottomColor =
                Color.Lerp(
                    settings.UnexploredColor,
                    Color.black,
                    settings.CurtainTopDarkness);

            bottomColor.a = 1f;

            _material.SetColor(
                BottomColorId,
                bottomColor);

            _material.SetColor(
                TopColorId,
                topColor);

            _material.SetFloat(
                TopBandFractionId,
                settings.CurtainTopBandFraction);

            _material.SetFloat(
                GradientPowerId,
                settings.CurtainGradientPower);

            _material.SetFloat(
                DebugVisualModeId,
                (float)settings.CurtainDebugVisualMode);

            bool effectiveDoubleSided =
                settings.CurtainDoubleSided
                || (settings
                        .CurtainForceDoubleSidedInNormalMode
                    && settings.CurtainDebugVisualMode
                        == FogCurtainDebugVisualMode.Off);

            _material.SetFloat(
                CullModeId,
                effectiveDoubleSided
                    ? (float)CullMode.Off
                    : (float)CullMode.Back);

            /*
             * Curtain є гарантованим overlay над world-геометрією.
             * Високі тайли враховуються на етапі побудови mesh через
             * highest-adjacent surface, а не через fragment discard.
             */
        }

        private void ApplyMesh()
        {
            _mesh.Clear(false);

            if (_vertices.Count == 0)
            {
                _root.SetActive(false);

                FogBoundaryCurtainRendererFeature
                    .ClearDrawData(
                        _mesh);

                return;
            }

            _mesh.SetVertices(
                _vertices);

            _mesh.SetUVs(
                0,
                _uvs);

            _mesh.SetColors(
                _colors);

            _mesh.SetTriangles(
                _triangles,
                0,
                true);

            _mesh.RecalculateBounds();

            _root.SetActive(true);

            FogBoundaryCurtainRendererFeature
                .SetDrawData(
                    _mesh,
                    _material,
                    Matrix4x4.identity,
                    true);
        }

        internal void ClearPresentation()
        {
            if (_mesh != null)
                _mesh.Clear(false);

            if (_root != null)
                _root.SetActive(false);

            FogBoundaryCurtainRendererFeature
                .ClearDrawData(
                    _mesh);
        }

        private void AddQuad(
            Vector3 bottomA,
            Vector3 bottomB,
            Vector3 topB,
            Vector3 topA,
            Vector3 outward,
            Color32 debugColor)
        {
            int vertexStart =
                _vertices.Count;

            _vertices.Add(bottomA);
            _vertices.Add(bottomB);
            _vertices.Add(topB);
            _vertices.Add(topA);

            _uvs.Add(
                new Vector2(
                    0f,
                    0f));

            _uvs.Add(
                new Vector2(
                    1f,
                    0f));

            _uvs.Add(
                new Vector2(
                    1f,
                    1f));

            _uvs.Add(
                new Vector2(
                    0f,
                    1f));

            _colors.Add(
                debugColor);

            _colors.Add(
                debugColor);

            _colors.Add(
                debugColor);

            _colors.Add(
                debugColor);

            Vector3 candidateNormal =
                Vector3.Cross(
                    bottomB - bottomA,
                    topB - bottomA);

            bool candidateFacesOutward =
                Vector3.Dot(
                    candidateNormal,
                    outward)
                >= 0f;

            if (candidateFacesOutward)
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 1);
                _triangles.Add(vertexStart + 2);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 3);
            }
            else
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 1);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 3);
                _triangles.Add(vertexStart + 2);
            }
        }

        private static void DestroyUnityObject(
            UnityEngine.Object value)
        {
            if (value == null)
                return;

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(
                    value);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(
                    value);
            }
        }
    }
}
