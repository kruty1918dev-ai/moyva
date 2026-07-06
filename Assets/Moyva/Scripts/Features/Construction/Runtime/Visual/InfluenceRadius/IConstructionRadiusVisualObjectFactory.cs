using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionRadiusVisualObjectFactory
    {
        ConstructionRadiusVisualHandle Create(string name, int sortingOffset, Mesh mesh);
    }
}
