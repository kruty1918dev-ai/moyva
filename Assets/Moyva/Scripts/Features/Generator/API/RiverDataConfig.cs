using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/RiverDataConfig", fileName = "RiverDataConfig")]
    public class RiverDataConfig : ScriptableObject
    {
        [Header("River Appearance")]
        public RiverWidthData[] WidthLayers;
        public int RiversCount = 2;

        [Header("Generation Constraints")]
        [Tooltip("Річка може початися, якщо висота тайла в цьому діапазоні")]
        public Vector2 StartHeightRange = new Vector2(0.7f, 1.0f);

        [Tooltip("Річка може закінчитися, якщо висота тайла в цьому діапазоні")]
        public Vector2 EndHeightRange = new Vector2(0.0f, 0.3f);
    }
}