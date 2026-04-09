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

        [Header("Height Vision")]
        [Min(0.01f)] public float ElevationStep = 0.15f;
        [Min(0)] public int ObserverHeightBonusPerStep = 1;
        [Min(0)] public int DownhillVisionBonusPerStep = 1;
        [Min(0)] public int UphillVisionPenaltyPerStep = 1;
        [Min(0)] public int MaxObserverHeightBonus = 4;
        [Min(0)] public int MaxDownhillVisionBonus = 2;
        [Min(0)] public int MaxUphillVisionPenalty = 6;
        [Min(0f)] public float OcclusionSlopeBias = 0.02f;

        [Header("Fog Colors")]
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);
        public Color ExploredColor   = new Color(0f, 0f, 0f, 0.5f);
    }
}
