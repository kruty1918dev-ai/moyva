using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/HeightMapSettings", fileName = "HeightMapSettings")]
    public class HeightMapSettings : ScriptableObject
    {
        public HeightLayer[] HeightLayers;
    }

    [System.Serializable]
    public class HeightLayer
    {
        public string TileID; // Ідентифікатор тайла, який відповідає цьому діапазону висот
        public float MinHeight;
        public float MaxHeight;
    }
}