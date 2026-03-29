using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [Serializable]
    public struct BiomeData
    {
        public string TileID;
        public string BiomeName; // Для зручності (напр. "Deep Lake", "Forest")
        
        [Header("Height Range (0.0 - 1.0)")]
        public float MinHeight;
        public float MaxHeight;

        [Header("Moisture Range (0.0 - 1.0)")]
        [Tooltip("0 - сухо/поле, 1 - мокро/озеро/болото")]
        public float MinMoisture;
        public float MaxMoisture;
    }
}