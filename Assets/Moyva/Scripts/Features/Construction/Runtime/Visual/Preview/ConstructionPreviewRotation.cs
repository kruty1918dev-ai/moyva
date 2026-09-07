using DG.Tweening;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPreviewRotation : MonoBehaviour
    {
        private Tween _rotation;
        private Quaternion _target;
        private float _yaw;
        private float _targetYaw;
        private bool _initialized;
        public void Apply(Quaternion previous, Quaternion target)
        {
            if (_initialized && Quaternion.Angle(_target, target) < 0.01f)
            {
                if (_rotation != null && _rotation.IsActive()) transform.rotation = previous;
                return;
            }
            _rotation?.Kill();
            if (!_initialized)
            {
                _yaw = previous.eulerAngles.y;
                _targetYaw = _yaw;
                _target = previous;
            }
            _targetYaw += Mathf.DeltaAngle(_target.eulerAngles.y, target.eulerAngles.y);
            _target = target;
            _initialized = true;
            transform.rotation = previous;
            if (!Application.isPlaying) { transform.rotation = target; return; }
            Vector3 angles = target.eulerAngles;
            _rotation = DOTween.To(() => _yaw, value =>
                { _yaw = value; transform.rotation = Quaternion.Euler(angles.x, value, angles.z); },
                _targetYaw, 0.15f).SetEase(Ease.OutCubic)
                .SetRecyclable(true).OnKill(() => _rotation = null);
        }
        private void OnDisable() { _rotation?.Kill(); _initialized = false; }
    }
}
