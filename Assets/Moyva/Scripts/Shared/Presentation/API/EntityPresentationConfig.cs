using System;
using UnityEngine;

namespace Kruty1918.Moyva.Presentation.API
{
    [Serializable]
    public class EntityPresentationConfig
    {
        [Min(0.0001f)]
        public float ScaleMultiplier = 1f;

        public Vector3 PositionOffset = Vector3.zero;
        public Vector3 RotationOffset = Vector3.zero;
        public float? GroundOffsetY { get; set; }
        public Color? Tint { get; set; }
        public bool? TeamColorEnabled { get; set; }
        public EntityOutlineConfig Outline = new EntityOutlineConfig();
        public EntityShadowConfig Shadows = new EntityShadowConfig();
        public GameObject PreviewPrefab;
        public EntitySelectionPresentationConfig Selection = new EntitySelectionPresentationConfig();

        public float ResolveScaleMultiplier()
            => IsFinite(ScaleMultiplier) && ScaleMultiplier > 0f
                ? ScaleMultiplier
                : 1f;

        public float ResolveGroundOffsetY(float fallback)
            => GroundOffsetY.HasValue && IsFinite(GroundOffsetY.Value)
                ? GroundOffsetY.Value
                : fallback;

        public float ResolveSelectionMarkerScale()
            => Selection != null
               && IsFinite(Selection.MarkerScale)
               && Selection.MarkerScale > 0f
                ? Selection.MarkerScale
                : 1f;

        public static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    [Serializable]
    public sealed class EntityOutlineConfig
    {
        public bool? Enabled { get; set; }
        [Min(0f)]
        public float? Width { get; set; }
        public Color? Color { get; set; }
    }

    [Serializable]
    public sealed class EntityShadowConfig
    {
        public bool? Cast { get; set; }
        public bool? Receive { get; set; }
    }

    [Serializable]
    public sealed class EntitySelectionPresentationConfig
    {
        [Min(0.0001f)]
        public float MarkerScale = 1f;
    }
}
