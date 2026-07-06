using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class WaterLayerMaterialPropertyWriter : IWaterLayerMaterialPropertyWriter
    {
        public void SetFloat(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        public void SetColor(Material material, string propertyName, Color value)
        {
            if (material != null && material.HasProperty(propertyName))
                material.SetColor(propertyName, value);
        }

        public void SetVector(Material material, string propertyName, Vector4 value)
        {
            if (material != null && material.HasProperty(propertyName))
                material.SetVector(propertyName, value);
        }
    }
}
