using System;

namespace Kruty1918.Moyva.Generator.API
{
    [System.Serializable]
    public struct BiomeData
    {
        public string TileID;
        public float HeightThreshold;
    }

    [Serializable]
    public struct RiverWidthData
    {
        public string TileID; // Наприклад "Sand", "Water_Shallow", "Water_Deep"
        public float Radius;  // Радіус пензля

        public string[] ObstacleTileIDs; // Ідентифікатори тайлів, які річка може прорізати (наприклад, "Grass", "Forest")
    }
}