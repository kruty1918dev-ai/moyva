using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogScreenSpaceTextureUpdater
    {
        private void LogShaderDiagnosticsIfNeeded(
            bool force,
            int stateHash)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            if (!screenSettings.LogShaderDiagnostics)
                return;

            float now =
                Time.realtimeSinceStartup;

            if (!force
                && now - _lastShaderDiagnosticTime
                < screenSettings.DiagnosticLogIntervalSeconds)
            {
                return;
            }

            _lastShaderDiagnosticTime =
                now;

            _shaderDiagnosticSequence++;

            ResolveWorldToGridTransform(
                out Vector3 gridOrigin,
                out Vector4 worldToGrid);

            Camera camera =
                Camera.main;

            var builder =
                new StringBuilder(3072);

            builder.Append(
                "[MOYVA_FOG_SHADER_DIAG] ");

            builder.Append(
                "seq=");

            builder.Append(
                _shaderDiagnosticSequence);

            builder.Append(
                " stateHash=");

            builder.Append(
                stateHash);

            builder.Append(
                " forcedByStateChange=");

            builder.Append(
                force);

            builder.Append(
                " map=");

            builder.Append(
                _width);

            builder.Append(
                "x");

            builder.Append(
                _height);

            builder.Append(
                " debugMode=");

            builder.Append(
                screenSettings.ShaderDebugMode);

            builder.Append(
                " flipY=");

            builder.Append(
                screenSettings.FlipTextureY);

            builder.Append(
                " gridOriginXZ=(");

            builder.Append(
                gridOrigin.x.ToString("F3"));

            builder.Append(
                ",");

            builder.Append(
                gridOrigin.y.ToString("F3"));

            builder.Append(
                ") fallbackPlaneY=");

            builder.Append(
                gridOrigin.z.ToString("F3"));

            builder.Append(
                " transparentFallbackOffsetY=");

            builder.Append(
                screenSettings
                    .TransparentFallbackPlaneOffsetY
                    .ToString("F3"));

            builder.Append(
                " worldToGrid=(");

            builder.Append(
                worldToGrid.x.ToString("F6"));

            builder.Append(
                ",");

            builder.Append(
                worldToGrid.y.ToString("F6"));

            builder.Append(
                ",");

            builder.Append(
                worldToGrid.z.ToString("F6"));

            builder.Append(
                ",");

            builder.Append(
                worldToGrid.w.ToString("F6"));

            builder.Append(
                ") edgeSoftness=");

            builder.Append(
                screenSettings.EdgeSoftness.ToString("F3"));

            builder.Append(
                " presentation=DepthAwareScreenSpace");

            builder.Append(
                " virtualDepth=");

            builder.Append(
                screenSettings.DepthAwareEdgeEnabled);

            builder.Append(
                " closeRadiusPx=");

            builder.Append(
                screenSettings.DepthAwareStateCloseRadiusPixels);

            builder.Append(
                " boundarySoftnessPx=");

            builder.Append(
                screenSettings.DepthAwareBoundarySoftnessPixels
                    .ToString("F2"));

            builder.Append(
                " legacyCurtain=");

            builder.Append(
                screenSettings.UseLegacyWorldCurtain);

            if (camera == null)
            {
                builder.Append(
                    " camera=null");

                return;
            }

            builder.Append(
                " camera=");

            builder.Append(
                camera.name);

            builder.Append(
                " projection=");

            builder.Append(
                camera.orthographic
                    ? "Orthographic"
                    : "Perspective");

            builder.Append(
                " position=");

            builder.Append(
                FormatVector3(
                    camera.transform.position));

            builder.Append(
                " rotation=");

            builder.Append(
                FormatVector3(
                    camera.transform.eulerAngles));

            builder.Append(
                " fov=");

            builder.Append(
                camera.fieldOfView.ToString("F2"));

            builder.Append(
                " orthoSize=");

            builder.Append(
                camera.orthographicSize.ToString("F2"));

            builder.Append(
                " near=");

            builder.Append(
                camera.nearClipPlane.ToString("F3"));

            builder.Append(
                " far=");

            builder.Append(
                camera.farClipPlane.ToString("F1"));

            builder.Append(
                " pixel=");

            builder.Append(
                camera.pixelWidth);

            builder.Append(
                "x");

            builder.Append(
                camera.pixelHeight);

            builder.AppendLine();

            AppendShaderProbe(
                builder,
                camera,
                "center",
                new Vector2(
                    0.5f,
                    0.5f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "upper",
                new Vector2(
                    0.5f,
                    0.8f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "lower",
                new Vector2(
                    0.5f,
                    0.2f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "left",
                new Vector2(
                    0.2f,
                    0.5f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "right",
                new Vector2(
                    0.8f,
                    0.5f),
                gridOrigin,
                worldToGrid,
                screenSettings);
        }

        private void AppendShaderProbe(
            StringBuilder builder,
            Camera camera,
            string label,
            Vector2 viewport,
            Vector3 gridOrigin,
            Vector4 worldToGrid,
            FogScreenSpaceSettings screenSettings)
        {
            Ray ray =
                camera.ViewportPointToRay(
                    new Vector3(
                        viewport.x,
                        viewport.y,
                        0f));

            builder.Append(
                " probe=");

            builder.Append(
                label);

            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    screenSettings.ShaderDiagnosticRaycastDistance,
                    screenSettings.ShaderDiagnosticRaycastMask,
                    QueryTriggerInteraction.Ignore))
            {
                Vector2 grid =
                    ConvertWorldToGrid(
                        hit.point,
                        gridOrigin,
                        worldToGrid,
                        screenSettings.FlipTextureY);

                Vector2Int cell =
                    new Vector2Int(
                        Mathf.RoundToInt(
                            grid.x),
                        Mathf.RoundToInt(
                            grid.y));

                Renderer hitRenderer =
                    hit.collider != null
                        ? hit.collider
                            .GetComponentInParent<Renderer>()
                        : null;

                Material material =
                    hitRenderer != null
                        ? hitRenderer.sharedMaterial
                        : null;

                builder.Append(
                    " hitCollider=");

                builder.Append(
                    hit.collider != null
                        ? hit.collider.name
                        : "unknown");

                builder.Append(
                    " renderer=");

                builder.Append(
                    hitRenderer != null
                        ? hitRenderer.name
                        : "null");

                builder.Append(
                    " shader=");

                builder.Append(
                    material != null
                    && material.shader != null
                        ? material.shader.name
                        : "null");

                builder.Append(
                    " world=");

                builder.Append(
                    FormatVector3(
                        hit.point));

                builder.Append(
                    " grid=(");

                builder.Append(
                    grid.x.ToString("F2"));

                builder.Append(
                    ",");

                builder.Append(
                    grid.y.ToString("F2"));

                builder.Append(
                    ") cell=");

                builder.Append(
                    cell);

                builder.Append(
                    " state=");

                builder.Append(
                    ResolveStateLabel(
                        cell));
            }
            else if (TryIntersectPlane(
                         ray,
                         gridOrigin.z,
                         out Vector3 planePoint))
            {
                Vector2 grid =
                    ConvertWorldToGrid(
                        planePoint,
                        gridOrigin,
                        worldToGrid,
                        screenSettings.FlipTextureY);

                Vector2Int cell =
                    new Vector2Int(
                        Mathf.RoundToInt(
                            grid.x),
                        Mathf.RoundToInt(
                            grid.y));

                builder.Append(
                    " hitCollider=none planeWorld=");

                builder.Append(
                    FormatVector3(
                        planePoint));

                builder.Append(
                    " grid=(");

                builder.Append(
                    grid.x.ToString("F2"));

                builder.Append(
                    ",");

                builder.Append(
                    grid.y.ToString("F2"));

                builder.Append(
                    ") cell=");

                builder.Append(
                    cell);

                builder.Append(
                    " state=");

                builder.Append(
                    ResolveStateLabel(
                        cell));
            }
            else
            {
                builder.Append(
                    " hitCollider=none plane=miss");
            }

            builder.AppendLine();
        }

    }
}
