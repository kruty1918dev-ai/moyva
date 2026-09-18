using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Глобальні звуки вказівника для uGUI-UI (HTML-кнопки MoyvaUI рендеряться
    /// як справжні Unity Selectable). Click / denied / hover / slider-tick —
    /// ключі беруться з audio-feedback пресету.
    /// </summary>
    public sealed class UiPointerAudioService : ITickable
    {
        private readonly IAudioService _audio;
        private readonly AudioFeedbackConfig _config;
        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>(16);

        private Selectable _hovered;
        private Selectable _pressed;
        private float _lastHoverTime;
        private float _lastDragTickTime;
        private Vector2 _lastPointerPos;

        public UiPointerAudioService(
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioFeedbackConfig config)
        {
            _audio = audio;
            _config = config;
        }

        public void Tick()
        {
            if (_audio == null || _config == null)
                return;

            var pointer = Pointer.current;
            if (pointer == null)
                return;

            Vector2 pos = pointer.position.ReadValue();
            bool moved = (pos - _lastPointerPos).sqrMagnitude > 0.25f;
            _lastPointerPos = pos;

            var hovered = RaycastSelectable(pos);
            if (!ReferenceEquals(hovered, _hovered))
            {
                _hovered = hovered;
                if (hovered != null
                    && _config.uiPointerHoverEnabled
                    && hovered.IsInteractable()
                    && Time.unscaledTime - _lastHoverTime >= _config.uiPointerHoverCooldown)
                {
                    _lastHoverTime = Time.unscaledTime;
                    Play(_config.uiPointerHover);
                }
            }

            bool pressed = pointer.press != null && pointer.press.wasPressedThisFrame;
            bool released = pointer.press != null && pointer.press.wasReleasedThisFrame;

            if (pressed)
            {
                _pressed = hovered;
                if (hovered != null)
                    Play(hovered.IsInteractable() ? _config.uiPointerClick : _config.uiPointerDenied);
            }

            if (released)
                _pressed = null;

            // Тихий "tick" під час перетягування слайдера.
            if (moved
                && _pressed is Slider slider
                && slider.IsInteractable()
                && Time.unscaledTime - _lastDragTickTime >= _config.uiPointerDragTickInterval)
            {
                _lastDragTickTime = Time.unscaledTime;
                Play(_config.uiPointerDragTick);
            }
        }

        private Selectable RaycastSelectable(Vector2 screenPos)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return null;

            _raycastResults.Clear();
            var eventData = new PointerEventData(eventSystem) { position = screenPos };
            eventSystem.RaycastAll(eventData, _raycastResults);

            for (int i = 0; i < _raycastResults.Count; i++)
            {
                var selectable = _raycastResults[i].gameObject
                    .GetComponentInParent<Selectable>();
                if (selectable != null)
                    return selectable;
            }

            return null;
        }

        private void Play(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _audio.Play(key);
        }
    }
}
