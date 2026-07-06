using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IWaterLayerMaterialApplier
    {
        void Apply(Material material, IWaterLayerMaterialSettings settings);
    }
}
