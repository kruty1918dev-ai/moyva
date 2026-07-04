using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        /// <summary>
        /// Чи дозволено приховувати world renderer-и під повністю unexplored fog.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Disables world renderers that are fully covered by unexplored fog.")]
        public bool EnableRendererCulling = true;

        /// <summary>
        /// Чи потрібен майже непрозорий unexplored fog, щоб renderer culling увімкнувся.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("If enabled, renderer culling works only when UnexploredAlpha is close to opaque (>= 0.99).")]
        public bool RequireOpaqueUnexploredForCulling = true;

        /// <summary>
        /// Layer mask renderer-ів, які може ховати fog culling service.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Layers affected by fog renderer culling.")]
        public LayerMask RendererCullingLayerMask = ~0;

        /// <summary>
        /// Максимальна кількість renderer-ів, які culling service перевіряє за кадр.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Maximum tracked renderers evaluated per frame. Lower values spread work across more frames.")]
        [MinValue(1)]
        public int RendererCullingMaxRenderersPerFrame = 384;

        /// <summary>
        /// Як часто culling service шукає нові renderer-и у світі.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("How often the culling service searches for newly spawned world renderers.")]
        [MinValue(0.05f)]
        public float RendererCullingDiscoveryInterval = 0.75f;

        /// <summary>
        /// Додатковий padding bounds-ів у клітинках для безпечного culling-а на краях.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Small bounds padding in map cells to avoid edge flicker when sprites move between cells.")]
        [MinValue(0f)]
        public float RendererCullingBoundsPaddingCells = 0f;

        /// <summary>
        /// Alpha для повністю unexplored fog state.
        /// Залишається корисною для legacy notes і safety checks у culling path.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Alpha for fully unexplored fog; still used by renderer culling safety checks.")]
        [Range(0f, 1f)]
        public float UnexploredAlpha = 1f;

        /// <summary>
        /// Alpha для explored fog state.
        /// Зберігається для migration notes та tuning explored presentation.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Alpha for explored fog; kept for preset migration and tuning notes.")]
        [Range(0f, 1f)]
        public float ExploredAlpha = 0.5f;
    }
}
