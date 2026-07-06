using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridOverlayRenderer : IConstructionBuildGridOverlayRenderer
    {
        private const int BuildGridRenderQueue = 3990;
        private static readonly int EdgeMaskPropertyId = Shader.PropertyToID("_EdgeMask");

        private GameObject _overlayGo;
        private Material _material;
        private MaterialPropertyBlock _propertyBlock;

        public bool MaterialReady => _material != null;

        public void Initialize(Transform parent, string shaderName)
        {
            Transform existing = parent.Find("ConstructionBuildGridOverlay");
            if (existing != null)
                Object.Destroy(existing.gameObject);

            _overlayGo = new GameObject("ConstructionBuildGridOverlay");
            _overlayGo.transform.SetParent(parent, false);
            _overlayGo.SetActive(false);

            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[ConstructionVisual] Shader '{shaderName}' not found. Construction build grid overlay is disabled.");
                return;
            }

            _material = new Material(shader)
            {
                name = "ConstructionBuildGridOverlay_Material",
                renderQueue = BuildGridRenderQueue
            };
        }

        public void ApplyStyle(Color lineColor, Color fillColor, float lineWidth)
        {
            if (_material == null)
                return;

            _material.SetColor("_LineColor", lineColor);
            _material.SetColor("_FillColor", fillColor);
            _material.SetFloat("_LineWidth", lineWidth);
        }

        public void SetVisible(bool visible)
        {
            if (_overlayGo != null)
                _overlayGo.SetActive(visible);
        }

        public void Draw(List<ConstructionBuildGridOverlayEntry> entries, IConstructionBuildGridDiagnostics diagnostics)
        {
            if (_material == null || entries.Count == 0)
                return;

            int prunedCount = 0;
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                ConstructionBuildGridOverlayEntry entry = entries[i];
                if (entry.Mesh == null)
                {
                    entries.RemoveAt(i);
                    prunedCount++;
                    continue;
                }

                if (entry.SourceRenderer != null && !entry.SourceRenderer.enabled)
                {
                    entries.RemoveAt(i);
                    prunedCount++;
                    continue;
                }

                DrawEntry(entry);
            }

            diagnostics.LogEntriesPruned(prunedCount, entries.Count);
        }

        private void DrawEntry(ConstructionBuildGridOverlayEntry entry)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.Clear();
            _propertyBlock.SetVector(EdgeMaskPropertyId, entry.EdgeMask);
            Graphics.DrawMesh(entry.Mesh, entry.Matrix, _material, entry.Layer, null, 0, _propertyBlock);
        }
    }
}
