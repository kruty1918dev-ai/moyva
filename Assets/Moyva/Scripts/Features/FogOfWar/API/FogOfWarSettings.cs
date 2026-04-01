using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    public class FogOfWarSettings : ScriptableObject
    {
        [Header("Vision")]
        public int DefaultVisionRange = 5;

        [Header("Fog Colors")]
        public Color UnexploredFogColor = new Color(0.03f, 0.03f, 0.08f, 1f);
        public Color ExploredFogColor   = new Color(0.08f, 0.10f, 0.14f, 0.65f);

        [Header("Perlin Noise — Unexplored")]
        public float NoiseScaleUnexplored    = 3.5f;
        public float NoiseSpeedUnexplored    = 0.04f;
        public float NoiseStrengthUnexplored = 0.25f;

        [Header("Perlin Noise — Explored")]
        public float NoiseScaleExplored    = 2.0f;
        public float NoiseSpeedExplored    = 0.02f;
        public float NoiseStrengthExplored = 0.15f;

        [Header("Edge Bleeding")]
        [Range(0f, 1f)] public float EdgeBleedRadius   = 0.35f;
        [Range(0f, 1f)] public float EdgeBleedStrength = 0.40f;

        [Header("Transitions")]
        [Range(0f, 0.5f)] public float TransitionSoftness = 0.12f;

        [Header("Texture")]
        public FilterMode TextureFilter = FilterMode.Bilinear;
    }
}
