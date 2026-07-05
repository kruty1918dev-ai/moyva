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

        [InlineProperty]
        [HideLabel]
        public BuildingPreviewSettings PreviewSettings = new BuildingPreviewSettings();
    }
}
