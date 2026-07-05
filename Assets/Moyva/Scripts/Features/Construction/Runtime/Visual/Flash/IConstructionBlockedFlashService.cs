using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBlockedFlashService
    {
        void Flash(GameObject target, bool isGhostPreview);
        void Tick();
        void Clear();
    }
}
