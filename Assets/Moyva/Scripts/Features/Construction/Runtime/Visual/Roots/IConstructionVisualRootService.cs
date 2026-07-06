using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionVisualRootService
    {
        Transform PreviewRoot { get; }
        Transform PlacedRoot { get; }
        Transform RadiusRoot { get; }
        void EnsureRoots();
    }
}
