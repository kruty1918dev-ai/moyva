using System;
using GiantGrey.TileWorldCreator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Описує один варіант TWC preset-а для побудови volume fog tile.
    /// Використовується лише visual layer і не впливає на gameplay fog state.
    /// </summary>
    [Serializable]
    public sealed class FogVolumeTilePresetVariant
    {
        /// <summary>
        /// TWC preset з dual-grid prefabs для цього варіанта.
        /// </summary>
        [Required]
        [AssetsOnly]
        [ValidateInput(nameof(HasDualGridPrefab), "TilePreset має містити dual-grid prefabs.")]
        public TilePreset Preset;

        /// <summary>
        /// Слот колонки, до якого належить цей preset.
        /// </summary>
        public FogVolumeTilePresetSlot Slot = FogVolumeTilePresetSlot.Top;

        /// <summary>
        /// Вага випадкового вибору варіанта серед інших preset-ів того ж стану.
        /// </summary>
        [Range(0f, 1f)]
        public float Weight = 1f;

        /// <summary>
        /// Додаткова локальна висота для цього tile variant.
        /// </summary>
        [MinValue(0f)]
        public float TileHeight;

        /// <summary>
        /// Показує, чи preset призначено взагалі.
        /// </summary>
        public bool IsConfigured => Preset != null;

        /// <summary>
        /// Повертає вагу в діапазоні [0..1].
        /// </summary>
        public float NormalizedWeight => Mathf.Clamp01(Weight);

        private bool HasDualGridPrefab(TilePreset preset)
        {
            return preset == null || FogTilePresetUtility.HasUsableDualGridPreset(preset);
        }
    }
}
