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

        [Header("Fog Tile Sprite")]
        public Sprite FogTileSprite;
        [Tooltip("Sprite sample size in pixels, measured from the sprite rect origin inside the texture atlas.")]
        public Vector2Int FogTileSpritePixelSize = new Vector2Int(16, 16);
        [Tooltip("Scale for tiling the fog cell texture. Higher = more tiles per cell")]
        [Min(1f)] public float FogTileTiling = 1f;

        [Header("Fog Icons")]
        [Tooltip("Array of icon sprites to cycle through fog cells in regular pattern")]
        public Sprite[] FogIconSprites;
        [Tooltip("Icon sprite sample size in pixels, measured from the icon sprite rect origin inside the texture atlas.")]
        public Vector2Int FogIconSpritePixelSize = new Vector2Int(16, 16);
        [Tooltip("Independent icon grid over map (X columns, Y rows). Icons are placed sequentially by this grid, not by tile cells")]
        public Vector2Int FogIconGridSize = new Vector2Int(10, 10);
        [Tooltip("Icon scale relative to cell size")]
        [Min(0.1f)] public float FogIconScale = 0.5f;
        [Tooltip("If enabled, place icon at cell center; if disabled, distribute across cell")]
        public bool CenterIcon = true;

        [Header("Transparency Blending")]
        [Tooltip("Alpha for fully unexplored fog")]
        [Range(0f, 1f)] public float UnexploredAlpha = 1f;
        [Tooltip("Alpha for explored fog")]
        [Range(0f, 1f)] public float ExploredAlpha = 0.5f;
        [Tooltip("Visible areas are fully transparent (alpha=0)")]
        public bool FullyTransparentWhenVisible = true;

        private void OnValidate()
        {
            FogTileSpritePixelSize = ClampSpritePixelSize(FogTileSpritePixelSize);
            FogIconSpritePixelSize = ClampSpritePixelSize(FogIconSpritePixelSize);
        }

        private static Vector2Int ClampSpritePixelSize(Vector2Int size)
        {
            return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }
    }
}
