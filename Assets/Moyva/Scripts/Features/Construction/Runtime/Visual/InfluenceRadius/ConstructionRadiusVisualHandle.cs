using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionRadiusVisualHandle
    {
        public ConstructionRadiusVisualHandle(GameObject gameObject, MeshRenderer renderer, Material material)
        {
            GameObject = gameObject;
            Renderer = renderer;
            Material = material;
        }

        public GameObject GameObject { get; }
        public MeshRenderer Renderer { get; }
        public Material Material { get; }
    }
}
