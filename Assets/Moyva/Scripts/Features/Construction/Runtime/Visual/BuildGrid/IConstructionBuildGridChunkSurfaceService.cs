
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridChunkSurfaceService
    {
        bool MaterialReady { get; }

        void Initialize(string shaderName);
        void ApplyStyle(Color lineColor, Color fillColor, float lineWidth);
        void Rebuild();
        void SetVisible(bool visible);
        void Hide();
        void ApplyChunkVisibility();
        void Clear();
    }
}
