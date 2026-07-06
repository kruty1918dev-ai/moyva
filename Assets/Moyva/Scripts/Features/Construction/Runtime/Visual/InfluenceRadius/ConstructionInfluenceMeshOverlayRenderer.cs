using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionInfluenceMeshOverlayRenderer : IConstructionInfluenceMeshOverlayRenderer
    {
        public void Show(ConstructionInfluenceRadiusOverlayState state, ConstructionInfluenceRadiusOverlayRequest request)
        {
            if (state == null || request.Material == null)
                return;

            Vector3 centerWorld = request.CenterWorld;
            float halfExtent = request.HalfExtent;
            state.Active = true;
            state.Material = request.Material;
            state.Bounds = new Bounds(
                new Vector3(centerWorld.x, 0f, centerWorld.z),
                new Vector3(halfExtent * 2f, 2048f, halfExtent * 2f));

            request.Material.SetVector("_CenterXZ", new Vector4(centerWorld.x, centerWorld.z, 0f, 0f));
            request.Material.SetFloat("_HalfExtent", halfExtent);
            request.Material.SetColor("_Color", new Color(1f, 1f, 1f, 0.95f));
            request.Material.SetColor("_FillColor", new Color(1f, 1f, 1f, request.FillAlpha));
            request.Material.SetFloat("_BorderWidth", request.BorderWidth);
            RebuildRenderers(state, request.ExcludedRoot);
        }

        public void Hide(ConstructionInfluenceRadiusOverlayState state)
        {
            if (state == null)
                return;

            state.Active = false;
            state.Renderers.Clear();
        }

        public void Draw(ConstructionInfluenceRadiusOverlayState state)
        {
            if (state == null || !state.Active || state.Material == null)
                return;

            for (int i = state.Renderers.Count - 1; i >= 0; i--)
            {
                MeshRenderer renderer = state.Renderers[i];
                if (!IsCandidate(renderer, state.Bounds, null))
                {
                    state.Renderers.RemoveAt(i);
                    continue;
                }

                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                DrawRendererMesh(renderer, meshFilter, state.Material);
            }
        }

        private static void RebuildRenderers(ConstructionInfluenceRadiusOverlayState state, Transform excludedRoot)
        {
            state.Renderers.Clear();
            MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (IsCandidate(renderers[i], state.Bounds, excludedRoot))
                    state.Renderers.Add(renderers[i]);
            }
        }

        private static bool IsCandidate(MeshRenderer renderer, Bounds bounds, Transform excludedRoot)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                return false;

            if (excludedRoot != null && renderer.transform.IsChildOf(excludedRoot))
                return false;

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            return renderer.bounds.Intersects(bounds) && meshFilter != null && meshFilter.sharedMesh != null;
        }

        private static void DrawRendererMesh(Renderer renderer, MeshFilter meshFilter, Material material)
        {
            Mesh mesh = meshFilter.sharedMesh;
            int subMeshCount = Mathf.Max(1, mesh.subMeshCount);
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                Graphics.DrawMesh(mesh, meshFilter.transform.localToWorldMatrix, material, renderer.gameObject.layer, null, subMeshIndex);
            }
        }
    }
}
