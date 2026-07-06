using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualBoundsAlignmentService : IConstructionVisualBoundsAlignmentService
    {
        public void AlignCenterXZ(GameObject instance, Vector3 targetCenter)
        {
            if (instance == null)
                return;

            Vector3 delta = ResolveCenterDelta(instance, targetCenter);
            if (Mathf.Abs(delta.x) <= 0.0001f && Mathf.Abs(delta.z) <= 0.0001f)
                return;

            Transform transform = instance.transform;
            transform.position = new Vector3(transform.position.x + delta.x, transform.position.y, transform.position.z + delta.z);
        }

        private static Vector3 ResolveCenterDelta(GameObject instance, Vector3 targetCenter)
        {
            if (!GridSurfacePlacementUtility.TryResolveRendererBounds(instance, out Bounds bounds))
            {
                Vector3 position = instance.transform.position;
                return new Vector3(targetCenter.x - position.x, 0f, targetCenter.z - position.z);
            }

            return new Vector3(targetCenter.x - bounds.center.x, 0f, targetCenter.z - bounds.center.z);
        }
    }
}
