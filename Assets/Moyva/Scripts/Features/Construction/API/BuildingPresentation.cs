using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingPreviewSettings
    {
        public Vector3 CameraOffset = new Vector3(4f, 5f, -6f);
        public Vector3 CameraEulerAngles = new Vector3(45f, -35f, 0f);
        [Min(1f)] public float OrthographicSize = 4f;
        public Color BackgroundColor = new Color(0f, 0f, 0f, 0f);
    }

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
