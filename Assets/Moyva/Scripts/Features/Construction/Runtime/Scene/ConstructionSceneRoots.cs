using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    [System.Serializable]
    public sealed class ConstructionSceneRoots
    {
        [BoxGroup("Roots")]
        public Transform PreviewRoot;

        [BoxGroup("Roots")]
        public Transform PlacedRoot;

        [BoxGroup("Roots")]
        public Transform RadiusRoot;

        [BoxGroup("Roots")]
        public Transform UIRoot;

        [BoxGroup("Roots")]
        public Transform DebugRoot;

        [Button, BoxGroup("Actions")]
        public void ClearEmptyRoots()
        {
            if (PreviewRoot != null && PreviewRoot.Equals(null))
                PreviewRoot = null;
            if (PlacedRoot != null && PlacedRoot.Equals(null))
                PlacedRoot = null;
            if (RadiusRoot != null && RadiusRoot.Equals(null))
                RadiusRoot = null;
            if (UIRoot != null && UIRoot.Equals(null))
                UIRoot = null;
            if (DebugRoot != null && DebugRoot.Equals(null))
                DebugRoot = null;
        }
    }
}
