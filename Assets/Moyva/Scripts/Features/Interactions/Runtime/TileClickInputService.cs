using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Zenject;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class TileClickInputService : ITickable
    {
        private const float TouchTapMaxMovePixels = 18f;
        private const float TouchTapMaxDurationSeconds = 0.45f;

        private readonly SignalBus _signalBus;
        private readonly IGridProjection _gridProjection;
        private readonly IWorldPointerGridResolver _pointerGridResolver;
        private readonly IGameplayInputPolicy _inputPolicy;

        private int _trackedTouchId = -1;
        private Vector2 _touchStartScreenPosition;
        private float _touchStartTime;
        private bool _touchStartedOverUi;
        private bool _touchMovedBeyondTap;
        private bool _multiTouchObserved;
        private Camera _cachedCamera;

        public TileClickInputService(
            SignalBus signalBus,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IWorldPointerGridResolver pointerGridResolver = null,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null)
        {
            _signalBus = signalBus;
            _gridProjection = gridProjection;
            _pointerGridResolver = pointerGridResolver;
            _inputPolicy = inputPolicy;
        }

        public void Tick()
        {
            if (HandleTouchInput())
                return;

            var mouse = Mouse.current;
            if (mouse == null)
                return;

            Vector2 screenPos = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
                TryFireMouseTileClick(screenPos, TilePointerButton.Primary);
            else if (mouse.rightButton.wasPressedThisFrame)
                TryFireMouseTileClick(screenPos, TilePointerButton.Secondary);
        }

        private bool HandleTouchInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            int pressedTouchCount = CountPressedTouches(touchscreen);
            if (pressedTouchCount > 1)
                _multiTouchObserved = true;

            var touches = touchscreen.touches;
            for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
            {
                TouchControl touch = touches[touchIndex];
                int touchId = touch.touchId.ReadValue();

                if (_trackedTouchId < 0 && touch.press.wasPressedThisFrame)
                {
                    BeginTouchTracking(touch, touchId);
                    return true;
                }

                if (touchId != _trackedTouchId)
                    continue;

                if (touch.press.isPressed)
                {
                    Vector2 currentPosition = touch.position.ReadValue();
                    if ((currentPosition - _touchStartScreenPosition).sqrMagnitude > TouchTapMaxMovePixels * TouchTapMaxMovePixels)
                        _touchMovedBeyondTap = true;

                    return true;
                }

                if (touch.press.wasReleasedThisFrame)
                {
                    CompleteTouchTracking(touch.position.ReadValue());
                    return true;
                }
            }

            if (pressedTouchCount > 0)
                return true;

            if (_trackedTouchId >= 0)
                ResetTouchTracking();

            return false;
        }

        private void BeginTouchTracking(TouchControl touch, int touchId)
        {
            _trackedTouchId = touchId;
            _touchStartScreenPosition = touch.position.ReadValue();
            _touchStartTime = Time.unscaledTime;
            _touchStartedOverUi = IsScreenPositionOverUi(_touchStartScreenPosition, touchId);
            _touchMovedBeyondTap = false;
            _multiTouchObserved = CountPressedTouches(Touchscreen.current) > 1;
        }

        private void CompleteTouchTracking(Vector2 releaseScreenPosition)
        {
            bool isTap = !_touchStartedOverUi
                && !_touchMovedBeyondTap
                && !_multiTouchObserved
                && Time.unscaledTime - _touchStartTime <= TouchTapMaxDurationSeconds;

            if (isTap)
            {
                var cam = ResolveCamera();
                if (cam != null)
                    FireTileClick(releaseScreenPosition, cam, TilePointerButton.Primary);
            }

            ResetTouchTracking();
        }

        private void ResetTouchTracking()
        {
            _trackedTouchId = -1;
            _touchStartScreenPosition = Vector2.zero;
            _touchStartTime = 0f;
            _touchStartedOverUi = false;
            _touchMovedBeyondTap = false;
            _multiTouchObserved = false;
        }

        private static int CountPressedTouches(Touchscreen touchscreen)
        {
            if (touchscreen == null)
                return 0;

            int pressedTouchCount = 0;
            var touches = touchscreen.touches;
            for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
            {
                if (touches[touchIndex].press.isPressed)
                    pressedTouchCount++;
            }

            return pressedTouchCount;
        }

        private bool IsScreenPositionOverUi(Vector2 screenPosition, int pointerId)
        {
            return _inputPolicy?.IsPointerOverUi(screenPosition, pointerId, interactiveOnly: false) ?? false;
        }

        private Camera ResolveCamera()
        {
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
                return _cachedCamera;

            _cachedCamera = Camera.main;
            return _cachedCamera != null && _cachedCamera.isActiveAndEnabled
                ? _cachedCamera
                : null;
        }

        private void TryFireMouseTileClick(Vector2 screenPosition, TilePointerButton button)
        {
            GameplayInputKind inputKind = button == TilePointerButton.Primary
                ? GameplayInputKind.PrimaryPointer
                : GameplayInputKind.SecondaryPointer;
            if (!(_inputPolicy?.CanProcess(inputKind, screenPosition) ?? true))
                return;

            Camera cam = ResolveCamera();
            if (cam != null)
                FireTileClick(screenPosition, cam, button);
        }

        private void FireTileClick(Vector2 screenPos, Camera cam, TilePointerButton button)
        {
            Vector2Int tilePos;
            if (_pointerGridResolver != null
                && _pointerGridResolver.TryScreenToGrid(screenPos, out tilePos))
            {
                FireResolvedTileClick(tilePos, button);
                return;
            }

            Vector3 worldPos = cam != null
                ? cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z))
                : Vector3.zero;
            tilePos = _gridProjection != null
                ? _gridProjection.WorldToGrid(worldPos)
                : new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

            FireResolvedTileClick(tilePos, button);
        }

        private void FireResolvedTileClick(Vector2Int tilePos, TilePointerButton button)
        {
            _signalBus.Fire(new TileClickedSignal
            {
                Position = tilePos,
                Button = button,
            });
        }
    }
}
