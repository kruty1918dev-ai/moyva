using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridOverlayRenderer
    {
        bool MaterialReady { get; }
        void Initialize(Transform parent, string shaderName);
        void ApplyStyle(Color lineColor, Color fillColor, float lineWidth);
        void SetVisible(bool visible);
        void Draw(List<ConstructionBuildGridOverlayEntry> entries, IConstructionBuildGridDiagnostics diagnostics);
    }
}
