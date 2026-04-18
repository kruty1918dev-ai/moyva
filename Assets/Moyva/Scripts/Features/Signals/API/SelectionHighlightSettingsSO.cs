using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    [CreateAssetMenu(fileName = "SelectionHighlightSettings", menuName = "Moyva/Signals/Selection Highlight Settings")]
    public sealed class SelectionHighlightSettingsSO : ScriptableObject
    {
        [Header("Outline")]
        [Min(0.25f)] public float OutlineSizePixels = 2f;
        public Color OutlineColorPrimary = new(0.2f, 0.95f, 0.3f, 1f);
        public Color OutlineColorSecondary = Color.white;

        [Header("Animation")]
        [Min(0f)] public float AnimationSpeed = 2.5f;
        [Range(0f, 1f)] public float AnimationMin = 0f;
        [Range(0f, 1f)] public float AnimationMax = 1f;
    }
}