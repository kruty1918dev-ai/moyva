using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/NoiseSettings", fileName = "DataNoiseSettings")]
    public class DataNoiseSettings : ScriptableObject
    {
        public float Scale = 20f;
        public int Octaves = 4;
        public float Persistance = 0.5f;
        public float Lacunarity = 2f;
        public Vector2 Offset = Vector2.zero;
    }
}