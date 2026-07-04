using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Групує налаштування runtime/preview TWC volume presentation для FogOfWar.
    /// Це лише visual config і не визначає gameplay fog state напряму.
    /// </summary>
    [Serializable]
    public sealed class FogVolumeTileSettings
    {
        /// <summary>
        /// Режим оновлення volume visual updater-а.
        /// </summary>
        [BoxGroup("Runtime")]
        public FogVolumeUpdateMode UpdateMode = FogVolumeUpdateMode.DebouncePerFrame;

        /// <summary>
        /// Інтервал між rebuild-ами у режимі <see cref="FogVolumeUpdateMode.Interval"/>.
        /// </summary>
        [BoxGroup("Runtime")]
        [ShowIf(nameof(UpdateMode), FogVolumeUpdateMode.Interval)]
        [MinValue(0.02f)]
        public float RebuildIntervalSeconds = 0.1f;

        /// <summary>
        /// Вмикає clustered mesh renderer для runtime dirty updates.
        /// Експериментальний path малює власні clustered meshes і не використовує TWC tile presets напряму.
        /// За замовчуванням вимкнено, щоб runtime fog зберігав ті tiles/presets, які передані в TWC volume.
        /// </summary>
        [BoxGroup("Runtime Clustered Renderer")]
        public bool UseClusteredRuntimeFogRenderer = false;

        /// <summary>
        /// Розмір одного fog cluster-а у клітинках для partial mesh rebuild.
        /// </summary>
        [BoxGroup("Runtime Clustered Renderer")]
        [MinValue(1)]
        public int ClusterSize = 16;

        /// <summary>
        /// Додатковий halo біля меж cluster-а, щоб edge-залежні meshes оновлювали сусідів.
        /// </summary>
        [BoxGroup("Runtime Clustered Renderer")]
        [MinValue(0)]
        public int ClusterPaddingCells = 1;

        /// <summary>
        /// Дозволяє clustered renderer-у перейти на full clustered rebuild, якщо dirty update зачіпає забагато clusters.
        /// </summary>
        [BoxGroup("Runtime Clustered Renderer")]
        public bool AllowFullRebuildFallback = true;

        /// <summary>
        /// Частка dirty clusters від карти, після якої partial update дорожчий за full clustered rebuild.
        /// </summary>
        [BoxGroup("Runtime Clustered Renderer")]
        [Range(0.01f, 1f)]
        public float FullRebuildDirtyClusterRatioThreshold = 0.35f;

        /// <summary>
        /// Чи логувати clustered fog update diagnostics.
        /// </summary>
        [BoxGroup("Runtime Clustered Renderer")]
        public bool LogClusterUpdates = true;

        /// <summary>
        /// Додатковий простір над світом для об'ємного fog volume.
        /// </summary>
        [BoxGroup("Runtime")]
        [MinValue(0f)]
        public float TopClearance = 0.08f;

        /// <summary>
        /// Чи використовувати cell size generated світу як базовий розмір fog tiles.
        /// </summary>
        [BoxGroup("Runtime")]
        public bool UseWorldCellSize = true;

        /// <summary>
        /// Ручний cell size для fog volume, якщо world cell size не використовується.
        /// </summary>
        [BoxGroup("Runtime")]
        [HideIf(nameof(UseWorldCellSize))]
        [MinValue(0.001f)]
        public float CellSizeOverride = 1f;

        /// <summary>
        /// Джерело висоти для побудови fog volume.
        /// </summary>
        [BoxGroup("Generated World Heights")]
        public FogVolumeHeightSource HeightSource = FogVolumeHeightSource.TerrainLevelMapThenHeightMap;

        /// <summary>
        /// Скільки world units відповідає одному рівню в `TerrainLevelMap`.
        /// Використовується, коли generated світ задає висоту дискретними level-ами.
        /// </summary>
        [BoxGroup("Generated World Heights")]
        [Tooltip("World units per TerrainLevelMap step when the generated world provides integer terrain levels.")]
        [MinValue(0.001f)]
        public float TerrainLevelHeightStep = 1f;

        /// <summary>
        /// Крок квантування height map для об'єднання близьких висот в один runtime layer.
        /// Допомагає не роздувати кількість TWC layer-ів на шумних мапах висот.
        /// </summary>
        [BoxGroup("Generated World Heights")]
        [Tooltip("Height values closer than this are built on the same TWC layer. Keeps runtime layer count stable for noisy HeightMap data.")]
        [MinValue(0.001f)]
        public float HeightLayerSnap = 0.01f;

        /// <summary>
        /// Чи слід автоматично очищати editor preview перед runtime стартом.
        /// </summary>
        [BoxGroup("Preview")]
        [Tooltip("Editor preview uses the current scene TWC grid when available. It is cleared automatically at runtime and rebuilt from generated world data.")]
        public bool ClearPreviewOnRuntimeStart = true;

        /// <summary>
        /// Fallback ширина preview grid-а, якщо у сцені немає доступного world source manager-а.
        /// </summary>
        [BoxGroup("Preview")]
        [MinValue(1)]
        public int PreviewFallbackWidth = 16;

        /// <summary>
        /// Fallback висота preview grid-а, якщо у сцені немає доступного world source manager-а.
        /// </summary>
        [BoxGroup("Preview")]
        [MinValue(1)]
        public int PreviewFallbackHeight = 16;

        /// <summary>
        /// Visual settings для повністю unexplored fog state.
        /// </summary>
        [BoxGroup("Unexplored Fog")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeStateTileSettings Unexplored = new FogVolumeStateTileSettings
        {
            LayerName = "Fog_Unexplored",
            LayerYOffset = 0f,
        };

        /// <summary>
        /// Visual settings для explored, але вже не visible fog state.
        /// </summary>
        [BoxGroup("Explored Fog")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeStateTileSettings Explored = new FogVolumeStateTileSettings
        {
            LayerName = "Fog_Explored",
            LayerYOffset = 0.02f,
        };

        /// <summary>
        /// Нормалізує мінімально валідні значення для runtime та preview volume build.
        /// </summary>
        public void EnsureDefaults()
        {
            RebuildIntervalSeconds = Mathf.Max(0.02f, RebuildIntervalSeconds);
            ClusterSize = Mathf.Max(1, ClusterSize);
            ClusterPaddingCells = Mathf.Clamp(ClusterPaddingCells, 0, ClusterSize);
            FullRebuildDirtyClusterRatioThreshold = Mathf.Clamp(FullRebuildDirtyClusterRatioThreshold, 0.01f, 1f);
            TopClearance = Mathf.Max(0f, TopClearance);
            CellSizeOverride = Mathf.Max(0.001f, CellSizeOverride);
            TerrainLevelHeightStep = Mathf.Max(0.001f, TerrainLevelHeightStep);
            HeightLayerSnap = Mathf.Max(0.001f, HeightLayerSnap);
            PreviewFallbackWidth = Mathf.Max(1, PreviewFallbackWidth);
            PreviewFallbackHeight = Mathf.Max(1, PreviewFallbackHeight);
            Unexplored ??= new FogVolumeStateTileSettings();
            Explored ??= new FogVolumeStateTileSettings();
            Unexplored.EnsureDefaults("Fog_Unexplored");
            Explored.EnsureDefaults("Fog_Explored");
        }
    }
}
