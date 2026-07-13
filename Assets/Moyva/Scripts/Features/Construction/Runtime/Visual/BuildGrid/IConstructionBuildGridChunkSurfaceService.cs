
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridChunkSurfaceService
    {
        bool MaterialReady { get; }
        bool IsUpdating { get; }

        void Initialize(string shaderName);
        void ApplyStyle(Color lineColor, Color fillColor, float lineWidth);
        void ResetWorld();
        void EnsureVisibleChunks(bool invalidateMasks);
        void InvalidateAllMasks();
        void InvalidateRegion(Vector2Int center, int radius);
        void ProcessUpdates(float budgetMilliseconds);
        void SetVisible(bool visible);
        void Hide();
        void ApplyChunkVisibility();
        void Clear();
        void Dispose();
    }
}
