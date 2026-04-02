using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    public class FogOfWarSettings : ScriptableObject
    {
        [Header("Vision Range")]
        public int DefaultVisionRange = 5;
        public int MinVisionRange = 1;
        public int MaxVisionRange = 12;

        [Header("Fog Colors")]
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);
        public Color ExploredColor   = new Color(0f, 0f, 0f, 0.5f);
    }
}
