using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Zenject;

namespace Kruty1918.Moyva.Shared.Controls
{
    /// <summary>
    /// Невеликий журнал натиснутих клавіш: показує останні акорди у кутку екрана.
    /// Вмикається/вимикається через PlayerControlSettingsData.InputLogEnabled.
    /// </summary>
    internal sealed class GameplayInputLogService : IInitializable, ITickable, IDisposable
    {
        internal const int MaxEntries = 3;
        internal const float EntryLifetimeSeconds = 4f;

        private readonly IPlayerControlSettingsService _settings;
        private readonly List<Entry> _entries = new List<Entry>(MaxEntries);
        private InputLogOverlayBehaviour _overlay;

        public GameplayInputLogService(
            [InjectOptional] IPlayerControlSettingsService settings = null)
        {
            _settings = settings;
        }

        internal IReadOnlyList<Entry> Entries => _entries;
        internal bool Enabled => _settings?.Settings.InputLogEnabled ?? true;

        public void Initialize()
        {
            var go = new GameObject("MoyvaInputLogOverlay");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _overlay = go.AddComponent<InputLogOverlayBehaviour>();
            _overlay.Bind(this);
        }

        public void Tick()
        {
            if (!Enabled)
            {
                if (_entries.Count > 0)
                    _entries.Clear();
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                foreach (KeyControl key in keyboard.allKeys)
                {
                    if (!key.wasPressedThisFrame || IsModifierKey(key.keyCode))
                        continue;

                    Record(keyboard, key.keyCode);
                }
            }

            PruneExpired();
        }

        public void Dispose()
        {
            if (_overlay != null)
                UnityEngine.Object.Destroy(_overlay.gameObject);
            _overlay = null;
            _entries.Clear();
        }

        private void Record(Keyboard keyboard, Key key)
        {
            var text = FormatChord(keyboard, key);
            if (_entries.Count > 0 && _entries[_entries.Count - 1].Text == text)
            {
                _entries[_entries.Count - 1] = new Entry(text, Time.unscaledTime);
                return;
            }

            _entries.Add(new Entry(text, Time.unscaledTime));
            if (_entries.Count > MaxEntries)
                _entries.RemoveRange(0, _entries.Count - MaxEntries);
        }

        private void PruneExpired()
        {
            float now = Time.unscaledTime;
            _entries.RemoveAll(entry => now - entry.Time > EntryLifetimeSeconds);
        }

        private static bool IsModifierKey(Key key) => key switch
        {
            Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift
                or Key.LeftAlt or Key.RightAlt or Key.LeftCommand or Key.RightCommand => true,
            _ => false
        };

        private static string FormatChord(Keyboard keyboard, Key key)
        {
            var text = key.ToString();
            bool ctrl = keyboard.ctrlKey.isPressed;
            bool shift = keyboard.shiftKey.isPressed;
            bool alt = keyboard.altKey.isPressed;
            if (!ctrl && !shift && !alt)
                return text;

            return string.Concat(
                ctrl ? "Ctrl+" : string.Empty,
                shift ? "Shift+" : string.Empty,
                alt ? "Alt+" : string.Empty,
                text);
        }

        internal readonly struct Entry
        {
            public readonly string Text;
            public readonly float Time;

            public Entry(string text, float time)
            {
                Text = text;
                Time = time;
            }
        }
    }

    /// <summary>IMGUI-рендер журналу введення; створюється сервісом.</summary>
    internal sealed class InputLogOverlayBehaviour : MonoBehaviour
    {
        private GameplayInputLogService _log;
        private GUIStyle _style;

        public void Bind(GameplayInputLogService log)
        {
            _log = log;
        }

        private void OnGUI()
        {
            if (_log == null || !_log.Enabled || _log.Entries.Count == 0)
                return;

            _style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 0, 0)
            };

            const float width = 108f;
            const float rowHeight = 14f;
            float height = _log.Entries.Count * rowHeight + 3f;
            var area = new Rect(8f, Screen.height - height - 8f, width, height);

            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);

            float now = Time.unscaledTime;
            for (int i = 0; i < _log.Entries.Count; i++)
            {
                var entry = _log.Entries[i];
                float age = now - entry.Time;
                float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, age - GameplayInputLogService.EntryLifetimeSeconds * 0.6f)
                    / (GameplayInputLogService.EntryLifetimeSeconds * 0.4f));
                GUI.color = new Color(1f, 1f, 1f, 0.35f + 0.65f * alpha);
                GUI.Label(new Rect(area.x, area.y + 1.5f + i * rowHeight, width, rowHeight), entry.Text, _style);
            }

            GUI.color = Color.white;
        }
    }
}
