using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/BiomesSettings", fileName = "DataBiomesSettings")]
    public class DataBiomesSettings : ScriptableObject
    {
        public BiomeData[] Biomes;

        public string DefaultTileID = "grass"; // Фолбек, якщо нічого не підійшло
        public float MoistureScale = 1.0f; // Масштаб для генерації вологості
    }
}