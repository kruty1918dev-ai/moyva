using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Everything needed to ground one sprite/card: the image, the card
    /// mesh (optional — without it a unit card convention is used), the
    /// world-space anchor, the ground surface sampler, scale and tuning.
    /// The surface sampler is a plain delegate so callers never leak
    /// domain types into the package.
    /// </summary>
    public readonly struct GroundingRequest
    {
        public readonly IPixelSource Pixels;
        public readonly SpriteGroundingProfile Profile;
        /// <summary>Optional card mesh; its quads provide the UV→local map.</summary>
        public readonly Mesh Mesh;
        /// <summary>World-space position the support point must land on (XZ).</summary>
        public readonly Vector3 AnchorPosition;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        /// <summary>World XZ → ground height. Null falls back to AnchorPosition.y.</summary>
        public readonly Func<Vector2, float> SurfaceHeight;
        /// <summary>Deliberate sink below the surface (e.g. hide stem gaps).</summary>
        public readonly float SinkMeters;
        /// <summary>Fallback card size used when <see cref="Mesh"/> is null or not a card.</summary>
        public readonly Vector2 FallbackQuadSize;

        public GroundingRequest(
            IPixelSource pixels,
            SpriteGroundingProfile profile,
            Mesh mesh,
            Vector3 anchorPosition,
            Quaternion rotation,
            Vector3 scale,
            Func<Vector2, float> surfaceHeight,
            float sinkMeters,
            Vector2 fallbackQuadSize)
        {
            Pixels = pixels;
            Profile = profile;
            Mesh = mesh;
            AnchorPosition = anchorPosition;
            Rotation = rotation;
            Scale = scale;
            SurfaceHeight = surfaceHeight;
            SinkMeters = sinkMeters;
            FallbackQuadSize = fallbackQuadSize;
        }
    }

    /// <summary>
    /// Placement result: world position/rotation plus the local-space data
    /// so callers can reuse bounds for footprint logic.
    /// </summary>
    public readonly struct GroundedFrame
    {
        public GroundedFrame(
            Vector3 position,
            Quaternion rotation,
            Vector3 supportLocal,
            Bounds visibleLocalBounds,
            Bounds visibleWorldBounds,
            bool hasVisibleData)
        {
            Position = position;
            Rotation = rotation;
            SupportLocal = supportLocal;
            VisibleLocalBounds = visibleLocalBounds;
            VisibleWorldBounds = visibleWorldBounds;
            HasVisibleData = hasVisibleData;
        }

        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        /// <summary>Local-space support point used for the correction.</summary>
        public readonly Vector3 SupportLocal;
        /// <summary>Visible-content bounds in prefab-local space.</summary>
        public readonly Bounds VisibleLocalBounds;
        /// <summary>Visible bounds transformed into world space by the result pose.</summary>
        public readonly Bounds VisibleWorldBounds;
        public readonly bool HasVisibleData;
    }

    /// <summary>
    /// Turns sprite metrics into a grounded world pose: the support point
    /// lands on <see cref="GroundingRequest.AnchorPosition"/> XZ and on the
    /// sampled ground height, minus the optional sink. All steps are pure
    /// functions of the request — identical input, identical pose.
    /// </summary>
    public static class GroundPlacementSolver
    {
        /// <summary>
        /// Local-space convention for requests without a usable card mesh:
        /// a unit card centred on the pivot in X, base at y = 0.
        /// </summary>
        public static GroundedFrame Solve(in GroundingRequest request)
        {
            Vector3 position = request.AnchorPosition;
            Quaternion rotation = request.Rotation;
            // default(Quaternion) is (0,0,0,0) — not a valid rotation; an
            // unset field reads as identity.
            if (rotation.x == 0f && rotation.y == 0f && rotation.z == 0f && rotation.w == 0f)
                rotation = Quaternion.identity;
            Vector3 scale = request.Scale == default ? Vector3.one : request.Scale;

            if (!TryComputeLocalFrame(request, out Bounds visibleLocal, out Vector3 supportLocal))
            {
                return new GroundedFrame(
                    position, rotation, Vector3.zero,
                    new Bounds(Vector3.zero, Vector3.zero),
                    new Bounds(position, Vector3.zero),
                    false);
            }

            Vector3 supportWorld = rotation * Vector3.Scale(supportLocal, scale);
            position.x -= supportWorld.x;
            position.z -= supportWorld.z;

            float groundY = request.SurfaceHeight != null
                ? request.SurfaceHeight(new Vector2(position.x, position.z))
                : request.AnchorPosition.y;
            if (float.IsNaN(groundY) || float.IsInfinity(groundY))
                groundY = request.AnchorPosition.y;
            position.y = groundY - supportWorld.y - Mathf.Max(0f, request.SinkMeters);

            Bounds worldBounds = TransformBounds(
                visibleLocal, position, rotation, scale);
            return new GroundedFrame(
                position, rotation, supportLocal, visibleLocal, worldBounds, true);
        }

        /// <summary>
        /// Resolves the visible bounds and support point in prefab-local
        /// space: through the card mesh's quads when provided, otherwise
        /// through the unit-card convention scaled by
        /// <see cref="GroundingRequest.FallbackQuadSize"/>.
        /// </summary>
        public static bool TryComputeLocalFrame(
            in GroundingRequest request,
            out Bounds visibleLocalBounds,
            out Vector3 supportLocal)
        {
            visibleLocalBounds = default;
            supportLocal = default;

            IPixelSource source = request.Pixels;
            if (source == null || source.Width <= 0 || source.Height <= 0)
                return false;

            var buffer = new Color32[source.Width * source.Height];
            if (!source.TryCopyPixels(buffer))
                return false;

            SpriteGroundingProfile profile = request.Profile;
            VisibleBounds bounds = AlphaBoundsAnalyzer.AnalyzePixels(
                buffer, source.Width, source.Height,
                profile.AlphaThreshold, profile.PaddingPixels);
            if (!bounds.HasContent)
                return false;

            Vector2 supportUv = SupportPointResolver.SolveUv(
                bounds, buffer, source.Width, source.Height,
                profile.AlphaThreshold, profile);

            if (request.Mesh != null)
            {
                var quads = new List<CardQuad>(4);
                if (QuadCardProjector.ExtractQuads(request.Mesh, quads) > 0)
                {
                    visibleLocalBounds = QuadCardProjector.ProjectLocalBounds(quads, bounds.Uv);
                    supportLocal = QuadCardProjector.ProjectPoint(quads, supportUv);
                    return true;
                }
            }

            // Unit-card fallback: pivot at base-centre, 1x1 quad.
            Vector2 quadSize = request.FallbackQuadSize;
            if (quadSize.x <= 0f) quadSize.x = 1f;
            if (quadSize.y <= 0f) quadSize.y = 1f;
            var unitQuad = new CardQuad(
                -1, -1, -1, -1,
                new Vector3(-quadSize.x * 0.5f, 0f, 0f),
                new Vector3(quadSize.x * 0.5f, 0f, 0f),
                new Vector3(-quadSize.x * 0.5f, quadSize.y, 0f),
                new Vector3(quadSize.x * 0.5f, quadSize.y, 0f));
            var single = new[] { unitQuad };
            visibleLocalBounds = QuadCardProjector.ProjectLocalBounds(single, bounds.Uv);
            supportLocal = QuadCardProjector.ProjectPoint(single, supportUv);
            return true;
        }

        private static Bounds TransformBounds(
            Bounds local, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            var bounds = new Bounds(
                matrix.MultiplyPoint3x4(local.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = local.center + Vector3.Scale(
                    local.extents,
                    new Vector3(
                        (i & 1) == 0 ? -1f : 1f,
                        (i & 2) == 0 ? -1f : 1f,
                        (i & 4) == 0 ? -1f : 1f));
                bounds.Encapsulate(matrix.MultiplyPoint3x4(corner));
            }
            return bounds;
        }
    }
}
