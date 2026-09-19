using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Height-sampling configuration shared by the screen-space fog visuals:
    /// the boundary curtain and the fog height sampler both resolve world
    /// heights through these settings.
    /// </summary>
    [Serializable]
    public sealed class FogVolumeTileSettings
    {
        /// <summary>
        /// Додатковий вертикальний простір між fog шарами різних висот.
        /// Залишай 0, якщо потрібна максимально щільна посадка без штучних проміжків.
        /// </summary>
        [BoxGroup("Runtime")]
        [Tooltip("Extra vertical gap between fog layers on different heights. Leave at 0 to keep layers tight.")]
        [MinValue(0f)]
        public float VerticalLayerSpacing = 0f;

        /// <summary>
        /// Додатковий простір над світом для fog visuals.
        /// </summary>
        [BoxGroup("Runtime")]
        [MinValue(0f)]
        public float TopClearance = 0.08f;

        /// <summary>
        /// Джерело висоти для fog visuals.
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
        /// Допомагає не роздувати кількість шарів на шумних мапах висот.
        /// </summary>
        [BoxGroup("Generated World Heights")]
        [Tooltip("Height values closer than this share the same visual layer. Keeps layer count stable for noisy HeightMap data.")]
        [MinValue(0.001f)]
        public float HeightLayerSnap = 0.01f;

        /// <summary>
        /// Нормалізує мінімально валідні значення.
        /// </summary>
        public void EnsureDefaults()
        {
            VerticalLayerSpacing = Mathf.Max(0f, VerticalLayerSpacing);
            TopClearance = Mathf.Max(0f, TopClearance);
            TerrainLevelHeightStep = Mathf.Max(0.001f, TerrainLevelHeightStep);
            HeightLayerSnap = Mathf.Max(0.001f, HeightLayerSnap);
        }
    }
}
