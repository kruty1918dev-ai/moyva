using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        /// <summary>
        /// Legacy tint для повністю unexplored стану в старому shader overlay path.
        /// Залишене для migration/debug і не є source of truth для нового volume fog.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);

        /// <summary>
        /// Legacy tint для explored стану в старому shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Color ExploredColor = new Color(0f, 0f, 0f, 0.5f);

        /// <summary>
        /// Legacy sprite тайла для 2D shader/quad fog presentation.
        /// Новий runtime volume path більше не читає це поле напряму.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Sprite FogTileSprite;

        /// <summary>
        /// Legacy pixel size fog tile sprite-а для shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogTileSpritePixelSize = new Vector2Int(16, 16);

        /// <summary>
        /// Legacy розмір одного fog tile в координатах клітинок для shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2 FogTileSizeInCells = Vector2.one;

        /// <summary>
        /// Legacy overlap у пікселях між сусідніми fog tile-ами, щоб зменшити seams у shader path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogTileSeamOverlapPixels = 1f;

        /// <summary>
        /// Legacy edge padding у пікселях для fog overlay texture.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogMapEdgePaddingPixels = 2f;

        /// <summary>
        /// Legacy overhang fog overlay за межі карти в клітинках.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogMapEdgeOverhangCells = 0.5f;

        /// <summary>
        /// Legacy tiling множник для fog tile texture в shader path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogTileTiling = 1f;

        /// <summary>
        /// Legacy набір icon sprite-ів для старого shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Sprite[] FogIconSprites;

        /// <summary>
        /// Legacy pixel size icon sprite-ів у shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogIconSpritePixelSize = new Vector2Int(16, 16);

        /// <summary>
        /// Legacy розмір сітки іконок для shader overlay presentation.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogIconGridSize = new Vector2Int(10, 10);

        /// <summary>
        /// Legacy scale іконок поверх fog overlay.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogIconScale = 0.5f;

        /// <summary>
        /// Legacy прапорець центрування іконки всередині клітинки shader overlay.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool CenterIcon = true;

        /// <summary>
        /// Legacy режим, у якому visible клітинки робляться повністю прозорими у shader path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool FullyTransparentWhenVisible = true;

        /// <summary>
        /// Legacy top clearance для старого 3D/shader fog presentation path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float Fog3DTopClearance = 0.08f;

        /// <summary>
        /// Legacy перемикач shader-based fog culling.
        /// Збережений для сумісності старих пресетів і migration notes.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool EnableShaderFogCulling = false;

        /// <summary>
        /// Legacy поріг для shader fog culling path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float ShaderFogCullThreshold = 0.01f;
    }
}
