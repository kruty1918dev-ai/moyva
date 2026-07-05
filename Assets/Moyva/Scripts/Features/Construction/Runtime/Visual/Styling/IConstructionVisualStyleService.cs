using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionVisualStyleService
    {
        void ApplyGhostStyle(GameObject rootObject, bool isValid);
        void ApplySolidStyle(GameObject rootObject);
        void EnsureBuildingSortingOrder(GameObject rootObject, int minOrder);
        void EnsureRenderersEnabled(GameObject rootObject);
        void DisableColliders(GameObject rootObject);
    }
}
