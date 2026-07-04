using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Налаштування одного fog state для TWC volume builder-а.
    /// Дозволяє окремо описати unexplored та explored presentation.
    /// </summary>
    [Serializable]
    public sealed class FogVolumeStateTileSettings
    {
        /// <summary>
        /// Чи будується цей visual state взагалі.
        /// </summary>
        [HorizontalGroup("State", Width = 80)]
        public bool Enabled = true;

        /// <summary>
        /// Назва runtime build layer-а у TWC configuration clone.
        /// </summary>
        [HorizontalGroup("State")]
        [Required]
        [ValidateInput(nameof(HasLayerName), "Layer name cannot be empty.")]
        public string LayerName = "Fog_State";

        /// <summary>
        /// Набір можливих preset-варіантів для цього fog state.
        /// </summary>
        [TableList(AlwaysExpanded = true, DrawScrollView = false)]
        [ValidateInput(nameof(HasRequiredPreset), "Enabled fog state needs at least one dual-grid TilePreset.")]
        public List<FogVolumeTilePresetVariant> TileVariants = new List<FogVolumeTilePresetVariant>
        {
            new FogVolumeTilePresetVariant()
        };

        /// <summary>
        /// Вертикальний offset build layer-а відносно базової висоти світу.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public float LayerYOffset;

        /// <summary>
        /// Додатковий scale для згенерованих fog prefabs.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public Vector3 ScaleOffset = Vector3.one;

        /// <summary>
        /// Shadow casting mode для build layer-а.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.Off;

        /// <summary>
        /// Unity layer для об'єктів цього fog state.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public LayerMask ObjectLayer = 0;

        /// <summary>
        /// Rendering layer mask для URP/light interaction.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public RenderingLayerMask RenderingLayer = 1;

        /// <summary>
        /// Тип collider-а, який TWC призначить fog tiles цього state.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public Configuration.ColliderType ColliderType = Configuration.ColliderType.none;

        /// <summary>
        /// Висота collider-а для build layer-а.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        [MinValue(0f)]
        public float TileColliderHeight;

        /// <summary>
        /// Додаткова екструзія collider-а.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        [MinValue(0f)]
        public float TileColliderExtrusionHeight;

        /// <summary>
        /// Чи потрібно інвертувати collision walls для TWC collider build.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public bool InvertCollisionWalls;

        /// <summary>
        /// Нормалізує обов'язкові значення для runtime build path.
        /// Викликається під час валідації settings або перед побудовою runtime config.
        /// </summary>
        /// <param name="fallbackLayerName">Назва layer-а за замовчуванням, якщо користувач не вказав свою.</param>
        public void EnsureDefaults(string fallbackLayerName)
        {
            if (string.IsNullOrWhiteSpace(LayerName))
                LayerName = fallbackLayerName;

            TileVariants ??= new List<FogVolumeTilePresetVariant>();
            if (TileVariants.Count == 0)
                TileVariants.Add(new FogVolumeTilePresetVariant());

            ScaleOffset = new Vector3(
                Mathf.Max(0.001f, ScaleOffset.x),
                Mathf.Max(0.001f, ScaleOffset.y),
                Mathf.Max(0.001f, ScaleOffset.z));
            TileColliderHeight = Mathf.Max(0f, TileColliderHeight);
            TileColliderExtrusionHeight = Mathf.Max(0f, TileColliderExtrusionHeight);
        }

        private bool HasLayerName(string layerName)
            => !Enabled || !string.IsNullOrWhiteSpace(layerName);

        private bool HasRequiredPreset(List<FogVolumeTilePresetVariant> variants)
        {
            if (!Enabled)
                return true;

            if (variants == null)
                return false;

            for (int i = 0; i < variants.Count; i++)
            {
                var variant = variants[i];
                if (variant != null
                    && variant.Preset != null
                    && FogTilePresetUtility.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }
    }
}
