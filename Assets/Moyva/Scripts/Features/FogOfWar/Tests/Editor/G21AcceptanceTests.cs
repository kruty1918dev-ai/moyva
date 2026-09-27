using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// G21 acceptance invariants for the fog boundary curtain: curtain
    /// geometry exists only along revealed↔unexplored edges (explored areas
    /// never gain walls, unexplored cells never lose cover), the curtain top
    /// rises to the highest adjacent surface so hidden heights stay covered,
    /// depth stays constant per edge, and disabling or disposing clears all
    /// geometry.
    /// </summary>
    public sealed class G21AcceptanceTests
    {
        private sealed class OrthoProjection : IGridProjection
        {
            public GridProjectionMode ProjectionMode => GridProjectionMode.Orthographic3D;
            public GridTopology Topology => GridTopology.Orthogonal;
            public GridWorldPlane WorldPlane => GridWorldPlane.XZ;
            public Vector3 GridToWorld(Vector2Int c)
                => new Vector3(c.x + 0.5f, 0f, c.y + 0.5f);
            public Vector3 GridToWorld(Vector2Int c, float elevation, float layerOffset = 0f)
                => new Vector3(c.x + 0.5f, elevation + layerOffset, c.y + 0.5f);
            public Vector2Int WorldToGrid(Vector3 w)
                => new Vector2Int(Mathf.FloorToInt(w.x), Mathf.FloorToInt(w.z));
            public IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int c)
            {
                yield return c + Vector2Int.left;
                yield return c + Vector2Int.right;
                yield return c + Vector2Int.down;
                yield return c + Vector2Int.up;
            }
            public float GetStepDistance(Vector2Int a, Vector2Int b) => 1f;
            public float EstimateDistance(Vector2Int a, Vector2Int b)
                => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            public Bounds GetWorldBounds(int w, int h)
                => new Bounds(new Vector3(w * 0.5f, 0f, h * 0.5f),
                    new Vector3(w, 1f, h));
        }

        private static readonly FieldInfo MeshField =
            typeof(FogBoundaryCurtainRenderer).GetField(
                "_mesh", BindingFlags.NonPublic | BindingFlags.Instance);

        private FogBoundaryCurtainRenderer _renderer;
        private readonly List<Object> _destroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            _renderer?.Dispose();
            _renderer = null;
            foreach (var o in _destroy)
                if (o != null) Object.DestroyImmediate(o);
            _destroy.Clear();
        }

        private static FogOfWarSettings Settings()
        {
            var s = new FogOfWarSettings();
            s.ScreenSpace.Enabled = true;
            s.ScreenSpace.CurtainEnabled = true;
            s.ScreenSpace.CurtainPerEdgeSurfaceProbe = false; // no raycasts in tests
            return s;
        }

        private static Color32[] Pixels(int w, int h, bool unexplored)
        {
            var px = new Color32[w * h];
            var c = unexplored ? new Color32(0, 255, 0, 255) : new Color32(0, 0, 0, 255);
            for (int i = 0; i < px.Length; i++)
                px[i] = c;
            return px;
        }

        private static FogWorldVisualContext Context(int w, int h, float[,] heights)
            => new FogWorldVisualContext(
                w, h,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f, true,
                new Bounds(new Vector3(w * 0.5f, 0f, h * 0.5f),
                    new Vector3(w, 4f, h)),
                heights, null);

        private Mesh Mesh => MeshField?.GetValue(_renderer) as Mesh;

        [Test]
        public void FullyRevealedMap_ProducesNoCurtain()
        {
            var heights = new float[8, 8];
            _renderer = new FogBoundaryCurtainRenderer(Settings(), new OrthoProjection());
            _renderer.Rebuild(Pixels(8, 8, unexplored: false), 8, 8,
                Context(8, 8, heights));
            // Revealed interior has no unexplored neighbours except the map
            // edge — which IS a curtain boundary (world ends there).
            // Interior quads must not exist: every vertex sits near a border.
            if (Mesh == null || Mesh.vertexCount == 0)
                return; // acceptable: feature may skip empty work
            foreach (var v in Mesh.vertices)
                Assert.IsTrue(v.x < 1.2f || v.z < 1.2f || v.x > 6.8f || v.z > 6.8f,
                    $"interior curtain vertex {v} inside a fully revealed map");
        }

        [Test]
        public void Boundary_OnlyAlongRevealedEdge_OffsetIntoHidden()
        {
            int w = 8, h = 8;
            var heights = new float[w, h];
            var px = Pixels(w, h, unexplored: false);
            for (int y = 0; y < h; y++)
            for (int x = 4; x < w; x++)
                px[x + y * w] = new Color32(0, 255, 0, 255); // right half hidden

            _renderer = new FogBoundaryCurtainRenderer(Settings(), new OrthoProjection());
            _renderer.Rebuild(px, w, h, Context(w, h, heights));
            Assert.IsNotNull(Mesh, "shader missing → presentation not built");
            Assert.Greater(Mesh.vertexCount, 0, "boundary must emit curtain quads");

            // Curtain stands at the x=4 world boundary: segments sit on the
            // logical edge and offset only a tiny epsilon into the hidden side.
            // Map borders legitimately emit their own curtains — filter to the
            // interior band so border verts don't pollute the interior assert.
            int bandVerts = 0;
            float minX = float.MaxValue, maxX = float.MinValue;
            foreach (var v in Mesh.vertices)
            {
                Assert.IsFalse(v.x > 0.5f && v.x < 3.9f && v.z > 0.5f && v.z < h - 0.5f,
                    $"curtain vertex {v} leaked inside revealed cells");
                if (v.x < 2.5f || v.x > 5.5f || v.z < 0.5f || v.z > h - 0.5f)
                    continue;
                Assert.LessOrEqual(v.x, 4.5f, "curtain strayed deep into hidden");
                bandVerts++;
                minX = Mathf.Min(minX, v.x);
                maxX = Mathf.Max(maxX, v.x);
            }
            Assert.Greater(bandVerts, 0, "the revealed↔hidden edge emitted no curtain");
            Assert.GreaterOrEqual(minX, 4f - 0.0001f,
                "curtain must not cut inside the revealed cell");
        }

        [Test]
        public void CurtainTop_RisesToHigherSide_ConstantDepth()
        {
            int w = 6, h = 4;
            var heights = new float[w, h];
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < h; y++)
                heights[x, y] = 2f;      // revealed plateau at 2m
            for (int x = 3; x < w; x++)
            for (int y = 0; y < h; y++)
                heights[x, y] = 5f;      // hidden ridge at 5m

            var px = Pixels(w, h, unexplored: false);
            for (int y = 0; y < h; y++)
            for (int x = 3; x < w; x++)
                px[x + y * w] = new Color32(0, 255, 0, 255);

            var settings = Settings();
            float depth = settings.ScreenSpace.CurtainWorldDepth
                          + settings.ScreenSpace.CurtainBottomPadding;
            _renderer = new FogBoundaryCurtainRenderer(settings, new OrthoProjection());
            _renderer.Rebuild(px, w, h, Context(w, h, heights));
            Assert.IsNotNull(Mesh);
            Assert.Greater(Mesh.vertexCount, 0);

            // Same interior-band filter: border curtains legitimately run at a
            // lower surface — only the x=3 revealed↔hidden edge is measured.
            int bandVerts = 0;
            float maxY = float.MinValue, minY = float.MaxValue;
            foreach (var v in Mesh.vertices)
            {
                if (v.x < 2.5f || v.x > 3.5f || v.z < 0.5f || v.z > h - 0.5f)
                    continue;
                bandVerts++;
                maxY = Mathf.Max(maxY, v.y);
                minY = Mathf.Min(minY, v.y);
            }
            Assert.Greater(bandVerts, 0, "the revealed↔hidden edge emitted no curtain");
            // Top must reach the HIDDEN surface (5m + offset), not the low
            // revealed one — otherwise the high unexplored tile shows through.
            Assert.GreaterOrEqual(maxY, 5f,
                "curtain top must cover the hidden high surface");
            // Constant depth: bottom = top − depth for every edge segment.
            Assert.LessOrEqual(maxY - minY,
                depth + settings.ScreenSpace.CurtainTopCapLift + 0.05f,
                "curtain depth exceeded its constant-depth band");
        }

        [Test]
        public void MapEdge_CountsAsUnexplored_CurtainOnBorder()
        {
            int w = 4, h = 4;
            var heights = new float[w, h];
            var px = Pixels(w, h, unexplored: false); // all revealed
            _renderer = new FogBoundaryCurtainRenderer(Settings(), new OrthoProjection());
            _renderer.Rebuild(px, w, h, Context(w, h, heights));
            Assert.IsNotNull(Mesh);
            Assert.Greater(Mesh.vertexCount, 0,
                "the outer map border must still draw a curtain — the world ends there");
        }

        [Test]
        public void DisabledCurtain_OrNullPixels_ClearGeometry()
        {
            int w = 4, h = 4;
            var heights = new float[w, h];
            var px = Pixels(w, h, unexplored: true);
            for (int i = 0; i < w; i++)
                px[i + i * w] = new Color32(0, 0, 0, 255); // diagonal revealed

            var settings = Settings();
            _renderer = new FogBoundaryCurtainRenderer(settings, new OrthoProjection());
            _renderer.Rebuild(px, w, h, Context(w, h, heights));
            Assert.IsNotNull(Mesh);
            Assert.Greater(Mesh.vertexCount, 0);

            settings.ScreenSpace.CurtainEnabled = false;
            _renderer.Rebuild(px, w, h, Context(w, h, heights));
            Assert.AreEqual(0, Mesh.vertexCount,
                "disabled curtain must clear its mesh");

            settings.ScreenSpace.CurtainEnabled = true;
            _renderer.Rebuild(null, w, h, Context(w, h, heights));
            Assert.AreEqual(0, Mesh.vertexCount, "null pixels must clear");
        }

        [Test]
        public void Disposed_IgnoresRebuild()
        {
            var heights = new float[4, 4];
            _renderer = new FogBoundaryCurtainRenderer(Settings(), new OrthoProjection());
            _renderer.Dispose();
            Assert.DoesNotThrow(() => _renderer.Rebuild(
                Pixels(4, 4, false), 4, 4, Context(4, 4, heights)));
        }
    }
}
