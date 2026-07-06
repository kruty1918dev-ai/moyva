using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPresentation
    {
        [AssetsOnly]
        [Required]
        public GameObject Prefab;

        [AssetsOnly]
        [PreviewField(64, ObjectFieldAlignment.Left)]
        public Sprite Icon;

        [AssetsOnly]
        [PreviewField(64, ObjectFieldAlignment.Left)]
        public Sprite RuntimePreview;

        public Color UiTint = Color.white;

        [Tooltip("Додатковий Y-offset для preview та поставленої будівлі. Корисно, якщо prefab візуально сидить нижче/вище surface.")]
        public float VisualYOffset;

        [InlineProperty]
        [HideLabel]
        public BuildingPreviewSettings PreviewSettings = new BuildingPreviewSettings();
    }
}
