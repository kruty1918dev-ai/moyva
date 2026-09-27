using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// G23 acceptance invariants for the influence-radius overlay: the request
    /// carries a world-space extent derived from the same radius the placement
    /// rules use ((radius + 0.5) × cellSize covers center + radius rings), the
    /// overlay bounds the shader square exactly, renderers under the excluded
    /// root never draw, Draw prunes dead candidates, and Hide clears state.
    /// </summary>
    public sealed class G23AcceptanceTests
    {
        private readonly List<Object> _destroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _destroy)
                if (o != null) Object.DestroyImmediate(o);
            _destroy.Clear();
        }

        private T Track<T>(T o) where T : Object
        {
            _destroy.Add(o);
            return o;
        }

        private static Material OverlayMaterial()
            => new Material(Shader.Find("Hidden/InternalErrorShader"));

        [Test]
        public void Request_HalfExtent_CoversCenterPlusRadiusCells()
        {
            // r=5 over 1m cells: the placement rule includes cell centers
            // within 5 steps — the overlay square must reach 5.5m so the
            // outermost in-range cell's own square is inside the border.
            var request = new ConstructionInfluenceRadiusOverlayRequest(
                new Vector3(10f, 0f, 10f), radius: 5, cellSize: 1f,
                OverlayMaterial(), 0.05f, 0.5f, null);
            Assert.AreEqual(5.5f, request.HalfExtent, 0.0001f);

            // Larger cells scale the boundary accordingly (grid↔world transform).
            var wide = new ConstructionInfluenceRadiusOverlayRequest(
                Vector3.zero, 5, 2f, OverlayMaterial(), 0.05f, 0.5f, null);
            Assert.AreEqual(11f, wide.HalfExtent, 0.0001f);

            // Degenerate input stays sane.
            var zero = new ConstructionInfluenceRadiusOverlayRequest(
                Vector3.zero, -3, 0f, OverlayMaterial(), 0f, 0f, null);
            Assert.AreEqual(0, zero.Radius);
            Assert.AreEqual(0.01f, zero.CellSize, 0.0001f);
        }

        [Test]
        public void Show_SetsBounds_AndPushesRadiusParams_ToMaterial()
        {
            var material = Track(OverlayMaterial());
            var renderer = new ConstructionInfluenceMeshOverlayRenderer();
            var state = new ConstructionInfluenceRadiusOverlayState();
            var request = new ConstructionInfluenceRadiusOverlayRequest(
                new Vector3(10f, 0f, 20f), 5, 1f, material, 0.05f, 0.5f, null);

            renderer.Show(state, request);
            Assert.IsTrue(state.Active);
            Assert.AreEqual(11f, state.Bounds.size.x, 0.0001f,
                "overlay bounds must match 2×HalfExtent");
            Assert.AreEqual(11f, state.Bounds.size.z, 0.0001f);
            Assert.AreEqual(10f, state.Bounds.center.x, 0.0001f);
            Assert.AreEqual(20f, state.Bounds.center.z, 0.0001f);
            Assert.AreEqual(5.5f, material.GetFloat("_HalfExtent"), 0.0001f);
            Assert.AreEqual(
                new Vector4(10f, 20f, 0f, 0f), material.GetVector("_CenterXZ"),
                "shader boundary center must be the world-space cell center");
        }

        [Test]
        public void Show_CollectsRenderers_InBounds_ButSkipsExcludedRoot()
        {
            var material = Track(OverlayMaterial());
            var mesh = Track(new Mesh { name = "g23quad" });
            mesh.vertices = new[] { Vector3.zero, Vector3.one, Vector3.up, Vector3.forward };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };

            var inRange = Track(new GameObject("inRange"));
            inRange.transform.position = new Vector3(2f, 0f, 2f);
            inRange.AddComponent<MeshFilter>().sharedMesh = mesh;
            inRange.AddComponent<MeshRenderer>();

            var excludedRoot = Track(new GameObject("excludedRoot")).transform;
            var excluded = Track(new GameObject("excluded"));
            excluded.transform.SetParent(excludedRoot, false);
            excluded.transform.position = new Vector3(2.5f, 0f, 2f);
            excluded.AddComponent<MeshFilter>().sharedMesh = mesh;
            excluded.AddComponent<MeshRenderer>();

            var far = Track(new GameObject("far"));
            far.transform.position = new Vector3(500f, 0f, 500f);
            far.AddComponent<MeshFilter>().sharedMesh = mesh;
            far.AddComponent<MeshRenderer>();

            var state = new ConstructionInfluenceRadiusOverlayState();
            new ConstructionInfluenceMeshOverlayRenderer().Show(state,
                new ConstructionInfluenceRadiusOverlayRequest(
                    Vector3.zero, 5, 1f, material, 0.05f, 0.5f, excludedRoot));

            Assert.IsTrue(state.Renderers.Contains(inRange.GetComponent<MeshRenderer>()),
                "in-bounds renderer must be collected");
            Assert.IsFalse(state.Renderers.Contains(excluded.GetComponent<MeshRenderer>()),
                "renderer under the excluded root (e.g. own visuals) must not draw");
            Assert.IsFalse(state.Renderers.Contains(far.GetComponent<MeshRenderer>()),
                "out-of-bounds renderer must not be collected");
        }

        [Test]
        public void Draw_RemovesDisabledRenderers_HideClears()
        {
            var material = Track(OverlayMaterial());
            var mesh = Track(new Mesh { name = "g23quad2" });
            mesh.vertices = new[] { Vector3.zero, Vector3.one };
            mesh.triangles = new int[0];

            var go = Track(new GameObject("cell"));
            go.transform.position = Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();

            var state = new ConstructionInfluenceRadiusOverlayState();
            var renderer = new ConstructionInfluenceMeshOverlayRenderer();
            renderer.Show(state, new ConstructionInfluenceRadiusOverlayRequest(
                Vector3.zero, 5, 1f, material, 0.05f, 0.5f, null));
            Assert.IsTrue(state.Renderers.Contains(mr));

            mr.enabled = false;
            renderer.Draw(state);
            Assert.IsFalse(state.Renderers.Contains(mr),
                "Draw must prune renderers that stopped being candidates");

            mr.enabled = true;
            renderer.Show(state, new ConstructionInfluenceRadiusOverlayRequest(
                Vector3.zero, 5, 1f, material, 0.05f, 0.5f, null));
            renderer.Hide(state);
            Assert.IsFalse(state.Active);
            Assert.AreEqual(0, state.Renderers.Count);
        }

        [Test]
        public void Show_NullMaterial_IsNoOp()
        {
            var state = new ConstructionInfluenceRadiusOverlayState();
            new ConstructionInfluenceMeshOverlayRenderer().Show(state,
                new ConstructionInfluenceRadiusOverlayRequest(
                    Vector3.zero, 5, 1f, null, 0.05f, 0.5f, null));
            Assert.IsFalse(state.Active);
        }
    }
}
