using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionSmoothVisualMotion : MonoBehaviour
    {
        private const float SnapDistance = 0.01f;
        private const float MinSharpness = 0.01f;

        private Vector3 _targetPosition;
        private float _sharpness = 18f;
        private bool _active;

        public static ConstructionSmoothVisualMotion AttachOrUpdate(GameObject root)
        {
            if (root == null)
                return null;

            var motion = root.GetComponent<ConstructionSmoothVisualMotion>();
            if (motion == null)
                motion = root.AddComponent<ConstructionSmoothVisualMotion>();

            motion.enabled = motion._active;
            return motion;
        }

        public void JumpToCurrent()
        {
            _targetPosition = transform.position;
            _active = false;
            enabled = false;
        }

        public void MoveTo(Vector3 targetPosition, float sharpness)
        {
            _targetPosition = targetPosition;
            _sharpness = Mathf.Max(MinSharpness, sharpness);
            _active = true;
            enabled = true;
        }

        private void Awake()
        {
            _targetPosition = transform.position;
            enabled = false;
        }

        private void Update()
        {
            if (!_active)
            {
                enabled = false;
                return;
            }

            float t = 1f - Mathf.Exp(-_sharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, _targetPosition, t);

            if ((transform.position - _targetPosition).sqrMagnitude > SnapDistance * SnapDistance)
                return;

            transform.position = _targetPosition;
            _active = false;
            enabled = false;
        }
    }
}
