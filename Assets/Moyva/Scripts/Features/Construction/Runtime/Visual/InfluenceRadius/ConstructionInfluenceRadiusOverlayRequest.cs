using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionInfluenceRadiusOverlayRequest
    {
        public ConstructionInfluenceRadiusOverlayRequest(
            Vector3 centerWorld,
            int radius,
            float cellSize,
            Material material,
            float fillAlpha,
            float borderWidth,
            Transform excludedRoot)
        {
            CenterWorld = centerWorld;
            Radius = Mathf.Max(0, radius);
            CellSize = Mathf.Max(0.01f, cellSize);
            Material = material;
            FillAlpha = fillAlpha;
            BorderWidth = borderWidth;
            ExcludedRoot = excludedRoot;
        }

        public Vector3 CenterWorld { get; }
        public int Radius { get; }
        public float CellSize { get; }
        public Material Material { get; }
        public float FillAlpha { get; }
        public float BorderWidth { get; }
        public Transform ExcludedRoot { get; }
        public float HalfExtent => (Radius + 0.5f) * CellSize;
    }
}
