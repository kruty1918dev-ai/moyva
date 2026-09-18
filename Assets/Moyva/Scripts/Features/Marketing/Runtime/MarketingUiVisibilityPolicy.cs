using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Marketing.Runtime
{
    /// <summary>
    /// Single authority for UI visibility during capture. Enumerates canvases
    /// once (bounded traversal of the gameplay scene roots), applies a policy,
    /// and restores the original state afterwards. No per-frame Find calls.
    /// </summary>
    public sealed class MarketingUiVisibilityPolicy
    {
        private sealed class CanvasState
        {
            public Canvas canvas;
            public bool wasEnabled;
        }

        private readonly List<CanvasState> _states = new List<CanvasState>();
        private bool _applied;

        public void Collect(Scene gameplayScene)
        {
            _states.Clear();
            if (!gameplayScene.IsValid()) return;
            foreach (var root in gameplayScene.GetRootGameObjects())
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                    _states.Add(new CanvasState { canvas = canvas, wasEnabled = canvas.enabled });
        }

        public void Apply(MarketingUiVisibility mode)
        {
            foreach (var s in _states)
            {
                if (s.canvas == null) continue;
                switch (mode)
                {
                    case MarketingUiVisibility.Hidden:
                        s.canvas.enabled = false;
                        break;
                    case MarketingUiVisibility.Minimal:
                        // Minimal = keep only HUD bars (heuristic: names without
                        // panel/modal/menu keywords stay visible).
                        string n = s.canvas.name.ToLowerInvariant();
                        s.canvas.enabled = !(n.Contains("panel") || n.Contains("modal") || n.Contains("menu"));
                        break;
                    default:
                        s.canvas.enabled = true;
                        break;
                }
            }
            _applied = true;
        }

        public void Restore()
        {
            if (!_applied) return;
            foreach (var s in _states)
                if (s.canvas != null) s.canvas.enabled = s.wasEnabled;
            _applied = false;
        }
    }
}
