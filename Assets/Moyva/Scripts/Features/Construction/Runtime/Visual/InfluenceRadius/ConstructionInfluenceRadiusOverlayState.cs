using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionInfluenceRadiusOverlayState
    {
        public bool Active;
        public Bounds Bounds;
        public Material Material;
        public readonly List<MeshRenderer> Renderers = new();
    }
}
