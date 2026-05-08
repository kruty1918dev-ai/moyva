using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    public class FogOfWarSettings : ScriptableObject
    {
        public const int FogTilePixelSize = 16;

        // ─── Vision Range ────────────────────────────────────────────────────
        public int DefaultVisionRange = 5;
        public int MinVisionRange = 1;
        public int MaxVisionRange = 12;

        // ─── Height Vision ───────────────────────────────────────────────────
        [Min(0.01f)] public float ElevationStep = 0.15f;
        [Min(0)] public int ObserverHeightBonusPerStep = 1;
        [Min(0)] public int DownhillVisionBonusPerStep = 1;
        [Min(0)] public int UphillVisionPenaltyPerStep = 1;
        [Min(0)] public int MaxObserverHeightBonus = 4;
        [Min(0)] public int MaxDownhillVisionBonus = 2;
        [Min(0)] public int MaxUphillVisionPenalty = 6;
        [Min(0f)] public float OcclusionSlopeBias = 0.02f;

        // ─── Colors ──────────────────────────────────────────────────────────
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);
        public Color ExploredColor   = new Color(0f, 0f, 0f, 0.5f);

        // ─── Tile ────────────────────────────────────────────────────────────
        public Sprite FogTileSprite;
        [Min(1f)] public float FogTileTiling = 1f;

        // ─── Bitmask autotiling (4-neighbor, 16 variants) ────────────────────
        public bool UseBitmaskAutotiling = true;
        public Sprite[] FogBitmaskSprites = new Sprite[16];

        // ─── Icons ───────────────────────────────────────────────────────────
        public Sprite[] FogIconSprites;
        public Vector2Int FogIconGridSize = new Vector2Int(10, 10);
        [Min(0.1f)] public float FogIconScale = 0.5f;

        // ─── Transparency ─────────────────────────────────────────────────────
        [Range(0f, 1f)] public float UnexploredAlpha = 1f;
        [Range(0f, 1f)] public float ExploredAlpha = 0.5f;
    }
}
