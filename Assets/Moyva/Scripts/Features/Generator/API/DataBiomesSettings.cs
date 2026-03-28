using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/BiomesSettings", fileName = "DataBiomesSettings")]
    public class DataBiomesSettings : ScriptableObject
    {
        public BiomeData[] Biomes;
    }
}