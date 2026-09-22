using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.InputRouting.API;
using Kruty1918.Moyva.Shared.Localization;
using Kruty1918.Localization;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    // P077 world-target adapter: pointer-hovers on placed buildings resolve a
    // short localized tooltip shown through the same UnityHtmlTooltipLayer the
    // HTML UI uses — no parallel world tooltip system. Clears when the pointer
    // is over UI, over a preview visual, or over nothing tooltip-worthy.
    internal sealed class GameplayWorldTooltipService : ITickable, IDisposable
    {
        private readonly IUnityHtmlHost _host;
        private readonly IConstructionPointerInputSource _pointerSource;
        private readonly IGameplayInputPolicy _inputPolicy;
        private readonly IBuildingRegistry _buildings;
        private readonly ILocalizationService _localization;
        private UnityEngine.Camera _camera;
        private bool _cameraSearched;

        // Test seam: EditMode physics does not answer raycasts, so the hit
        // resolver is swappable while the hover/policy/text pipeline stays real.
        internal Func<Ray, ConstructionBuildingPointerTarget> RaycastTarget = DefaultRaycast;

        public GameplayWorldTooltipService(
            IUnityHtmlHost host,
            IConstructionPointerInputSource pointerSource,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null,
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] ILocalizationService localization = null,
            [InjectOptional] UnityEngine.Camera camera = null)
        {
            _host = host;
            _pointerSource = pointerSource;
            _inputPolicy = inputPolicy;
            _buildings = buildings;
            _localization = localization;
            _camera = camera;
        }

        public void Tick()
        {
            if (_host == null)
                return;

            ConstructionPointerSnapshot pointer =
                _pointerSource != null ? _pointerSource.ReadPointerSnapshot() : ConstructionPointerSnapshot.None;
            if (!pointer.HasPointer || pointer.IsTouch)
            {
                Clear();
                return;
            }
            if (_inputPolicy != null && _inputPolicy.IsPointerOverUi(pointer.Position, pointer.PointerId))
            {
                Clear();
                return;
            }

            UnityEngine.Camera camera = ResolveCamera();
            if (camera == null)
            {
                Clear();
                return;
            }

            var target = RaycastTarget?.Invoke(camera.ScreenPointToRay(pointer.Position));
            if (target == null || target.IsPreviewVisual)
            {
                Clear();
                return;
            }

            string text = ResolveTooltipText(target);
            _host.SetWorldTooltip(text, pointer.Position);
        }

        private string ResolveTooltipText(ConstructionBuildingPointerTarget target)
        {
            string name = _buildings?.GetById(target.BuildingId)?.DisplayName;
            if (string.IsNullOrWhiteSpace(name))
                name = target.BuildingId;
            return _localization?.T(name) ?? name;
        }

        private void Clear() => _host.SetWorldTooltip(null, Vector2.zero);

        private UnityEngine.Camera ResolveCamera()
        {
            if (_camera != null)
                return _camera;
            if (!_cameraSearched)
            {
                _cameraSearched = true;
                _camera = UnityEngine.Camera.main;
            }
            return _camera;
        }

        private static ConstructionBuildingPointerTarget DefaultRaycast(Ray ray)
        {
            return Physics.Raycast(ray, out RaycastHit hit, 2000f)
                ? hit.collider.GetComponentInParent<ConstructionBuildingPointerTarget>()
                : null;
        }

        public void Dispose()
        {
            _host?.SetWorldTooltip(null, Vector2.zero);
        }
    }
}
