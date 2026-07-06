using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IWaterLayerMaterialPropertyWriter
    {
        void SetFloat(Material material, string propertyName, float value);
        void SetColor(Material material, string propertyName, Color value);
        void SetVector(Material material, string propertyName, Vector4 value);
    }
}
