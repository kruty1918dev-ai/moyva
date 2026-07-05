using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Profiles/Input", fileName = "ConstructionInputProfile")]
    public sealed class ConstructionInputProfileSO : ScriptableObject
    {
        [BoxGroup("Touch")]
        [MinValue(0f)]
        [SerializeField] private float _touchTapMaxMovePixels = 18f;

        [BoxGroup("Touch")]
        [MinValue(0f)]
        [SerializeField] private float _touchTapMaxDurationSeconds = 0.45f;

        [BoxGroup("Behavior")]
        [SerializeField] private bool _enableMousePendingPreviewDrag = true;

        [BoxGroup("Behavior")]
        [SerializeField] private bool _enableTouchPendingPreviewDrag = true;

        [BoxGroup("Behavior")]
        [SerializeField] private bool _enableMultiTouchCancel = true;

        [BoxGroup("UI")]
        [SerializeField] private bool _blockInteractiveUi = true;

        [BoxGroup("UI")]
        [SerializeField] private bool _allowClicksThroughNonInteractiveUi = true;

        public float TouchTapMaxMovePixels => Mathf.Max(0f, _touchTapMaxMovePixels);
        public float TouchTapMaxDurationSeconds => Mathf.Max(0f, _touchTapMaxDurationSeconds);
        public bool EnableMousePendingPreviewDrag => _enableMousePendingPreviewDrag;
        public bool EnableTouchPendingPreviewDrag => _enableTouchPendingPreviewDrag;
        public bool EnableMultiTouchCancel => _enableMultiTouchCancel;
        public bool BlockInteractiveUI => _blockInteractiveUi;
        public bool AllowClicksThroughNonInteractiveUI => _allowClicksThroughNonInteractiveUi;
    }
}
